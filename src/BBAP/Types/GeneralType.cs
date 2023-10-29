namespace BBAP.Types;

public record GeneralType(string Name) : IType {
    public string AbapName => Name;
}