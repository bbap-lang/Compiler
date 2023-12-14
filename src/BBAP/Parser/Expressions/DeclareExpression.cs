using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Variables;

namespace BBAP.Parser.Expressions;

public record DeclareExpression(int Line,
    VariableExpression Variable,
    TypeExpression Type,
    SetExpression? SetExpression,
    MutabilityType MutabilityType) : IExpression;