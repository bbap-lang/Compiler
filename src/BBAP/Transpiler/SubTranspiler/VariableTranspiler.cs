using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Variables;

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

            builder.Append(variable.Name);
        }
    }
}