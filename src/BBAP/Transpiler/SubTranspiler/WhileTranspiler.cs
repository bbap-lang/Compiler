using System.Text;
using BBAP.Parser.Expressions.Blocks;

namespace BBAP.Transpiler.SubTranspiler; 

public static class WhileTranspiler {
    public static void Run(WhileExpression whileExpression, TranspilerState state) {
        state.Builder.AppendLine();
        state.Builder.Append("WHILE ");
        ValueTranspiler.Run(whileExpression.Condition, state);
        state.Builder.AppendLine(".");
        state.Builder.AddIntend();
        Transpiler.TranspileBlock(whileExpression.BlockContent, state);
        state.Builder.RemoveIntend();
        state.Builder.AppendLine("ENDWHILE.");
        state.Builder.AppendLine();
    }
}