using System.Text;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler;
using BBAP.Transpiler;
using BBAP.Types;

namespace BBAP.Functions; 

public interface IFunction {
    public bool Matches(IType[] inputs, IType[] outputs);
    public string Name { get; }

    public void Render(AbapBuilder builder, IEnumerable<VariableExpression> inputs, IEnumerable<VariableExpression> outputs);
}