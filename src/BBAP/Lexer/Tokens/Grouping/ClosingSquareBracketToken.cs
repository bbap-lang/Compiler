namespace BBAP.Lexer.Tokens.Grouping;

public class ClosingSquareBracketToken : IToken {
    public ClosingSquareBracketToken(int line) {
        Line = line;
    }

    public int Line { get; }
}