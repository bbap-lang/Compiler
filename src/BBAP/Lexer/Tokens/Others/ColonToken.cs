namespace BBAP.Lexer.Tokens.Others;

public class ColonToken : IToken {
    public ColonToken(int line) {
        Line = line;
    }

    public int Line { get; }
}