using System.Collections.Immutable;

namespace BBAP.Parser.Expressions;

public record ReturnExpression(int Line, ImmutableArray<IExpression> ReturnValues) : IExpression;