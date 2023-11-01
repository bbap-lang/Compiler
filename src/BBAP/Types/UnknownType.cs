using System.Diagnostics;

namespace BBAP.Types; 

public class UnknownType: IType {
    public string Name => "Unknown";
    public string AbapName => Name;

    public IType? InheritsFrom => null;

    public SupportedOperator SupportedOperators => SupportedOperator.None;
}