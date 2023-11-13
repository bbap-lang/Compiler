using System.Diagnostics;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Calculations;
using BBAP.PreTranspiler.Expressions;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.PreTranspiler.SubPreTranspiler; 

public class BooleanPreTranspiler {
    public static Result<IExpression[]> Run(PreTranspilerState state, BooleanExpression booleanExpression) {
        
        Result<IExpression[]> leftResult = ValueSplitter.Run(state, booleanExpression.Left);
        if (!leftResult.TryGetValue(out IExpression[]? left)) {
            return leftResult;
        }
        
        Result<IExpression[]> rightResult = ValueSplitter.Run(state, booleanExpression.Right);
        if (!rightResult.TryGetValue(out IExpression[]? right)) {
            return rightResult;
        }

        int line = booleanExpression.Line;
        
        IExpression lastLeftEx = left.Last();
        IExpression lastRightEx = right.Last();

        if(lastLeftEx is not ISecondStageValue lastLeft) {
            throw new UnreachableException();
        }
        
        if(lastRightEx is not ISecondStageValue lastRight) {
            throw new UnreachableException();
        }

        IType leftType = lastLeft.Type;
        IType rightType = lastRight.Type;
        
        Result<IType> typeResult = state.Types.Get(line,"BOOL");
        if (!typeResult.TryGetValue(out IType? typeBool)) {
            throw new UnreachableException();
        }

        if(!leftType.IsCastableTo(typeBool)) {
            return Error(line, $"The type {leftType.Name} is not castable to {typeBool.Name}.");
        }
        
        if(!rightType.IsCastableTo(typeBool)) {
            return Error(line, $"The type {rightType.Name} is not castable to {typeBool.Name}.");
        }

        SecondStageCalculationType calculationType = booleanExpression.BooleanType switch {
            BooleanType.And => SecondStageCalculationType.And,
            BooleanType.Or => SecondStageCalculationType.Or,
            BooleanType.Xor => SecondStageCalculationType.Xor,
            
            _ => throw new UnreachableException()
        };

        var newComparison = new SecondStageCalculationExpression(line, typeBool, calculationType, lastLeft, lastRight);

        IExpression[] combined = left.Concat(right)
                                     .Remove(lastLeft)
                                     .Remove(lastRight)
                                     .Append(newComparison)
                                     .ToArray();
        
        return Ok(combined);
    }
}