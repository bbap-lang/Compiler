namespace BBAP.Parser.Expressions.Calculations; 

public record EmptyComparisonExpression (int Line, ComparisonType ComparisonType): IExpression;