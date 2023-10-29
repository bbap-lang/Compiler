namespace BBAP.Lexer.Tokens.Grouping;

public class OpeningGenericBracketToken : IToken {
    public OpeningGenericBracketToken(int line) {
        Line = line;
    }

    public int Line { get; }
}