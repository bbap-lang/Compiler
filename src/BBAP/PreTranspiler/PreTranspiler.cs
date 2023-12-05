using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.Functions;
using BBAP.Lexer.Tokens.Keywords;
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
    public Result<ImmutableArray<IExpression>> Run(ImmutableArray<IExpression> inputTree, PreTranspilerState state) {
        var preRegisterResult = PreRegister(inputTree, state);
        if (!preRegisterResult.IsSuccess) {
            return preRegisterResult.ToErrorResult();
        }
        
        Result<ImmutableArray<IExpression>> result = RunBlock(state, inputTree);

        if (!result.TryGetValue(out ImmutableArray<IExpression> newTree)) {
            return result.ToErrorResult();
        }
        
        return Ok(newTree);
    }

    private Result<int> PreRegister(ImmutableArray<IExpression> inputTree, PreTranspilerState state) {
        IEnumerable<AliasExpression> aliasExpressions = inputTree.OfType<AliasExpression>();
        IEnumerable<StructExpression> structExpressions = inputTree.OfType<StructExpression>();
        IEnumerable<FunctionExpression> functionExpressions = inputTree.OfType<FunctionExpression>();
        
        foreach (AliasExpression alias in aliasExpressions) {
            Result<int> createResult = AliasPreTranspiler.Create(alias, state);
            if(!createResult.IsSuccess) {
                return createResult.ToErrorResult();
            }
        }
        foreach (StructExpression @struct in structExpressions) {
            Result<int> createResult = StructPreTranspiler.Create(@struct, state);
            if(!createResult.IsSuccess) {
                return createResult.ToErrorResult();
            }
        }
        
        
        foreach (AliasExpression alias in aliasExpressions) {
            Result<int> createResult = AliasPreTranspiler.PostCreate(alias, state);
            if(!createResult.IsSuccess) {
                return createResult.ToErrorResult();
            }
        }
        
        
        foreach (StructExpression @struct in structExpressions) {
            Result<int> createResult = StructPreTranspiler.PostCreate(@struct, state);
            if(!createResult.IsSuccess) {
                return createResult.ToErrorResult();
            }
        }
        
        foreach (FunctionExpression function in functionExpressions) {
            Result<SecondStageFunctionExpression> expressionResult = FunctionPreTranspiler.Create(function, null, state);
            if (!expressionResult.TryGetValue(out SecondStageFunctionExpression? expression)) {
                return expressionResult.ToErrorResult();
            }

            state.AddFunction(expression);
        }
        
        IEnumerable<ExtendExpression> extendExpressions = inputTree.OfType<ExtendExpression>();
        foreach (ExtendExpression extend in extendExpressions) {
            Result<SecondStageFunctionExpression[]> expressionResult = ExtendPreTranspiler.Create(extend, state);
            if (!expressionResult.TryGetValue(out SecondStageFunctionExpression[]? expressions)) {
                return expressionResult.ToErrorResult();
            }

            foreach (SecondStageFunctionExpression expression in expressions) {
                state.AddFunction(expression);
            }
        }

        return Ok();
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
            SetExpression setExpression => SetPreTranspiler.Run(setExpression, state, false),
            IncrementExpression incrementExpression => IncrementPreTranspiler.Run(incrementExpression, state),
            FunctionExpression functionExpression => FunctionPreTranspiler.Replace(functionExpression, null, state),
            ExtendExpression extendExpression => ExtendPreTranspiler.Replace(extendExpression, state),
                
            IfExpression ifExpression => IfPreTranspiler.Run(ifExpression, state),
            ForExpression forExpression => ForPreTranspiler.Run(forExpression, state),
            WhileExpression whileExpression => WhilePreTranspiler.Run(whileExpression, state),
            
            FunctionCallSetExpression functionCallSetExpression => FunctionCallSetPreTranspiler.Run(functionCallSetExpression, state),
            FunctionCallExpression fc => FunctionCallPreTranspiler.Run(state, fc),
            ReturnExpression re => ReturnPreTranspiler.Run(re, state),

            AliasExpression aliasExpression => AliasPreTranspiler.Replace(aliasExpression, state),
            StructExpression structExpression => StructPreTranspiler.Replace(structExpression, state),
            StructSetExpression structSetExpression => NewStructPreTranspiler.Run(structSetExpression, state),
            
            _ => Ok(new[]{expression})
        };
    }
}