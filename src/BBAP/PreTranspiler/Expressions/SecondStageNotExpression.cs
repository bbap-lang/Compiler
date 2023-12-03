using BBAP.Parser.Expressions;

namespace BBAP.PreTranspiler.Expressions; 

public record SecondStageNotExpression(int Line, TypeExpression Type, IExpression InnerExpression) : ISecondStageValue;