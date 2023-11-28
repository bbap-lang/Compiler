using BBAP.PreTranspiler;
using BBAP.Types;

namespace BBAP.Parser.Expressions.Sql;

public class AllFields : IVariable {
    public IType Type => new UnknownType();

    public string Name => "*";
}