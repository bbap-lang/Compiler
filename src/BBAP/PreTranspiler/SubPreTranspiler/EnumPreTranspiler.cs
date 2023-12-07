using System.Collections.Immutable;
using BBAP.Parser.Expressions;
using BBAP.PreTranspiler.Expressions;
using BBAP.Results;
using BBAP.Types;
using BBAP.Types.Types.FullTypes;

namespace BBAP.PreTranspiler.SubPreTranspiler; 

public class EnumPreTranspiler {
    public static Result<IExpression[]> Create(PreTranspilerState state, EnumExpression enumExpression) {
        Result<IType> sourceTypeResult = state.Types.Get(enumExpression.Line, enumExpression.Type.Type.Name);
        if (!sourceTypeResult.TryGetValue(out IType? sourceType)) return sourceTypeResult.ToErrorResult();

        var newValues = new Dictionary<string, SecondStageValueExpression>();
        foreach ((string name, IExpression value) in enumExpression.Values) {
            Result<IExpression[]> newValueResult = ValueSplitter.Run(state, value, false);
            if (!newValueResult.TryGetValue(out IExpression[]? newValueRange)) return newValueResult.ToErrorResult();
            
            if (newValueRange.Length != 1) return Error(enumExpression.Line, $"Enum value '{name}' can't be an expression");

            IExpression lastValue = newValueRange[0];
            
            if (lastValue is not SecondStageValueExpression newValue) return Error(enumExpression.Line, $"Enum value '{name}' can't be an expression");
            newValues.Add(name, newValue);
        }
        
        IType type = new EnumType(enumExpression.Name, sourceType, newValues.ToImmutableDictionary());
        Result<int> addResult = state.Types.Add(type, enumExpression.Line);
        
        if (!addResult.IsSuccess) return addResult.ToErrorResult();
        
        return Ok(Array.Empty<IExpression>());
    }
    
}