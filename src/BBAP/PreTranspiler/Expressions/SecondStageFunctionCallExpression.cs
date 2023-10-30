using System.Collections.Immutable;
using BBAP.Functions;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.Types;

namespace BBAP.PreTranspiler.Expressions;

// public record FunctionCallExpression(int Line, string Name, ImmutableArray<IExpression> Parameters): IExpression;
public record SecondStageFunctionCallExpression(int Line,
    IFunction Function,
    ImmutableArray<VariableExpression> Parameters,
    ImmutableArray<VariableExpression> Outputs) : ISecondStageValue {
    public IType Type => Function.SingleType;
}