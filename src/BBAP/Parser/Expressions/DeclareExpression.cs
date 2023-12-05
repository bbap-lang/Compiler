using BBAP.Parser.Expressions.Values;

namespace BBAP.Parser.Expressions;

public record DeclareExpression(int Line,
    VariableExpression Variable,
    TypeExpression Type,
    SetExpression? SetExpression) : IExpression;