using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.PreTranspiler.SubPreTranspiler;

public static class SetPreTranspiler {
    public static Result<IExpression[]> Run(SetExpression setExpression, PreTranspilerState state, bool ignoreNotDeclared) {
        IVariable? variable;
        if (!ignoreNotDeclared) {
            Result<IVariable> variableResult
                = state.GetVariable(setExpression.Variable.Variable, setExpression.Variable.Line);
            if (!variableResult.TryGetValue(out variable)) {
                return variableResult.ToErrorResult();
            }
        } else {
            variable = setExpression.Variable.Variable;
        }

        Result<IExpression[]> splittedValueResult = ValueSplitter.Run(state, setExpression.Value);
        if (!splittedValueResult.TryGetValue(out IExpression[]? splittedValue)) {
            return splittedValueResult;
        }

        IExpression lastValueExpression = splittedValue.Last();

        if (lastValueExpression is not ISecondStageValue lastValue) {
            throw new UnreachableException();
        }

        if (!ignoreNotDeclared && !lastValue.Type.IsCastableTo(variable.Type)) {
            return Error(lastValue.Line, $"Cannot cast {lastValue.Type} to {variable.Type}");
        }

        var variableExpression = new VariableExpression(setExpression.Line, variable);

        IExpression newExpression;
        if (lastValue is SecondStageFunctionCallExpression funcCall) {
            newExpression = funcCall with { Outputs = ImmutableArray.Create(variableExpression) };
        } else {
            newExpression = setExpression with { Value = lastValue, Variable = variableExpression };
        }


        IExpression[] newExpressions = splittedValue.Remove(lastValue).Append(newExpression).ToArray();

        return Ok(newExpressions);
    }
}