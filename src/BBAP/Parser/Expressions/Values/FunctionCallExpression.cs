using System.Collections.Immutable;

namespace BBAP.Parser.Expressions.Values; 

public record FunctionCallExpression(int Line, CombinedWord Name, ImmutableArray<IExpression> Parameters): IExpression;