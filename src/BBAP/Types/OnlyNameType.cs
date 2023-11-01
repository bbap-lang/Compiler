namespace BBAP.Types; 

public record OnlyNameType(string Name, string AbapName = "INVALID", IType? InheritsFrom = null, SupportedOperator SupportedOperators = SupportedOperator.None) : IType;