using BBAP.Types;

namespace BBAP.PreTranspiler.Variables;

public record FieldVariable(IType Type, string Name, IVariable SourceVariable) : IVariable {
    public MutabilityType MutabilityType => MutabilityType.Mutable;
}