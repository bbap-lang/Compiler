namespace BBAP.Lexer.Tokens.Grouping;

public class ClosingGenericBracketToken : IToken {
    public ClosingGenericBracketToken(int line) {
        Line = line;
    }

    public int Line { get; }
}