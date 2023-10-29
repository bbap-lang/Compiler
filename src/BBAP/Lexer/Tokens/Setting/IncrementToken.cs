namespace BBAP.Lexer.Tokens.Setting;

public class IncrementToken : IToken {
    public IncrementToken(int line) {
        Line = line;
    }

    public int Line { get; }
}