﻿using System.Diagnostics;

namespace BBAP.Types.Types.ParserTypes;

public record OnlyNameGenericType(string Name, OnlyNameType GenericType) : IType {
    public string AbapName => throw new UnreachableException();

    public IType? InheritsFrom => null;

    public SupportedOperator SupportedOperators => SupportedOperator.None;

    public string DeclareKeyWord => throw new UnreachableException();
}