using BBAP.Lexer.Tokens;

namespace BBAP.Results; 

public record InvalidTokenError(IToken Token) : Error(Token.Line, $"Unexpected Token '{Token}'.") {
}