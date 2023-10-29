namespace BBAP.Parser.Expressions.Calculations;

public record EmptyMathCalculationExpression(int Line, CalculationType CalculationType) : IExpression;