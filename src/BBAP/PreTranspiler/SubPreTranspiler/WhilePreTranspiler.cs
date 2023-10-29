using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Parser.Expressions.Calculations;
using BBAP.Results;

namespace BBAP.PreTranspiler.SubPreTranspiler; 

public static class WhilePreTranspiler {
    public static Result<IExpression[]> Run(WhileExpression whileExpression, PreTranspilerState state) {
         state.StackIn();
         
          IExpression[] additionalConditionExpressions = Array.Empty<IExpression>();
          IExpression condition;
          
          if (whileExpression.Condition is BooleanExpression) {
               condition = whileExpression.Condition;
          } else if (whileExpression.Condition is ComparisonExpression comparisonExpression) {
               Result<IExpression[]> splittedConditionResult = ComparisonPreTranspiler.Run(state, comparisonExpression);
               if (!splittedConditionResult.TryGetValue(out additionalConditionExpressions)) {
                    return splittedConditionResult;
               }

               condition = additionalConditionExpressions.Last();
               additionalConditionExpressions = additionalConditionExpressions.Remove(condition).ToArray();
          } else {
               throw new UnreachableException();
          }
          
          Result<ImmutableArray<IExpression>> blockResult = PreTranspiler.RunBlock(state, whileExpression.BlockContent);
          if (!blockResult.TryGetValue(out ImmutableArray<IExpression> block)) {
               return blockResult.ToErrorResult();
          }

          IExpression[] conditionAdditionsWithoutDeclarations = DeclarePreTranspiler.RemoveDeclarations(additionalConditionExpressions);
          ImmutableArray<IExpression> blockWithAdditionalConditions = block.Concat(conditionAdditionsWithoutDeclarations).ToImmutableArray();

          var newExpression = new WhileExpression(condition.Line, condition, blockWithAdditionalConditions);

          IExpression[] combined = additionalConditionExpressions.Append(newExpression).ToArray();
          
          state.StackOut();
          return Ok(combined);
    }
}