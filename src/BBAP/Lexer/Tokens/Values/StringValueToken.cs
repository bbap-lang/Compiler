namespace BBAP.Lexer.Tokens.Values;

public record StringValueToken(string Value, int Line, QuotationMark QuotationMark) : IToken;

public enum QuotationMark {
    Single,
    Double
}

public static class QuotationMarks {
    public static QuotationMark FromChar(char c) {
        return c switch {
            '\'' => QuotationMark.Single,
            '"' => QuotationMark.Double,
            _ => throw new ArgumentOutOfRangeException(nameof(c), c, null)
        };
    }
}