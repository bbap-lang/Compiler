using System.Diagnostics;

namespace BBAP.Types.Types.FullTypes;

public class AnyTableType : IType{
    public AnyTableType(IType? inheritsFrom) {
        InheritsFrom = inheritsFrom;
    }

    public string Name => "TABLE";

    public string AbapName => throw new UnreachableException();

    public IType? InheritsFrom { get; }

    public SupportedOperator SupportedOperators => SupportedOperator.None;

    public string DeclareKeyWord => throw new UnreachableException();
}