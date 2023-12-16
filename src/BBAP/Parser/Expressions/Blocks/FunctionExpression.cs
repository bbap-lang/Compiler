﻿using System.Collections.Immutable;

namespace BBAP.Parser.Expressions.Blocks;

public record FunctionExpression(int Line,
    string Name,
    bool IsReadOnly,
    ImmutableArray<ParameterExpression> Parameters,
    ImmutableArray<TypeExpression> OutputTypes,
    ImmutableArray<IExpression> BlockContent) : IExpression;

public record StaticFunctionExpression(int Line,
    string Name,
    bool IsReadOnly,
    ImmutableArray<ParameterExpression> Parameters,
    ImmutableArray<TypeExpression> OutputTypes,
    ImmutableArray<IExpression> BlockContent) : FunctionExpression(Line, Name, IsReadOnly, Parameters, OutputTypes, BlockContent);