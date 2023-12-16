using BBAP.Functions;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.Types.Types.FullTypes;

namespace BBAP.Transpiler.SubTranspiler;

public static class FunctionTranspiler {
    public static void Run(SecondStageFunctionExpression functionExpression, TranspilerState state) {
        AbapBuilder builder = state.Builder;

        builder.AppendLine();
        builder.Append("FORM ");
        builder.Append(functionExpression.Name);

        bool hasMutThis = functionExpression.Attributes.Is(FunctionAttributes.Method)
                       && !functionExpression.Attributes.Is(FunctionAttributes.Static)
                       && !functionExpression.Attributes.Is(FunctionAttributes.ReadOnly);

        
        VariableExpression[] inputArray = functionExpression.Parameters.Skip(hasMutThis ? 1 : 0).ToArray();

        VariableExpression[] outputArray;
        if (hasMutThis) {
            outputArray = functionExpression.ReturnVariables.Append(functionExpression.Parameters.First()).ToArray();
        } else {
            outputArray = functionExpression.ReturnVariables.ToArray();
        }
        
        if (inputArray.Length > 0) {
            builder.AppendLine();
            builder.Append("\tUSING ");
            foreach (VariableExpression variable in inputArray.Skip(hasMutThis ? 1 : 0)) {
                VariableTranspiler.Run(variable, state.Builder);
                builder.Append(' ');
                TypeTranspiler.Run(variable.Variable.Type, builder);
                builder.Append(' ');
            }
        }

        if (outputArray.Length > 0) {
            builder.AppendLine();
            builder.Append("\tCHANGING ");

            foreach (VariableExpression returnVariable in outputArray) {
                VariableTranspiler.Run(returnVariable, state.Builder);
                builder.Append(' ');
                TypeTranspiler.Run(returnVariable.Variable.Type, builder);
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