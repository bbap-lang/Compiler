using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.Functions;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.PreTranspiler.SubPreTranspiler;

public static class FunctionCallPreTranspiler {
    public static Result<IExpression[]> Run(PreTranspilerState state, FunctionCallExpression functionCallExpression) {
        var additionalExpressions = new List<IExpression>();
        var parameters = new List<SecondStageValueExpression>();

        Result<IFunction> functionResult = state.GetFunction(functionCallExpression.Name, functionCallExpression.Line);
        if (!functionResult.TryGetValue(out IFunction function)) {
            return functionResult.ToErrorResult();
        }

        foreach (IExpression parameter in functionCallExpression.Parameters) {
            Result<IExpression[]> result = ValueSplitter.Run(state, parameter);
            if (!result.TryGetValue(out IExpression[]? expressions)) {
                return result;
            }

            IExpression lastGeneral = expressions.Last();
            if (lastGeneral is not ISecondStageValues last) {
                throw new UnreachableException();
            }

            additionalExpressions.AddRange(expressions.Remove(last));

            VariableExpression newVar = state.CreateRandomNewVar(last.Line, last.Type);
            var setExpression = new SetExpression(last.Line, newVar, SetType.Generic, last);
            var typeExpression = new TypeExpression(last.Line, last.Type);
            var declareExpression = new DeclareExpression(last.Line, newVar, typeExpression, setExpression);

            additionalExpressions.Add(declareExpression);

            var valueEx = new SecondStageValueExpression(newVar.Line, last.Type, newVar);
            parameters.Add(valueEx);
        }

        IType[] parameterTypes = parameters.Select(GetType).ToArray();

        var outputs = new IType[0];
        ImmutableArray<VariableExpression> outputVariables = new VariableExpression[0].ToImmutableArray();

        if (!function.Matches(parameterTypes, outputs)) {
            return Error(functionCallExpression.Line, $"Invalid function parameters");
        }

        ImmutableArray<VariableExpression> parameterVariables = parameters.Select(x => x.Value)
                                                                          .OfType<VariableExpression>()
                                                                          .ToImmutableArray();

        var newExpression
            = new SecondStageFunctionCallExpression(functionCallExpression.Line, function, parameterVariables,
                outputVariables);

        IExpression[] combined = additionalExpressions.Append(newExpression).ToArray();

        return Ok(combined);
    }

    private static IType GetType(IExpression expression) {
        return expression switch {
            SecondStageCalculationExpression ce => ce.Type,
            SecondStageValueExpression ve => ve.Type,
            _ => throw new UnreachableException()
        };
    }
}