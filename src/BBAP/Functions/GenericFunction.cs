using System.Collections.Immutable;
using System.Text;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler;
using BBAP.PreTranspiler.Expressions;
using BBAP.Transpiler;
using BBAP.Types;

namespace BBAP.Functions; 

public record GenericFunction(string Name, ImmutableArray<IType> Parameters, ImmutableArray<IType> ReturnTypes): IFunction {
    
    public void Render(AbapBuilder builder, IEnumerable<VariableExpression> inputs, IEnumerable<VariableExpression> outputs) {
        throw new NotImplementedException();
    }

    public bool Matches(IType[] inputs, IType[] outputs) {
        if (Parameters.Length != inputs.Length) {
            return false;
        }

        return !inputs.Where((t, i) => Parameters[i] != t).Any();
    }

}