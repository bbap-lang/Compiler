namespace BBAP.Lexer.Tokens.Keywords;

public record FunctionToken(int Line) : IToken {
    public const string Name = "FUNC";
}