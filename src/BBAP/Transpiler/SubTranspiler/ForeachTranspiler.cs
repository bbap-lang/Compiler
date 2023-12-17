using BBAP.Parser.Expressions.Blocks;

namespace BBAP.Transpiler.SubTranspiler;

public class ForeachTranspiler {
    public static void Run(ForeachExpression foreachExpression, TranspilerState state) {
        state.Builder.Append("LOOP AT ");
        VariableTranspiler.Run(foreachExpression.TableExpression, state.Builder);
        state.Builder.Append(" ASSIGNING ");
        VariableTranspiler.Run(foreachExpression.VariableExpression, state.Builder);
        state.Builder.AppendLine(".");
        state.Builder.AddIntend();
        Transpiler.TranspileBlock(foreachExpression.BlockContent, state, false);
        state.Builder.RemoveIntend();
        state.Builder.AppendLine("ENDLOOP.");
    }
}