namespace BBAP.Parser.Expressions;

public record ParameterExpression(int Line, string Name, TypeExpression Type) : IExpression;