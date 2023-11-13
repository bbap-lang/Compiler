using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Parser.Expressions.Calculations;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.PreTranspiler.SubPreTranspiler;

public static class WhilePreTranspiler {
    public static Result<IExpression[]> Run(WhileExpression whileExpression, PreTranspilerState state) {
        state.StackIn();

        Result<ImmutableArray<IExpression>> blockResult = PreTranspiler.RunBlock(state, whileExpression.BlockContent);
        if (!blockResult.TryGetValue(out ImmutableArray<IExpression> block)) {
            return blockResult.ToErrorResult();
        }


        if (whileExpression.Condition is BooleanValueExpression booleanValueExpression) {
            if (booleanValueExpression.Value) {
                return Ok(block.ToArray());
            }

            return Ok(Array.Empty<IExpression>());
        }

        Result<(IExpression[] additional, ISecondStageValue Condition)> contitionResult = ConditionPreTranspiler.Run(whileExpression.Condition, state);
        if(!contitionResult.TryGetValue(out var conditionData)){
            return contitionResult.ToErrorResult();
        }
        
        (IExpression[] additionalConditionExpressions, ISecondStageValue condition) = conditionData;

        IExpression[] conditionAdditionsWithoutDeclarations
            = DeclarePreTranspiler.RemoveDeclarations(additionalConditionExpressions);
        ImmutableArray<IExpression> blockWithAdditionalConditions
            = block.Concat(conditionAdditionsWithoutDeclarations).ToImmutableArray();

        var newExpression = new WhileExpression(condition.Line, condition, blockWithAdditionalConditions);

        IExpression[] combined = additionalConditionExpressions.Append(newExpression).ToArray();

        state.StackOut();
        return Ok(combined);
    }
}