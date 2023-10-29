namespace BBAP.Lexer.Tokens.Comparing;

public class LessThenToken : IToken {
    public LessThenToken(int line) {
        Line = line;
    }

    public int Line { get; }
}