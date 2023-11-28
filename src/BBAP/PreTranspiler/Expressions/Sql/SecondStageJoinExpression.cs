using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Sql;
using BBAP.Parser.Expressions.Values;

namespace BBAP.PreTranspiler.Expressions.Sql; 

public record SecondStageJoinExpression(int Line, VariableExpression Table, JoinType JoinType, SecondStageSqlFilterExpression On) : IExpression;