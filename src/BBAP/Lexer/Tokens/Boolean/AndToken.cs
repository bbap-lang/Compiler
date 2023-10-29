namespace BBAP.Lexer.Tokens.Boolean;

public class AndToken : IToken {
    public AndToken(int line) {
        Line = line;
    }

    public int Line { get; }
}