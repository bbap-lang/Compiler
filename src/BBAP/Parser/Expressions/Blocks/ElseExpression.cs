using System.Collections.Immutable;

namespace BBAP.Parser.Expressions.Blocks;

public record ElseExpression(int Line, ImmutableArray<IExpression> BlockContent) : IExpression;