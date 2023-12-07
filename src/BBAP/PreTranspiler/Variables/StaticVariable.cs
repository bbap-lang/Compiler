using BBAP.Types;

namespace BBAP.PreTranspiler.Variables; 

public record StaticVariable(IType Type, string Name, IType SourceType) :IVariable;