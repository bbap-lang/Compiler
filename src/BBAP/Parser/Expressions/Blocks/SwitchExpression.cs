using System.Collections.Immutable;
using BBAP.Parser.Expressions.Values;

namespace BBAP.Parser.Expressions.Blocks;

public record SwitchExpression(int Line, VariableExpression Variable, ImmutableArray<CaseExpression> Cases, ImmutableArray<IExpression>? DefaultCase) : IExpression;