using BBAP.Types;

namespace BBAP.PreTranspiler; 

public interface IVariable {
    public IType Type { get; }
    public string Name { get; }
}

public static class VariableMethods {
    public static IVariable GetTopVariable(this IVariable variable) {
        IVariable topVariable = variable;
        while (topVariable is FieldVariable fieldVariable) {
            topVariable = fieldVariable.SourceVariable;
        }

        return topVariable;
    }
    
    public static IVariable[] Unwrap(this IVariable variable) {
        List<IVariable> variables = new();
        IVariable currentVariable = variable;
        while (currentVariable is FieldVariable fieldVariable) {
            variables.Add(fieldVariable);
            currentVariable = fieldVariable.SourceVariable;
        }

        variables.Add(currentVariable);
        variables.Reverse();
        return variables.ToArray();
    }
}