using System.Collections.Immutable;

namespace BBAP.Parser.Expressions.Blocks;

public record ForExpression(int Line,
    IExpression Initializer,
    IExpression Condition,
    IExpression Runner,
    ImmutableArray<IExpression> Block) : IExpression;