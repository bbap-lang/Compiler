using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Variables;
using BBAP.Results;
using BBAP.Types;
using BBAP.Types.Types.FullTypes;

namespace BBAP.PreTranspiler.SubPreTranspiler;

public class ForeachPreTranspiler {
    public static Result<IExpression[]> Run(ForeachExpression expression, PreTranspilerState state) {
        state.StackIn(StackType.Loop);
        var tableResult = state.GetVariable(expression.TableExpression.Variable, expression.TableExpression.Line);
        if (!tableResult.TryGetValue(out IVariable? table)) return tableResult.ToErrorResult();

        if (!table.Type.IsCastableTo(TypeCollection.AnyTableType))
            return Error(expression.TableExpression.Line, "Cannot iterate over a non-table type");

        TableType tableType;
        switch (table.Type) {
            case AliasType aliasType: {
                if (aliasType.GetRealType() is not TableType realTableType) throw new UnreachableException();

                tableType = realTableType;
                break;
            }
            case TableType realTableType:
                tableType = realTableType;
                break;
            default:
                throw new UnreachableException();
        }

        var expressions = new List<IExpression>();
        IVariable variable;
        if (expression.Declaration) {
            var fieldSymbolType = new FieldSymbolType(tableType.ContentType);
            Result<string> variableNameResult = state.CreateVar(expression.VariableExpression.Variable.Name,
                                                            fieldSymbolType,
                                                            MutabilityType.Immutable,
                                                            expression.VariableExpression.Line);
            if (!variableNameResult.TryGetValue(out string? variableName)) return variableNameResult.ToErrorResult();
            
            Result<IVariable> variableResult = state.GetVariable(variableName, expression.VariableExpression.Line);
            if (!variableResult.TryGetValue(out variable)) throw new UnreachableException();

            VariableExpression fieldSymbolExpression = expression.VariableExpression with { Variable = variable };
            var typeExpression = new TypeExpression(expression.VariableExpression.Line, fieldSymbolType);
            var declareExpression = new DeclareExpression(expression.VariableExpression.Line, fieldSymbolExpression, typeExpression, null, MutabilityType.Immutable);
            expressions.Add(declareExpression);
        } else {
            Result<IVariable> variableResult = state.GetVariable(expression.VariableExpression.Variable.Name, expression.VariableExpression.Line);
            if (!variableResult.TryGetValue(out variable)) return variableResult.ToErrorResult();
            
            if(variable.Type is not FieldSymbolType fieldSymbolType)
                return Error(expression.VariableExpression.Line, "The variable must be a field symbol type.");
            
            if (!tableType.ContentType.IsCastableTo(fieldSymbolType.ContentType))
                return Error(expression.VariableExpression.Line, "The table type must be castable to the variable type.");
        }

        Result<ImmutableArray<IExpression>> contentBlockResult = PreTranspiler.RunBlock(state, expression.BlockContent);
        if (!contentBlockResult.TryGetValue(out ImmutableArray<IExpression> contentBlock)) return contentBlockResult.ToErrorResult();
        
        VariableExpression variableExpression = expression.VariableExpression with { Variable = variable };
        var tableExpression = expression.TableExpression with { Variable = table };

        var foreachExpression = new ForeachExpression(expression.Line, expression.Declaration, variableExpression, tableExpression, contentBlock);
        expressions.Add(foreachExpression);
        
        state.StackOut();
        return Ok(expressions.ToArray());
    }
}