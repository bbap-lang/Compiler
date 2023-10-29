﻿using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.Types;

namespace BBAP.Transpiler.SubTranspiler; 

public static class FunctionTranspiler {
    public static void Run(SecondStageFunctionExpression functionExpression, TranspilerState state) {
        AbapBuilder builder = state.Builder;
        
        builder.AppendLine();
        builder.Append("FORM ");
        builder.Append(functionExpression.Name);

        if (functionExpression.Parameters.Length > 0) {
            builder.Append(" USING ");
            foreach ((_, VariableExpression? variable, IType? type) in functionExpression.Parameters) {
                builder.Append(variable.Name);
                builder.Append(" TYPE ");
                builder.Append(type.AbapName);
                builder.Append(' ');
            }
        }
        builder.AppendLine('.');
        
        builder.AddIntend();
        Transpiler.TranspileBlock(functionExpression.ContentBlock, state);
        builder.RemoveIntend();
        
        builder.AppendLine("ENDFORM.");
        builder.AppendLine();
    }
}