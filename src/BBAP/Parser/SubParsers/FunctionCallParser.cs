using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Lexer.Tokens.Others;
using BBAP.Lexer.Tokens.Setting;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Variables;
using BBAP.Results;
using BBAP.Types.Types.ParserTypes;

namespace BBAP.Parser.SubParsers;

public static class FunctionCallParser {
    public static Result<FunctionCallExpression> Run(ParserState state, CombinedWord names) {
        var parameters = new List<IExpression>();

        IToken? lastToken = null;

        while (lastToken is not ClosingGenericBracketToken) {
            Result<IExpression> parameterResult = ValueParser.FullExpression(state, out lastToken,
                                                                             typeof(ClosingGenericBracketToken),
                                                                             typeof(CommaToken));

            if (!parameterResult.TryGetValue(out IExpression? parameter)) return parameterResult.ToErrorResult();

            if (parameter is EmptyExpression) {
                if (lastToken is CommaToken) return Error(lastToken.Line, "Unexpected Symbol ',' expected ')'");

                break;
            }

            parameters.Add(parameter);
        }

        return Ok(new FunctionCallExpression(names.Line, names, parameters.ToImmutableArray()));
    }

    public static Result<IExpression> RunFull(ParserState state, int valueLine) {
        var variables = new List<VariableExpression>();

        IToken endToken;
        do {
            Result<UnknownWordToken> nameResult = state.Next<UnknownWordToken>();
            if (!nameResult.TryGetValue(out UnknownWordToken? nameToken)) return nameResult.ToErrorResult();

            var newVariable = new VariableExpression(nameToken.Line, new Variable(new UnknownType(), nameToken.Value, MutabilityType.Mutable));
            variables.Add(newVariable);

            Result<IToken> commaResult = state.Next(typeof(CommaToken), typeof(ClosingGenericBracketToken));
            if (!commaResult.TryGetValue(out endToken)) return commaResult.ToErrorResult();
        } while (endToken is not ClosingGenericBracketToken);

        Result<SetToken> setTokenResult = state.Next<SetToken>();

        if (!setTokenResult.IsSuccess) return setTokenResult.ToErrorResult();


        var nameTokenList = new List<UnknownWordToken>();

        Result<CombinedWord> combinedNameResult = UnknownWordParser.ParseWord(state);
        if (!combinedNameResult.TryGetValue(out CombinedWord? nameTokens)) return combinedNameResult.ToErrorResult();

        Result<OpeningGenericBracketToken> openingBracketResult = state.Next<OpeningGenericBracketToken>();
        if (!openingBracketResult.IsSuccess) return openingBracketResult.ToErrorResult();

        Result<FunctionCallExpression> functionCallResult = Run(state, nameTokens);

        if (!functionCallResult.TryGetValue(out FunctionCallExpression? functionCallExpression)) return functionCallResult.ToErrorResult();

        bool res = state.SkipSemicolon();

        var newFunctionCall
            = new FunctionCallSetExpression(functionCallExpression.Line, nameTokens,
                                            functionCallExpression.Parameters, variables.ToImmutableArray());

        state.SkipSemicolon();
        return Ok<IExpression>(newFunctionCall);
    }
}