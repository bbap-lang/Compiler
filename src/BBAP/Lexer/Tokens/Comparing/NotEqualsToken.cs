namespace BBAP.Lexer.Tokens.Comparing;

public class NotEqualsToken : IToken {
    public NotEqualsToken(int line) {
        Line = line;
    }

    public int Line { get; }
}