using System.Collections.Immutable;

namespace BBAP.Parser.Expressions.Blocks; 

public record IfExpression(int Line, IExpression Condition, ImmutableArray<IExpression> BlockContent) : IExpression;