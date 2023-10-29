using System.Diagnostics;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Calculations;
using BBAP.PreTranspiler.Expressions;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.PreTranspiler.SubPreTranspiler; 

public static class ComparisonPreTranspiler {
    public static Result<IExpression[]> Run(PreTranspilerState state, ComparisonExpression comparisonExpression) {
        Result<IExpression[]> leftResult = ValueSplitter.Run(state, comparisonExpression.Left);
        if (!leftResult.TryGetValue(out IExpression[]? left)) {
            return leftResult;
        }
        
        Result<IExpression[]> rightResult = ValueSplitter.Run(state, comparisonExpression.Right);
        if (!rightResult.TryGetValue(out IExpression[]? right)) {
            return rightResult;
        }

        int line = comparisonExpression.Line;
        
        IExpression lastLeft = left.Last();
        IExpression lastRight = right.Last();

        Result<IType> typeResult = state.Types.Get(line,"BOOL");
        if (!typeResult.TryGetValue(out IType? type)) {
            throw new UnreachableException();
        }

        SecondStageCalculationType calculationType = comparisonExpression.ComparisonType switch {
            ComparisonType.Equals => SecondStageCalculationType.Equals,
            ComparisonType.NotEquals => SecondStageCalculationType.NotEquals,
            ComparisonType.SmallerThen => SecondStageCalculationType.SmallerThen,
            ComparisonType.SmallerThenOrEquals => SecondStageCalculationType.SmallerThenOrEquals,
            ComparisonType.GreaterThen => SecondStageCalculationType.GreaterThen,
            ComparisonType.GreaterThenOrEquals => SecondStageCalculationType.GreaterThenOrEquals,

            _ => throw new UnreachableException()
        };

        var newComparison = new SecondStageCalculationExpression(line, type, calculationType, lastLeft, lastRight);

        var combined = left.Concat(right)
            .Remove(lastLeft)
            .Remove(lastRight)
            .Append(newComparison)
            .ToArray();
        
        return Ok<IExpression[]>(combined);
    }
}