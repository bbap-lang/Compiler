using BBAP.Parser.Expressions;
using BBAP.Results;

namespace BBAP.PreTranspiler.SubPreTranspiler; 

public static class SetPreTranspiler {
    public static Result<IExpression[]> Run(SetExpression setExpression, PreTranspilerState state) {
        Result<IExpression[]> splittedValueResult = ValueSplitter.Run(state, setExpression.Value);
        if (!splittedValueResult.TryGetValue(out IExpression[]? splittedValue)) {
            return splittedValueResult;
        }

        IExpression lastValue = splittedValue.Last();

        SetExpression newSetExpression = setExpression with { Value = lastValue };
        
        IExpression[] newExpressions = splittedValue.Remove(lastValue).Append(newSetExpression).ToArray();
        
        return Ok(newExpressions);
    }
}