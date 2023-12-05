using System.Collections.Immutable;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.Results;

namespace BBAP.PreTranspiler.SubPreTranspiler;

public static class ForPreTranspiler {
    public static Result<IExpression[]> Run(ForExpression forExpression, PreTranspilerState state) {
        state.StackIn();

        Result<IExpression[]> initResult = PreTranspiler.RunExpression(state, forExpression.Initializer);

        if (!initResult.TryGetValue(out IExpression[]? init)) return initResult;


        Result<ImmutableArray<IExpression>> blockResult = PreTranspiler.RunBlock(state, forExpression.Block);
        if (!blockResult.TryGetValue(out ImmutableArray<IExpression> block)) return blockResult.ToErrorResult();

        if (forExpression.Condition is BooleanValueExpression booleanValueExpression) {
            if (booleanValueExpression.Value) return Ok(block.ToArray());

            return Ok(Array.Empty<IExpression>());
        }

        Result<(IExpression[] additional, ISecondStageValue Condition)> contitionResult
            = ConditionPreTranspiler.Run(forExpression.Condition, state);
        if (!contitionResult.TryGetValue(out (IExpression[] additional, ISecondStageValue Condition) conditionData))
            return contitionResult.ToErrorResult();

        (IExpression[] additionalConditionExpressions, ISecondStageValue condition) = conditionData;


        Result<IExpression[]> runnerResult = PreTranspiler.RunExpression(state, forExpression.Runner);

        if (!runnerResult.TryGetValue(out IExpression[]? runner)) return runnerResult;

        IExpression[] conditionAdditionsWithoutDeclarations
            = DeclarePreTranspiler.RemoveDeclarations(additionalConditionExpressions);
        ImmutableArray<IExpression> blockWithRunner
            = block.Concat(runner).Concat(conditionAdditionsWithoutDeclarations).ToImmutableArray();

        var whileExpression = new WhileExpression(condition.Line, condition, blockWithRunner);

        IExpression[] combined = init.Concat(additionalConditionExpressions).Append(whileExpression).ToArray();

        state.StackOut();
        return Ok(combined);
    }
}