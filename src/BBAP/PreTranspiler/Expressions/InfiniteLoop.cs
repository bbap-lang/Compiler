using System.Collections.Immutable;
using BBAP.Parser.Expressions;

namespace BBAP.PreTranspiler.Expressions;

public record InfiniteLoop(int Line, ImmutableArray<IExpression> BlockContent) : IExpression;