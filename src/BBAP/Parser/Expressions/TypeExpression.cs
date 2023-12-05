using BBAP.Types;

namespace BBAP.Parser.Expressions;

public record TypeExpression(int Line, IType Type) : IExpression;