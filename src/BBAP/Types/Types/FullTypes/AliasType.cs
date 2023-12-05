namespace BBAP.Types.Types.FullTypes;

public record AliasType(string Name, IType SourceType, bool Public) : IType {
    public string AbapName => Public ? Name : SourceType.AbapName;

    public IType? InheritsFrom => SourceType;

    public SupportedOperator SupportedOperators => SourceType.SupportedOperators;

    public string DeclareKeyWord => SourceType.DeclareKeyWord;

    public IType GetRealType() {
        if (SourceType is AliasType aliasType) return aliasType.GetRealType();

        return SourceType;
    }
}