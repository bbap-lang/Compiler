using System.Diagnostics;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.PreTranspiler.Variables;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.PreTranspiler.SubPreTranspiler;

public static class ReturnPreTranspiler {
    public static Result<IExpression[]> Run(ReturnExpression returnExpression, PreTranspilerState state) {
        List<IExpression> newExpressions = new();
        List<ISecondStageValue> newValues = new();

        foreach (IExpression value in returnExpression.ReturnValues) {
            Result<IExpression[]> result = ValueSplitter.Run(state, value);
            if (!result.TryGetValue(out IExpression[]? newExpression)) return result;

            IExpression newValueGeneral = newExpression.Last();
            if (newValueGeneral is not ISecondStageValue newValue) throw new UnreachableException();


            newExpressions.AddRange(newExpression.Remove(newValue));
            newValues.Add(newValue);
        }

        IVariable[] returnVariables = state.GetCurrentReturnVariables();

        if (newValues.Count != returnVariables.Length)
            return Error(returnExpression.Line,
                         "The amount of return values does not match the amount of return variables");

        foreach ((IVariable returnVariable, ISecondStageValue value) in
                 returnVariables.Select((x, i) => (x, newValues[i]))) {
            if (!value.Type.Type.IsCastableTo(returnVariable.Type))
                return Error(returnExpression.Line,
                             $"The type of the return value ({value.Type.Type.Name}) does not match the type of the return variable ({returnVariable.Type.Name})");

            var typeExpression = new TypeExpression(returnExpression.Line, returnVariable.Type);
            var variableExpression
                = new VariableExpression(returnExpression.Line, new Variable(returnVariable.Type, returnVariable.Name));
            var setExpression = new SetExpression(returnExpression.Line, variableExpression, SetType.Generic, value);
            var declareExpression
                = new DeclareExpression(returnExpression.Line, variableExpression, typeExpression, setExpression);
            newExpressions.Add(declareExpression);
        }

        return Ok(newExpressions.ToArray());
    }
}