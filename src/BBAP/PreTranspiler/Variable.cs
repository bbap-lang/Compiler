using BBAP.Types;

namespace BBAP.PreTranspiler; 

public record struct Variable(IType Type, string Name);