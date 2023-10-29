namespace BBAP.Lexer.Tokens.Operators;

public class PlusToken : IToken {
    public PlusToken(int line) {
        Line = line;
    }

    public int Line { get; }
}