using System.Collections.Immutable;

namespace BBAP.Parser.Expressions.Blocks;

public record CaseExpression(int Line, IExpression Condition, ImmutableArray<IExpression> CaseBlock);