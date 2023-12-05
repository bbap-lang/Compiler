namespace BBAP.Parser.Expressions;

public record ParameterExpression(int Line, string Name, string Type) : IExpression;