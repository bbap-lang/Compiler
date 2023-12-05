using BBAP.Parser.Expressions;

namespace BBAP.PreTranspiler.Expressions;

public record SecondStageCalculationExpression(int Line,
    TypeExpression Type,
    SecondStageCalculationType CalculationType,
    IExpression Left,
    IExpression Right) : ISecondStageValue;