using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;

namespace BBAP.Transpiler.SubTranspiler;

public static class FunctionTranspiler {
    public static void Run(SecondStageFunctionExpression functionExpression, TranspilerState state) {
        AbapBuilder builder = state.Builder;

        builder.AppendLine();
        builder.Append("FORM ");
        builder.Append(functionExpression.Name);

        if (functionExpression.Parameters.Length > 0) {
            builder.AppendLine();
            builder.Append("\tUSING ");
            foreach (VariableExpression variable in functionExpression.Parameters) {
                VariableTranspiler.Run(variable, state.Builder);
                builder.Append(" TYPE ");
                builder.Append(variable.Variable.Type.AbapName);
                builder.Append(' ');
            }
        }

        if (functionExpression.ReturnVariables.Length > 0) {
            builder.AppendLine();
            builder.Append("\tCHANGING ");
            foreach (VariableExpression returnVariable in functionExpression.ReturnVariables) {
                VariableTranspiler.Run(returnVariable, state.Builder);
                builder.Append(" TYPE ");
                builder.Append(returnVariable.Variable.Type.AbapName);
                builder.Append(' ');
            }
        }

        builder.AppendLine('.');
        builder.AppendLine();

        builder.AddIntend();
        Transpiler.TranspileBlock(functionExpression.ContentBlock, state);
        builder.RemoveIntend();

        builder.AppendLine("ENDFORM.");
        builder.AppendLine();
    }
}