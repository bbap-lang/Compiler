using System.Collections.Immutable;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Lexer.Tokens.Others;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Results;

namespace BBAP.Parser.SubParsers;

public static class ForParser {
    public static Result<IExpression> Run(ParserState state) {
        Result<OpeningGenericBracketToken> openingBracketResult = state.Next<OpeningGenericBracketToken>();
        if (!openingBracketResult.TryGetValue(out OpeningGenericBracketToken? openingBracket))
            return openingBracketResult.ToErrorResult();

        Result<IExpression[]> initExpressionResult = Parser.ParseNextStatement(state);
        if (!initExpressionResult.TryGetValue(out IExpression[]? initExpressionArray)) return initExpressionResult.ToErrorResult();
        
        if(initExpressionArray.Length != 1) return Error(initExpressionArray[1].Line, "Can't declare multiple variables in a for for head.");
        IExpression initExpression = initExpressionArray[0];

        Result<IExpression> conditionResult = BooleanParser.Run(state, out _, typeof(SemicolonToken));
        if (!conditionResult.TryGetValue(out IExpression? condition)) return conditionResult;

        Result<IExpression[]> runningExpressionResult = Parser.ParseNextStatement(state);
        if (!runningExpressionResult.TryGetValue(out IExpression[]? runningExpressionArray)) return runningExpressionResult.ToErrorResult();
        
        if(runningExpressionArray.Length != 1) return Error(runningExpressionArray[1].Line, "Can't declare multiple variables in a for for head.");
        IExpression runningExpression = runningExpressionArray[0];

        Result<ClosingGenericBracketToken> closingBracketResult = state.Next<ClosingGenericBracketToken>();
        if (!closingBracketResult.IsSuccess) return closingBracketResult.ToErrorResult();

        Result<ImmutableArray<IExpression>> blockResult = Parser.ParseBlock(state, true);
        if (!blockResult.TryGetValue(out ImmutableArray<IExpression> block)) return blockResult.ToErrorResult();

        var forExpression = new ForExpression(openingBracket.Line, initExpression, condition, runningExpression, block);

        return Ok<IExpression>(forExpression);
    }
}