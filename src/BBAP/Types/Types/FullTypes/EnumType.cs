using System.Collections.Immutable;
using BBAP.Parser.Expressions;
using BBAP.PreTranspiler.Expressions;

namespace BBAP.Types.Types.FullTypes; 

public record EnumType(string Name, IType SourceType, ImmutableDictionary<string, SecondStageValueExpression> Values) : IType {
    public string AbapName => SourceType.AbapName;

    public IType? InheritsFrom => SourceType;

    public SupportedOperator SupportedOperators => SourceType.SupportedOperators;

    public string DeclareKeyWord => SourceType.DeclareKeyWord;
    
    
}