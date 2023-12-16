using System.Collections.Immutable;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.PreTranspiler.Expressions;
using BBAP.PreTranspiler.Expressions.Sql;
using BBAP.PreTranspiler.Variables;
using BBAP.Results;
using BBAP.Transpiler.SubTranspiler;

namespace BBAP.Transpiler;

public class Transpiler {
    public Result<string> Run(ImmutableArray<IExpression> expressions) {
        var state = new TranspilerState();
        
        state.Builder.AppendLine("\" --- THIS CODE IS AUTOMATICALLY GENERATED FROM BBAP ---");
        state.Builder.AppendLine();

        TranspileBlock(expressions, state, true);

        return Ok(state.Builder.ToString());
    }

    public static void TranspileBlock(ImmutableArray<IExpression> expressions, TranspilerState state, bool includeDeclarations) {
        if (includeDeclarations) {
            WriteDeclarations(expressions, state);
        }

        foreach (IExpression expression in expressions) {
            switch (expression) {
                case SetExpression setExpression:
                    SetTranspiler.Run(setExpression, state);
                    break;
                case DeclareExpression declareExpression:
                    if (declareExpression.SetExpression is null) break;
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
                case InfiniteLoop infiniteLoop:
                    InfiniteLoopTranspiler.Run(infiniteLoop, state);
                    break;
                
                case BreakLoopExpression:
                    LoopAdditionTranspiler.RunBreak(state.Builder);
                    break;
                
                case ContinueLoopExpression:
                    LoopAdditionTranspiler.RunContinue(state.Builder);
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

    private static void WriteDeclarations(ImmutableArray<IExpression> expressions, TranspilerState state) {
        IEnumerable<StructExpression> structDeclarations = GetAllOfType<StructExpression>(expressions);
        IEnumerable<DeclareExpression> declarations = GetAllOfType<DeclareExpression>(expressions);
        foreach (StructExpression structExpression in structDeclarations) {
            StructTranspiler.Run(structExpression, state);
        }

        DeclareExpression[] constDeclarations = declarations.Where(x => x.Variable.Variable.MutabilityType == MutabilityType.Const).ToArray();
        DeclareExpression[] notConstDeclarations = declarations.Where(x => x.Variable.Variable.MutabilityType != MutabilityType.Const).ToArray();
        
        state.Builder.AppendLine();
        WriteDeclarations(state, notConstDeclarations, constDeclarations);

        state.Builder.AppendLine();
        state.Builder.AppendLine();
    }

    private static void WriteDeclarations(TranspilerState state,
        DeclareExpression[] notConstDeclarations,
        DeclareExpression[] constDeclarations) {

        if (notConstDeclarations.Length > 0) {
            state.Builder.Append("DATA:\t");
            state.Builder.AddIntend();
            foreach (DeclareExpression declaration in notConstDeclarations) {
                state.Builder.Append(declaration.Variable.Variable.Name);
                state.Builder.Append(' ');
                TypeTranspiler.Run(declaration.Type, state.Builder);
                state.Builder.AppendLine();
            }

            state.Builder.RemoveIntend();

            state.Builder.AppendLine(".");
        }

        if (constDeclarations.Length > 0) {
            state.Builder.AppendLine();
            state.Builder.Append("CONSTANTS:\t");
            state.Builder.AddIntend();
            foreach (DeclareExpression declaration in constDeclarations) {
                state.Builder.Append(declaration.Variable.Variable.Name);
                state.Builder.Append(' ');
                TypeTranspiler.Run(declaration.Type, state.Builder);
                state.Builder.AppendLine();
            }

            state.Builder.RemoveIntend();

            state.Builder.AppendLine(".");
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
                    foreach (T p in AddElseDeclarations<T>(ifExpression)) {
                        yield return p;
                    }

                    break;
                }
            }
        }
    }

    private static IEnumerable<T> AddElseDeclarations<T>(IfExpression ifExpression) {
        foreach (T decEx in GetAllOfType<T>(ifExpression.BlockContent)) {
            yield return decEx;
        }

        if(ifExpression.ElseExpression is IfExpression elseIfExpression) {
            foreach (T decEx in AddElseDeclarations<T>(elseIfExpression)) {
                yield return decEx;
            }
        }

        if (ifExpression.ElseExpression is ElseExpression elseExpression) {
            foreach (T decEx in GetAllOfType<T>(elseExpression.BlockContent)) {
                yield return decEx;
            }
        }
    }
}