using System.Text;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler;
using BBAP.Results;
using BBAP.Transpiler;
using BBAP.Types;

namespace BBAP.Functions; 

public interface IFunction {
    public Result<int> Matches(IType[] inputs, IType[] outputs, int line);
    public string Name { get; }
    public IType SingleType { get; }
    public bool IsSingleTypeOutput { get; }
    public bool IsMethod { get; }

    public void Render(AbapBuilder builder, IEnumerable<VariableExpression> inputs, IEnumerable<VariableExpression> outputs);
}