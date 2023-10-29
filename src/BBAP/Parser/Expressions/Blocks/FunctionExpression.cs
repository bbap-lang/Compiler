using System.Collections.Immutable;
using BBAP.Functions;
using BBAP.Parser.Expressions.Values;

namespace BBAP.Parser.Expressions.Blocks; 

public record FunctionExpression (int Line, string Name, ImmutableArray<ParameterExpression> Parameters, ImmutableArray<TypeExpression> OutputTypes, ImmutableArray<IExpression> BlockContent) : IExpression;