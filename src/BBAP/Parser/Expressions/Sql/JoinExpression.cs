using BBAP.Parser.Expressions.Values;

namespace BBAP.Parser.Expressions.Sql;

public record JoinExpression
    (int Line, VariableExpression Table, JoinType JoinType, SqlFilterExpression On) : IExpression;