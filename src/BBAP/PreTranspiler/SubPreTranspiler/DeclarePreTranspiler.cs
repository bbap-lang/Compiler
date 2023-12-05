﻿using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.PreTranspiler.Variables;
using BBAP.Results;
using BBAP.Types;
using BBAP.Types.Types.ParserTypes;

namespace BBAP.PreTranspiler.SubPreTranspiler;

public static class DeclarePreTranspiler {
    public static Result<IExpression[]> Run(DeclareExpression declareExpression, PreTranspilerState state) {
        IType type;
        IExpression[] additionalExpressions = Array.Empty<IExpression>();
        Result<string> newVarResult;
        string? newVar;
        VariableExpression variableExpression;

        if (declareExpression.SetExpression is null) {
            if (declareExpression.Type.Type is UnknownType)
                return Error(declareExpression.Line,
                             "The type must be defined for declarations without initial value.");

            Result<IType> typeResult
                = TypePreTranspiler.Run(declareExpression.Type.Type, state, declareExpression.Type.Line);

            if (!typeResult.TryGetValue(out type)) return typeResult.ToErrorResult();

            newVarResult = state.CreateVar(declareExpression.Variable.Variable.Name, type, declareExpression.Line);

            if (!newVarResult.TryGetValue(out newVar)) return newVarResult.ToErrorResult();


            variableExpression = new VariableExpression(declareExpression.Line, new Variable(type, newVar));
            DeclareExpression newDeclareExpressionWithoutSet = declareExpression with {
                Variable = variableExpression, Type = declareExpression.Type with { Type = type }
            };

            return Ok(new IExpression[] { newDeclareExpressionWithoutSet });
        }

        Result<IExpression[]> splittedValueResult = SetPreTranspiler.Run(declareExpression.SetExpression, state, true);
        if (!splittedValueResult.TryGetValue(out IExpression[]? splittedValue)) return splittedValueResult;

        IExpression newSetExpressionUnknown = splittedValue.Last();

        if (newSetExpressionUnknown is SecondStageFunctionCallExpression funcCall) {
            Result<IType> newVarTypeResultFunc = GetTypeFromValue(declareExpression, funcCall, state);
            if (!newVarTypeResultFunc.TryGetValue(out type)) return newVarTypeResultFunc.ToErrorResult();

            newVarResult = state.CreateVar(declareExpression.Variable.Variable.Name, type, declareExpression.Line);

            if (!newVarResult.TryGetValue(out newVar)) return newVarResult.ToErrorResult();

            variableExpression = new VariableExpression(declareExpression.Line, new Variable(type, newVar));

            var emptyDeclare = new DeclareExpression(declareExpression.Line, variableExpression,
                                                     new TypeExpression(funcCall.Line, type), null);

            SecondStageFunctionCallExpression newFuncCall = funcCall with {
                Outputs = ImmutableArray.Create(variableExpression)
            };

            IExpression[] addExpressions = splittedValue.Concat(additionalExpressions).Remove(newSetExpressionUnknown)
                                                        .Append(emptyDeclare).Append(newFuncCall).ToArray();

            return Ok(addExpressions);
        }

        if (newSetExpressionUnknown is not SetExpression setExpression) throw new UnreachableException();

        if (setExpression.Value is not ISecondStageValue value) throw new UnreachableException();

        Result<IType> newVarTypeResult = GetTypeFromValue(declareExpression, value, state);
        if (!newVarTypeResult.TryGetValue(out type)) return newVarTypeResult.ToErrorResult();

        additionalExpressions = splittedValue;

        newVarResult = state.CreateVar(declareExpression.Variable.Variable.Name, type, declareExpression.Line);

        if (!newVarResult.TryGetValue(out newVar)) return newVarResult.ToErrorResult();

        variableExpression = new VariableExpression(declareExpression.Line, new Variable(type, newVar));

        var typeExpression = new TypeExpression(value.Line, type);

        SetExpression newSetExpression = setExpression with { Variable = variableExpression };

        DeclareExpression newDeclareExpression = declareExpression with {
            SetExpression = newSetExpression, Type = typeExpression, Variable = variableExpression
        };

        IExpression[] newExpressions
            = additionalExpressions.Remove(setExpression).Append(newDeclareExpression).ToArray();

        return Ok(newExpressions);
    }

    public static Result<IType> GetTypeFromValue(DeclareExpression declareExpression,
        ISecondStageValue value,
        PreTranspilerState state) {
        if (declareExpression.Type.Type is UnknownType) {
            return Ok(value.Type.Type);
        }

        Result<IType> typeResult
            = TypePreTranspiler.Run(declareExpression.Type.Type, state, declareExpression.Type.Line);

        if (!typeResult.TryGetValue(out IType? declaredType)) return typeResult.ToErrorResult();
        Result<int> typeCheckResult = SetPreTranspiler.CheckTypes(false, value, declaredType);
        if (!typeCheckResult.IsSuccess) return typeCheckResult.ToErrorResult();

        return Ok(declaredType);
    }

    public static IExpression[] RemoveDeclarations(IExpression[] expressions) {
        var newExpressions = new IExpression[expressions.Length];

        for (int i = 0; i < expressions.Length; i++) {
            IExpression expression = expressions[i];
            if (expression is not DeclareExpression declareExpression) {
                newExpressions[i] = expression;
                continue;
            }

            if (declareExpression.SetExpression is null) continue;

            newExpressions[i] = declareExpression.SetExpression;
        }

        return newExpressions;
    }
}