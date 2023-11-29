using BBAP.Parser.Expressions.Values;
using BBAP.Transpiler;
using BBAP.Transpiler.SubTranspiler;
using BBAP.Types;

namespace BBAP.Functions.AbapFunctions; 

public class Concatenate : IFunction{
    public bool Matches(IType[] inputs, IType[] outputs) {
        return outputs.Length == 1 && inputs.Length > 1;
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