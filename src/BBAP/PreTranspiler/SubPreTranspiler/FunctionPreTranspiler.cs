using System.Collections.Immutable;
using System.Reflection.Metadata;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.PreTranspiler.SubPreTranspiler;

public static class FunctionPreTranspiler {
    public static Result<IExpression[]> Replace(FunctionExpression functionExpression, PreTranspilerState state) {
        SecondStageFunctionExpression declaredFunction
            = state.GetDeclaredFunction(functionExpression.Name);

        state.StackIn(declaredFunction.StackName);


        Variable[] returnVariables = declaredFunction.ReturnVariables.ToArray();
        state.GoIntoFunction(returnVariables);

        Result<ImmutableArray<IExpression>>
            blockResult = PreTranspiler.RunBlock(state, functionExpression.BlockContent);
        if (!blockResult.TryGetValue(out ImmutableArray<IExpression> block)) {
            return blockResult.ToErrorResult();
        }

        SecondStageFunctionExpression newFunctionExpression = declaredFunction with { ContentBlock = block};

        state.GoOutOfFunction();
        state.StackOut();
        return Ok(new IExpression[] { newFunctionExpression });
    }

    public static Result<SecondStageFunctionExpression> Create(FunctionExpression functionExpression,
        PreTranspilerState state) {
        var stackName = state.StackIn();
        var parameters = new List<Variable>();
        foreach (ParameterExpression parameter in functionExpression.Parameters) {
            Result<IType> typeResult = state.Types.Get(parameter.Line, parameter.Type);
            if (!typeResult.TryGetValue(out IType? type)) {
                return typeResult.ToErrorResult();
            }

            Result<string> variableNameResult = state.CreateVar(parameter.Name, type, parameter.Line);

            if (!variableNameResult.TryGetValue(out string? variableName)) {
                return variableNameResult.ToErrorResult();
            }

            var variable = new Variable(type, variableName);

            parameters.Add(variable);
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
        
        Result<Variable>[] returnVariablesResults = returnTypes.Select(returnType
                                                                       => state.CreateRandomNewVar(functionExpression.Line,
                                                                        returnType.Type))
                                                           .Select(x => state.GetVariable(x.Name, x.Line))
                                                           .ToArray();

        var returnVariables = new Variable[returnVariablesResults.Length];
        foreach ((Result<Variable> newVariable, int index) in returnVariablesResults.Select((x, i) => (x, i))) {
            if (!newVariable.TryGetValue(out Variable variable)) {
                return newVariable.ToErrorResult();
            }

            returnVariables[index] = variable;
        }

        var newFunctionExpression = new SecondStageFunctionExpression(functionExpression.Line, functionExpression.Name,
                                                                      parameters.ToImmutableArray(),
                                                                      returnVariables.ToImmutableArray(),
                                                                      ImmutableArray<IExpression>.Empty, stackName);

        state.StackOut();
        return Ok(newFunctionExpression);
    }
}