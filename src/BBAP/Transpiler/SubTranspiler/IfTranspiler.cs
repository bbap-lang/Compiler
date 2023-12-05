using System.Diagnostics;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;

namespace BBAP.Transpiler.SubTranspiler;

public static class IfTranspiler {
    public static void Run(IfExpression ifExpression, TranspilerState state, bool isElse = false) {
        if (!isElse) state.Builder.AppendLine();

        state.Builder.Append("IF ");
        ValueTranspiler.Run(ifExpression.Condition, state);
        state.Builder.AppendLine(".");
        state.Builder.AddIntend();
        Transpiler.TranspileBlock(ifExpression.BlockContent, state);
        state.Builder.RemoveIntend();
        if (ifExpression.ElseExpression is not null) RunElse(ifExpression.ElseExpression, state);

        if (isElse) return;

        state.Builder.AppendLine("ENDIF.");
        state.Builder.AppendLine();
    }

    public static void RunElse(IExpression expression, TranspilerState state) {
        if (expression is IfExpression ifExpression) {
            state.Builder.Append("ELSE");
            Run(ifExpression, state, true);
            return;
        }

        if (expression is not ElseExpression elseExpression) throw new UnreachableException();

        state.Builder.AppendLine("ELSE.");
        state.Builder.AddIntend();
        Transpiler.TranspileBlock(elseExpression.BlockContent, state);
        state.Builder.RemoveIntend();
    }
}