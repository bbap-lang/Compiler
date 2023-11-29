using System.Runtime.InteropServices.JavaScript;
using BBAP.Parser.Expressions.Values;
using BBAP.Results;
using BBAP.Transpiler;
using BBAP.Transpiler.SubTranspiler;
using BBAP.Types;

namespace BBAP.Functions.AbapFunctions; 

public class Concatenate : IFunction{
    public Result<int> Matches(IType[] inputs, IType[] outputs, int line) {
        if (outputs.Length > 1) {
            return Error(line, $"The function 'CONCATENATE' has only one return value.");
        }

        if (inputs.Length < 1) {
            return Error(line, "At least one parameter is required in the function call of 'CONCATENATE'.");
        }

        IType? output = outputs.FirstOrDefault();
        if (output is not null && !TypeCollection.StringType.IsCastableTo(output)) {
            return Error(line, "In the function call of 'CONCATENATE', the variable for the return value is not a string.");
        }
        
        
        foreach ((IType type, int index) in inputs.Select((t, i) =>(t, i))) {
            if (!type.IsCastableTo(TypeCollection.StringType)) {
                return Error(line, $"The parameter type {type.Name}  is not castable to {TypeCollection.StringType} in the function call of 'CONCATENATE'.");
            }
        }
        
        return Ok();
    }

    public string Name => "CONCATENATE";

    public IType SingleType => TypeCollection.StringType;

    public bool IsSingleTypeOutput => true;

    public bool IsMethod => false;

    public void Render(AbapBuilder builder, IEnumerable<VariableExpression> inputs, IEnumerable<VariableExpression> outputs) {
        builder.Append("CONCATENATE ");
        foreach (VariableExpression input in inputs) {
            VariableTranspiler.Run(input, builder);
            builder.Append(' ');
        }
        builder.Append("INTO ");
        VariableTranspiler.Run(outputs.First(), builder);
        builder.AppendLine('.');
    }
}