using System.Text;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler;
using BBAP.Transpiler;
using BBAP.Types;

namespace BBAP.Functions.AbapFunctions; 

// ABAP: WRITE
public class Print: IFunction {

    public string Name => "PRINT";
    public IType SingleType => new UnknownType();
    public bool IsSingleTypeOutput => false;

    public bool IsMethod => false;

    public bool Matches(IType[] inputs, IType[] outputs) {
        return inputs.Length > 0 && outputs.Length == 0;
    }

    public void Render(AbapBuilder builder, IEnumerable<VariableExpression> inputs, IEnumerable<VariableExpression> outputs) {
        foreach (VariableExpression input in inputs) {
            builder.Append("WRITE ");
            builder.Append(input.Variable.Name);
            builder.AppendLine(".");
        }
    }

    public IType? ReturnType => null;
}