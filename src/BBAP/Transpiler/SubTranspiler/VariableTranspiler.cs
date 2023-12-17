using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Variables;
using BBAP.Types.Types.FullTypes;

namespace BBAP.Transpiler.SubTranspiler;

public class VariableTranspiler {
    public static void Run(VariableExpression variableExpression, AbapBuilder builder) {
        IVariable startVariable = variableExpression.Variable;
        List<IVariable> variables = new() { startVariable };

        while (startVariable is FieldVariable fieldVariable) {
            startVariable = fieldVariable.SourceVariable;
            variables.Add(startVariable);
        }

        variables.Reverse();

        bool first = true;
        foreach (IVariable variable in variables) {
            if (first)
                first = false;
            else
                builder.Append('-');

            if (variable.Type is FieldSymbolType) {
                builder.Append('<');
            }
            builder.Append(variable.Name);
            
            if (variable.Type is FieldSymbolType) {
                builder.Append('>');
            }
        }
    }
}