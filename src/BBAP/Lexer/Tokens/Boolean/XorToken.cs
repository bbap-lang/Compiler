namespace BBAP.Lexer.Tokens.Boolean;

public class XorToken : IToken {
    public XorToken(int line) {
        Line = line;
    }

    public int Line { get; }
}