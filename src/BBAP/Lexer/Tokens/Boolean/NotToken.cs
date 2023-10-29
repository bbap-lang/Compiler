namespace BBAP.Lexer.Tokens.Boolean;

public class NotToken : IToken {
    public NotToken(int line) {
        Line = line;
    }

    public int Line { get; }
}