namespace BBAP.Types.Types.FullTypes;

public record DefaultType
    (string Name, string AbapName, IType? InheritsFrom, SupportedOperator SupportedOperators) : IType {
    public string DeclareKeyWord => "TYPE";
}