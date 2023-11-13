namespace BBAP.Parser.Expressions.Values; 

public record BooleanValueExpression(int Line, bool Value): IExpression;