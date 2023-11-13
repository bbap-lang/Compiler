using System.Collections.Immutable;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Parser.Expressions.Values;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.PreTranspiler.SubPreTranspiler; 

public static class StructPreTranspiler {
    public static Result<IExpression[]> Run(StructExpression structExpression, PreTranspilerState state) {
        var fields = new List<VariableExpression>();
        foreach (VariableExpression field in structExpression.Fields) {
            Result<IType> typeResult = state.Types.Get(field.Line, field.Variable.Type.Name);
            if(!typeResult.TryGetValue(out IType? type)) {
                return typeResult.ToErrorResult();
            }
            
            VariableExpression newField = field with { Variable = new Variable(type, field.Variable.Name)};
            fields.Add(newField);
        }

        ImmutableArray<Variable> fieldVariables = fields.Select(x => x.Variable).OfType<Variable>().ToImmutableArray();
        var structType = new StructType(structExpression.Name, fieldVariables);

        Result<int> addTypeResult = state.Types.Add(structType, structExpression.Line);

        if (!addTypeResult.TryGetValue(out _)) {
            return addTypeResult.ToErrorResult();
        }
        
        StructExpression newStruct = structExpression with { Fields = fields.ToImmutableArray() };

        return Ok(new IExpression[] { newStruct });
    }
}