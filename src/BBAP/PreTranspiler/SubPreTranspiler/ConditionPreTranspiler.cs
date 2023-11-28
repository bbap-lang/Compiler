using System.Diagnostics;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.PreTranspiler.SubPreTranspiler; 

public static class ConditionPreTranspiler {
    public static Result<( IExpression[] additional, ISecondStageValue Condition)> Run(IExpression baseCondition, PreTranspilerState state) {
        
        Result<IExpression[]> splittedConditionResult = ValueSplitter.Run(state, baseCondition, true);
        if (!splittedConditionResult.TryGetValue(out IExpression[]? splittedCondition)) {
            return splittedConditionResult.ToErrorResult();
        }
        
        IExpression conditionExpression = splittedCondition.Last();

        splittedCondition = splittedCondition.Remove(conditionExpression).ToArray();

        if (conditionExpression is not ISecondStageValue condition) {
            throw new UnreachableException();
        }

        Result<IType> typeBoolResult = state.Types.Get(condition.Line, Keywords.Boolean);
        if(!typeBoolResult.TryGetValue(out IType? typeBool)){
            throw new UnreachableException();
        }

        if (!condition.Type.Type.IsCastableTo(typeBool)) {
            return Error(condition.Line, "The condition is not castable to boolean.");
        }
        
        if(condition is SecondStageValueExpression conditionValue) {
            var trueExpression = new SecondStageValueExpression(conditionValue.Line, new TypeExpression(conditionValue.Line, typeBool),
                                                                new BooleanValueExpression(conditionValue.Line, true));
            condition = new SecondStageCalculationExpression(conditionValue.Line, new TypeExpression(conditionValue.Line, typeBool),
                                                             SecondStageCalculationType.Equals, conditionValue,trueExpression );
        }
        
        return Ok((splittedCondition.Remove(condition).ToArray(), condition));
    }
}