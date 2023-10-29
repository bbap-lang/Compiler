namespace BBAP.Parser.Expressions.Values; 

public record FloatExpression(int Line, double Value): IExpression;