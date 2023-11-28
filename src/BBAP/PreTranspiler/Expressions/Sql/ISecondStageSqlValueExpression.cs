using BBAP.Parser.Expressions;

namespace BBAP.PreTranspiler.Expressions.Sql;

public interface ISecondStageSqlValueExpression : IExpression {
    public TypeExpression Type { get; }
}