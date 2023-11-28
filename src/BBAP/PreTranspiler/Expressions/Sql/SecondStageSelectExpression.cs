using System.Collections.Immutable;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;

namespace BBAP.PreTranspiler.Expressions.Sql;

public record SecondStageSelectExpression(int Line,
    ImmutableArray<SecondStageSqlVariableExpression> OutputFields,
    VariableExpression From,
    ImmutableArray<SecondStageJoinExpression> Joins,
    SecondStageSqlFilterExpression? Where,
    ImmutableArray<SecondStageSqlVariableExpression> OrderBy,
    long? Limit,
    VariableExpression? OutputVariable) : IExpression;