namespace BBAP.Lexer.Tokens.Values;

public class IntValueToken : IToken {
    public IntValueToken(long value, int line) {
        Value = value;
        Line = line;
    }

    public long Value { get; init; }

    public int Line { get; }
}