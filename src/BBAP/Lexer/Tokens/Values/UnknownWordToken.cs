using System.Diagnostics.CodeAnalysis;

namespace BBAP.Lexer.Tokens.Values;

public class UnknownWordToken: IToken {
    [SetsRequiredMembers]
    public UnknownWordToken(string value, int line) {
        Value = value;
        Line = line;
    }

    public int Line { get; }
    public string Value { get; init; }
}