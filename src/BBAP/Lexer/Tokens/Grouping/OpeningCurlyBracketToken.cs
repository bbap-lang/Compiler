namespace BBAP.Lexer.Tokens.Grouping;

public class OpeningCurlyBracketToken : IToken {
    public OpeningCurlyBracketToken(int line) {
        Line = line;
    }

    public int Line { get; }
}