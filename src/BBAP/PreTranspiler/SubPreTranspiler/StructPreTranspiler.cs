using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Variables;
using BBAP.Results;
using BBAP.Types;
using BBAP.Types.Types.FullTypes;
using BBAP.Types.Types.ParserTypes;

namespace BBAP.PreTranspiler.SubPreTranspiler;

public static class StructPreTranspiler {
    public static Result<int> Create(StructExpression structExpression, PreTranspilerState state) {
        var structType = new OnlyNameType(structExpression.Name);

        Result<int> addTypeResult = state.Types.Add(structType, structExpression.Line);
        if (!addTypeResult.IsSuccess) return addTypeResult.ToErrorResult();

        return Ok();
    }

    public static Result<int> PostCreate(StructExpression structExpression, PreTranspilerState state) {
        var fields = new List<VariableExpression>();
        foreach (VariableExpression field in structExpression.Fields) {
            if (field.Variable.Type.Name == structExpression.Name)
                return Error(field.Line, "Recursive structs are not allowed.");

            Result<IType> typeResult = state.Types.Get(field.Line, field.Variable.Type.Name);
            if (!typeResult.TryGetValue(out IType? type)) return typeResult.ToErrorResult();

            VariableExpression newField = field with { Variable = new Variable(type, field.Variable.Name) };

            fields.Add(newField);
        }

        ImmutableArray<Variable> fieldVariables = fields.Select(x => x.Variable).OfType<Variable>().ToImmutableArray();
        var structType = new StructType(structExpression.Name, fieldVariables);

        Result<IType> oldTypeResult = state.Types.Get(structExpression.Line, structExpression.Name);
        if (!oldTypeResult.TryGetValue(out IType? oldType)) throw new UnreachableException();

        state.ReplaceType(oldType, structType);

        return Ok();
    }

    public static Result<IExpression[]> Replace(StructExpression structExpression, PreTranspilerState state) {
        Result<IType> savedType = state.Types.Get(structExpression.Line, structExpression.Name);
        if (!savedType.IsSuccess) throw new UnreachableException();

        var fields = new List<VariableExpression>();
        foreach (VariableExpression field in structExpression.Fields) {
            Result<IType> typeResult = state.Types.Get(field.Line, field.Variable.Type.Name);
            if (!typeResult.TryGetValue(out IType? type)) return typeResult.ToErrorResult();

            VariableExpression newField = field with { Variable = new Variable(type, field.Variable.Name) };
            fields.Add(newField);
        }

        StructExpression newStruct = structExpression with { Fields = fields.ToImmutableArray() };

        return Ok(new IExpression[] { newStruct });
    }
}