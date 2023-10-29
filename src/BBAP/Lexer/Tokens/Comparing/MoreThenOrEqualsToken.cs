namespace BBAP.Lexer.Tokens.Comparing;

public class MoreThenOrEqualsToken : IToken {
    public MoreThenOrEqualsToken(int line) {
        Line = line;
    }

    public int Line { get; }
}