namespace BBAP.Lexer.Tokens.Grouping;

public class ClosingCurlyBracketToken : IToken {
    public ClosingCurlyBracketToken(int line) {
        Line = line;
    }

    public int Line { get; }
}