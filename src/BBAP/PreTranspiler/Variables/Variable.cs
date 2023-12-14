using BBAP.Types;

namespace BBAP.PreTranspiler.Variables;

public record Variable(IType Type, string Name, MutabilityType MutabilityType) : IVariable;