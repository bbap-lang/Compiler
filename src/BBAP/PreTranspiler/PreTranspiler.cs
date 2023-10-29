using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Parser.Expressions.Calculations;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.PreTranspiler.SubPreTranspiler;
using BBAP.Results;
using BBAP.Transpiler;

namespace BBAP.PreTranspiler; 

public class PreTranspiler {
    public Result<ImmutableArray<IExpression>> Run(ImmutableArray<IExpression> inputTree) {
        var state = new PreTranspilerState();

        Result<int> registerFunctionsResult = RegisterFunctions(inputTree, state);

        if (!registerFunctionsResult.IsSuccess) {
            return registerFunctionsResult.ToErrorResult();
        }
        
        Result<ImmutableArray<IExpression>> result = RunBlock(state, inputTree);

        if (!result.TryGetValue(out ImmutableArray<IExpression> newTree)) {
            return result.ToErrorResult();
        }
        
        return Ok(newTree);
    }

    private Result<int> RegisterFunctions(ImmutableArray<IExpression> inputTree, PreTranspilerState state) {
        IEnumerable<FunctionExpression> functionExpressions = inputTree.OfType<FunctionExpression>();
        foreach (FunctionExpression function in functionExpressions) {
            Result<SecondStageFunctionExpression> expressionResult = FunctionPreTranspiler.Create(function, state);
            if (!expressionResult.TryGetValue(out SecondStageFunctionExpression? expression)) {
                return expressionResult.ToErrorResult();
            }

            state.AddFunction(expression);
        }

        return Ok(0);
    }

    public static Result<ImmutableArray<IExpression>> RunBlock(PreTranspilerState state, ImmutableArray<IExpression> expressions) {

        var newTree = new List<IExpression>();
        foreach (IExpression expression in expressions) {
            Result<IExpression[]> result = RunExpression(state, expression);

            if (!result.TryGetValue(out IExpression[]? newExpression)) {
                return result.ToErrorResult();
            }
            
            newTree.AddRange(newExpression);
        }
        
        
        return Ok(newTree.ToImmutableArray());
    }

    public static Result<IExpression[]> RunExpression(PreTranspilerState state, IExpression expression) {
        return expression switch {
            DeclareExpression declareExpression => DeclarePreTranspiler.Run(declareExpression, state),
            SetExpression setExpression => SetPreTranspiler.Run(setExpression, state),
            IncrementExpression incrementExpression => IncrementPreTranspiler.Run(incrementExpression, state),
            FunctionExpression functionExpression => FunctionPreTranspiler.Replace(functionExpression, state),
                
            IfExpression ifExpression => IfPreTranspiler.Run(ifExpression, state),
            ForExpression forExpression => ForPreTranspiler.Run(forExpression, state),
            WhileExpression whileExpression => WhilePreTranspiler.Run(whileExpression, state),
            
            FunctionCallExpression fc => FunctionCallPreTranspiler.Run(state, fc),

            _ => Ok(new[]{expression})
        };
    }
}