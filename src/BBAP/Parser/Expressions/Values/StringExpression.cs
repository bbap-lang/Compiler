using BBAP.Lexer.Tokens.Values;

namespace BBAP.Parser.Expressions.Values;

public record StringExpression(int Line, string Value, QuotationMark QuotationMark) : IExpression;