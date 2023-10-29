namespace BBAP.Lexer.Tokens.Operators;

public class BitAndToken : IToken {
    public BitAndToken(int line) {
        Line = line;
    }

    public int Line { get; }
}