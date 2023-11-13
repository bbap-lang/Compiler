using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.Parser.SubParsers; 

public static class TypeParser {
    public static Result<TypeExpression> Run(ParserState state) {
        Result<IToken> result = state.Next(typeof(UnknownWordToken));

        if (!result.TryGetValue(out IToken? token) || token is not UnknownWordToken typeToken) {
            return result.ToErrorResult();
        }

        var type = new OnlyNameType(typeToken.Value);
        var typeExpression = new TypeExpression(typeToken.Line, type);
        return Ok(typeExpression);
    }
}