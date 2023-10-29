namespace BBAP.Lexer.Tokens.Others;

public class SemicolonToken : IToken {
    public SemicolonToken(int line) {
        Line = line;
    }

    public int Line { get; }
}