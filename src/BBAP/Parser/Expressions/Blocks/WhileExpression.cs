using System.Collections.Immutable;

namespace BBAP.Parser.Expressions.Blocks;

public record WhileExpression(int Line, IExpression Condition, ImmutableArray<IExpression> BlockContent) : IExpression;