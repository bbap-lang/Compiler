using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Calculations;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.Results;
using BBAP.Types;
using Error = BBAP.Results.Error;

namespace BBAP.PreTranspiler.SubPreTranspiler;

public static class ValueSplitter {
    public static Result<IExpression[]> Run(PreTranspilerState state, IExpression expression) {
        Result<IExpression> newExpressionResult = expression switch {
            FloatExpression => CreateExpression(state, expression.Line, Keywords.Float, expression),
            IntExpression => CreateExpression(state, expression.Line, Keywords.Int, expression),
            NegativeExpression => throw new NotImplementedException(), // needs to be splitted up
            NotExpression => throw new NotImplementedException(),
            StringExpression => CreateExpression(state, expression.Line, Keywords.String, expression),

            VariableExpression ve => CreateVarExpression(state, ve),

            _ => Error(new TemporaryError()),
        };

        if (newExpressionResult.TryGetValue(out IExpression newExpression)) {
            return Ok(new[] { newExpression });
        }

        if (newExpressionResult.Error is not TemporaryError) {
            return newExpressionResult.ToErrorResult();
        }

        Result<IExpression[]> newExpressionsResult = expression switch {
            MathCalculationExpression mathEx => SplitMathCalculation(state, mathEx),
            ComparisonExpression comparisonEx => ComparisonPreTranspiler.Run(state, comparisonEx),
            // BooleanExpression booleanExpression => BooleanPreTranspiler.Run(state, booleanExpression),

            FunctionCallExpression fc => FunctionCallPreTranspiler.Run(state, fc),

            _ => throw new UnreachableException(),
        };

        return newExpressionsResult;
    }

    private static Result<IExpression> CreateVarExpression(PreTranspilerState state, VariableExpression expression) {
        string varName = expression.Name;
        Result<Variable> variableResult = state.GetVariable(varName, expression.Line);

        if (!variableResult.TryGetValue(out Variable variable)) {
            return variableResult.ToErrorResult();
        }

        var newVariableExpression = new VariableExpression(expression.Line, variable.Name, variable.Type);
        return CreateExpression(state, expression.Line, variable.Type.Name, newVariableExpression);
    }

    private static Result<IExpression[]> SplitMathCalculation(PreTranspilerState state,
        MathCalculationExpression expression) {
        SecondStageCalculationType calculationType = expression.CalculationType switch {
            CalculationType.Plus => SecondStageCalculationType.Plus,
            CalculationType.Minus => SecondStageCalculationType.Minus,
            CalculationType.Multiply => SecondStageCalculationType.Multiply,
            CalculationType.Divide => SecondStageCalculationType.Divide,
            CalculationType.Modulo => SecondStageCalculationType.Modulo,
            CalculationType.BitwiseAnd => SecondStageCalculationType.BitwiseAnd,
            CalculationType.BitwiseOr => SecondStageCalculationType.BitwiseOr,

            _ => throw new UnreachableException()
        };

        Result<IExpression[]> leftResult = Run(state, expression.Left);
        if (!leftResult.TryGetValue(out IExpression[]? left)) {
            return leftResult;
        }

        Result<IExpression[]> rightResult = Run(state, expression.Right);
        if (!rightResult.TryGetValue(out IExpression[]? right)) {
            return rightResult;
        }

        IExpression lastLeft = left.Last();
        IExpression lastRight = right.Last();

        Result<(bool NeedsSplit, IType NewType)> needsSplitResult = NeedsSplit(lastLeft, lastRight, state);

        if(!needsSplitResult.TryGetValue(out (bool NeedsSplit, IType NewType) needsSplitTuple)) {
            return needsSplitResult.ToErrorResult();
        }
        
        (bool needsSplit, IType newType) = needsSplitTuple;

        if (!newType.SupportsOperator(calculationType.ToSupportedOperator())) {
            return Error(lastLeft.Line, $"Type '{newType.Name}' does not support operator '{calculationType}'.");
        }

        IExpression leftValue = lastLeft;
        IExpression rightValue = lastRight;
        IEnumerable<IExpression> combined = left.Concat(right)
                                                .Remove(lastLeft)
                                                .Remove(lastRight);
        
        
        if (needsSplit) {
            if (lastLeft is SecondStageFunctionCallExpression leftFunc) {
                Result<(IExpression Value, IExpression Declaration)> extractedMethodResult = ExtractFunctionCall(state, leftFunc);
                if(!extractedMethodResult.TryGetValue(out (IExpression Value, IExpression Declaration) extractedMethod)) {
                    return extractedMethodResult.ToErrorResult();
                }

                (IExpression newValue, IExpression callDeclaration) = extractedMethod;

                combined.Append(callDeclaration);
                leftValue = newValue;
            }
            
            if (lastRight is SecondStageFunctionCallExpression rightFunc) {
                Result<(IExpression Value, IExpression Declaration)> extractedMethodResult = ExtractFunctionCall(state, rightFunc);
                if(!extractedMethodResult.TryGetValue(out (IExpression Value, IExpression Declaration) extractedMethod)) {
                    return extractedMethodResult.ToErrorResult();
                }

                (IExpression newValue, IExpression callDeclaration) = extractedMethod;

                combined.Append(callDeclaration);
                rightValue = newValue;
            }
            
            if (lastLeft is SecondStageCalculationExpression leftCalc) {
                (DeclareExpression leftDeclare, SecondStageValueExpression leftVar) = ExtractVariable(state, expression, leftCalc.Type, lastLeft);
                combined = combined.Append(leftDeclare);
                leftValue = leftVar;
            }

            if (lastRight is SecondStageCalculationExpression rightCalc) {
                (DeclareExpression rightDeclare, SecondStageValueExpression rightVar) = ExtractVariable(state, expression, rightCalc.Type, lastRight);
                combined = combined.Append(rightDeclare);
                rightValue = rightVar;
            }

            var newExpression = new SecondStageCalculationExpression(expression.Line, newType, calculationType, leftValue, rightValue);

            IExpression[] combinedArray = combined.Append(newExpression)
                .ToArray();

            return Ok(combinedArray);
        }


        var combinedExpression
            = new SecondStageCalculationExpression(expression.Line, newType, calculationType, lastLeft, lastRight);

        return Ok(left.Concat(right)
            .Remove(lastLeft)
            .Remove(lastRight)
            .Append(combinedExpression)
            .ToArray());
    }

    private static Result<(IExpression Value, IExpression Declaration)> ExtractFunctionCall(PreTranspilerState state, SecondStageFunctionCallExpression functionCall) {
        if (!functionCall.Function.IsSingleType) {
            return Error(functionCall.Line, "Calculations with function calls that return more then one value are not supported.");
        }

        VariableExpression returnVarEx = functionCall.Outputs.First();
        Result<Variable> returnVarResult = state.GetVariable(returnVarEx.Name, returnVarEx.Line);
        if (!returnVarResult.TryGetValue(out Variable returnVar)) {
            throw new UnreachableException();
        }

        VariableExpression internalVar = state.CreateRandomNewVar(functionCall.Line, functionCall.Type);

        IExpression newFunctionCall = functionCall with { Outputs = ImmutableArray.Create(internalVar) };
        IExpression value = new SecondStageValueExpression(functionCall.Line, returnVar.Type, internalVar);

        return Ok((value, functionCall: newFunctionCall));
    }

    private static ( DeclareExpression Declaration, SecondStageValueExpression Value) ExtractVariable(
        PreTranspilerState state,
        MathCalculationExpression expression,
        IType NewType,
        IExpression lastLeft) {
        var typeExpression = new TypeExpression(expression.Line, NewType);
        var leftVar = state.CreateRandomNewVar(expression.Line, NewType);
        var leftSet = new SetExpression(lastLeft.Line, leftVar, SetType.Generic, lastLeft);
        var newDeclareExpression = new DeclareExpression(expression.Line, leftVar, typeExpression, leftSet);

        var newValueExpression = new SecondStageValueExpression(expression.Line, NewType, leftVar);

        return (newDeclareExpression, newValueExpression);
    }


    private static Result<(bool NeedsSplit, IType NewType)> NeedsSplit(IExpression left,
        IExpression right,
        PreTranspilerState state) {

        bool isFunction = left is SecondStageFunctionCallExpression || right is SecondStageFunctionCallExpression;
        
        IType leftType = GetExpressionType(left);
        IType rightType = GetExpressionType(right);

        IType typeString = ForceGetType("STRING", state);
        
        if (leftType.IsCastableTo(rightType)) {
            return Ok((isFunction || rightType == typeString, rightType));
        }

        if (rightType.IsCastableTo(leftType)) {
            return Ok((isFunction || leftType == typeString, leftType));
        }

        return Error(right.Line, $"Type '{rightType.Name}' is not castable to '{leftType.Name}'.");
    }

    private static IType ForceGetType(string typeName, PreTranspilerState state) {
        var typeResult = state.Types.Get(0, typeName);
        if (!typeResult.TryGetValue(out IType type)) {
            throw new UnreachableException();
        }

        return type;
    }

    private static int AnyType(string typeName, PreTranspilerState state, params IType[] types) {
        var type = ForceGetType(typeName, state);

        int typesMatch = types.Count(t => t == type);

        return typesMatch;
    }

    private static IType GetExpressionType(IExpression last) {
        return last switch {
            SecondStageCalculationExpression mathEx => mathEx.Type,
            SecondStageValueExpression valueEx => valueEx.Type,
            _ => throw new UnreachableException()
        };
    }

    private static Result<IExpression> CreateExpression(PreTranspilerState state,
        int line,
        string typeName,
        IExpression value) {
        var typeResult = state.Types.Get(line, typeName);
        if (!typeResult.TryGetValue(out IType type)) {
            return typeResult.ToErrorResult();
        }

        return Ok<IExpression>(new SecondStageValueExpression(line, type, value));
    }

    private record TemporaryError() : Error(0, string.Empty);
}