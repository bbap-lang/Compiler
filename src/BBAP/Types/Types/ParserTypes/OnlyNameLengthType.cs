namespace BBAP.Types.Types.ParserTypes;

public record OnlyNameLengthType(string Name,
    long Length,
    string AbapName = "INVALID",
    IType? InheritsFrom = null,
    SupportedOperator SupportedOperators = SupportedOperator.None) : IType {
    public string DeclareKeyWord => "INVALID";
}