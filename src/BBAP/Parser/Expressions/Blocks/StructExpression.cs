using System.Collections.Immutable;
using BBAP.Parser.Expressions.Values;

namespace BBAP.Parser.Expressions.Blocks; 

public record StructExpression(int Line, string Name, ImmutableArray<VariableExpression>  Fields) : IExpression;