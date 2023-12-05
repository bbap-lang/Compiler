using BBAP.Parser.Expressions;

namespace BBAP.PreTranspiler.Expressions;

public record SecondStageValueExpression(int Line, TypeExpression Type, IExpression Value) : ISecondStageValue;