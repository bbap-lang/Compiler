using BBAP.Parser.Expressions;

namespace BBAP.PreTranspiler.Expressions.Sql;

public record SecondStageSqlBaseValueExpression
    (int Line, TypeExpression Type, IExpression Value) : SecondStageValueExpression(Line, Type, Value),
        ISecondStageSqlValueExpression;
// public record SecondStageSqlBaseValueExpression(int Line, TypeExpression Type, SecondStageValueExpression Value) : ISecondStageSqlValueExpression;