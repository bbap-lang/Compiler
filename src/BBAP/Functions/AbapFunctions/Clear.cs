using System.Diagnostics;
using BBAP.Parser.Expressions.Values;
using BBAP.Results;
using BBAP.Transpiler;
using BBAP.Transpiler.SubTranspiler;
using BBAP.Types;
using BBAP.Types.Types.FullTypes;

namespace BBAP.Functions.AbapFunctions;

public class Clear : IFunction{
    
    public string Name => "ANY_CLEAR";

    public IType GetSingleType(IType[] inputs) => throw new UnreachableException();

    public bool IsSingleTypeOutput => false;

    public FunctionAttributes Attributes => FunctionAttributes.Method;

    public Result<int> Matches(IType[] inputs, IType[] outputs, int line) {
        if(outputs.Length != 0) return Error(line, "'Clear' has no return value.");
        if(inputs.Length != 1) return Error(line, "There is no parameter for 'Clear'.");

        return Ok();
    }

    public void Render(AbapBuilder builder, IEnumerable<VariableExpression> inputs, IEnumerable<VariableExpression> outputs) {
        builder.Append("CLEAR ");
        VariableTranspiler.Run(inputs.First(), builder);
        builder.AppendLine('.');
    }

    public Result<IType[]> GetReturnTypes(int length, int line) {
        return Ok(Array.Empty<IType>());
    }
}