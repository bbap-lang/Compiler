﻿using System.Text;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler;
using BBAP.Results;
using BBAP.Transpiler;
using BBAP.Types;

namespace BBAP.Functions; 

public interface IFunction {
    public Result<int> Matches(IType[] inputs, IType[] outputs, int line);
    public string Name { get; }
    public IType SingleType { get; }
    public bool IsSingleTypeOutput { get; }
    public FunctionAttributes Attributes { get; }
    
    public bool IsMethod => (Attributes & FunctionAttributes.Method) == FunctionAttributes.Method;
    public bool IsStatic => (Attributes & FunctionAttributes.Static) == FunctionAttributes.Static;

    public void Render(AbapBuilder builder, IEnumerable<VariableExpression> inputs, IEnumerable<VariableExpression> outputs);
}

[Flags]
public enum FunctionAttributes {
    None = 0,
    Method = 1 << 0,
    Static = 1 << 1,
}

public static class FunctionAttributesExtension {
    public static bool Is(this FunctionAttributes attributes, FunctionAttributes flag) => (attributes & flag) == flag;
}