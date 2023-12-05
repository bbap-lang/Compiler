using System.Collections.Immutable;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Lexer.Tokens.Others;
using BBAP.Parser.Expressions;
using BBAP.Results;

namespace BBAP.Parser.SubParsers;

public static class ReturnParser {
    public static Result<IExpression> Run(ParserState state, int line) {
        bool usesBrackets = true;

        Result<IToken> tempResult = state.Next(typeof(OpeningGenericBracketToken), typeof(SemicolonToken));
        if (!tempResult.TryGetValue(out IToken? token)) {
            state.Revert();
            usesBrackets = false;
        } else if (token is SemicolonToken) {
            return Ok<IExpression>(new ReturnExpression(line, ImmutableArray<IExpression>.Empty));
        }

        Type[] endTokens = usesBrackets
            ? new[] { typeof(ClosingGenericBracketToken), typeof(CommaToken) }
            : new[] { typeof(SemicolonToken) };

        var returnValues = new List<IExpression>();

        IToken? lastToken = null;
        while (lastToken is not ClosingGenericBracketToken && lastToken is not SemicolonToken) {
            Result<IExpression> valueResult = ValueParser.FullExpression(state, out lastToken, endTokens);
            if (!valueResult.TryGetValue(out IExpression value)) return valueResult;

            returnValues.Add(value);
        }

        if (usesBrackets) state.SkipSemicolon();

        var returnExpression = new ReturnExpression(line, returnValues.ToImmutableArray());

        return Ok<IExpression>(returnExpression);
    }
}