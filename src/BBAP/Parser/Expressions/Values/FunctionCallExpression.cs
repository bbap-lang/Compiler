using System.Collections.Immutable;

namespace BBAP.Parser.Expressions.Values; 

public record FunctionCallExpression(int Line, string Name, ImmutableArray<IExpression> Parameters): IExpression;