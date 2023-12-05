using System.Collections.Immutable;
using System.Text;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.PreTranspiler.Expressions.Sql;
using BBAP.PreTranspiler.SubPreTranspiler;
using BBAP.Results;
using BBAP.Transpiler.SubTranspiler;

namespace BBAP.Transpiler;

public class Transpiler {
    public Result<string> Run(ImmutableArray<IExpression> expressions) {
        var state = new TranspilerState();
        
        IEnumerable<StructExpression> structDeclarations = GetAllOfType<StructExpression>(expressions);
        IEnumerable<DeclareExpression> declarations = GetAllOfType<DeclareExpression>(expressions);

        state.Builder.AppendLine("\" --- THIS CODE IS AUTOMATICALLY GENERATED FROM BBAP ---");
        state.Builder.AppendLine();
        foreach (StructExpression structExpression in structDeclarations) {
            StructTranspiler.Run(structExpression, state);
        }
        state.Builder.AppendLine();
        state.Builder.Append("DATA:\t");
        state.Builder.AddIntend();
        foreach (DeclareExpression declaration in declarations) {
            state.Builder.Append(declaration.Variable.Variable.Name);
            state.Builder.Append(' ');
            TypeTranspiler.Run(declaration.Type, state.Builder);
            state.Builder.AppendLine();
        }
        state.Builder.RemoveIntend();

        state.Builder.AppendLine(".");

        state.Builder.AppendLine();
        state.Builder.AppendLine();
        
        TranspileBlock(expressions, state);
        
        return Ok(state.Builder.ToString());
    }

    public static void TranspileBlock(ImmutableArray<IExpression> expressions, TranspilerState state) {
        foreach (IExpression expression in expressions) {
            switch (expression) {
                case SetExpression setExpression:
                    SetTranspiler.Run(setExpression, state);
                    break;
                case DeclareExpression declareExpression:
                    if (declareExpression.SetExpression is null) {
                        break;
                    }
                    SetTranspiler.Run(declareExpression.SetExpression, state);
                    break;
                case WhileExpression whileExpression:
                    WhileTranspiler.Run(whileExpression, state);
                    break;
                case IfExpression ifExpression:
                    IfTranspiler.Run(ifExpression, state);
                    break;
                case SecondStageFunctionCallExpression functionCallExpression:
                    FunctionCallTranspiler.Run(functionCallExpression, state);
                    break;
                case SecondStageFunctionExpression functionExpression:
                    FunctionTranspiler.Run(functionExpression, state);
                    break;
                case SecondStageSelectExpression selectExpression:
                    SelectTranspiler.Run(selectExpression, state);
                    break;
                case AliasExpression aliasExpression:
                    AliasTranspiler.Run(aliasExpression, state);
                    break;
                
                // expressions to skip
                case StructExpression:
                    
                    break;
                default:
                    state.Builder.Append("\" ");
                    state.Builder.AppendLine(expression.GetType().Name);
                    break;
            }
        }
    }
    
    
    
    private static IEnumerable<T> GetAllOfType<T>(ImmutableArray<IExpression> expressions) {
        foreach (IExpression expression in expressions) {
            switch (expression) {
                case T declareExpression:
                    yield return declareExpression;
                    break;

                case WhileExpression whileExpression: {
                    foreach (T decEx in GetAllOfType<T>(whileExpression.BlockContent)) {
                        yield return decEx;
                    }

                    break;
                }

                case IfExpression ifExpression: {
                    foreach (T decEx in GetAllOfType<T>(ifExpression.BlockContent)) {
                        yield return decEx;
                    }

                    break;
                }
            }
        }
    }
    
}