namespace BBAP.Lexer.Tokens.Operators;

public class MinusToken : IToken {
    public MinusToken(int line) {
        Line = line;
    }

    public int Line { get; }
}