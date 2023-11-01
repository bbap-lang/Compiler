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
        Variable? variable;
        if (!ignoreNotDeclared) {
            Result<Variable> variableResult
                = state.GetVariable(setExpression.Variable.Name, setExpression.Variable.Line);
            if (!variableResult.TryGetValue(out variable)) {
                return variableResult.ToErrorResult();
            }
        } else {
            variable = new Variable(setExpression.Variable.Type, setExpression.Variable.Name);
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

        var variableExpression = new VariableExpression(setExpression.Line, variable.Name, variable.Type);

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