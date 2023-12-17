using System.Collections.Immutable;
using BBAP.Parser.Expressions.Values;

namespace BBAP.Parser.Expressions.Blocks;

public record ForeachExpression(int Line, bool Declaration, VariableExpression VariableExpression, VariableExpression TableExpression , ImmutableArray<IExpression> BlockContent) : IExpression;