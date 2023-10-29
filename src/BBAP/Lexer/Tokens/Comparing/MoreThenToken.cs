namespace BBAP.Lexer.Tokens.Comparing;

public class MoreThenToken : IToken {
    public MoreThenToken(int line) {
        Line = line;
    }

    public int Line { get; }
}