using System.Diagnostics;
using System.Globalization;
using System.Text;
using BBAP.Lexer.Tokens.Values;
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
            
            case SecondStageNotExpression ne:
                BuildNot(ne, state);
                break;
            
            default:
                throw new UnreachableException();
        }
    }

    private static void BuildNot(SecondStageNotExpression expression, TranspilerState state) {
        state.Builder.Append("NOT ");
        Run(expression.InnerExpression, state);
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
            
            case BooleanValueExpression be:
                    builder.Append(be.Value ? "ABAP_TRUE" : "ABAP_FALSE");
                break;
            
            case VariableExpression vae:
                VariableTranspiler.Run(vae, builder);
                break;
            
            default:
                throw new UnreachableException();
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
            SecondStageCalculationType.BitwiseAnd => " BIT-AND ",
            SecondStageCalculationType.BitwiseOr => " BIT-OR ",

            SecondStageCalculationType.Equals => " EQ ",
            SecondStageCalculationType.NotEquals => " NE ",
            SecondStageCalculationType.GreaterThen => " GT ",
            SecondStageCalculationType.SmallerThen => " LT ",
            SecondStageCalculationType.GreaterThenOrEquals => " GE ",
            SecondStageCalculationType.SmallerThenOrEquals => " LE ",
            
            SecondStageCalculationType.And => " AND ",
            SecondStageCalculationType.Or => " OR ",
            SecondStageCalculationType.Xor => " XOR ",
            
            
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