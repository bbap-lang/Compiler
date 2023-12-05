namespace BBAP.Types;

public class AnyType : IType {
    public string Name => "";
    public string AbapName => Name;
    public IType? InheritsFrom { get; } = null;
    public SupportedOperator SupportedOperators { get; } = SupportedOperator.None;

    public string DeclareKeyWord => "INVALID";
}