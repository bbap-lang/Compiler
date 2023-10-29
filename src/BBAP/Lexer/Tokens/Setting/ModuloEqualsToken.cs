namespace BBAP.Lexer.Tokens.Setting;

public class ModuloEqualsToken : IToken {
    public ModuloEqualsToken(int line) {
        Line = line;
    }

    public int Line { get; }
}