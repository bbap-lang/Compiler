using BBAP.PreTranspiler;
using BBAP.Types;

namespace BBAP.Parser.Expressions.Values; 

public record VariableExpression(int Line, IVariable Variable): IExpression;