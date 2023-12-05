using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.Results;

namespace BBAP.PreTranspiler.SubPreTranspiler;

public static class IfPreTranspiler {
    public static Result<IExpression[]> Run(IfExpression ifExpression, PreTranspilerState state) {
        state.StackIn();
        Result<ImmutableArray<IExpression>> blockResult = PreTranspiler.RunBlock(state, ifExpression.BlockContent);
        if (!blockResult.TryGetValue(out ImmutableArray<IExpression> block)) return blockResult.ToErrorResult();

        if (ifExpression.Condition is BooleanValueExpression booleanValueExpression) {
            if (booleanValueExpression.Value) return Ok(block.ToArray());
            return Ok(Array.Empty<IExpression>());
        }

        Result<(IExpression[] additional, ISecondStageValue Condition)> contitionResult
            = ConditionPreTranspiler.Run(ifExpression.Condition, state);
        if (!contitionResult.TryGetValue(out (IExpression[] additional, ISecondStageValue Condition) conditionData))
            return contitionResult.ToErrorResult();

        (IExpression[] splittedCondition, ISecondStageValue condition) = conditionData;

        state.StackOut();

        var additionalElse = new List<IExpression>();
        IExpression? elseExpression = null;

        if (ifExpression.ElseExpression is not null) {
            Result<IExpression[]> elseResult = RunElse(ifExpression.ElseExpression, state);
            if (!elseResult.TryGetValue(out IExpression[]? elseExpressionList)) return elseResult.ToErrorResult();

            elseExpression = elseExpressionList.Last();
            additionalElse.AddRange(elseExpressionList.Remove(elseExpression));
        }

        IfExpression newExpression = ifExpression with {
            Condition = condition, BlockContent = block, ElseExpression = elseExpression
        };

        IExpression[] combined = splittedCondition.Remove(condition).Concat(additionalElse).Append(newExpression)
                                                  .ToArray();
        return Ok(combined);
    }

    private static Result<IExpression[]> RunElse(IExpression expression, PreTranspilerState state) {
        if (expression is IfExpression ifExpression) return Run(ifExpression, state);

        if (expression is not ElseExpression elseExpression) throw new UnreachableException();

        state.StackIn();
        Result<ImmutableArray<IExpression>> blockResult = PreTranspiler.RunBlock(state, elseExpression.BlockContent);
        if (!blockResult.TryGetValue(out ImmutableArray<IExpression> block)) return blockResult.ToErrorResult();

        state.StackOut();
        return Ok<IExpression[]>(new[] { elseExpression with { BlockContent = block } });
    }
}