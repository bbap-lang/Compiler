namespace BBAP.Lexer.Tokens.Values;

public class BooleanValueToken : IToken {
    public const string NameTrue = "TRUE";
    public const string NameFalse = "FALSE";

    public BooleanValueToken(int line, bool value) {
        Line = line;
        Value = value;
    }

    public bool Value { get; }

    public int Line { get; }
}