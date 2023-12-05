using BBAP.Parser.Expressions;

namespace BBAP.PreTranspiler.Expressions;

public interface ISecondStageValue : IExpression {
    public TypeExpression Type { get; }
}