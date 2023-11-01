using System.Diagnostics;
using System.Globalization;
using System.Text;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;

namespace BBAP.Transpiler.SubTranspiler; 

public static class ValueTranspiler {
    public static void Run(IExpression expression, TranspilerState state) {
        switch (expression) {
            case SecondStageValueExpression ve:
                BuildValue(ve, state.Builder);
                break;
            
            case SecondStageCalculationExpression ce:
                BuildCalculation(ce, state);
                break;
        }
    }

    private static void BuildValue(SecondStageValueExpression ve, AbapBuilder builder) {
        switch (ve.Value) {
            case FloatExpression fe:
                builder.Append(fe.Value.ToString(CultureInfo.InvariantCulture));
                break;
            case IntExpression ie:
                builder.Append(ie.Value);
                break;
            case StringExpression se:
                builder.Append('\'');
                builder.Append(EscapeString(se.Value));
                builder.Append('\'');
                break;
            
            case VariableExpression vae:
                builder.Append(vae.Name);
                break;
        }
    }

    private static void BuildCalculation(SecondStageCalculationExpression ce, TranspilerState state) {
        state.Builder.Append('(');
        Run(ce.Left, state);
        state.Builder.Append(GetCalculationType(ce.CalculationType));
        Run(ce.Right, state);
        state.Builder.Append(')');
    }

    private static string GetCalculationType(SecondStageCalculationType type) {
        return type switch {
            SecondStageCalculationType.Plus => " + ",
            SecondStageCalculationType.Minus => " - ",
            SecondStageCalculationType.Multiply => " * ",
            SecondStageCalculationType.Divide => " / ",
            SecondStageCalculationType.Modulo => " MOD ",
            SecondStageCalculationType.BitwiseAnd => throw new NotImplementedException(),
            SecondStageCalculationType.BitwiseOr => throw new NotImplementedException(),

            SecondStageCalculationType.Equals => " EQ ",
            SecondStageCalculationType.NotEquals => " NE ",
            SecondStageCalculationType.GreaterThen => " GT ",
            SecondStageCalculationType.SmallerThen => " LT ",
            SecondStageCalculationType.GreaterThenOrEquals => " GE ",
            SecondStageCalculationType.SmallerThenOrEquals => " LE ",
            
            _ => throw new UnreachableException()
        };
    }
    
    private static string EscapeString(string input) {
        return input.Replace("\n", "\\n")
            .Replace("\t", "\\t")
            .Replace("\r", "\\r")
            .Replace("\0", "\\0")
            .Replace("\b", "\\b")
            .Replace("\\", @"\\")
            .Replace("\b", "\\b")
            .Replace("\b", "\\b")
            .Replace("\b", "\\b");

    }
}