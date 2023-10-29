using System.Collections.Immutable;
using BBAP.Parser.Expressions;
using BBAP.Types;

namespace BBAP.PreTranspiler.Expressions; 

public record SecondStageFunctionExpression(int Line,
    string Name,
    ImmutableArray<SecondStageParameterExpression> Parameters,
    ImmutableArray<TypeExpression> ReturnTypes,
    ImmutableArray<IExpression> ContentBlock, string StackName): IExpression;