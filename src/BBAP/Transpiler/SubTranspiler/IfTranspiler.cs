using BBAP.Parser.Expressions.Blocks;

namespace BBAP.Transpiler.SubTranspiler; 

public static class IfTranspiler {
    public static void Run(IfExpression ifExpression, TranspilerState state) {
        state.Builder.AppendLine();
        state.Builder.Append("IF ");
        ValueTranspiler.Run(ifExpression.Condition, state);
        state.Builder.AppendLine(".");
        state.Builder.AddIntend();
        Transpiler.TranspileBlock(ifExpression.BlockContent, state);
        state.Builder.RemoveIntend();
        state.Builder.AppendLine("ENDIF.");
        state.Builder.AppendLine();
    }
}