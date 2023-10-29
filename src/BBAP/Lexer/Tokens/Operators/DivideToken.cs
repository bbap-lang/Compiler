namespace BBAP.Lexer.Tokens.Operators;

public class DivideToken : IToken {
    public DivideToken(int line) {
        Line = line;
    }

    public int Line { get; }
}