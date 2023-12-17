using System.Diagnostics;
using BBAP.Parser.Expressions.Values;
using BBAP.Results;
using BBAP.Transpiler;
using BBAP.Transpiler.SubTranspiler;
using BBAP.Types;
using BBAP.Types.Types.FullTypes;

namespace BBAP.Functions.AbapFunctions;

public class TableAppend : IFunction{
    public string Name => "TABLE_APPEND";

    public IType GetSingleType(IType[] inputs) => throw new UnreachableException();

    public bool IsSingleTypeOutput => false;

    public FunctionAttributes Attributes => FunctionAttributes.Method;

    public Result<int> Matches(IType[] inputs, IType[] outputs, int line) {
        if(outputs.Length != 0) return Error(line, "'Append' has no return value.");
        if(inputs.Length != 2) return Error(line, "Exactly one parameter is required in the function call of 'Append'.");

        IType inputType = inputs[0];
        if (inputType is AliasType aliasType) {
            inputType = aliasType.GetRealType();
        }
        
        if (inputType is not TableType tableType)
            return Error(line,
                         $"The variable of type {inputs[0].Name} is not a table in the function call of 'Append'.");
        
        if(!inputs[1].IsCastableTo(tableType.ContentType)) return Error(line, $"The parameter type {inputs[1].Name} is not castable to the table of type {inputs[0].Name} in the function call of 'Append'.");

        return Ok();
    }

    public void Render(AbapBuilder builder, IEnumerable<VariableExpression> inputs, IEnumerable<VariableExpression> outputs) {
        (VariableExpression table, VariableExpression value) = inputs.GetFirstAndSecond();
        
        builder.Append("APPEND ");
        VariableTranspiler.Run(value, builder);
        builder.Append(" TO ");
        VariableTranspiler.Run(table, builder);
        builder.AppendLine(".");
    }

    public Result<IType[]> GetReturnTypes(int length, int line) {
        return Ok(Array.Empty<IType>());
    }
}