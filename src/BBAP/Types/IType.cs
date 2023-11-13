namespace BBAP.Types;

public interface IType {
    public string Name { get; }
    public string AbapName { get; }
    public IType? InheritsFrom { get; }
    public SupportedOperator SupportedOperators { get; }
}

public static class ITypeExtensions {
    public static bool IsCastableTo(this IType baseType, IType targetType) {
        if(targetType is AliasType aliasType) {
            targetType = aliasType.SourceType;
        }
        
        while (baseType != targetType) {
            if (baseType.InheritsFrom is null) {
                return false;
            }

            baseType = baseType.InheritsFrom;
        }

        return true;
    }

    public static bool SupportsOperator(this IType type, SupportedOperator @operator) {
        return (type.SupportedOperators & @operator) == @operator;
    }
}

[Flags]
public enum SupportedOperator {
    None = 0,

    Plus = 1 << 0,
    Minus = 1 << 1,
    Multiply = 1 << 2,
    Divide = 1 << 3,
    Modulo = 1 << 4,

    BitwiseAnd = 1 << 5,
    BitwiseOr = 1 << 6,

    Equals = 1 << 7,
    NotEquals = 1 << 8,
    SmallerThen = 1 << 9,
    GreaterThen = 1 << 10,
    SmallerThenOrEquals = 1 << 11,
    GreaterThenOrEquals = 1 << 12,

    And = 1 << 13,
    Or = 1 << 14,
    Xor = 1 << 15,

    AllMath = Plus | Minus | Multiply | Divide | Modulo | BitwiseAnd | BitwiseOr,

    AllComparison = Equals | NotEquals | SmallerThen | GreaterThen | SmallerThenOrEquals | GreaterThenOrEquals,

    AllBoolean = And | Or | Xor,

    All = AllMath | AllComparison | AllBoolean
}