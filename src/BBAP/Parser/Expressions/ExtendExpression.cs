using System.Collections.Immutable;
using BBAP.Parser.Expressions.Blocks;

namespace BBAP.Parser.Expressions;

public record ExtendExpression
    (int Line, TypeExpression Type, ImmutableArray<FunctionExpression> Functions) : IExpression;