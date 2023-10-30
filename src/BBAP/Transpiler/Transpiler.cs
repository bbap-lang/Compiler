using System.Collections.Immutable;
using System.Text;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.PreTranspiler.SubPreTranspiler;
using BBAP.Results;
using BBAP.Transpiler.SubTranspiler;

namespace BBAP.Transpiler;

public class Transpiler {
    public Result<string> Run(ImmutableArray<IExpression> expressions) {
        var state = new TranspilerState();
        
        IEnumerable<DeclareExpression> declarations = GetAllDeclarations(expressions);

        state.Builder.AppendLine("\" --- THIS CODE IS AUTOMATICALLY GENERATED FROM BBAP ---");
        state.Builder.AppendLine();
        state.Builder.AppendLine();
        state.Builder.Append("DATA:\t");
        state.Builder.AddIntend();
        foreach (DeclareExpression declaration in declarations) {
            state.Builder.AppendLine($"{declaration.Variable.Name} TYPE {declaration.Type.Type.AbapName}");
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
                case DeclareExpression { SetExpression: not null } declareExpression:
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
                default:
                    state.Builder.Append("\" ");
                    state.Builder.AppendLine(expression.GetType().Name);
                    break;
            }
        }
    }
    
    private static IEnumerable<DeclareExpression> GetAllDeclarations(ImmutableArray<IExpression> expressions) {
        foreach (IExpression expression in expressions) {
            switch (expression) {
                case DeclareExpression declareExpression:
                    yield return declareExpression;
                    break;

                case WhileExpression whileExpression: {
                    foreach (DeclareExpression decEx in GetAllDeclarations(whileExpression.BlockContent)) {
                        yield return decEx;
                    }

                    break;
                }

                case IfExpression ifExpression: {
                    foreach (DeclareExpression decEx in GetAllDeclarations(ifExpression.BlockContent)) {
                        yield return decEx;
                    }

                    break;
                }
            }
        }
    }
}