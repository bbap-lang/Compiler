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
        var valueResult = ValueParser.FullExpression(state, out endToken, endTokens);
        
        if (!valueResult.TryGetValue(out IExpression expression)) {
            return valueResult;
        }

        if (expression is not BooleanExpression && expression is not ComparisonExpression) {
            return Error(expression.Line, "Invalid expression for boolean-statement.");
        }

        return valueResult;
    }

}