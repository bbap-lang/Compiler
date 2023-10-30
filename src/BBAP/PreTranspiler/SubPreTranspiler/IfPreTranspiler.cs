using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Parser.Expressions.Calculations;
using BBAP.Results;

namespace BBAP.PreTranspiler.SubPreTranspiler; 

public static class IfPreTranspiler {
    public static Result<IExpression[]> Run(IfExpression ifExpression, PreTranspilerState state) {
        state.StackIn();
        Result<ImmutableArray<IExpression>> blockResult = PreTranspiler.RunBlock(state, ifExpression.BlockContent);
        if (!blockResult.TryGetValue(out ImmutableArray<IExpression> block)) {
            return blockResult.ToErrorResult();
        }
        
        if (ifExpression.Condition is BooleanExpression) {
            return Ok<IExpression[]>(new [] { ifExpression with { BlockContent = block } });
        }

        if (ifExpression.Condition is not ComparisonExpression comparisonExpression) {
            throw new UnreachableException();
        }
        
        Result<IExpression[]> splittedConditionResult = ComparisonPreTranspiler.Run(state, comparisonExpression);
        if (!splittedConditionResult.TryGetValue(out IExpression[]? splittedCondition)) {
            return splittedConditionResult;
        }

        IExpression condition = splittedCondition.Last();
        IfExpression newExpression = ifExpression with { Condition = condition, BlockContent = block };

        IExpression[] combined = splittedCondition.Remove(condition).Append(newExpression).ToArray();
        
        state.StackOut();
        return Ok(combined);
    }
}