namespace BBAP.Lexer.Tokens.Values;

public class StringValueToken : IToken {
    public StringValueToken(string value, int line) {
        Line = line;
        Value = value;
    }

    public string Value { get; init; }

    public int Line { get; }
}