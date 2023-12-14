using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.PreTranspiler.Expressions.Sql;
using BBAP.PreTranspiler.Variables;
using BBAP.Results;
using BBAP.Types;
using BBAP.Types.Types.FullTypes;

namespace BBAP.PreTranspiler.SubPreTranspiler;

public static class SetPreTranspiler {
    public static Result<IExpression[]> Run(SetExpression setExpression,
        PreTranspilerState state,
        bool ignoreNotDeclared) {
        IVariable? variable;
        if (!ignoreNotDeclared) {
            Result<IVariable> variableResult
                = state.GetVariable(setExpression.Variable.Variable, setExpression.Variable.Line);
            if (!variableResult.TryGetValue(out variable)) return variableResult.ToErrorResult();
            if(variable.MutabilityType != MutabilityType.Mutable) return Error(setExpression.Line, $"Cannot set the value of a non mutable variable '{variable.Name}'");
        } else {
            variable = setExpression.Variable.Variable;
        }

        Result<IExpression[]> splittedValueResult = ValueSplitter.Run(state, setExpression.Value);
        if (!splittedValueResult.TryGetValue(out IExpression[]? splittedValue)) return splittedValueResult;

        IExpression lastValueExpression = splittedValue.Last();

        if (lastValueExpression is SecondStageSelectExpression selectExpression) {
            Result<SecondStageSelectExpression> newSelectResult = RunSelect(selectExpression, setExpression, state);
            if (!newSelectResult.TryGetValue(out SecondStageSelectExpression? newSelect))
                return newSelectResult.ToErrorResult();

            return Ok(splittedValue.Remove(selectExpression).Append(newSelect).ToArray());
        }

        if (lastValueExpression is not ISecondStageValue lastValue) throw new UnreachableException();

        Result<int> typeCheckResult = CheckTypes(ignoreNotDeclared, lastValue, variable.Type);
        if (!typeCheckResult.IsSuccess) return typeCheckResult.ToErrorResult();

        var variableExpression = new VariableExpression(setExpression.Line, variable);

        IExpression newExpression;
        if (lastValue is SecondStageFunctionCallExpression funcCall)
            newExpression = funcCall with { Outputs = ImmutableArray.Create(variableExpression) };
        else
            newExpression = setExpression with { Value = lastValue, Variable = variableExpression };


        IExpression[] newExpressions = splittedValue.Remove(lastValue).Append(newExpression).ToArray();

        return Ok(newExpressions);
    }

    public static Result<int> CheckTypes(bool ignoreNotDeclared, ISecondStageValue value, IType variableType) {
        if (!ignoreNotDeclared && !value.Type.Type.IsCastableTo(variableType)) {
            if (!(value is SecondStageValueExpression { Value: StringExpression stringExpression }
               && variableType.IsCastableTo(TypeCollection.BaseCharType)))
                return Error(value.Line, $"Cannot cast {value.Type.Type.Name} to {variableType.Name}");

            IType rawType = variableType;
            if (rawType is AliasType aliasType) rawType = aliasType;

            if (rawType is CharType charType && charType.Length < stringExpression.Value.Length)
                return Error(value.Line,
                             $"The string has '{stringExpression.Value.Length}' characters, but the char array it should be stored in, can only store '{charType.Length}' characters.");
        }

        return Ok();
    }

    private static Result<SecondStageSelectExpression> RunSelect(SecondStageSelectExpression selectExpression,
        SetExpression setExpression,
        PreTranspilerState state) {
        Result<IVariable> variableResult
            = state.GetVariable(setExpression.Variable.Variable, setExpression.Variable.Line);
        if (!variableResult.TryGetValue(out IVariable? variable)) return variableResult.ToErrorResult();

        IType outputType = variable.Type;
        if (outputType is TableType tableType) outputType = tableType.ContentType;

        if (outputType is not StructType structType) {
            if (selectExpression.OutputFields.Length != 1)
                return Error(setExpression.Variable.Line,
                             $"The variable is of type '{outputType.Name}', but the select returns '{selectExpression.OutputFields.Length}' fields.");

            IType fieldType = selectExpression.OutputFields[0].Type.Type;
            if (!fieldType.IsCastableTo(outputType) && !outputType.IsCastableTo(fieldType))
                return Error(setExpression.Variable.Line,
                             $"The select returns a field of type '{fieldType.Name}', but the variable is of type '{outputType.Name}'. The types are not castable.");

            goto EndSelect;
        }

        if (selectExpression.OutputFields.Length != structType.Fields.Length)
            return Error(setExpression.Variable.Line,
                         $"The type '{outputType.Name}' has '{structType.Fields.Length}' fields, but the select returns '{selectExpression.OutputFields.Length}' fields.");

        for (int i = 0; i < selectExpression.OutputFields.Length; i++) {
            IType fieldType = selectExpression.OutputFields[i].Type.Type;
            IType structFieldType = structType.Fields[i].Type;
            if (!fieldType.IsCastableTo(structFieldType) && !structFieldType.IsCastableTo(fieldType))
                return Error(setExpression.Variable.Line,
                             $"The select returns a field of type '{fieldType.Name}', but the variable '{setExpression.Variable.Variable.Name}' is of type '{structFieldType.Name}'. The types are not castable.");
        }

        EndSelect:
        var variableExpression = new VariableExpression(setExpression.Line, variable);

        SecondStageSelectExpression newExpression = selectExpression with { OutputVariable = variableExpression };

        return Ok(newExpression);
    }
}