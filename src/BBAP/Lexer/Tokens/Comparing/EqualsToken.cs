namespace BBAP.Lexer.Tokens.Comparing;

public class EqualsToken : IToken {
    public EqualsToken(int line) {
        Line = line;
    }

    public int Line { get; }
}