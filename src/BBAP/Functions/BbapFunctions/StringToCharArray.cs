using BBAP.Parser.Expressions.Values;
using BBAP.Transpiler;
using BBAP.Transpiler.SubTranspiler;
using BBAP.Types;

namespace BBAP.Functions.BbapFunctions; 

public class StringToCharArray: IFunction{
    public bool Matches(IType[] inputs, IType[] outputs) {
        return outputs.Length <= 1 && inputs.Length == 1;
    }

    public string Name => "STRING_TOCHARARRAY";

    public IType SingleType => TypeCollection.BaseCharType;

    public bool IsSingleType => true;

    public void Render(AbapBuilder builder, IEnumerable<VariableExpression> inputs, IEnumerable<VariableExpression> outputs) {
        VariableExpression? output = outputs.FirstOrDefault();
        VariableExpression input = inputs.First();

        if (output is null) {
            return;
        }
        
        VariableTranspiler.Run(output, builder);
        builder.Append(" = ");
        VariableTranspiler.Run(input, builder);
        builder.AppendLine(".");
    }
}