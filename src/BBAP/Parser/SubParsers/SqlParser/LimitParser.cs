using BBAP.Lexer.Tokens.Sql;
using BBAP.Lexer.Tokens.Values;
using BBAP.Results;

namespace BBAP.Parser.SubParsers.SqlParser;

public class LimitParser {
    public static Result<long?> Run(ParserState state) {
        Result<LimitToken> limitResult = state.Next<LimitToken>();
        if (!limitResult.IsSuccess) {
            state.Revert();
            return Ok<long?>(null);
        }

        Result<IntValueToken> numberResult = state.Next<IntValueToken>();
        if (!numberResult.TryGetValue(out IntValueToken? numberToken)) return numberResult.ToErrorResult();

        return Ok<long?>(numberToken.Value);
    }
}