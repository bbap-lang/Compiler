using BBAP.Parser.Expressions.Values;
using BBAP.Parser.SubParsers;

namespace BBAP.Parser.Expressions; 

public record SetExpression(int Line, VariableExpression Variable, SetType SetType, IExpression Value) : IExpression;