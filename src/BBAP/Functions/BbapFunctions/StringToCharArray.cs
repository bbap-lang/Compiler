﻿using BBAP.Parser.Expressions.Values;
using BBAP.Results;
using BBAP.Transpiler;
using BBAP.Transpiler.SubTranspiler;
using BBAP.Types;

namespace BBAP.Functions.BbapFunctions; 

public class StringToCharArray: IFunction{
    public Result<int> Matches(IType[] inputs, IType[] outputs, int line) {
        IType? output = outputs.FirstOrDefault();
        IType input = inputs.First();

        if (output is not null && !TypeCollection.BaseCharType.IsCastableTo(output)) {
            return Error(line, "The return type is not castable to CHAR in the function call of 'String.ToCharArray'.");
        }

        if (!input.IsCastableTo(TypeCollection.StringType)) {
            return Error(line, "The parameter is not castable to STRING in the function call of 'String.ToCharArray'.");
        }

        return Ok();
    }

    public string Name => "STRING_TOCHARARRAY";

    public IType SingleType => TypeCollection.BaseCharType;

    public bool IsSingleTypeOutput => true;

    public bool IsMethod => true;

    public void Render(AbapBuilder builder, IEnumerable<VariableExpression> inputs, IEnumerable<VariableExpression> outputs) {
        VariableExpression? output = outputs.FirstOrDefault();
        VariableExpression input = inputs.First();

        if (output is null) {
            return;
        }
        
        VariableTranspiler.Run(output, builder);
        builder.Append(" = ");
        VariableTranspiler.Run(input, builder);
        builder.AppendLine(".");
    }
}