namespace BBAP.Lexer.Tokens.Boolean;

public class OrToken : IToken {
    public OrToken(int line) {
        Line = line;
    }

    public int Line { get; }
}