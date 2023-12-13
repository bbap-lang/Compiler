using BBAP.Lexer.Tokens.Others;
using BBAP.Parser.Expressions;
using BBAP.Results;

namespace BBAP.Parser.SubParsers;

public class LoopAdditionExpression {
    public static Result<IExpression> RunBreak(ParserState state, int line) {
        Result<SemicolonToken> semicolonResult = state.Next<SemicolonToken>();

        if (!semicolonResult.IsSuccess) return semicolonResult.ToErrorResult();
        
        return Ok<IExpression>(new BreakLoopExpression(line));
    }
    
    public static Result<IExpression> RunContinue(ParserState state, int line) {
        Result<SemicolonToken> semicolonResult = state.Next<SemicolonToken>();

        if (!semicolonResult.IsSuccess) return semicolonResult.ToErrorResult();
        
        return Ok<IExpression>(new ContinueLoopExpression(line));
    }
}