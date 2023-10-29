namespace BBAP.Lexer.Tokens.Keywords;

public class IfToken : IToken {
    public const string Name = "IF";

    public IfToken(int line) {
        Line = line;
    }

    public int Line { get; }
}