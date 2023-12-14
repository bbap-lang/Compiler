using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.PreTranspiler.Variables;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.PreTranspiler.SubPreTranspiler;

public class SwitchPreTranspiler {
    public static Result<IExpression[]> Run(SwitchExpression switchExpression, PreTranspilerState state) {
        Result<IVariable> inputVariableResult = state.GetVariable(switchExpression.Variable.Variable, switchExpression.Line);
        if (!inputVariableResult.TryGetValue(out IVariable? inputVariable)) return inputVariableResult.ToErrorResult();
        VariableExpression variableExpression = switchExpression.Variable with { Variable = inputVariable };
        var variableValue = new SecondStageValueExpression(variableExpression.Line,
                                                           new TypeExpression(switchExpression.Line,
                                                                              inputVariable.Type), variableExpression);

        Result<IType> booleanTypeRes = state.Types.Get(0, Keywords.Boolean);
        if (!booleanTypeRes.TryGetValue(out IType? booleanType)) throw new UnreachableException();
        
        List<IfExpression> ifExpressions = new();
        foreach (CaseExpression caseExpression in switchExpression.Cases) {
            Result<ImmutableArray<IExpression>>
                newBlockResult = PreTranspiler.RunBlock(state, caseExpression.CaseBlock);
            if (!newBlockResult.TryGetValue(out ImmutableArray<IExpression> newBlock))
                return newBlockResult.ToErrorResult();

            Result<IExpression[]> valueResult = ValueSplitter.Run(state, caseExpression.Condition, true);
            if (!valueResult.TryGetValue(out IExpression[]? values)) return valueResult.ToErrorResult();
            if (values.Length != 1) return Error(caseExpression.Line, "Switch case condition must be a single value.");
            IExpression valueGeneral = values[0];
            if (valueGeneral is not ISecondStageValue value)
                return Error(caseExpression.Line, "Switch case condition must be a value.");
            if (!value.Type.Type.IsCastableTo(inputVariable.Type))
                return Error(caseExpression.Line, "Switch case condition must be the same type as the input variable.");

            var equalsExpression = new SecondStageCalculationExpression(value.Line,
                                                                        new TypeExpression(value.Line, booleanType),
                                                                        SecondStageCalculationType.Equals, value,variableValue );
            
            var nextIfExpression = new IfExpression(caseExpression.Line, equalsExpression, newBlock, null);

            ifExpressions.Add(nextIfExpression);
        }

        if (switchExpression.DefaultCase is not null) {
            Result<ImmutableArray<IExpression>> newBlockResult
                = PreTranspiler.RunBlock(state, switchExpression.DefaultCase.Value);
            if (!newBlockResult.TryGetValue(out ImmutableArray<IExpression> newBlock))
                return newBlockResult.ToErrorResult();

            if (ifExpressions.Count == 0) {
                return Ok(newBlock.ToArray());
            }

            if (newBlock.Length > 0) {
                var elseExpression = new ElseExpression(newBlock[0].Line, newBlock);
                ifExpressions[^1] = ifExpressions[^1] with { ElseExpression = elseExpression };
            }
        }

        for (int i = ifExpressions.Count - 2; i >= 0; i--) {
            ifExpressions[i] = ifExpressions[i] with { ElseExpression = ifExpressions[i + 1] };
        }

        return Ok(new IExpression[] { ifExpressions[0] });
    }
}