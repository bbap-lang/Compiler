using System.Diagnostics.CodeAnalysis;

namespace BBAP.Lexer.Tokens.Values;

public class IntValueToken : IToken{
    public IntValueToken(int value, int line) {
        Value = value;
        Line = line;
    }

    public int Line { get; }
    public int Value { get; init; }
}