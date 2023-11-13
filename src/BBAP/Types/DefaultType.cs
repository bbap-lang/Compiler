using System.Security.Cryptography.X509Certificates;

namespace BBAP.Types; 

public record DefaultType(string Name, string AbapName, IType? InheritsFrom, SupportedOperator SupportedOperators): IType {
    public string DeclareKeyWord => "TYPE";
}