namespace BBAP.Lexer.Tokens.Setting;

public class PlusEqualsToken : IToken {
    public PlusEqualsToken(int line) {
        Line = line;
    }

    public int Line { get; }
}