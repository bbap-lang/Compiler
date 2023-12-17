using System.Diagnostics;
using BBAP.Parser.Expressions.Values;
using BBAP.Results;
using BBAP.Transpiler;
using BBAP.Types;
using BBAP.Types.Types.ParserTypes;

namespace BBAP.Functions.AbapFunctions;

// ABAP: WRITE
public class Print : IFunction {
    public IType? ReturnType => null;

    public string Name => "PRINT";
    public IType GetSingleType(IType[] inputs) => throw new UnreachableException();
    public bool IsSingleTypeOutput => false;

    public FunctionAttributes Attributes => FunctionAttributes.None;

    public Result<int> Matches(IType[] inputs, IType[] outputs, int line) {
        if (outputs.Length != 0) return Error(line, "'Print' has no return value.");

        if (inputs.Length < 1)
            return Error(line, "At least one parameter is required in the function call of 'Print'.");

        foreach ((IType type, int index) in inputs.Select((t, i) => (t, i))) {
            if (!type.IsCastableTo(TypeCollection.StringType))
                return Error(line,
                             $"The parameter type {type.Name}  is not castable to {TypeCollection.StringType} in the function call of 'Print'.");
        }

        return Ok();
    }

    public void Render(AbapBuilder builder,
        IEnumerable<VariableExpression> inputs,
        IEnumerable<VariableExpression> outputs) {
        foreach (VariableExpression input in inputs) {
            builder.Append("WRITE ");
            builder.Append(input.Variable.Name);
            builder.AppendLine(".");
        }
    }
    
    
    public Result<IType[]> GetReturnTypes(int length, int line) {
        if(length > 0) return Error(line, "'PrintLine' has no return value.");
        
        return Ok(Array.Empty<IType>());
    }
}