using System.Collections.Immutable;
using BBAP.Parser.Expressions.Values;

namespace BBAP.Parser.Expressions; 

public record StructSetExpression(int Line, VariableExpression Variable, ImmutableArray<SetExpression> Fields) : IExpression;