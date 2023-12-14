using System.Diagnostics;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Calculations;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.PreTranspiler.Variables;
using BBAP.Results;

namespace BBAP.PreTranspiler.SubPreTranspiler;

public static class IncrementPreTranspiler {
    public static Result<IExpression[]> Run(IncrementExpression incrementExpression, PreTranspilerState state) {
        Result<IVariable> variableResult
            = state.GetVariable(incrementExpression.Variable.Variable.Name, incrementExpression.Line);
        if (!variableResult.TryGetValue(out IVariable variable)) return variableResult.ToErrorResult();
        if(variable.MutabilityType != MutabilityType.Mutable) return Error(incrementExpression.Line, $"Cannot set the value of a non mutable variable '{variable.Name}'");


        VariableExpression variableExpression = incrementExpression.Variable with { Variable = variable };
        SetType setType = incrementExpression.IncrementType switch {
            IncrementType.Plus => SetType.Plus,
            IncrementType.Minus => SetType.Minus,
            _ => throw new UnreachableException()
        };

        var oneExpression = new IntExpression(incrementExpression.Line, 1);
        var valueExpression = new SecondStageValueExpression(incrementExpression.Line,
                                                             new TypeExpression(incrementExpression.Line,
                                                                                variable.Type), oneExpression);
        var setExpression = new SetExpression(incrementExpression.Line, variableExpression, setType, valueExpression);

        return Ok(new IExpression[] { setExpression });
    }
}