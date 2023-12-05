using System.Collections.Immutable;
using BBAP.Functions;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.PreTranspiler.Variables;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.PreTranspiler.SubPreTranspiler;

public static class FunctionCallSetPreTranspiler {
    public static Result<IExpression[]> Run(FunctionCallSetExpression functionCallSetExpression,
        PreTranspilerState state) {
        var returnVariables = new List<VariableExpression>();

        foreach (VariableExpression returnVariable in functionCallSetExpression.ReturnVariables) {
            Result<IVariable> varResult = state.GetVariable(returnVariable.Variable.Name, returnVariable.Line);

            if (!varResult.TryGetValue(out IVariable? newVariable)) return varResult.ToErrorResult();

            VariableExpression newVariableExpression = returnVariable with { Variable = newVariable };

            returnVariables.Add(newVariableExpression);
        }


        var parameters = new List<SecondStageParameterExpression>();
        var newTree = new List<IExpression>();

        Result<string> functionNameResult = FunctionCallPreTranspiler.GetName(state, functionCallSetExpression.Name);
        if (!functionNameResult.TryGetValue(out string? functionName)) return functionNameResult.ToErrorResult();

        Result<IFunction> functionResult = state.GetFunction(functionName, functionCallSetExpression.Line);
        if (!functionResult.TryGetValue(out IFunction? function)) return functionResult.ToErrorResult();


        if (function.IsMethod && !function.IsStatic) {
            Result<VariableExpression> thisParameterResult
                = FunctionCallPreTranspiler.GetThisVariable(state, functionCallSetExpression.Name);
            if (!thisParameterResult.TryGetValue(out VariableExpression? thisParameter))
                return thisParameterResult.ToErrorResult();
            var typeExpression = new TypeExpression(thisParameter.Line, thisParameter.Variable.Type);

            var parameterExpression
                = new SecondStageParameterExpression(functionCallSetExpression.Name.Line, thisParameter,
                                                     typeExpression);
            parameters.Add(parameterExpression);
        }


        foreach (IExpression parameter in functionCallSetExpression.Parameters) {
            Result<FunctionCallPreTranspiler.ExtractParameterResult> result
                = FunctionCallPreTranspiler.ExtractParameter(state, parameter);

            if (!result.TryGetValue(out FunctionCallPreTranspiler.ExtractParameterResult extractParameterResult))
                return result.ToErrorResult();

            newTree.AddRange(extractParameterResult.AdditionalExpressions);
            newTree.Add(extractParameterResult.DeclareExpression);
            parameters.Add(new SecondStageParameterExpression(extractParameterResult.NewParameter.Line,
                                                              extractParameterResult.NewParameter,
                                                              new
                                                                  TypeExpression(extractParameterResult.NewParameter.Line,
                                                                   extractParameterResult.NewParameter.Variable
                                                                       .Type)));
        }

        IType[] inputTypes = parameters.Select(x => x.Type.Type).ToArray();
        IType[] returnTypes = returnVariables.Select(x => x.Variable.Type).ToArray();
        Result<int> matchResult = function.Matches(inputTypes, returnTypes, functionCallSetExpression.Line);
        if (!matchResult.IsSuccess) return matchResult.ToErrorResult();

        var newExpression = new SecondStageFunctionCallExpression(functionCallSetExpression.Line, function,
                                                                  parameters.ToImmutableArray(),
                                                                  returnVariables.ToImmutableArray());

        newTree.Add(newExpression);
        return Ok(newTree.ToArray());
    }
}