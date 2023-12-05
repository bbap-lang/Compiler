using BBAP.Parser.Expressions;

namespace BBAP.PreTranspiler.Expressions.Sql;

public record SecondStageSqlFilterExpression(int Line,
    TypeExpression Type,
    SqlFilterOperator Operator,
    ISecondStageSqlValueExpression Left,
    ISecondStageSqlValueExpression Right) : ISecondStageSqlValueExpression;

public enum SqlFilterOperator {
    Plus,
    Minus,
    Divide,
    Multiply,
    Modulo,

    Equals,
    NotEquals,
    GreaterThan,
    SmallerThan,
    GreaterThanOrEquals,
    SmallerThanOrEquals,

    And,
    Or
}