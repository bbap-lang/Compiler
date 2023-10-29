namespace BBAP.Parser.Expressions.Values; 

public record VariableExpression(int Line, string Name): IExpression;