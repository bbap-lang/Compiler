namespace BBAP.Parser.Expressions.Sql;

public record SqlFilterExpression(int Line, IExpression Value) : IExpression;