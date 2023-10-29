using System.Diagnostics.CodeAnalysis;

namespace BBAP.Lexer.Tokens.Values;

public class FloatValueToken : IToken {
    public FloatValueToken(double value, int line) {
        Value = value;
        Line = line;
    }

    public int Line { get; }
    public double Value { get; init; }
}