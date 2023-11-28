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
        
        IExpression lastLeftEx = left.Last();
        IExpression lastRightEx = right.Last();

        if(lastLeftEx is not ISecondStageValue lastLeft) {
            throw new UnreachableException();
        }
        
        if(lastRightEx is not ISecondStageValue lastRight) {
            throw new UnreachableException();
        }

        IType leftType = lastLeft.Type.Type;
        IType rightType = lastRight.Type.Type;
        
        if(!leftType.SupportsOperator(comparisonExpression.ComparisonType.ToSupportedOperator())) {
            return Error(line, $"The type {leftType.Name} does not support the operator {comparisonExpression.ComparisonType}.");
        }
        
        if(!rightType.SupportsOperator(comparisonExpression.ComparisonType.ToSupportedOperator())) {
            return Error(line, $"The type {rightType.Name} does not support the operator {comparisonExpression.ComparisonType}.");
        }
        
        Result<IType> typeResult = state.Types.Get(line,"BOOL");
        if (!typeResult.TryGetValue(out IType? typeBool)) {
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

        var newComparison = new SecondStageCalculationExpression(line, new TypeExpression(line, typeBool), calculationType, lastLeft, lastRight);

        var combined = left.Concat(right)
            .Remove(lastLeft)
            .Remove(lastRight)
            .Append(newComparison)
            .ToArray();
        
        return Ok<IExpression[]>(combined);
    }
}