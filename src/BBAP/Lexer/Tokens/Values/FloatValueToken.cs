namespace BBAP.Lexer.Tokens.Values;

public class FloatValueToken : IToken {
    public FloatValueToken(double value, int line) {
        Value = value;
        Line = line;
    }

    public double Value { get; init; }

    public int Line { get; }
}