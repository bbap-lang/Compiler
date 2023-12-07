using BBAP.Types;

namespace BBAP.Parser.Expressions; 

public record EnumExpression(int Line, string Name, TypeExpression Type, Dictionary<string, IExpression> Values) : IExpression;