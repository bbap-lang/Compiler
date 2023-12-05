using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Sql;
using BBAP.Parser.Expressions.Sql;
using BBAP.Results;

namespace BBAP.Parser.SubParsers.SqlParser;

public class WhereParser {
    public static Result<SqlFilterExpression?> Run(ParserState state, out IToken lastToken, Type[] endTokensTypes) {
        Result<WhereToken> whereResult = state.Next<WhereToken>();
        if (!whereResult.IsSuccess) {
            state.Revert();
            lastToken = null;
            return Ok<SqlFilterExpression?>(null);
        }

        Result<SqlFilterExpression> filterResult = FilterParser.Run(state, out lastToken,
                                                                    endTokensTypes.Append(typeof(OrderToken))
                                                                        .Append(typeof(LimitToken)).ToArray());
        if (!filterResult.TryGetValue(out SqlFilterExpression? filterExpression)) return filterResult.ToErrorResult();

        return Ok<SqlFilterExpression?>(filterExpression);
    }
}