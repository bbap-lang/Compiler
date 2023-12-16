using System.Diagnostics;

namespace BBAP.Types.Types.FullTypes;

public class AnyType : IType {
    public string Name => "ANY";

    public string AbapName => throw new UnreachableException();

    public IType? InheritsFrom => null;

    public SupportedOperator SupportedOperators => SupportedOperator.None;

    public string DeclareKeyWord => throw new UnreachableException();
}