namespace BBAP.Lexer.Tokens.Setting;

public class DecrementToken : IToken {
    public DecrementToken(int line) {
        Line = line;
    }

    public int Line { get; }
}