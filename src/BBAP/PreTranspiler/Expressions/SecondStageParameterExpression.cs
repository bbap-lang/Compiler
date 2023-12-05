using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;

namespace BBAP.PreTranspiler.Expressions;

public record SecondStageParameterExpression
    (int Line, VariableExpression Variable, TypeExpression Type) : ISecondStageValue;