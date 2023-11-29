﻿using System.Text;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler;
using BBAP.Results;
using BBAP.Transpiler;
using BBAP.Types;

namespace BBAP.Functions.AbapFunctions; 

// ABAP: WRITE
public class Print: IFunction {

    public string Name => "PRINT";
    public IType SingleType => new UnknownType();
    public bool IsSingleTypeOutput => false;

    public bool IsMethod => false;

    public Result<int> Matches(IType[] inputs, IType[] outputs, int line) {
        if (outputs.Length != 0) {
            return Error(line, "'Print' has no return value.");
        }

        if (inputs.Length < 1) {
            return Error(line, "At least one parameter is required in the function call of 'Print'.");
        }
        
        foreach ((IType type, int index) in inputs.Select((t, i) =>(t, i))) {
            if (!type.IsCastableTo(TypeCollection.StringType)) {
                return Error(line, $"The parameter type {type.Name}  is not castable to {TypeCollection.StringType} in the function call of 'Print'.");
            }
        }
        
        return Ok();
    }

    public void Render(AbapBuilder builder, IEnumerable<VariableExpression> inputs, IEnumerable<VariableExpression> outputs) {
        foreach (VariableExpression input in inputs) {
            builder.Append("WRITE ");
            builder.Append(input.Variable.Name);
            builder.AppendLine(".");
        }
    }

    public IType? ReturnType => null;
}