namespace BBAP.Types; 

public record AliasType(string Name, IType SourceType, bool AbapAlias) : IType {
    public string AbapName => AbapAlias ? Name : SourceType.AbapName;

    public IType? InheritsFrom => SourceType;

    public SupportedOperator SupportedOperators => SourceType.SupportedOperators;

    public string DeclareKeyWord => SourceType.DeclareKeyWord;
}