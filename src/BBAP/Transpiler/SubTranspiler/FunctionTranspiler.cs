using BBAP.Functions;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;

namespace BBAP.Transpiler.SubTranspiler;

public static class FunctionTranspiler {
    public static void Run(SecondStageFunctionExpression functionExpression, TranspilerState state) {
        AbapBuilder builder = state.Builder;

        builder.AppendLine();
        builder.Append("FORM ");
        builder.Append(functionExpression.Name);

        bool hasThis = functionExpression.Attributes.Is(FunctionAttributes.Method)
                    && !functionExpression.Attributes.Is(FunctionAttributes.Static);
        
        if (functionExpression.Parameters.Length > (hasThis ? 1 : 0)) {
            builder.AppendLine();
            builder.Append("\tUSING ");
            foreach (VariableExpression variable in functionExpression.Parameters.Skip(hasThis ? 1 : 0)) {
                VariableTranspiler.Run(variable, state.Builder);
                builder.Append(" TYPE ");
                builder.Append(variable.Variable.Type.AbapName);
                builder.Append(' ');
            }
        }

        if (functionExpression.ReturnVariables.Length > 0 || hasThis) {
            builder.AppendLine();
            builder.Append("\tCHANGING ");
            if (hasThis) {
                builder.Append(" TYPE ");
                builder.Append(functionExpression.Parameters.First().Variable.Type.AbapName);
                builder.Append(' ');
            }
            
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
        Transpiler.TranspileBlock(functionExpression.ContentBlock, state, true);
        builder.RemoveIntend();

        builder.AppendLine("ENDFORM.");
        builder.AppendLine();
    }
}