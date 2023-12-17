using System.Diagnostics;
using BBAP.Parser.Expressions.Values;
using BBAP.Results;
using BBAP.Transpiler;
using BBAP.Transpiler.SubTranspiler;
using BBAP.Types;
using BBAP.Types.Types.FullTypes;

namespace BBAP.Functions.AbapFunctions;

public class ToFieldSymbol : IFunction{
    public string Name => "TABLE_TOFIELDSYMBOL";

    public IType GetSingleType(IType[] inputs) { 
        if(inputs[0] is not TableType tableType) throw new UnreachableException();
        
        return new FieldSymbolType(tableType.ContentType);
    }

    public bool IsSingleTypeOutput => false;

    public FunctionAttributes Attributes => FunctionAttributes.Method | FunctionAttributes.ReadOnly;

    public Result<int> Matches(IType[] inputs, IType[] outputs, int line) {
        if (inputs.Length != 1) return Error(line, "'ASSIGNTO' has no parameters.");
        if (outputs.Length > 1) return Error(line, "'ASSIGNTO' only has one return value.");
        
        if(inputs[0] is not TableType tableType) return Error(line, "'ASSIGNTO' is only available for tables.");

        if (outputs.Length < 1) {
            return Ok();
        }
        if(outputs[0] is not FieldSymbolType fieldSymbol) return Error(line, "A table is only assignable to a field symbol.");
        
        if(!tableType.ContentType.IsCastableTo(fieldSymbol.ContentType)) return Error(line, $"The table type {tableType.ContentType.Name} is not castable to {fieldSymbol.ContentType.Name}.");
        
        return Ok();
    }

    public void Render(AbapBuilder builder, IEnumerable<VariableExpression> inputs, IEnumerable<VariableExpression> outputs) {
        VariableExpression table = inputs.First();
        VariableExpression fieldSymbol = outputs.First();
        
        builder.Append("ASSIGN ");
        VariableTranspiler.Run(table, builder);
        builder.Append(" TO ");
        VariableTranspiler.Run(fieldSymbol, builder);
        builder.AppendLine(".");
    }

    public Result<IType[]> GetReturnTypes(int length, int line) {
        throw new NotImplementedException();
    }
}