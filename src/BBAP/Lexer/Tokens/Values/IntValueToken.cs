using System.Diagnostics.CodeAnalysis;

namespace BBAP.Lexer.Tokens.Values;

public class IntValueToken : IToken{
    public IntValueToken(long value, int line) {
        Value = value;
        Line = line;
    }

    public int Line { get; }
    public long Value { get; init; }
}