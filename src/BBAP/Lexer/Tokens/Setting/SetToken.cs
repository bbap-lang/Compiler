namespace BBAP.Lexer.Tokens.Setting;

public class SetToken : IToken {
    public SetToken(int line) {
        Line = line;
    }

    public int Line { get; }
}