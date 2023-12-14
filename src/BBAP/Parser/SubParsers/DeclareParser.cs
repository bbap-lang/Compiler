using System.Collections.Immutable;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Lexer.Tokens.Others;
using BBAP.Lexer.Tokens.Setting;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Variables;
using BBAP.Results;
using BBAP.Types.Types.ParserTypes;

namespace BBAP.Parser.SubParsers;

public static class DeclareParser {
    public static Result<IExpression> Run(ParserState state, int line, bool isConst) {
        var mutabilityType = isConst ? MutabilityType.Const : MutabilityType.Mutable;
        
        Result<OpeningGenericBracketToken> openBracketResult = state.Next<OpeningGenericBracketToken>();
        
        ImmutableArray<UnknownWordToken> variableNames;
        if (openBracketResult.IsSuccess) {
            Result<ImmutableArray<UnknownWordToken>> variables = ParseVariables(state);
            if (!variables.TryGetValue(out variableNames)) return variables.ToErrorResult();
            
        } else {
            state.Revert();
            Result<UnknownWordToken> variableNameResult = state.Next<UnknownWordToken>();
            if (!variableNameResult.TryGetValue(out UnknownWordToken? tempVarName))
                return variableNameResult.ToErrorResult();

            variableNames = ImmutableArray.Create(tempVarName);
        }

        if (variableNames.Length > 1) {
            return ParseFunctionCall(state, line, variableNames, mutabilityType);
        }
        UnknownWordToken variableName = variableNames[0];
        
        Result<IToken> result = state.Next(typeof(SetToken), typeof(ColonToken), typeof(SemicolonToken));

        if (!result.TryGetValue(out IToken? token)) return result.ToErrorResult();

        TypeExpression? typeExpression;
        VariableExpression? variableExpression;
        DeclareExpression? declareExpression;
        if (token is ColonToken) {
            if(variableNames.Length != 1) return Error(line, "Can't declare multiple variables with a type.");
            
            Result<TypeExpression> typeResult = TypeParser.Run(state);
            if (!typeResult.TryGetValue(out typeExpression)) return typeResult.ToErrorResult();

            result = state.Next(typeof(SetToken), typeof(SemicolonToken));

            if (!result.TryGetValue(out token)) return result.ToErrorResult();

            
            if (token is SemicolonToken) {
                variableExpression
                    = new VariableExpression(line, new Variable(new UnknownType(), variableName.Value, mutabilityType));
                declareExpression = new DeclareExpression(variableName.Line, variableExpression, typeExpression, null, mutabilityType);
                return Ok<IExpression>(declareExpression);
            }
        } else {
            typeExpression = new TypeExpression(variableName.Line, new UnknownType());
        }

        Result<IExpression> valueResult = ValueParser.FullExpression(state, out _, typeof(SemicolonToken));
        if (!valueResult.TryGetValue(out IExpression? value)) return valueResult.ToErrorResult();

        variableExpression
            = new VariableExpression(variableName.Line, new Variable(new UnknownType(), variableName.Value, mutabilityType));
        var setExpression = new SetExpression(value.Line, variableExpression, SetType.Generic, value);
        declareExpression = new DeclareExpression(variableName.Line, variableExpression, typeExpression, setExpression, mutabilityType);
        return Ok<IExpression>(declareExpression);
    }

    private static Result<IExpression> ParseFunctionCall(ParserState state, int line, ImmutableArray<UnknownWordToken> variableNames, MutabilityType mutabilityType) {
        Result<SetToken> setTokenResult = state.Next<SetToken>();

        if (!setTokenResult.IsSuccess) return setTokenResult.ToErrorResult();

        Result<CombinedWord> combinedNameResult = UnknownWordParser.ParseWord(state);
        if (!combinedNameResult.TryGetValue(out CombinedWord? nameToken)) return combinedNameResult.ToErrorResult();

        Result<OpeningGenericBracketToken> openingBracketResult = state.Next<OpeningGenericBracketToken>();
        if (!openingBracketResult.IsSuccess) return openingBracketResult.ToErrorResult();

        Result<FunctionCallExpression> functionCallResult = FunctionCallParser.Run(state, nameToken);

        if (!functionCallResult.TryGetValue(out FunctionCallExpression? functionCallExpression)) return functionCallResult.ToErrorResult();

        state.SkipSemicolon();

        ImmutableArray<VariableExpression> variableExpressions
            = variableNames.Select(x => new VariableExpression(x.Line, new Variable(new UnknownType(), x.Value, mutabilityType))).ToImmutableArray();
        
        var functionCallSetExpression = new DeclareFunctionCallSetExpression(functionCallExpression.Line,
                                                                             functionCallExpression.Name,
                                                                             functionCallExpression.Parameters,
                                                                             variableExpressions, mutabilityType);

        
        return Ok<IExpression>(functionCallSetExpression);
    }

    private static Result<ImmutableArray<UnknownWordToken>> ParseVariables(ParserState state) {
        var variables = new List<UnknownWordToken>();
        while (true) {
            Result<UnknownWordToken> variableNameResult = state.Next<UnknownWordToken>();
            if (!variableNameResult.TryGetValue(out UnknownWordToken? variableName)) return variableNameResult.ToErrorResult();

            variables.Add(variableName);

            Result<IToken> result = state.Next(typeof(CommaToken), typeof(ClosingGenericBracketToken));
            if (!result.TryGetValue(out IToken? endToken)) return result.ToErrorResult();

            if (endToken is ClosingGenericBracketToken) break;
        }

        return Ok(variables.ToImmutableArray());
    }
}