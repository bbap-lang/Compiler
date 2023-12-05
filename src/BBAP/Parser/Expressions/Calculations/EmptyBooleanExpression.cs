namespace BBAP.Parser.Expressions.Calculations;

public record EmptyBooleanExpression(int Line, BooleanType BooleanType) : IExpression;