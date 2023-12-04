using BBAP.Lexer.Tokens;

namespace BBAP.Results;

public record InvalidTokenError(IToken Token, Type? CorrectToken = null) : Error(Token.Line, CorrectToken is null ?  $"Unexpected Token '{Token}'." : $"Unexpected Token '{Token}', expected '{CorrectToken.Name}'.");