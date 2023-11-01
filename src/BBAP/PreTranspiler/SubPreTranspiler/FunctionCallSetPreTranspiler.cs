using System.Collections.Immutable;
using BBAP.Functions;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.Results;

namespace BBAP.PreTranspiler.SubPreTranspiler; 

public static class FunctionCallSetPreTranspiler {
    public static Result<IExpression[]> Run(FunctionCallSetExpression functionCallSetExpression, PreTranspilerState state) {
        var returnVariables = new List<VariableExpression>();

        foreach (VariableExpression returnVariable in functionCallSetExpression.ReturnVariables) {
            Result<Variable> varResult = state.GetVariable(returnVariable.Name, returnVariable.Line);

            if(!varResult.TryGetValue(out Variable? newVariable)) {
                return varResult.ToErrorResult();
            }
            
            var newVariableExpression = new VariableExpression(returnVariable.Line, returnVariable.Name, newVariable.Type);
            
            returnVariables.Add(newVariableExpression);
        }

        var parameters = new List<VariableExpression>();
        var newTree = new List<IExpression>();
        
        foreach (IExpression parameter in functionCallSetExpression.Parameters) {
            Result<FunctionCallPreTranspiler.ExtractParameterResult> result = FunctionCallPreTranspiler.ExtractParameter(state, parameter);
            
            if(!result.TryGetValue(out var extractParameterResult)) {
                return result.ToErrorResult();
            }
            
            newTree.AddRange(extractParameterResult.AdditionalExpressions);
            newTree.Add(extractParameterResult.DeclareExpression);
            parameters.Add(extractParameterResult.NewParameter);
        }

        Result<IFunction> functionResult = state.GetFunction(functionCallSetExpression.Name, functionCallSetExpression.Line);
        if(!functionResult.TryGetValue(out IFunction? function)) {
            return functionResult.ToErrorResult();
        }

        var newExpression = new SecondStageFunctionCallExpression(functionCallSetExpression.Line, function,
                                                                  parameters.ToImmutableArray(),
                                                                  returnVariables.ToImmutableArray());

        newTree.Add(newExpression);
        return Ok(newTree.ToArray());
    }
}