using System.Collections.Immutable;
using BBAP.Parser.Expressions.Values;

namespace BBAP.Parser.Expressions; 

public record FunctionCallSetExpression(int Line, string Name, ImmutableArray<IExpression> Parameters, ImmutableArray<VariableExpression> ReturnVariables): IExpression;