using System.Text;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler;
using BBAP.Transpiler;
using BBAP.Types;

namespace BBAP.Functions.AbapFunctions; 

public class PrintLine: IFunction {
    public IType SingleType => new UnknownType();
    public bool IsSingleType => false;
    
    public bool Matches(IType[] inputs, IType[] outputs) {
        return outputs.Length == 0;
    }

    public string Name => "PRINTLINE";
    
    public void Render(AbapBuilder builder, IEnumerable<VariableExpression> inputs, IEnumerable<VariableExpression> outputs) {
        foreach (VariableExpression input in inputs) {
            builder.Append("WRITE ");
            builder.Append(input.Variable.Name);
            builder.AppendLine(".");
        }
        builder.AppendLine("NEW-LINE.");
    }

    public IType? ReturnType => null;
}