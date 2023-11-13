using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;

namespace BBAP.Transpiler.SubTranspiler; 

public static class FunctionCallTranspiler {
    public static void Run(SecondStageFunctionCallExpression functionCallExpression, TranspilerState state) {
        functionCallExpression.Function.Render(state.Builder, functionCallExpression.Parameters.Select(x => x.Variable), functionCallExpression.Outputs);
    }
}