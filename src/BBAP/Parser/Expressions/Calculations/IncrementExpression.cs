using BBAP.Parser.Expressions.Values;

namespace BBAP.Parser.Expressions.Calculations;

public record IncrementExpression(int Line, VariableExpression Variable, IncrementType IncrementType) : IExpression;