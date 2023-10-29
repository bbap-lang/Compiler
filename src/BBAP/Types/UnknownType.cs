using System.Diagnostics;

namespace BBAP.Types; 

public class UnknownType: IType {
    public string Name => "Unknown";
    public string AbapName => Name;
}