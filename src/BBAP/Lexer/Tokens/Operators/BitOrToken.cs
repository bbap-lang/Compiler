namespace BBAP.Lexer.Tokens.Operators;

public class BitOrToken : IToken {
    public BitOrToken(int line) {
        Line = line;
    }

    public int Line { get; }
}