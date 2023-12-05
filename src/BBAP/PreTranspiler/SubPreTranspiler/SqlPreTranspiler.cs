using System.Collections.Immutable;
using System.Diagnostics;
using System.Net.Mime;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Calculations;
using BBAP.Parser.Expressions.Sql;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.PreTranspiler.Expressions.Sql;
using BBAP.Results;
using BBAP.Types;
using Error = BBAP.Results.Error;

namespace BBAP.PreTranspiler.SubPreTranspiler; 

public static class SqlPreTranspiler {
    public static Result<IExpression[]> RunSelect(SqlSelectExpression sqlSelectExpression, PreTranspilerState state) {
        VariableExpression[] rawTables = sqlSelectExpression.Joins.Select(x => x.Table).Append(sqlSelectExpression.From).ToArray();

        Result<IVariable[]> tablesResult = rawTables.Select(x => GetVariableExpressionFromTable(x, state)).Wrap();
        
        if (!tablesResult.TryGetValue(out IVariable[]? tables)) {
            return tablesResult.ToErrorResult();
        }
        
        var outputFields = new HashSet<SecondStageSqlVariableExpression>();
        foreach (VariableExpression variableExpression in sqlSelectExpression.OutputFields) {
            var variableResult = RunVariableExpression(state, variableExpression, tables);
            if (!variableResult.TryGetValue(out ISecondStageSqlValueExpression? variableValue)) {
                return variableResult.ToErrorResult();
            }

            if(variableValue is not SecondStageSqlVariableExpression variable) {
                throw new UnreachableException();
            }
            
            outputFields.Add(variable);
        }

        var fromResult = state.GetVariable(sqlSelectExpression.From.Variable, sqlSelectExpression.From.Line);
        if (!fromResult.TryGetValue(out IVariable? from)) {
            return fromResult.ToErrorResult();
        }

        Result<SecondStageJoinExpression[]> joinsResult = sqlSelectExpression.Joins.Select(join => {
            Result<ISecondStageSqlValueExpression> whereResult = RunFilter(sqlSelectExpression.Where, state, tables);
            if (!whereResult.TryGetValue(out ISecondStageSqlValueExpression? filterValue)) {
                return whereResult.ToErrorResult();
            }

            if (filterValue is not SecondStageSqlFilterExpression filter) {
                return Error(sqlSelectExpression.Where.Line, "The where expression must be a filter expression.");
            }

            Result<IVariable> tableResult = state.GetVariable(join.Table.Variable, join.Table.Line);
            if (!tableResult.TryGetValue(out IVariable? table)) {
                return tableResult.ToErrorResult();
            }

            var tableExpression = join.Table with { Variable = table };

            var newJoin = new SecondStageJoinExpression(join.Line, tableExpression, join.JoinType, filter);

            return Ok(newJoin);
        }).Wrap();
        
        if (!joinsResult.TryGetValue(out SecondStageJoinExpression[]? joins)) {
            return joinsResult.ToErrorResult();
        }

        SecondStageSqlFilterExpression? whereExpression = null;
        if (sqlSelectExpression.Where is not null) {
            var whereResult = RunFilter(sqlSelectExpression.Where, state, tables);
            if (!whereResult.TryGetValue(out ISecondStageSqlValueExpression? filterValue)) {
                return whereResult.ToErrorResult();
            }

            if(filterValue is not SecondStageSqlFilterExpression filter) {
                return Error(sqlSelectExpression.Where.Line, "The where expression must be a filter expression.");
            }

            whereExpression = filter;
        }
        
        var orderBy = new List<SecondStageSqlVariableExpression>();
        foreach (VariableExpression variableExpression in sqlSelectExpression.Orderby) {
            var variableResult = RunVariableExpression(state, variableExpression, tables);
            if (!variableResult.TryGetValue(out ISecondStageSqlValueExpression? variableValue)) {
                return variableResult.ToErrorResult();
            }

            if(variableValue is not SecondStageSqlVariableExpression variable) {
                throw new UnreachableException();
            }
            
            orderBy.Add(variable);
        }
        
        var newSelectExpression = new SecondStageSelectExpression(sqlSelectExpression.Line, outputFields.ToImmutableArray(),
                                                                  sqlSelectExpression.From, joins.ToImmutableArray(),
                                                                  whereExpression, orderBy.ToImmutableArray(),
                                                                  sqlSelectExpression.Limit, null);
        
        return Ok<IExpression[]>(new[] {newSelectExpression});
    }

    private static Result<IVariable> GetVariableExpressionFromTable(VariableExpression variableExpression, PreTranspilerState state) {
        Result<IVariable> variableResult = state.GetVariable(variableExpression.Variable, variableExpression.Line);
        if (!variableResult.TryGetValue(out IVariable? variable)) {
            return variableResult.ToErrorResult();
        }

        if (variable.Type is not TableType && !(variable.Type is AliasType aliasType && aliasType.GetRealType() is TableType)) {
            return Error(variableExpression.Line, $"The variable '{variable.Name}' is not a table type.");
        }
        
        return Ok(variable);
    }
    
    private static Result<ISecondStageSqlValueExpression> RunFilter(IExpression value, PreTranspilerState state, IVariable[] tables) {
        return value switch {
            MathCalculationExpression mathCalculationExpression => RunMathCalculationExpression(state, mathCalculationExpression, tables),
            BooleanExpression booleanExpression => RunBooleanExpression(state, booleanExpression, tables),
            ComparisonExpression comparisonExpression => RunComparisonExpression(state, comparisonExpression, tables),
            VariableExpression variableExpression => RunVariableExpression(state, variableExpression, tables),
            FloatExpression floatExpression => WrapInValue(state, floatExpression),
            IntExpression intExpression => WrapInValue(state, intExpression),
            StringExpression stringExpression => WrapInValue(state, stringExpression),
            BooleanValueExpression booleanValueExpression => WrapInValue(state, booleanValueExpression),
            SqlFilterExpression sqlFilterExpression => RunFilter(sqlFilterExpression.Value, state, tables),
            _ => Error(value.Line, $"The value {value} is not a valid sql value.")
        };
    }

    private static Result<ISecondStageSqlValueExpression> WrapInValue(PreTranspilerState state, IExpression expression) {
        Result<IType> typeResult = expression switch {
            FloatExpression => state.Types.Get(expression.Line, Keywords.Float),
            IntExpression => state.Types.Get(expression.Line, Keywords.Int),
            StringExpression => state.Types.Get(expression.Line, Keywords.String),
            BooleanValueExpression => state.Types.Get(expression.Line, Keywords.Boolean),
            _ => Error(expression.Line, $"The value {expression} is not a valid sql value.")
        };
        
        if(!typeResult.TryGetValue(out IType? type)) {
            return typeResult.ToErrorResult();
        }
        
        var typeExpression = new TypeExpression(expression.Line, type);

        var wrappedValue = new SecondStageSqlBaseValueExpression(expression.Line, typeExpression, expression);
        return Ok<ISecondStageSqlValueExpression>(wrappedValue);
    }

    private static Result<ISecondStageSqlValueExpression> RunVariableExpression(PreTranspilerState state, VariableExpression variableExpression, IVariable[] tables) {

        if (variableExpression.Variable.GetTopVariable() is not Variable rawTopVariable) {
            return Error(variableExpression.Line,
                         $"The variable '{variableExpression.Variable}' is not a valid variable.");
        }

        var topVariableResult = state.GetVariable(rawTopVariable, variableExpression.Line);

        if (!topVariableResult.TryGetValue(out IVariable? topVariable)) {
            return topVariableResult.ToErrorResult();
        }
        
        IType type;
        IVariable variable;
        
        InsertType insertType;
        if (GetTableType(topVariable) is TableType tableType){
            if (!tables.Contains(topVariable)) {
                return Error(variableExpression.Line,
                             $"The select statement does not include the table '{topVariable.Name}'.");
            }

            IVariable[] variableChain = variableExpression.Variable.Unwrap();

            if (variableChain.Length == 1) {
                return Error(variableExpression.Line, $"The table variable '{topVariable.Name}' must be followed by a field.");
            }

            IType currentType = tableType.ContentType;
            if (currentType is AliasType aliasTypeTemp) {
                currentType = aliasTypeTemp.GetRealType();
            }
            
            variable = topVariable;
            foreach (IVariable localVariable in variableChain.Skip(1)) {
                if (currentType is not StructType structType) {
                    return Error(variableExpression.Line, $"The variable/field '{currentType.Name}' is not a struct type and can therefor not contain the field '{localVariable.Name}'.");
                }
                
                Variable? currentField = structType.Fields.FirstOrDefault(x => x.Name == localVariable.Name);
                if(currentField is null) {
                    return Error(variableExpression.Line, $"The struct type '{structType.Name}' does not contain the field '{localVariable.Name}'.");
                }

                currentType = currentField.Type;
                if (currentType is AliasType aliasType) {
                    currentType = aliasType.GetRealType();
                }
                
                variable = new FieldVariable(currentType, currentField.Name, variable);
                
            }

            type = currentType;
            
            insertType = InsertType.InSqlStatement;
        } else {       
            Result<IVariable> variableResult = state.GetVariable(variableExpression.Variable, variableExpression.Line);

            if (!variableResult.TryGetValue(out variable)) {
                return variableResult.ToErrorResult();
            }
            type = variable.Type;

            insertType = InsertType.FromCode;
        }

        var typeExpression = new TypeExpression(variableExpression.Line, type);
        var newExpression = new SecondStageSqlVariableExpression(variableExpression.Line, typeExpression, insertType,
                                                                 variableExpression with { Variable = variable });
        return Ok<ISecondStageSqlValueExpression>(newExpression);
    }

    private static IType GetTableType(IVariable topVariable) {
        if (topVariable.Type is TableType tableType) {
            return tableType;
        }

        if (topVariable.Type is AliasType topAliasType && topAliasType.GetRealType() is TableType tableTypeTemp) {
            return tableTypeTemp;
        }

        return topVariable.Type;
    }

    private static Result<ISecondStageSqlValueExpression> RunComparisonExpression(PreTranspilerState state, ComparisonExpression comparisonExpression, IVariable[] tables) {
        Result<ISecondStageSqlValueExpression> leftResult = RunFilter(comparisonExpression.Left, state, tables);
        if (!leftResult.TryGetValue(out ISecondStageSqlValueExpression? left)) {
            return leftResult.ToErrorResult();
        }

        Result<ISecondStageSqlValueExpression> rightResult = RunFilter(comparisonExpression.Right, state, tables);
        if (!rightResult.TryGetValue(out ISecondStageSqlValueExpression? right)) {
            return rightResult.ToErrorResult();
        }

        TypeExpression type;
        if (left.Type.Type.IsCastableTo(right.Type.Type)) {
            type = left.Type;
        } else if (right.Type.Type.IsCastableTo(left.Type.Type)) {
            type = right.Type;
        } else {
            return Error(comparisonExpression.Line, $"Cannot cast {left.Type.Type} to {right.Type.Type} or vice versa.");
        }

        Result<SqlFilterOperator> operatorResult
            = GetOperator(comparisonExpression.ComparisonType, comparisonExpression.Line);
        if (!operatorResult.TryGetValue(out SqlFilterOperator @operator)) {
            return operatorResult.ToErrorResult();
        }

        return Ok<ISecondStageSqlValueExpression>(new SecondStageSqlFilterExpression(comparisonExpression.Line, type,
                                                       @operator, left, right));
    }

    private static Result<ISecondStageSqlValueExpression> RunBooleanExpression(PreTranspilerState state, BooleanExpression booleanExpression, IVariable[] tables) {
        Result<ISecondStageSqlValueExpression> leftResult = RunFilter(booleanExpression.Left, state, tables);
        if (!leftResult.TryGetValue(out ISecondStageSqlValueExpression? left)) {
            return leftResult.ToErrorResult();
        }

        Result<ISecondStageSqlValueExpression> rightResult = RunFilter(booleanExpression.Right, state, tables);
        if (!rightResult.TryGetValue(out ISecondStageSqlValueExpression? right)) {
            return rightResult.ToErrorResult();
        }

        TypeExpression type;
        if (left.Type.Type.IsCastableTo(right.Type.Type)) {
            type = left.Type;
        } else if (right.Type.Type.IsCastableTo(left.Type.Type)) {
            type = right.Type;
        } else {
            return Error(booleanExpression.Line, $"Cannot cast {left.Type.Type} to {right.Type.Type} or vice versa.");
        }

        Result<SqlFilterOperator> operatorResult = GetOperator(booleanExpression.BooleanType, booleanExpression.Line);
        if (!operatorResult.TryGetValue(out SqlFilterOperator @operator)) {
            return operatorResult.ToErrorResult();
        }

        return Ok<ISecondStageSqlValueExpression>(new SecondStageSqlFilterExpression(booleanExpression.Line, type,
                                                       @operator, left, right));
    }

    private static Result<ISecondStageSqlValueExpression> RunMathCalculationExpression(PreTranspilerState state,
        MathCalculationExpression mathCalculationExpression, IVariable[] tables) {
        Result<ISecondStageSqlValueExpression> leftResult = RunFilter(mathCalculationExpression.Left, state, tables);
        if (!leftResult.TryGetValue(out ISecondStageSqlValueExpression? left)) {
            return leftResult.ToErrorResult();
        }

        Result<ISecondStageSqlValueExpression> rightResult = RunFilter(mathCalculationExpression.Right, state, tables);
        if (!rightResult.TryGetValue(out ISecondStageSqlValueExpression? right)) {
            return rightResult.ToErrorResult();
        }

        TypeExpression type;
        if (left.Type.Type.IsCastableTo(right.Type.Type)) {
            type = left.Type;
        } else if (right.Type.Type.IsCastableTo(left.Type.Type)) {
            type = right.Type;
        } else {
            return Error(mathCalculationExpression.Line,
                         $"Cannot cast {left.Type.Type} to {right.Type.Type} or vice versa.");
        }

        Result<SqlFilterOperator> operatorResult
            = GetOperator(mathCalculationExpression.CalculationType, mathCalculationExpression.Line);
        if (!operatorResult.TryGetValue(out SqlFilterOperator @operator)) {
            return operatorResult.ToErrorResult();
        }

        return Ok<ISecondStageSqlValueExpression>(new SecondStageSqlFilterExpression(mathCalculationExpression.Line, type,
                                                       @operator, left, right));
    }

    private static Result<SqlFilterOperator> GetOperator(ComparisonType comparisonExpressionComparisonType, int line) {
        return comparisonExpressionComparisonType switch {
            ComparisonType.Equals => Ok(SqlFilterOperator.Equals),
            ComparisonType.NotEquals => Ok(SqlFilterOperator.NotEquals),
            ComparisonType.GreaterThen => Ok(SqlFilterOperator.GreaterThan),
            ComparisonType.GreaterThenOrEquals => Ok(SqlFilterOperator.GreaterThanOrEquals),
            ComparisonType.SmallerThen => Ok(SqlFilterOperator.SmallerThan),
            ComparisonType.SmallerThenOrEquals => Ok(SqlFilterOperator.SmallerThanOrEquals),
            _ => throw new UnreachableException()
        };
    }

    private static Result<SqlFilterOperator> GetOperator(BooleanType booleanExpressionBooleanType, int line) {
        return booleanExpressionBooleanType switch {
            BooleanType.And => Ok(SqlFilterOperator.And),
            BooleanType.Or => Ok(SqlFilterOperator.Or),
            BooleanType.Xor => Error(line, "Xor is not supported in sql."), // TODO: Support xor, with changing the structure of the sql filter expression
            _ => throw new UnreachableException()
        };
    }

    private static Result<SqlFilterOperator> GetOperator(CalculationType calculationType, int line) {
        return calculationType switch {
            CalculationType.Plus => Ok(SqlFilterOperator.Plus),
            CalculationType.Minus => Ok(SqlFilterOperator.Minus),
            CalculationType.Divide => Ok(SqlFilterOperator.Divide),
            CalculationType.Multiply => Ok(SqlFilterOperator.Multiply),
            CalculationType.Modulo => Ok(SqlFilterOperator.Modulo),
            CalculationType.BitwiseAnd => Error(line, "Bitwise operations are not supported in sql."),
            CalculationType.BitwiseOr => Error(line, "Bitwise operations are not supported in sql."),
            _ => throw new UnreachableException()
        };
    }
}