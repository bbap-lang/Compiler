using System.Diagnostics;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.PreTranspiler.SubPreTranspiler;

public static class DeclarePreTranspiler {
    public static Result<IExpression[]> Run(DeclareExpression declareExpression, PreTranspilerState state) {
        ISecondStageValues? value = null;
        IType type;
        IExpression[] additionalExpressions = Array.Empty<IExpression>();
        Result<string> newVarResult;
        string? newVar;
        VariableExpression variableExpression;
        
        if (declareExpression.SetExpression is null) {
            if (declareExpression.Type.Type is UnknownType) {
                return Error(declareExpression.Line,
                    "The type must be defined for declarations without initial value.");
            }

            type = declareExpression.Type.Type;
            
            newVarResult = state.CreateVar(declareExpression.Variable.Name, type, declareExpression.Line);

            if (!newVarResult.TryGetValue(out newVar)) {
                return newVarResult.ToErrorResult();
            }


            variableExpression = new VariableExpression(declareExpression.Line, newVar);
            DeclareExpression newDeclareExpressionWithoutSet = declareExpression with {
                 Variable = variableExpression
            };

            return Ok(new IExpression[]{newDeclareExpressionWithoutSet});
        } else {
            Result<IExpression[]> splittedValueResult = ValueSplitter.Run(state, declareExpression.SetExpression.Value);
            if (!splittedValueResult.TryGetValue(out IExpression[]? splittedValue)) {
                return splittedValueResult;
            }

            IExpression lastValue = splittedValue.Last();

            if (lastValue is not ISecondStageValues tmpValue) {
                throw new UnreachableException();
            }

            value = tmpValue;
            type = value.Type;
            additionalExpressions = splittedValue;
        }

        newVarResult = state.CreateVar(declareExpression.Variable.Name, type, declareExpression.Line);

        if (!newVarResult.TryGetValue(out newVar)) {
            return newVarResult.ToErrorResult();
        }


        variableExpression = new VariableExpression(declareExpression.Line, newVar);
        
        SetExpression setExpression = declareExpression.SetExpression with {
            Value = value, Variable = variableExpression
        };
        var typeExpression = new TypeExpression(value.Line, type);
        DeclareExpression newDeclareExpression = declareExpression with {
            SetExpression = setExpression, Type = typeExpression, Variable = variableExpression
        };

        IExpression[] newExpressions = additionalExpressions.Remove(value).Append(newDeclareExpression).ToArray();

        return Ok(newExpressions);
    }

    public static IExpression[] RemoveDeclarations(IExpression[] expressions) {
        var newExpressions = new IExpression[expressions.Length];

        for (int i = 0; i < expressions.Length; i++) {
            IExpression expression = expressions[i];
            if (expression is not DeclareExpression declareExpression) {
                newExpressions[i] = expression;
                continue;
            }
            if (declareExpression.SetExpression is null) {
                continue;
            }
            
            newExpressions[i] = declareExpression.SetExpression;
        }
        
        return newExpressions;
    }
}