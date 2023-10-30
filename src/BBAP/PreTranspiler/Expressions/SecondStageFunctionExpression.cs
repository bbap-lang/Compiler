﻿using System.Collections.Immutable;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.Types;

namespace BBAP.PreTranspiler.Expressions; 

public record SecondStageFunctionExpression(int Line,
    string Name,
    ImmutableArray<Variable> Parameters,
    ImmutableArray<Variable> ReturnVariables,
    ImmutableArray<IExpression> ContentBlock, string StackName): IExpression;