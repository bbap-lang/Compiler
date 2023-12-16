using System.Diagnostics;

namespace BBAP.Types.Types.ParserTypes;

public record OnlyNameLengthType(string Name,
    long Length) : IType {
    public string AbapName => throw new UnreachableException();

    public IType? InheritsFrom => null;

    public SupportedOperator SupportedOperators => throw new UnreachableException();

    public string DeclareKeyWord => throw new UnreachableException();
}