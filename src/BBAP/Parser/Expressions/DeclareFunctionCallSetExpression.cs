using System.Collections.Immutable;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Variables;

namespace BBAP.Parser.Expressions;

public record DeclareFunctionCallSetExpression(int Line,
    CombinedWord Name,
    ImmutableArray<IExpression> Parameters,
    ImmutableArray<VariableExpression> ReturnVariables,
    MutabilityType Mutability): IExpression;