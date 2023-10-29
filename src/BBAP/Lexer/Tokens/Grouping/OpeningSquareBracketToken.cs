namespace BBAP.Lexer.Tokens.Grouping;

public class OpeningSquareBracketToken : IToken {
    public OpeningSquareBracketToken(int line) {
        Line = line;
    }

    public int Line { get; }
}