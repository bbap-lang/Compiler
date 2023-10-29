using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Parser.Expressions.Calculations;
using BBAP.Results;

namespace BBAP.PreTranspiler.SubPreTranspiler; 

public static class ForPreTranspiler {
     public static Result<IExpression[]> Run(ForExpression forExpression, PreTranspilerState state) {
          state.StackIn();

          var initResult = PreTranspiler.RunExpression(state, forExpression.Initializer);

          if (!initResult.TryGetValue(out IExpression[]? init)) {
               return initResult;
          }
          
          IExpression[] additionalConditionExpressions = Array.Empty<IExpression>();
          IExpression condition;
          
          if (forExpression.Condition is BooleanExpression) {
               condition = forExpression.Condition;
          } else if (forExpression.Condition is ComparisonExpression comparisonExpression) {
               Result<IExpression[]> splittedConditionResult = ComparisonPreTranspiler.Run(state, comparisonExpression);
               if (!splittedConditionResult.TryGetValue(out additionalConditionExpressions)) {
                    return splittedConditionResult;
               }

               condition = additionalConditionExpressions.Last();
               additionalConditionExpressions = additionalConditionExpressions.Remove(condition).ToArray();
          } else {
               throw new UnreachableException();
          }
        
          
          Result<IExpression[]> runnerResult = PreTranspiler.RunExpression(state, forExpression.Runner);

          if (!runnerResult.TryGetValue(out IExpression[]? runner)) {
               return runnerResult;
          }
          
          Result<ImmutableArray<IExpression>> blockResult = PreTranspiler.RunBlock(state, forExpression.Block);
          if (!blockResult.TryGetValue(out ImmutableArray<IExpression> block)) {
               return blockResult.ToErrorResult();
          }

          IExpression[] conditionAdditionsWithoutDeclarations = DeclarePreTranspiler.RemoveDeclarations(additionalConditionExpressions);
          ImmutableArray<IExpression> blockWithRunner = block.Concat(runner).Concat(conditionAdditionsWithoutDeclarations).ToImmutableArray();

          var whileExpression = new WhileExpression(condition.Line, condition, blockWithRunner);

          IExpression[] combined = init.Concat(additionalConditionExpressions).Append(whileExpression).ToArray();
          
          state.StackOut();
          return Ok(combined);
     }
}