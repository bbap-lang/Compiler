namespace BBAP.Parser.Expressions.Values;

public record NegativeExpression(int Line, IExpression InnerExpression) : IExpression;