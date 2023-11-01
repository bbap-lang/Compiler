using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler;
using BBAP.PreTranspiler.Expressions;
using BBAP.Types;

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
            foreach ((IType type, string name) in functionExpression.Parameters) {
                builder.Append(name);
                builder.Append(" TYPE ");
                builder.Append(type.AbapName);
                builder.Append(' ');
            }
        }
        
        if(functionExpression.ReturnVariables.Length > 0){
            builder.AppendLine();
            builder.Append("\tCHANGING ");
            foreach (Variable returnVariable in functionExpression.ReturnVariables) {
                builder.Append(returnVariable.Name);
                builder.Append(" TYPE ");
                builder.Append(returnVariable.Type.AbapName);
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