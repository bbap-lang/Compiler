namespace BBAP.Lexer.Tokens.Operators;

public class MultiplyToken : IToken {
    public MultiplyToken(int line) {
        Line = line;
    }

    public int Line { get; }
}