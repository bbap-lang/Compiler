namespace BBAP.Types.Types.ParserTypes;

public record OnlyNameType(string Name,
    string AbapName = "INVALID",
    IType? InheritsFrom = null,
    SupportedOperator SupportedOperators = SupportedOperator.None) : IType {
    public string DeclareKeyWord => "INVALID";
}