using System.Diagnostics;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Keywords;
using BBAP.Parser.Expressions;
using BBAP.Results;

namespace BBAP.Parser.SubParsers;

public class PublicParser {
    public static Result<IExpression> Run(ParserState state) {
        Result<IToken> nextTokenResult = state.Next(typeof(AliasToken));
        if (!nextTokenResult.TryGetValue(out IToken? nextToken)) return nextTokenResult.ToErrorResult();

        return nextToken switch {
            AliasToken => AliasParser.Run(state, true),
            _ => throw new UnreachableException()
        };
    }
}