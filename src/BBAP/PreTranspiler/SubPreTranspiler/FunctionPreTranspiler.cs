using System.Collections.Immutable;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.PreTranspiler.SubPreTranspiler; 

public static class FunctionPreTranspiler {

    public static Result<IExpression[]> Replace(FunctionExpression functionExpression, PreTranspilerState state) {
        SecondStageFunctionExpression declaredFunction = state.GetDeclaredFunction(functionExpression.Line, functionExpression.Name);
        
        state.StackIn(declaredFunction.StackName);
        Result<ImmutableArray<IExpression>> blockResult = PreTranspiler.RunBlock(state, functionExpression.BlockContent);
        if (!blockResult.TryGetValue(out ImmutableArray<IExpression> block)) {
            return blockResult.ToErrorResult();
        }

        SecondStageFunctionExpression newFunctionExpression = declaredFunction with { ContentBlock = block };

        state.StackOut();
        return Ok(new IExpression[]{newFunctionExpression});
    }

    public static Result<SecondStageFunctionExpression> Create(FunctionExpression functionExpression, PreTranspilerState state) {
        var stackName = state.StackIn();
        var parameters = new List<SecondStageParameterExpression>();
        foreach (ParameterExpression parameter in functionExpression.Parameters) {
            Result<IType> typeResult = state.Types.Get(parameter.Line, parameter.Type);
            if (!typeResult.TryGetValue(out IType? type)) {
                return typeResult.ToErrorResult();
            }

            Result<string> variableNameResult = state.CreateVar(parameter.Name, type, parameter.Line);

            if (!variableNameResult.TryGetValue(out string? variableName)) {
                return variableNameResult.ToErrorResult();
            }

            var variableExpression = new VariableExpression(parameter.Line, variableName);

            var newParameter = new SecondStageParameterExpression(parameter.Line, variableExpression, type);
            
            parameters.Add(newParameter);
        }

        var returnTypes = new List<TypeExpression>();

        foreach (TypeExpression typeExpression in functionExpression.OutputTypes) {
            Result<IType> typeResult = state.Types.Get(typeExpression.Line, typeExpression.Type.Name);
            if (!typeResult.TryGetValue(out IType? type)) {
                return typeResult.ToErrorResult();
            }

            TypeExpression newTypeExpression = typeExpression with { Type = type };
            returnTypes.Add(newTypeExpression);
        }
        
        var newFunctionExpression = new SecondStageFunctionExpression(functionExpression.Line, functionExpression.Name,
                                                                      parameters.ToImmutableArray(),
                                                                      returnTypes.ToImmutableArray(),
                                                                      ImmutableArray<IExpression>.Empty, stackName);
                    
        state.StackOut();
        return Ok(newFunctionExpression);
    }
}