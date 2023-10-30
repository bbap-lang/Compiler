using BBAP.Types;

namespace BBAP.Parser.Expressions.Values; 

public record VariableExpression(int Line, string Name, IType Type): IExpression;