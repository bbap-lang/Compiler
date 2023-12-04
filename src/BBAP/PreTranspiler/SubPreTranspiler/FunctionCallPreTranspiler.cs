using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.Functions;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.Parser.SubParsers;
using BBAP.PreTranspiler.Expressions;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.PreTranspiler.SubPreTranspiler;

public static class FunctionCallPreTranspiler {
    public static Result<IExpression[]> Run(PreTranspilerState state, FunctionCallExpression functionCallExpression) {
        var additionalExpressions = new List<IExpression>();
        var parameters = new List<SecondStageParameterExpression>();

        Result<string> functionNameResult = GetName(state, functionCallExpression.Name);
        if (!functionNameResult.TryGetValue(out string? functionName)) {
            return functionNameResult.ToErrorResult();
        }

        Result<IFunction> functionResult = state.GetFunction(functionName, functionCallExpression.Line);
        if (!functionResult.TryGetValue(out IFunction? function)) {
            return functionResult.ToErrorResult();
        }

        if (function.IsMethod && !function.IsStatic) {
            Result<VariableExpression> thisParameterResult = GetThisVariable(state, functionCallExpression.Name);
            if(!thisParameterResult.TryGetValue(out VariableExpression? thisParameter)) {
                return thisParameterResult.ToErrorResult();
            }
            
            var typeExpression = new TypeExpression(thisParameter.Line, thisParameter.Variable.Type);
            
            var parameterExpression = new SecondStageParameterExpression(functionCallExpression.Name.Line, thisParameter, typeExpression);
            parameters.Add(parameterExpression);
        }

        foreach (IExpression parameter in functionCallExpression.Parameters) {
            Result<ExtractParameterResult> result = ExtractParameter(state, parameter);

            if (!result.TryGetValue(out ExtractParameterResult extractParameterResult)) {
                return result.ToErrorResult();
            }

            additionalExpressions.AddRange(extractParameterResult.AdditionalExpressions);
            additionalExpressions.Add(extractParameterResult.DeclareExpression);
            parameters.Add(new SecondStageParameterExpression(extractParameterResult.NewParameter.Line,
                                                              extractParameterResult.NewParameter,
                                                              new
                                                                  TypeExpression(extractParameterResult.NewParameter.Line,
                                                                   extractParameterResult.NewParameter.Variable
                                                                       .Type)));
        }

        IType[] parameterTypes = parameters.Select(x => x.Variable.Variable.Type).ToArray();

        var outputs = new IType[0];
        ImmutableArray<VariableExpression> outputVariables = new VariableExpression[0].ToImmutableArray();

        Result<int> matchResult = function.Matches(parameterTypes, outputs, functionCallExpression.Line);
        if (!matchResult.IsSuccess) {
            return matchResult.ToErrorResult();
        }

        var newExpression
            = new SecondStageFunctionCallExpression(functionCallExpression.Line, function,
                                                    parameters.ToImmutableArray(),
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


        VariableExpression newVar = state.CreateRandomNewVar(last.Line, last.Type.Type);
        var setExpression = new SetExpression(last.Line, newVar, SetType.Generic, last);
        var typeExpression = new TypeExpression(last.Line, last.Type.Type);
        var declareExpression = new DeclareExpression(last.Line, newVar, typeExpression, setExpression);

        return Ok(new ExtractParameterResult(expressions.Remove(last), declareExpression, newVar));
    }

    public record struct ExtractParameterResult(
        IEnumerable<IExpression> AdditionalExpressions,
        DeclareExpression DeclareExpression,
        VariableExpression NewParameter
    );

    public static Result<string> GetName(PreTranspilerState state, CombinedWord words) {
        if (words.GetCombinedWordType() == CombinedWordType.TypeOrStaticFunction) {
            return GetStaticName(state, words);
        }

        if (words.Variable.Length == 1) {
            return Ok(words.Variable[0]);
        }

        string functionName = words.Variable[^1];

        IVariable rawVariable = VariableParser.Run(words with { Variable = words.Variable[..^1] }).Variable;

        Result<IVariable> variableResult = state.GetVariable(rawVariable, words.Line);
        if (!variableResult.TryGetValue(out IVariable? variable)) {
            return variableResult.ToErrorResult();
        }

        return Ok($"{variable.Type.Name}.{functionName}");
    }

    public static Result<VariableExpression> GetThisVariable(PreTranspilerState state, CombinedWord words) {
        IVariable rawVariable = VariableParser.Run(words with { Variable = words.Variable[..^1] }).Variable;
        
        Result<IVariable> variableResult = state.GetVariable(rawVariable, words.Line);
        if (!variableResult.TryGetValue(out IVariable? variable)) {
            return variableResult.ToErrorResult();
        }
        
        return Ok(new VariableExpression(words.Line, variable));
    }
    
    private static Result<string> GetStaticName(PreTranspilerState state, CombinedWord words) {
        string functionName = words.NameSpace[^1];
        string typeName = words.NameSpace[^2];

        Result<IType> typeResult = state.Types.Get(words.Line, typeName);
        if (!typeResult.TryGetValue(out IType? type)) {
            return typeResult.ToErrorResult();
        }

        return Ok($"{type.Name}.{functionName}");
    }
}