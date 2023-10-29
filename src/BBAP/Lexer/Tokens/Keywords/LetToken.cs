namespace BBAP.Lexer.Tokens.Keywords;

public class LetToken : IToken {
    public const string Name = "LET";

    public LetToken(int line) {
        Line = line;
    }

    public int Line { get; }
}