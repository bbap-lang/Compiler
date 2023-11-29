using System.Collections.Immutable;
using System.Text;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler;
using BBAP.PreTranspiler.Expressions;
using BBAP.Transpiler;
using BBAP.Types;

namespace BBAP.Functions;

public record GenericFunction
    (string Name, ImmutableArray<IVariable> Parameters, ImmutableArray<IVariable> ReturnTypes, bool IsMethod) : IFunction {
    public bool IsSingleTypeOutput => ReturnTypes.Length == 1;
    public IType SingleType => ReturnTypes.FirstOrDefault()?.Type ?? new UnknownType();

    public void Render(AbapBuilder builder,
        IEnumerable<VariableExpression> inputs,
        IEnumerable<VariableExpression> outputs) {
        builder.Append("PERFORM ");
        builder.AppendLine(Name);

        if (Parameters.Length > 0) {
            builder.Append("\tUSING ");
            foreach ((VariableExpression input, IVariable parameter) in inputs.Select((x, i) => (x, Parameters[i]))) {
                builder.Append(parameter.Name);
                builder.Append(' ');
                builder.Append(input.Variable.Name);
                builder.Append(' ');
            }
        }

        if (ReturnTypes.Length > 0) {
            builder.AppendLine();
            builder.Append("\tCHANGING ");
            foreach ((VariableExpression output, IVariable returnVar) in outputs.Select((x, i) => (x, ReturnTypes[i]))) {
                builder.Append(returnVar.Name);
                builder.Append(' ');
                builder.Append(output.Variable.Name);
                builder.Append(' ');
            }
        }

        builder.AppendLine('.');
    }

    public bool Matches(IType[] inputs, IType[] outputs) {
        if (Parameters.Length != inputs.Length) {
            return false;
        }

        return !inputs.Where((t, i) => Parameters[i].Type != t).Any()
            && !outputs.Where((t, i) => ReturnTypes[i].Type != t).Any();
    }

}