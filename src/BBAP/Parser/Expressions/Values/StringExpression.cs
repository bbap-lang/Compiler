namespace BBAP.Parser.Expressions.Values;

public record StringExpression(int Line, string Value) : IExpression;