using BBAP.Lexer.Tokens;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Sql;
using BBAP.Results;

namespace BBAP.Parser.SubParsers.SqlParser;

public static class FilterParser {
    public static Result<SqlFilterExpression>
        Run(ParserState state, out IToken lastToken, params Type[] endTokensTypes) {
        Result<IExpression> expressionResult = ValueParser.FullExpression(state, out lastToken, endTokensTypes);
        if (!expressionResult.TryGetValue(out IExpression? expression)) return expressionResult.ToErrorResult();

        return Ok(new SqlFilterExpression(expression.Line, expression));
    }
}