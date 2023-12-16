using System.Collections.Immutable;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Variables;
using BBAP.Results;
using BBAP.Transpiler;
using BBAP.Transpiler.SubTranspiler;
using BBAP.Types;
using BBAP.Types.Types.ParserTypes;

namespace BBAP.Functions;

public record GenericFunction(
    string Name,
    ImmutableArray<IVariable> Parameters,
    ImmutableArray<IVariable> ReturnTypes,
    FunctionAttributes Attributes) : IFunction {
    public bool IsSingleTypeOutput => ReturnTypes.Length == 1;
    public IType SingleType => ReturnTypes.FirstOrDefault()?.Type ?? new UnknownType();

    public void Render(AbapBuilder builder,
        IEnumerable<VariableExpression> inputs,
        IEnumerable<VariableExpression> outputs) {
        bool hasMutThis = Attributes.Is(FunctionAttributes.Method) && !Attributes.Is(FunctionAttributes.Static) && !Attributes.Is(FunctionAttributes.ReadOnly);
        
        builder.Append("PERFORM ");
        builder.AppendLine(Name);

        if (Parameters.Length > (hasMutThis ? 1 : 0)) {
            builder.Append("\tUSING ");
            foreach ((VariableExpression input, IVariable parameter) in inputs.Select((x, i) => (x, Parameters[i])).Skip(hasMutThis ? 1 : 0)) {
                // builder.Append(parameter.Name);
                // builder.Append( " = ");
                VariableTranspiler.Run(input, builder);
                builder.Append(' ');
            }
        }

        if (ReturnTypes.Length > 0 || hasMutThis) {
            builder.AppendLine();
            builder.Append("\tCHANGING ");

            if (hasMutThis) {
                builder.Append(inputs.First().Variable.Name);
                builder.Append(' ');
            }

            foreach ((VariableExpression output, IVariable returnVar) in
                     outputs.Select((x, i) => (x, ReturnTypes[i]))) {
                // builder.Append(returnVar.Name);
                // builder.Append( " = ");
                VariableTranspiler.Run(output, builder);
                builder.Append(' ');
            }
        }

        builder.AppendLine('.');
    }

    public Result<IType[]> GetReturnTypes(int length, int line) {
        if (length > ReturnTypes.Length) return Error(line, "The number of return variables is too large.");

        return Ok(ReturnTypes.Select(x => x.Type).Take(length).ToArray());
    }

    public Result<int> Matches(IType[] inputs, IType[] outputs, int line) {
        if (Parameters.Length != inputs.Length)
            return Error(line, $"The number of parameters does not match in the function call for '{Name}'.");

        foreach ((IType type, int index) in inputs.Select((t, i) => (t, i))) {
            if (!type.IsCastableTo(Parameters[index].Type))
                return Error(line,
                             $"The parameter type {type.Name}  is not castable to {Parameters[index].Name} in the function call for '{Name}'.");
        }

        foreach ((IType type, int index) in outputs.Select((t, i) => (t, i))) {
            if (!ReturnTypes[index].Type.IsCastableTo(type))
                return Error(line,
                             $"The return type {ReturnTypes[index].Type.Name} is not castable to {type.Name} in the function call for '{Name}'.");
        }

        return Ok();
    }
}