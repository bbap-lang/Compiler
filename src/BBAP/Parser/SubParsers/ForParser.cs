using System.Collections.Immutable;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Lexer.Tokens.Others;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Results;

namespace BBAP.Parser.SubParsers; 

public static class ForParser {
    public static Result<IExpression> Run(ParserState state) {
        Result<IToken> result = state.Next(typeof(OpeningGenericBracketToken));
        if (!result.TryGetValue(out IToken openingBracket)) {
            return result.ToErrorResult();
        }
        
        Result<IExpression> initExpressionResult = Parser.ParseNextStatement(state);
        if (!initExpressionResult.TryGetValue(out IExpression? initExpression)) {
            return initExpressionResult;
        }
        
        Result<IExpression> conditionResult = BooleanParser.Run(state, out _, typeof(SemicolonToken));
        if (!conditionResult.TryGetValue(out IExpression? condition)) {
            return conditionResult;
        }
        
        Result<IExpression> runningExpressionResult = Parser.ParseNextStatement(state);
        if (!runningExpressionResult.TryGetValue(out IExpression? runningExpression)) {
            return runningExpressionResult;
        }
        
        result = state.Next(typeof(ClosingGenericBracketToken));
        if (!result.IsSuccess) {
            return result.ToErrorResult();
        }

        Result<ImmutableArray<IExpression>> blockResult = Parser.ParseBlock(state, true);
        if (!blockResult.TryGetValue(out ImmutableArray<IExpression> block)) {
            return blockResult.ToErrorResult();
        }

        var forExpression = new ForExpression(openingBracket.Line, initExpression, condition, runningExpression, block);
        
        return Ok<IExpression>(forExpression);
    }
}