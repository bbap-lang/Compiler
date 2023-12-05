using System.Collections.Immutable;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Lexer.Tokens.Keywords;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Results;

namespace BBAP.Parser.SubParsers;

public static class IfParser {
    public static Result<IExpression> Run(ParserState state, int line) {
        Result<IExpression> conditionResult = BooleanParser.Run(state, out _, typeof(OpeningCurlyBracketToken));
        if (!conditionResult.TryGetValue(out IExpression? condition)) return conditionResult;

        Result<ImmutableArray<IExpression>> blockContentResult = Parser.ParseBlock(state, false);
        if (!blockContentResult.TryGetValue(out ImmutableArray<IExpression> blockContent))
            return blockContentResult.ToErrorResult();

        Result<ElseToken> elseResult = state.Next<ElseToken>();
        if (!elseResult.TryGetValue(out ElseToken? elseToken)) {
            state.Revert();
            return Ok<IExpression>(new IfExpression(line, condition, blockContent, null));
        }

        Result<IExpression> elseExpressionResult = ParseElse(state, elseToken.Line);
        if (!elseExpressionResult.TryGetValue(out IExpression? elseExpression))
            return elseExpressionResult.ToErrorResult();

        var ifExpression = new IfExpression(line, condition, blockContent, elseExpression);
        return Ok<IExpression>(ifExpression);
    }

    public static Result<IExpression> ParseElse(ParserState state, int line) {
        Result<IToken> startTokenResult = state.Next(typeof(IfToken), typeof(OpeningCurlyBracketToken));

        if (!startTokenResult.TryGetValue(out IToken? startToken)) return startTokenResult.ToErrorResult();

        if (startToken is IfToken) return Run(state, startToken.Line);

        Result<ImmutableArray<IExpression>> blockContentResult = Parser.ParseBlock(state, false);
        if (!blockContentResult.TryGetValue(out ImmutableArray<IExpression> blockContent))
            return blockContentResult.ToErrorResult();

        var elseExpression = new ElseExpression(line, blockContent);
        return Ok<IExpression>(elseExpression);
    }
}