using System.Diagnostics;
using BBAP.Types;

namespace BBAP.PreTranspiler.Expressions; 

public enum SecondStageCalculationType {
    Plus,
    Minus,
    Multiply,
    Divide,
    Modulo,
    BitwiseAnd,
    BitwiseOr,
    
    Equals,
    NotEquals,
    GreaterThen,
    SmallerThen,
    GreaterThenOrEquals,
    SmallerThenOrEquals,
    
    And,
    Or,
    Xor
}

public static class SecondStageCalculationTypeExtensions {
    public static bool IsMath(this SecondStageCalculationType type) {
        return type switch {
            SecondStageCalculationType.Plus => true,
            SecondStageCalculationType.Minus => true,
            SecondStageCalculationType.Multiply => true,
            SecondStageCalculationType.Divide => true,
            SecondStageCalculationType.Modulo => true,
            SecondStageCalculationType.BitwiseAnd => true,
            SecondStageCalculationType.BitwiseOr => true,

            _ => false
        };
    }
    
    public static bool IsComparison(this SecondStageCalculationType type) {
        return type switch {
            SecondStageCalculationType.Equals => true,
            SecondStageCalculationType.NotEquals => true,
            SecondStageCalculationType.GreaterThen => true,
            SecondStageCalculationType.GreaterThenOrEquals => true,
            SecondStageCalculationType.SmallerThen => true,
            SecondStageCalculationType.SmallerThenOrEquals => true,

            _ => false
        };
    }

    public static SupportedOperator ToSupportedOperator(this SecondStageCalculationType baseOperator) {
        return baseOperator switch {
            SecondStageCalculationType.Plus => SupportedOperator.Plus,
            SecondStageCalculationType.Minus => SupportedOperator.Minus,
            SecondStageCalculationType.Multiply => SupportedOperator.Multiply,
            SecondStageCalculationType.Divide => SupportedOperator.Divide,
            SecondStageCalculationType.Modulo => SupportedOperator.Modulo,
            SecondStageCalculationType.BitwiseAnd => SupportedOperator.BitwiseAnd,
            SecondStageCalculationType.BitwiseOr => SupportedOperator.BitwiseOr,
            SecondStageCalculationType.Equals => SupportedOperator.Equals,
            SecondStageCalculationType.NotEquals => SupportedOperator.NotEquals,
            SecondStageCalculationType.GreaterThen => SupportedOperator.GreaterThen,
            SecondStageCalculationType.GreaterThenOrEquals => SupportedOperator.GreaterThenOrEquals,
            SecondStageCalculationType.SmallerThen => SupportedOperator.SmallerThen,
            SecondStageCalculationType.SmallerThenOrEquals => SupportedOperator.SmallerThenOrEquals,
            
            _ => throw new UnreachableException()
        };
    }
}