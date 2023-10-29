namespace BBAP.Lexer.Tokens.Others;

public class CommaToken : IToken {
    public CommaToken(int line) {
        Line = line;
    }

    public int Line { get; }
}