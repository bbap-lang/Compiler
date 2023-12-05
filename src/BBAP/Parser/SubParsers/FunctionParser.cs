using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Lexer.Tokens.Others;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Results;
using BBAP.Types.Types.ParserTypes;

namespace BBAP.Parser.SubParsers;

public static class FunctionParser {
    public static Result<IExpression> Run(ParserState state, int line) {
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

            Result<OpeningCurlyBracketToken> openingCurlyBracketResult = state.Next<OpeningCurlyBracketToken>();
            if (!openingCurlyBracketResult.TryGetValue(out _)) return openingCurlyBracketResult.ToErrorResult();
        }

        Result<ImmutableArray<IExpression>> blockResult = Parser.ParseBlock(state, false);

        if (!blockResult.TryGetValue(out ImmutableArray<IExpression> block)) return blockResult.ToErrorResult();

        return Ok<IExpression>(new FunctionExpression(line, nameToken.Value, parameters, returnTypes, block));
    }

    private static Result<ImmutableArray<TypeExpression>> GetReturnTypes(ParserState state) {
        var types = new List<TypeExpression>();

        Result<OpeningGenericBracketToken> openingBracketResult = state.Next<OpeningGenericBracketToken>();
        if (!openingBracketResult.TryGetValue(out _)) return openingBracketResult.ToErrorResult();

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

            Result<IToken> endOfParameterResult = state.Next(typeof(CommaToken), typeof(ClosingGenericBracketToken));
            if (!endOfParameterResult.TryGetValue(out IToken? endOfParameterToken))
                return endOfParameterResult.ToErrorResult();

            if (endOfParameterToken is ClosingGenericBracketToken) return Ok(types.ToImmutableArray());
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


            Result<UnknownWordToken> typeResult = state.Next<UnknownWordToken>();
            if (!typeResult.TryGetValue(out UnknownWordToken? typeToken)) return typeResult.ToErrorResult();

            var parameterExpression = new ParameterExpression(nameToken.Line, nameToken.Value, typeToken.Value);
            parameters.Add(parameterExpression);


            Result<IToken> endOfParameterResult = state.Next(typeof(ColonToken), typeof(ClosingGenericBracketToken));
            if (!endOfParameterResult.TryGetValue(out IToken? endOfParameterToken))
                return endOfParameterResult.ToErrorResult();

            if (endOfParameterToken is ClosingGenericBracketToken) return Ok(parameters.ToImmutableArray());
        }
    }
}