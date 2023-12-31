﻿using System.Collections.Immutable;
using BBAP.Functions;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;

namespace BBAP.PreTranspiler.Expressions;

public record SecondStageFunctionExpression(int Line,
    string Name,
    ImmutableArray<VariableExpression> Parameters,
    ImmutableArray<VariableExpression> ReturnVariables,
    ImmutableArray<IExpression> ContentBlock,
    string StackName,
    FunctionAttributes Attributes) : IExpression;