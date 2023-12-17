namespace BBAP.Types.Types.FullTypes;

public record FieldSymbolType(IType ContentType) : IType{
    public string Name => $"FIELDSYMBOL<{ContentType.Name}>";
    public string AbapName => $"{ContentType.Name}";
    public IType? InheritsFrom => ContentType;
    public SupportedOperator SupportedOperators => ContentType.SupportedOperators;

    public string DeclareKeyWord => "TYPE";
    
}