namespace BBAP.Lexer.Tokens.Setting;

public class MinusEqualsToken : IToken {
    public MinusEqualsToken(int line) {
        Line = line;
    }

    public int Line { get; }
}