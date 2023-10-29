namespace BBAP.Lexer.Tokens.Keywords;

public class DoToken : IToken {
    public const string Name = "DO";

    public DoToken(int line) {
        Line = line;
    }

    public int Line { get; }
}