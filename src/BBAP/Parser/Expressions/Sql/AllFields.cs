using BBAP.PreTranspiler.Variables;
using BBAP.Types;
using BBAP.Types.Types.ParserTypes;

namespace BBAP.Parser.Expressions.Sql;

public class AllFields : IVariable {
    public IType Type => new UnknownType();

    public string Name => "*";
}