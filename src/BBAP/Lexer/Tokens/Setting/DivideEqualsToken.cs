namespace BBAP.Lexer.Tokens.Setting;

public class DivideEqualsToken : IToken {
    public DivideEqualsToken(int line) {
        Line = line;
    }

    public int Line { get; }
}