namespace BBAP.Parser.Expressions.Calculations; 

public record BooleanExpression(int Line, BooleanType BooleanType, IExpression Left, IExpression Right): IExpression;