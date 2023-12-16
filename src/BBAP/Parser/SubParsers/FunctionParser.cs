using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Lexer.Tokens.Keywords;
using BBAP.Lexer.Tokens.Others;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Results;
using BBAP.Types.Types.ParserTypes;

namespace BBAP.Parser.SubParsers;

public static class FunctionParser {
    public static Result<IExpression> Run(ParserState state, int line) {
        Result<ReadOnlyToken> readOnlyResult = state.Next<ReadOnlyToken>();
        if (!readOnlyResult.IsSuccess) state.Revert();

        bool isReadOnly = readOnlyResult.IsSuccess;

        Result<UnknownWordToken> nameResult = state.Next<UnknownWordToken>();
        if (!nameResult.TryGetValue(out UnknownWordToken? nameToken)) return nameResult.ToErrorResult();

        Result<OpeningGenericBracketToken> openingBracketResult = state.Next<OpeningGenericBracketToken>();
        if (!openingBracketResult.TryGetValue(out _)) return openingBracketResult.ToErrorResult();

        Result<ImmutableArray<ParameterExpression>> parameterResult = GetParameters(state);
        if (!parameterResult.TryGetValue(out ImmutableArray<ParameterExpression> parameters))
            return parameterResult.ToErrorResult();

        Result<IToken> blockStartOrReturnTypeResult = state.Next(typeof(OpeningCurlyBracketToken), typeof(ColonToken));
        if (!blockStartOrReturnTypeResult.TryGetValue(out IToken? blockStartOrReturnTypeToken))
            return blockStartOrReturnTypeResult.ToErrorResult();

        var returnTypes = ImmutableArray<TypeExpression>.Empty;

        if (blockStartOrReturnTypeToken is ColonToken) {
            Result<ImmutableArray<TypeExpression>> returnTypesResult = GetReturnTypes(state);
            if (!returnTypesResult.TryGetValue(out returnTypes)) return returnTypesResult.ToErrorResult();
        }

        Result<ImmutableArray<IExpression>> blockResult = Parser.ParseBlock(state, false);

        if (!blockResult.TryGetValue(out ImmutableArray<IExpression> block)) return blockResult.ToErrorResult();

        return Ok<IExpression>(new FunctionExpression(line, nameToken.Value, isReadOnly, parameters, returnTypes, block));
    }

    private static Result<ImmutableArray<TypeExpression>> GetReturnTypes(ParserState state) {
        var types = new List<TypeExpression>();

        Result<OpeningGenericBracketToken> openingBracketResult = state.Next<OpeningGenericBracketToken>();
        bool usedBracket = openingBracketResult.IsSuccess;

        if (!usedBracket) {
            state.Revert();
        }
        
        while (true) {
            Result<IToken> typeResult = state.Next(typeof(UnknownWordToken), typeof(ClosingGenericBracketToken));
            if (!typeResult.TryGetValue(out IToken? startToken)) return typeResult.ToErrorResult();

            if (startToken is ClosingGenericBracketToken) {
                if (types.Count == 0) return Ok(ImmutableArray<TypeExpression>.Empty);

                return Error(startToken.Line, "Unexpected ')', expected type.");
            }

            if (startToken is not UnknownWordToken typeToken) throw new UnreachableException();

            var typeEx = new TypeExpression(typeToken.Line, new OnlyNameType(typeToken.Value));
            types.Add(typeEx);

            Result<IToken> endOfParameterResult = state.Next(typeof(CommaToken), typeof(ClosingGenericBracketToken), typeof(OpeningCurlyBracketToken));
            if (!endOfParameterResult.TryGetValue(out IToken? endOfParameterToken))
                return endOfParameterResult.ToErrorResult();

            if(endOfParameterToken is ClosingGenericBracketToken && !usedBracket) return Error(endOfParameterToken.Line, "Unexpected ')', expected '{'.");
            if(endOfParameterToken is OpeningCurlyBracketToken && usedBracket) return Error(endOfParameterToken.Line, "Unexpected '{', expected ')'.");
            if (endOfParameterToken is CommaToken && !usedBracket) return Error(endOfParameterToken.Line, "Unexpected ',', expected '{'.");

            if (endOfParameterToken is ClosingGenericBracketToken) {
                Result<OpeningCurlyBracketToken> openingCurlyBracketResult = state.Next<OpeningCurlyBracketToken>();
                if (!openingCurlyBracketResult.IsSuccess) return openingCurlyBracketResult.ToErrorResult();
                return Ok(types.ToImmutableArray());
            }
            
            if (endOfParameterToken is OpeningCurlyBracketToken) {
                return Ok(types.ToImmutableArray());
            }
        }
    }

    private static Result<ImmutableArray<ParameterExpression>> GetParameters(ParserState state) {
        var parameters = new List<ParameterExpression>();

        while (true) {
            Result<IToken> nameResult = state.Next(typeof(UnknownWordToken), typeof(ClosingGenericBracketToken));
            if (!nameResult.TryGetValue(out IToken? startToken)) return nameResult.ToErrorResult();

            if (startToken is ClosingGenericBracketToken) {
                if (parameters.Count == 0) return Ok(ImmutableArray<ParameterExpression>.Empty);

                return Error(startToken.Line, "Unexpected ')', expected parameter.");
            }

            if (startToken is not UnknownWordToken nameToken) throw new UnreachableException();


            Result<ColonToken> colonResult = state.Next<ColonToken>();
            if (!colonResult.TryGetValue(out _)) return colonResult.ToErrorResult();

            Result<TypeExpression> typeResult = TypeParser.Run(state);

            if (!typeResult.TryGetValue(out TypeExpression? parameterType)) return typeResult.ToErrorResult();

            var parameterExpression = new ParameterExpression(nameToken.Line, nameToken.Value, parameterType);
            parameters.Add(parameterExpression);


            Result<IToken> endOfParameterResult = state.Next(typeof(ColonToken), typeof(ClosingGenericBracketToken));
            if (!endOfParameterResult.TryGetValue(out IToken? endOfParameterToken))
                return endOfParameterResult.ToErrorResult();

            if (endOfParameterToken is ClosingGenericBracketToken) return Ok(parameters.ToImmutableArray());
        }
    }
}