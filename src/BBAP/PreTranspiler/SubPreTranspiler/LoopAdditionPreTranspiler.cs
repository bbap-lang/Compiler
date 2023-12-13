using BBAP.Parser.Expressions;
using BBAP.Results;

namespace BBAP.PreTranspiler.SubPreTranspiler;

public class LoopAdditionPreTranspiler {
    public static Result<IExpression[]> RunBreak(PreTranspilerState state, int line) {
        if (!state.IsIn(StackType.Loop)) {
            return Error(line, "Cannot break, if not in loop");
        }
        
        return Ok<IExpression[]>(new[] {new BreakLoopExpression(line)});
    }
    
    public static Result<IExpression[]> RunContinue(PreTranspilerState state, int line) {
        if (!state.IsIn(StackType.Loop)) {
            return Error(line, "Cannot continue, if not in loop");
        }
        
        return Ok<IExpression[]>(new[] {new ContinueLoopExpression(line)});
    }
}