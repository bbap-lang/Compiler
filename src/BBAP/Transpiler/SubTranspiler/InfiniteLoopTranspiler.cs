using BBAP.PreTranspiler.Expressions;

namespace BBAP.Transpiler.SubTranspiler;

public class InfiniteLoopTranspiler {
    public static void Run(InfiniteLoop infiniteLoop, TranspilerState state) {
        state.Builder.AppendLine("DO.");
        state.Builder.AddIntend();

        Transpiler.TranspileBlock(infiniteLoop.BlockContent, state, false);

        state.Builder.RemoveIntend();
        state.Builder.AppendLine("ENDDO.");
    }
}