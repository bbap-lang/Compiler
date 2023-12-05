using BBAP.Parser.Expressions.Blocks;
using BBAP.Parser.Expressions.Values;

namespace BBAP.Transpiler.SubTranspiler;

public static class StructTranspiler {
    public static void Run(StructExpression structExpression, TranspilerState state) {
        state.Builder.Append("TYPES: BEGIN OF ");
        state.Builder.Append(structExpression.Name);
        state.Builder.AppendLine(',');

        state.Builder.AddIntend();
        foreach (VariableExpression field in structExpression.Fields) {
            state.Builder.Append(field.Variable.Name);
            TypeTranspiler.Run(field.Variable.Type, state.Builder);
            state.Builder.AppendLine(',');
        }

        state.Builder.RemoveIntend();

        state.Builder.Append("END OF ");
        state.Builder.Append(structExpression.Name);
        state.Builder.AppendLine('.');
    }
}