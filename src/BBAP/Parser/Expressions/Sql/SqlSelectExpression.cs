using System.Collections.Immutable;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler;

namespace BBAP.Parser.Expressions.Sql; 

public record SqlSelectExpression(int Line, ImmutableArray<VariableExpression> OutputFields, VariableExpression From, ImmutableArray<JoinExpression> Joins, SqlFilterExpression? Where, ImmutableArray<VariableExpression> Orderby, long? Limit) : IExpression;