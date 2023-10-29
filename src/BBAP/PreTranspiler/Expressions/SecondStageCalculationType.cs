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
}