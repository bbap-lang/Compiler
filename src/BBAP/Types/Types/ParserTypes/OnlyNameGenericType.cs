namespace BBAP.Types.Types.ParserTypes;

public record OnlyNameGenericType(string Name, OnlyNameType GenericType) : IType {
    public string AbapName => "INVALID";

    public IType? InheritsFrom => null;

    public SupportedOperator SupportedOperators => SupportedOperator.None;

    public string DeclareKeyWord => "INVALID";
}