﻿namespace BBAP.Parser.Expressions; 

public record AliasExpression(int Line, string Name, TypeExpression SourceType) : IExpression;