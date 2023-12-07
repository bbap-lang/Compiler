namespace BBAP.Types.Types.ParserTypes;

public record OnlyNameType(string Name) : IType {
    public string DeclareKeyWord => "INVALID";
    public string AbapName => "INVALID";
    public IType? InheritsFrom => null;
    public SupportedOperator SupportedOperators => SupportedOperator.None;
}