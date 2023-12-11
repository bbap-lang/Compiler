using System.Collections.Immutable;
using System.Diagnostics;
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


    public static Result<IExpression[]> RunDeclare(DeclareFunctionCallSetExpression decFunctionCallSetExpression,
        PreTranspilerState state) {
        
        Result<string> functionNameResult = FunctionCallPreTranspiler.GetName(state, decFunctionCallSetExpression.Name);
        if (!functionNameResult.TryGetValue(out string? functionName)) return functionNameResult.ToErrorResult();
        
        Result<IFunction> functionResult = state.GetFunction(functionName, decFunctionCallSetExpression.Line);
        if (!functionResult.TryGetValue(out IFunction? function)) return functionResult.ToErrorResult();


        Result<IType[]> returnVariableTypesResult = function.GetReturnTypes(decFunctionCallSetExpression.ReturnVariables.Length, decFunctionCallSetExpression.Line);
        if (!returnVariableTypesResult.TryGetValue(out IType[]? returnVariableTypes))
            return returnVariableTypesResult.ToErrorResult();
        
        
        var returnVariables = new List<VariableExpression>();
        foreach ((VariableExpression returnVariable, int index) in decFunctionCallSetExpression.ReturnVariables.Select((x, i) => (x, i))) {
            IType returnType = returnVariableTypes[index];

            Result<string> newVarResult = state.CreateVar(returnVariable.Variable.Name, returnType, returnVariable.Line);
            if (!newVarResult.TryGetValue(out string? newVariableName)) return newVarResult.ToErrorResult();

            Result<IVariable> newVariableResult = state.GetVariable(newVariableName, decFunctionCallSetExpression.Line);
            if (!newVariableResult.TryGetValue(out IVariable? newVariable)) throw new UnreachableException();
            
            VariableExpression newVariableExpression = returnVariable with { Variable = newVariable };

            returnVariables.Add(newVariableExpression);
        }

        var allExpressions = new List<IExpression>();

        foreach (VariableExpression returnVariable in returnVariables) {
            var typeExpression = new TypeExpression(returnVariable.Line, returnVariable.Variable.Type);
            var declareExpression = new DeclareExpression(returnVariable.Line, returnVariable, typeExpression, null);
            allExpressions.Add(declareExpression);
        }

        var functionCallSetExpression = new FunctionCallSetExpression(decFunctionCallSetExpression.Line,
                                                                      decFunctionCallSetExpression.Name,
                                                                      decFunctionCallSetExpression.Parameters,
                                                                      decFunctionCallSetExpression.ReturnVariables);

        Result<IExpression[]> result = Run(functionCallSetExpression, state);
        if (!result.TryGetValue(out IExpression[]? additionalExpressions)) return result.ToErrorResult();

        allExpressions.AddRange(additionalExpressions);

        return Ok(allExpressions.ToArray());
    }
}