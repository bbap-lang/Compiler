using System.Diagnostics;
using BBAP.Types;

namespace BBAP.Parser.Expressions.Calculations; 

public enum ComparisonType {
    Equals,
    NotEquals,
    GreaterThen,
    SmallerThen,
    GreaterThenOrEquals,
    SmallerThenOrEquals,
}

public static class ComparisonTypeExtensions {
    public static SupportedOperator ToSupportedOperator(this ComparisonType comparisonType) => comparisonType switch {
        ComparisonType.Equals => SupportedOperator.Equals,
        ComparisonType.NotEquals => SupportedOperator.NotEquals,
        ComparisonType.GreaterThen => SupportedOperator.GreaterThen,
        ComparisonType.SmallerThen => SupportedOperator.SmallerThen,
        ComparisonType.GreaterThenOrEquals => SupportedOperator.GreaterThenOrEquals,
        ComparisonType.SmallerThenOrEquals => SupportedOperator.SmallerThenOrEquals,
        _ => throw new UnreachableException()
    };
}