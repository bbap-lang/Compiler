using System.Collections.Immutable;
using BBAP.Parser.Expressions;
using BBAP.PreTranspiler.Expressions;
using BBAP.Results;

namespace BBAP.PreTranspiler.SubPreTranspiler; 

public static class SetPreTranspiler {
    public static Result<IExpression[]> Run(SetExpression setExpression, PreTranspilerState state) {
        Result<IExpression[]> splittedValueResult = ValueSplitter.Run(state, setExpression.Value);
        if (!splittedValueResult.TryGetValue(out IExpression[]? splittedValue)) {
            return splittedValueResult;
        }

        IExpression lastValue = splittedValue.Last();

        IExpression newExpression;
        if (lastValue is SecondStageFunctionCallExpression funcCall) {
            newExpression = funcCall with { Outputs = ImmutableArray.Create(setExpression.Variable) };
        } else {
            newExpression = setExpression with { Value = lastValue };
        }

        
        IExpression[] newExpressions = splittedValue.Remove(lastValue).Append(newExpression).ToArray();
        
        return Ok(newExpressions);
    }
}