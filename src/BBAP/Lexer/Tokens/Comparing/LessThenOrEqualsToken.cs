namespace BBAP.Lexer.Tokens.Comparing;

public class LessThenOrEqualsToken : IToken {
    public LessThenOrEqualsToken(int line) {
        Line = line;
    }

    public int Line { get; }
}