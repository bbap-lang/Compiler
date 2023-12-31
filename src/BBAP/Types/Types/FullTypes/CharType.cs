﻿namespace BBAP.Types.Types.FullTypes;

public record BaseCharType(IType? InheritsFrom) : IType {
    public string Name => Keywords.Char;

    public SupportedOperator SupportedOperators
        => SupportedOperator.Equals | SupportedOperator.NotEquals | SupportedOperator.Plus;

    public string DeclareKeyWord => "TYPE";
    public string AbapName => "C";
}

public record CharType(IType? InheritsFrom, long Length) : IType {
    public string Name => Keywords.Char;

    public SupportedOperator SupportedOperators
        => SupportedOperator.Equals | SupportedOperator.NotEquals | SupportedOperator.Plus;

    public string DeclareKeyWord => "TYPE";
    public string AbapName => $"C LENGTH {Length}";
}