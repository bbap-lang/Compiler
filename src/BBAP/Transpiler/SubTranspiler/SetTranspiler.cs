using System.Diagnostics;
using System.Text;
using BBAP.Parser.Expressions;
using BBAP.PreTranspiler;
using BBAP.PreTranspiler.Expressions;
using BBAP.Transpiler.SubTranspiler;

namespace BBAP.Transpiler;

public static class SetTranspiler {
    public static void Run(SetExpression setExpression, TranspilerState state) {
        VariableTranspiler.Run(setExpression.Variable, state.Builder);
        state.Builder.Append(GetSetType(setExpression.SetType));
        ValueTranspiler.Run(setExpression.Value, state);
        state.Builder.AppendLine(".");
    }

    private static string GetSetType(SetType setType) {
        return setType switch {
            SetType.Generic => " = ",
            SetType.Plus => " += ",
            SetType.Minus => " -= ",
            SetType.Multiplication => " *= ",
            SetType.Devide => " /= ",
            SetType.Modulo => " %= ",
            _ => throw new UnreachableException()
        };
    }
}