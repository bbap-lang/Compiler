namespace BBAP.Lexer.Tokens.Setting;

public class MultiplyEqualsToken : IToken {
    public MultiplyEqualsToken(int line) {
        Line = line;
    }

    public int Line { get; }
}