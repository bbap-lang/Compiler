using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.ExtensionMethods;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.PreTranspiler.Variables;
using BBAP.Results;
using BBAP.Types;
using BBAP.Types.Types.FullTypes;

namespace BBAP.PreTranspiler.SubPreTranspiler;

public class NewStructPreTranspiler {
    public static Result<IExpression[]> Run(StructSetExpression structSetExpression, PreTranspilerState state) {
        Result<IVariable> structVarResult
            = state.GetVariable(structSetExpression.Variable.Variable.Name, structSetExpression.Line);
        if (!structVarResult.TryGetValue(out IVariable? structVar)) throw new UnreachableException();

        IType generalType = structVar.Type;
        if (generalType is not StructType type)
            return Error(structSetExpression.Line, $"{generalType.Name} is not a struct");

        var structVarExpression = new VariableExpression(structSetExpression.Line, structVar);
        var structTypeExpression = new TypeExpression(structSetExpression.Line, type);

        var declareExpression
            = new DeclareExpression(structSetExpression.Line, structVarExpression, structTypeExpression, null);

        List<IExpression> additionalExpressions = new();
        Result<IExpression[]> setExpressionsResults = structSetExpression.Fields.Select(x => {
            Variable? field = type.Fields.FirstOrDefault(y => y.Name == x.Variable.Variable.Name);

            if (field is null)
                return Error(x.Line, $"Field '{x.Variable.Variable.Name}' not found in struct '{type.Name}'");

            Result<IExpression[]> valueResult = ValueSplitter.Run(state, x.Value);
            if (!valueResult.TryGetValue(out IExpression[]? valueExpressions)) return valueResult.ToErrorResult();

            additionalExpressions.AddRange(valueExpressions[..^1]);

            IExpression valueExpression = valueExpressions[^1];

            if (valueExpression is not ISecondStageValue value) throw new UnreachableException();

            if (!value.Type.Type.IsCastableTo(field.Type))
                return Error(x.Line, $"{x.Variable.Variable.Type.AbapName} is not castable to {field.Type.AbapName}");

            var newFieldVariable = new FieldVariable(type, field.Name, structVar);
            var variableExpression = new VariableExpression(x.Line, newFieldVariable);


            IExpression setExpression;
            if (value is SecondStageFunctionCallExpression funcCall)
                setExpression = funcCall with { Outputs = ImmutableArray.Create(variableExpression) };
            else
                setExpression = new SetExpression(x.Line, variableExpression, SetType.Generic, value);

            return Ok(setExpression);
        }).ToArray().Wrap();

        if (!setExpressionsResults.TryGetValue(out IExpression[]? setExpressions))
            return setExpressionsResults.ToErrorResult();

        IExpression[] expressions
            = ArrayBuilder<IExpression>.Concat(additionalExpressions).Concat(setExpressions).Build();
        return Ok(expressions);
    }
}