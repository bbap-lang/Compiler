using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;

namespace BBAP.PreTranspiler.Expressions.Sql;

public record SecondStageSqlVariableExpression(int Line,
    TypeExpression Type,
    InsertType InsertType,
    VariableExpression Variable) : ISecondStageSqlValueExpression;

public enum InsertType {
    InSqlStatement,
    FromCode
}