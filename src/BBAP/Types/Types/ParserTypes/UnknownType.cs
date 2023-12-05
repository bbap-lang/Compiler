namespace BBAP.Types.Types.ParserTypes;

public class UnknownType : IType {
    public string Name => "Unknown";
    public string AbapName => Name;

    public IType? InheritsFrom => null;

    public SupportedOperator SupportedOperators => SupportedOperator.None;

    public string DeclareKeyWord => "INVALID";
}