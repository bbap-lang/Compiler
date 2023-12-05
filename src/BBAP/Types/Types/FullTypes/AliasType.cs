namespace BBAP.Types; 

public record AliasType(string Name, IType SourceType, bool Public) : IType {
    public string AbapName => Public ? Name : SourceType.AbapName;

    public IType? InheritsFrom => SourceType;

    public SupportedOperator SupportedOperators => SourceType.SupportedOperators;

    public string DeclareKeyWord => SourceType.DeclareKeyWord;
}