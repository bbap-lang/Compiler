using System.Collections.Immutable;
using BBAP.Functions;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;

namespace BBAP.PreTranspiler.Expressions;

// public record FunctionCallExpression(int Line, string Name, ImmutableArray<IExpression> Parameters): IExpression;
public record SecondStageFunctionCallExpression(int Line,
    IFunction Function,
    ImmutableArray<SecondStageParameterExpression> Parameters,
    ImmutableArray<VariableExpression> Outputs) : ISecondStageValue {
    public TypeExpression Type => new(Line, Function.GetSingleType(Parameters.Select(x => x.Type.Type).ToArray()));
}