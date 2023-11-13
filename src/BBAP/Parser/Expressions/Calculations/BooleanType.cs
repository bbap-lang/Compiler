using System.Diagnostics;
using BBAP.Types;

namespace BBAP.Parser.Expressions.Calculations; 

public enum BooleanType {
    And,
    Or,
    Xor
}

public static class BooleanTypeExtensions {
    public static SupportedOperator ToSupportedOperator(this BooleanType booleanType) {
        return booleanType switch {
            BooleanType.And => SupportedOperator.And,
            BooleanType.Or => SupportedOperator.Or,
            BooleanType.Xor => SupportedOperator.Xor,
            _ => throw new UnreachableException()
        };
    }
}