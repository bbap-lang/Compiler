using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Boolean;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Calculations;
using BBAP.Parser.Expressions.Values;
using BBAP.Parser.ExtensionMethods;
using BBAP.Results;

namespace BBAP.Parser.SubParsers;

public static class BooleanParser {
    public static Result<IExpression> Run(ParserState state, out IToken endToken, params Type[] endTokens){
        Result<IExpression> valueResult = ValueParser.FullExpression(state, out endToken, endTokens);
        
        if (!valueResult.TryGetValue(out IExpression expression)) {
            return valueResult;
        }

        return valueResult;
    }

}