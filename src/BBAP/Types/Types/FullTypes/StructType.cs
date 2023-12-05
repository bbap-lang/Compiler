using System.Collections.Immutable;
using BBAP.PreTranspiler.Variables;

namespace BBAP.Types.Types.FullTypes;

public record StructType(string Name, ImmutableArray<Variable> Fields) : IType {
    public SupportedOperator SupportedOperators => SupportedOperator.None;

    public string DeclareKeyWord => "TYPE";

    public IType? InheritsFrom => null;
    public string AbapName => Name;
}