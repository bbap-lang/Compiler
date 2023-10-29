namespace BBAP.Parser.Expressions.Calculations;

public record MathCalculationExpression(int Line, CalculationType CalculationType, IExpression Left, IExpression Right) : IExpression;