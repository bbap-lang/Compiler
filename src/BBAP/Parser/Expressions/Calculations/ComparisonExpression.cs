namespace BBAP.Parser.Expressions.Calculations; 

public record ComparisonExpression(int Line, ComparisonType ComparisonType, IExpression Left, IExpression Right): IExpression;