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
        var parameters = new List<SecondStageParameterExpression>();

        Result<IFunction> functionResult = state.GetFunction(functionCallExpression.Name, functionCallExpression.Line);
        if (!functionResult.TryGetValue(out IFunction function)) {
            return functionResult.ToErrorResult();
        }

        foreach (IExpression parameter in functionCallExpression.Parameters) {
            Result<ExtractParameterResult> result = ExtractParameter(state, parameter);
            
            if(!result.TryGetValue(out ExtractParameterResult extractParameterResult)) {
                return result.ToErrorResult();
            }
            
            additionalExpressions.AddRange(extractParameterResult.AdditionalExpressions);
            additionalExpressions.Add(extractParameterResult.DeclareExpression);
            parameters.Add(new SecondStageParameterExpression(extractParameterResult.NewParameter.Line, extractParameterResult.NewParameter, extractParameterResult.NewParameter.Variable.Type));
        }

        IType[] parameterTypes = parameters.Select(x => x.Variable.Variable.Type).ToArray();

        var outputs = new IType[0];
        ImmutableArray<VariableExpression> outputVariables = new VariableExpression[0].ToImmutableArray();

        if (!function.Matches(parameterTypes, outputs)) {
            return Error(functionCallExpression.Line, $"Invalid function parameters");
        }

        var newExpression
            = new SecondStageFunctionCallExpression(functionCallExpression.Line, function, parameters.ToImmutableArray(),
                outputVariables);

        IExpression[] combined = additionalExpressions.Append(newExpression).ToArray();

        return Ok(combined);
    }

    public static Result<ExtractParameterResult> ExtractParameter(PreTranspilerState state, IExpression parameter) {
        
        Result<IExpression[]> result = ValueSplitter.Run(state, parameter);
        if (!result.TryGetValue(out IExpression[]? expressions)) {
            return result.ToErrorResult();
        }

        IExpression lastGeneral = expressions.Last();
        if (lastGeneral is not ISecondStageValue last) {
            throw new UnreachableException();
        }


        VariableExpression newVar = state.CreateRandomNewVar(last.Line, last.Type);
        var setExpression = new SetExpression(last.Line, newVar, SetType.Generic, last);
        var typeExpression = new TypeExpression(last.Line, last.Type);
        var declareExpression = new DeclareExpression(last.Line, newVar, typeExpression, setExpression);

        return Ok(new ExtractParameterResult(expressions.Remove(last), declareExpression, newVar));
    }

    public record struct ExtractParameterResult(
        IEnumerable<IExpression> AdditionalExpressions,
        DeclareExpression DeclareExpression,
        VariableExpression NewParameter
    );
}