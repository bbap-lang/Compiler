namespace BBAP.Parser.Expressions.Values; 

public record NotExpression(int Line, IExpression Inner): IExpression;