using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.Functions;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Parser.Expressions.Calculations;
using BBAP.Parser.Expressions.Sql;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.PreTranspiler.Expressions.Sql;
using BBAP.PreTranspiler.Variables;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.PreTranspiler.SubPreTranspiler;

public static class ValueSplitter {
    public static Result<IExpression[]> Run(PreTranspilerState state,
        IExpression expression,
        bool isInCondition = false) {
        IType typeBool = ForceGetType(Keywords.Boolean, state);

        Result<IExpression> newExpressionResult = expression switch {
            FloatExpression => CreateExpression(state, expression.Line, Keywords.Float, expression),
            IntExpression => CreateExpression(state, expression.Line, Keywords.Int, expression),
            StringExpression => CreateExpression(state, expression.Line, Keywords.String, expression),
            BooleanValueExpression => CreateExpression(state, expression.Line, Keywords.Boolean, expression),

            VariableExpression ve => CreateVarExpression(state, ve),

            _ => Error(new TemporaryError())
        };

        if (newExpressionResult.TryGetValue(out IExpression newExpression)) {
            if (newExpression is not ISecondStageValue newValue) throw new UnreachableException();

            if (isInCondition && newValue.Type.Type.IsCastableTo(typeBool)) {
                var trueExpression
                    = new SecondStageValueExpression(newValue.Line, new TypeExpression(newValue.Line, typeBool),
                                                     new BooleanValueExpression(newValue.Line, true));
                newValue = new SecondStageCalculationExpression(newValue.Line,
                                                                new TypeExpression(newValue.Line, typeBool),
                                                                SecondStageCalculationType.Equals, newValue,
                                                                trueExpression);
            }

            return Ok<IExpression[]>(new[] { newValue });
        }

        if (newExpressionResult.Error is not TemporaryError) return newExpressionResult.ToErrorResult();

        Result<IExpression[]> newExpressionsResult = expression switch {
            MathCalculationExpression mathEx => SplitMathCalculation(state, mathEx),
            ComparisonExpression comparisonEx => ComparisonPreTranspiler.Run(state, comparisonEx),
            BooleanExpression booleanExpression => BooleanPreTranspiler.Run(state, booleanExpression),
            NotExpression notExpression => CreateNotExpression(state, notExpression),
            NegativeExpression negativeExpression => CreateNegativeExpression(state, negativeExpression),

            FunctionCallExpression fc => FunctionCallPreTranspiler.Run(state, fc),

            SqlSelectExpression sqlSelectExpression => SqlPreTranspiler.RunSelect(sqlSelectExpression, state),

            _ => throw new UnreachableException()
        };

        if (!newExpressionsResult.TryGetValue(out IExpression[]? newExpressions)) return newExpressionsResult;

        IExpression lastExpression = newExpressions.Last();
        if (lastExpression is SecondStageSelectExpression) return Ok(newExpressions);

        if (lastExpression is not ISecondStageValue lastValue) throw new UnreachableException();

        IType lastType = lastValue.Type.Type;

        if (!isInCondition && lastType.IsCastableTo(typeBool)) {
            IEnumerable<IExpression> boolExpressions = CreateBoolExpression(lastValue, state);

            newExpressions = newExpressions.Remove(lastExpression).Concat(boolExpressions).ToArray();
        }

        return Ok(newExpressions);
    }

    private static Result<IExpression[]> CreateNegativeExpression(PreTranspilerState state,
        NegativeExpression expression) {
        Result<IExpression[]> innerExpressionResult = Run(state, expression.InnerExpression);
        if (!innerExpressionResult.TryGetValue(out IExpression[]? allExpressions))
            return innerExpressionResult.ToErrorResult();

        IExpression lastExpression = allExpressions.Last();

        if (lastExpression is not ISecondStageValue innerValue) throw new UnreachableException();

        Result<IType> doubleTypeResult = state.Types.Get(innerValue.Line, Keywords.Double);
        if (!doubleTypeResult.TryGetValue(out IType? doubleType)) return doubleTypeResult.ToErrorResult();

        if (!innerValue.Type.Type.IsCastableTo(doubleType))
            return Error(expression.Line, $"Type '{innerValue.Type.Type.Name}' is not a number.");

        var zeroValue
            = new SecondStageValueExpression(expression.Line, innerValue.Type, new IntExpression(expression.Line, 0));

        var newExpression = new SecondStageCalculationExpression(expression.Line, innerValue.Type,
                                                                 SecondStageCalculationType.Minus, zeroValue,
                                                                 innerValue);


        IExpression[] combined = allExpressions.Remove(lastExpression).Append(newExpression).ToArray();
        return Ok(combined);
    }

    private static Result<IExpression[]> CreateNotExpression(PreTranspilerState state, NotExpression expression) {
        Result<IExpression[]> innerExpressionResult = Run(state, expression.Inner, true);
        if (!innerExpressionResult.TryGetValue(out IExpression[]? allExpressions))
            return innerExpressionResult.ToErrorResult();

        IExpression lastExpression = allExpressions.Last();

        if (lastExpression is not ISecondStageValue innerValue) throw new UnreachableException();

        Result<IType> boolTypeResult = state.Types.Get(innerValue.Line, Keywords.Boolean);
        if (!boolTypeResult.TryGetValue(out IType? boolType)) return boolTypeResult.ToErrorResult();

        if (!innerValue.Type.Type.IsCastableTo(boolType))
            return Error(expression.Line, $"Type '{innerValue.Type.Type.Name}' is not a boolean.");


        var newExpression
            = new SecondStageNotExpression(expression.Line, new TypeExpression(expression.Line, boolType), innerValue);

        IExpression[] combined = allExpressions.Remove(lastExpression).Append(newExpression).ToArray();
        return Ok(combined);
    }

    private static IEnumerable<IExpression>
        CreateBoolExpression(ISecondStageValue lastValue, PreTranspilerState state) {
        VariableExpression newVar = state.CreateRandomNewVar(lastValue.Line, lastValue.Type.Type);
        var setTrueExpression = new SetExpression(lastValue.Line, newVar, SetType.Generic,
                                                  new SecondStageValueExpression(lastValue.Line, lastValue.Type,
                                                   new BooleanValueExpression(lastValue.Line, true)));
        var setFalseExpression = new SetExpression(lastValue.Line, newVar, SetType.Generic,
                                                   new SecondStageValueExpression(lastValue.Line, lastValue.Type,
                                                    new BooleanValueExpression(lastValue.Line, false)));

        var elseExpression = new ElseExpression(lastValue.Line, ImmutableArray.Create<IExpression>(setFalseExpression));
        var ifExpression = new IfExpression(lastValue.Line, lastValue,
                                            ImmutableArray.Create<IExpression>(setTrueExpression), elseExpression);

        yield return ifExpression;
        yield return new SecondStageValueExpression(lastValue.Line, lastValue.Type, newVar);
    }

    private static Result<IExpression> CreateVarExpression(PreTranspilerState state, VariableExpression expression) {
        Result<IVariable> variableResult = state.GetVariable(expression.Variable, expression.Line);

        if (!variableResult.TryGetValue(out IVariable variable)) return variableResult.ToErrorResult();

        VariableExpression newVariableExpression = expression with { Variable = variable };
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
        if (!leftResult.TryGetValue(out IExpression[]? left)) return leftResult;

        Result<IExpression[]> rightResult = Run(state, expression.Right);
        if (!rightResult.TryGetValue(out IExpression[]? right)) return rightResult;

        IExpression lastLeft = left.Last();
        IExpression lastRight = right.Last();

        Result<(bool NeedsSplit, IType NewType)> needsSplitResult = NeedsSplit(lastLeft, lastRight, state);

        if (!needsSplitResult.TryGetValue(out (bool NeedsSplit, IType NewType) needsSplitTuple))
            return needsSplitResult.ToErrorResult();

        (bool needsSplit, IType newType) = needsSplitTuple;

        if (!newType.SupportsOperator(calculationType.ToSupportedOperator()))
            return Error(lastLeft.Line, $"Type '{newType.Name}' does not support operator '{calculationType}'.");

        IExpression leftValue = lastLeft;
        IExpression rightValue = lastRight;
        List<IExpression> combined = left.Concat(right)
                                         .Remove(lastLeft)
                                         .Remove(lastRight)
                                         .ToList();


        if (needsSplit) {
            if (lastLeft is SecondStageFunctionCallExpression leftFunc) {
                Result<(IExpression Value, IExpression Declaration)> extractedMethodResult
                    = ExtractFunctionCall(state, leftFunc);
                if (!extractedMethodResult
                        .TryGetValue(out (IExpression Value, IExpression Declaration) extractedMethod))
                    return extractedMethodResult.ToErrorResult();

                (IExpression newValue, IExpression callDeclaration) = extractedMethod;

                combined.Add(callDeclaration);
                leftValue = newValue;
            }

            if (lastRight is SecondStageFunctionCallExpression rightFunc) {
                Result<(IExpression Value, IExpression Declaration)> extractedMethodResult
                    = ExtractFunctionCall(state, rightFunc);
                if (!extractedMethodResult
                        .TryGetValue(out (IExpression Value, IExpression Declaration) extractedMethod))
                    return extractedMethodResult.ToErrorResult();

                (IExpression newValue, IExpression callDeclaration) = extractedMethod;

                combined.Add(callDeclaration);
                rightValue = newValue;
            }

            if (lastLeft is SecondStageCalculationExpression leftCalc) {
                (DeclareExpression leftDeclare, SecondStageValueExpression leftVar)
                    = ExtractVariable(state, expression, leftCalc.Type.Type, lastLeft);

                combined.Add(leftDeclare);
                leftValue = leftVar;
            }

            if (lastRight is SecondStageCalculationExpression rightCalc) {
                (DeclareExpression rightDeclare, SecondStageValueExpression rightVar)
                    = ExtractVariable(state, expression, rightCalc.Type.Type, lastRight);

                combined.Add(rightDeclare);
                rightValue = rightVar;
            }

            ISecondStageValue newExpression;
            if (newType == TypeCollection.StringType) {
                Result<IFunction> concatFunctionResult = state.GetFunction("CONCATENATE", expression.Line);
                if (!concatFunctionResult.TryGetValue(out IFunction? concatFunction)) throw new UnreachableException();

                VariableExpression newVar = state.CreateRandomNewVar(expression.Line, TypeCollection.StringType);
                if (leftValue is not SecondStageValueExpression leftValueExpression
                 || leftValueExpression.Value is not VariableExpression leftVariableExpression)
                    throw new UnreachableException();

                if (rightValue is not SecondStageValueExpression rightValueExpression
                 || rightValueExpression.Value is not VariableExpression rightVariableExpression)
                    throw new UnreachableException();

                var leftParameter = new SecondStageParameterExpression(leftVariableExpression.Line,
                                                                       leftVariableExpression,
                                                                       leftValueExpression.Type);


                var rightParameter = new SecondStageParameterExpression(rightVariableExpression.Line,
                                                                        rightVariableExpression,
                                                                        rightValueExpression.Type);

                newExpression = new SecondStageFunctionCallExpression(expression.Line, concatFunction,
                                                                      ImmutableArray.Create(leftParameter,
                                                                       rightParameter),
                                                                      ImmutableArray.Create(newVar));
            } else {
                newExpression
                    = new SecondStageCalculationExpression(expression.Line,
                                                           new TypeExpression(expression.Line, newType),
                                                           calculationType, leftValue,
                                                           rightValue);
            }

            combined.Add(newExpression);
            IExpression[] combinedArray = combined.ToArray();


            return Ok(combinedArray);
        }

        ISecondStageValue combinedExpression
            = new SecondStageCalculationExpression(expression.Line, new TypeExpression(expression.Line, newType),
                                                   calculationType, lastLeft, lastRight);


        combined.Add(combinedExpression);

        return Ok(combined.ToArray());
    }

    private static Result<(IExpression Value, IExpression Declaration)> ExtractFunctionCall(PreTranspilerState state,
        SecondStageFunctionCallExpression functionCall) {
        if (!functionCall.Function.IsSingleTypeOutput)
            return Error(functionCall.Line,
                         "Calculations with function calls that return more then one value are not supported.");

        VariableExpression returnVarEx = functionCall.Outputs.First();
        Result<IVariable> returnVarResult = state.GetVariable(returnVarEx.Variable.Name, returnVarEx.Line);
        if (!returnVarResult.TryGetValue(out IVariable returnVar)) throw new UnreachableException();

        VariableExpression internalVar = state.CreateRandomNewVar(functionCall.Line, functionCall.Type.Type);

        IExpression newFunctionCall = functionCall with { Outputs = ImmutableArray.Create(internalVar) };
        IExpression value = new SecondStageValueExpression(functionCall.Line,
                                                           new TypeExpression(functionCall.Line, returnVar.Type),
                                                           internalVar);

        return Ok((value, functionCall: newFunctionCall));
    }

    private static ( DeclareExpression Declaration, SecondStageValueExpression Value) ExtractVariable(
        PreTranspilerState state,
        MathCalculationExpression expression,
        IType NewType,
        IExpression lastLeft) {
        var typeExpression = new TypeExpression(expression.Line, NewType);
        VariableExpression leftVar = state.CreateRandomNewVar(expression.Line, NewType);
        var leftSet = new SetExpression(lastLeft.Line, leftVar, SetType.Generic, lastLeft);
        var newDeclareExpression = new DeclareExpression(expression.Line, leftVar, typeExpression, leftSet);

        var newValueExpression
            = new SecondStageValueExpression(expression.Line, new TypeExpression(expression.Line, NewType), leftVar);

        return (newDeclareExpression, newValueExpression);
    }


    private static Result<(bool NeedsSplit, IType NewType)> NeedsSplit(IExpression left,
        IExpression right,
        PreTranspilerState state) {
        bool isFunction = left is SecondStageFunctionCallExpression || right is SecondStageFunctionCallExpression;

        IType leftType = GetExpressionType(left);
        IType rightType = GetExpressionType(right);

        IType typeString = ForceGetType("STRING", state);

        if (leftType.IsCastableTo(rightType)) return Ok((isFunction || rightType == typeString, rightType));

        if (rightType.IsCastableTo(leftType)) return Ok((isFunction || leftType == typeString, leftType));

        return Error(right.Line, $"Type '{rightType.Name}' is not castable to '{leftType.Name}'.");
    }

    private static IType ForceGetType(string typeName, PreTranspilerState state) {
        Result<IType> typeResult = state.Types.Get(0, typeName);
        if (!typeResult.TryGetValue(out IType type)) throw new UnreachableException();

        return type;
    }

    private static IType GetExpressionType(IExpression last) {
        return last switch {
            SecondStageCalculationExpression mathEx => mathEx.Type.Type,
            SecondStageValueExpression valueEx => valueEx.Type.Type,
            _ => throw new UnreachableException()
        };
    }

    private static Result<IExpression> CreateExpression(PreTranspilerState state,
        int line,
        string typeName,
        IExpression value) {
        Result<IType> typeResult = state.Types.Get(line, typeName);
        if (!typeResult.TryGetValue(out IType type)) return typeResult.ToErrorResult();

        return Ok<IExpression>(new SecondStageValueExpression(line, new TypeExpression(line, type), value));
    }

    private record TemporaryError() : Error(0, string.Empty);
}