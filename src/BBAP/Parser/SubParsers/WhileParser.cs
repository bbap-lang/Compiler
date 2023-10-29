using System.Collections.Immutable;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Results;

namespace BBAP.Parser.SubParsers; 

public class WhileParser {
    public static Result<IExpression> Run(ParserState state, int line) {
        Result<IExpression> conditionResult = BooleanParser.Run(state, out _, typeof(OpeningCurlyBracketToken));
        if (!conditionResult.TryGetValue(out IExpression? condition)) {
            return conditionResult;
        }
        
        Result<ImmutableArray<IExpression>> blockContentResult = Parser.ParseBlock(state, true);
        if (!blockContentResult.TryGetValue(out ImmutableArray<IExpression> blockContent)) {
            return blockContentResult.ToErrorResult();
        }

        var whileExpression = new WhileExpression(line, condition, blockContent);
        return Ok<IExpression>(whileExpression);
    }
}