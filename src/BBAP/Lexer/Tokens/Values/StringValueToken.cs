namespace BBAP.Lexer.Tokens.Values;

public class StringValueToken: IToken {
    public StringValueToken(string value, int line) {
        Line = line;
        Value = value;
    }

    public int Line { get; }
    public string Value { get; init; }
}