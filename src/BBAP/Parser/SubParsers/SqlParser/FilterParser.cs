using System.Diagnostics;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Boolean;
using BBAP.Lexer.Tokens.Comparing;
using BBAP.Lexer.Tokens.Others;
using BBAP.Lexer.Tokens.Sql;
using BBAP.Parser.Expressions.Sql;
using BBAP.Parser.Expressions.Values;
using BBAP.Results;

namespace BBAP.Parser.SubParsers.SqlParser; 

public static class FilterParser {
    public static Result<SqlFilterExpression> Run(ParserState state, out IToken lastToken, params Type[] endTokensTypes) {
        var expressionResult = ValueParser.FullExpression(state, out lastToken, endTokensTypes);
        if(!expressionResult.TryGetValue(out var expression)) {
            return expressionResult.ToErrorResult();
        }

        return Ok(new SqlFilterExpression(expression.Line, expression));
    }
}