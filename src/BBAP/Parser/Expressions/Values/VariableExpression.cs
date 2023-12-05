using BBAP.PreTranspiler.Variables;

namespace BBAP.Parser.Expressions.Values;

public record VariableExpression(int Line, IVariable Variable) : IExpression;