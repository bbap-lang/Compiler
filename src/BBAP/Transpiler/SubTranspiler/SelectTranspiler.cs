using BBAP.PreTranspiler.Expressions.Sql;

namespace BBAP.Transpiler.SubTranspiler;

public static class SelectTranspiler {
    public static void Run(SecondStageSelectExpression selectExpression, TranspilerState state) {
        state.Builder.Append("SELECT ");
        state.Builder.AddIntend();
        for (int i = 0; i < selectExpression.OutputFields.Length; i++) {
            RunField(selectExpression.OutputFields[i], state.Builder);
            if (i != selectExpression.OutputFields.Length - 1) state.Builder.Append(", ");
        }

        state.Builder.AppendLine();
        state.Builder.Append("FROM ");
        VariableTranspiler.Run(selectExpression.From, state.Builder);
        state.Builder.AppendLine();
        if (selectExpression.Where is not null) {
            state.Builder.Append("WHERE ");
            RunFilter(selectExpression.Where, state);
            state.Builder.AppendLine();
        }

        if (selectExpression.OrderBy.Length > 0) {
            state.Builder.Append("ORDER BY ");
            foreach (SecondStageSqlVariableExpression orderByField in selectExpression.OrderBy) {
                RunField(orderByField, state.Builder);
                state.Builder.Append(' ');
            }

            state.Builder.AppendLine();
        }

        if (selectExpression.Limit is not null) {
            state.Builder.Append("UP TO ");
            state.Builder.Append(selectExpression.Limit.Value);
            state.Builder.AppendLine(" ROWS");
        }

        if (selectExpression.OutputVariable is not null) {
            state.Builder.Append("INTO ");
            VariableTranspiler.Run(selectExpression.OutputVariable, state.Builder);
        }

        state.Builder.AppendLine(".");
        state.Builder.RemoveIntend();
    }

    private static void RunField(SecondStageSqlVariableExpression field, AbapBuilder builder) {
        if (field.InsertType == InsertType.FromCode) builder.Append('@');
        VariableTranspiler.Run(field.Variable, builder);
    }

    private static void RunFilter(SecondStageSqlFilterExpression filterExpression, TranspilerState state) {
        ISecondStageSqlValueExpression left = filterExpression.Left;
        RunValue(left, state);

        state.Builder.Append(' ');
        PrintOperator(filterExpression.Operator, state.Builder);
        state.Builder.Append(' ');

        ISecondStageSqlValueExpression right = filterExpression.Right;
        RunValue(right, state);
    }

    private static void RunValue(ISecondStageSqlValueExpression right, TranspilerState state) {
        switch (right) {
            case SecondStageSqlFilterExpression rightFilter:
                state.Builder.Append('(');
                RunFilter(rightFilter, state);
                state.Builder.Append(')');
                break;
            case SecondStageSqlVariableExpression rightVariable:
                RunField(rightVariable, state.Builder);
                break;
            case SecondStageSqlBaseValueExpression rightBaseValue:
                ValueTranspiler.Run(rightBaseValue, state);
                break;
        }
    }

    private static void PrintOperator(SqlFilterOperator filterExpressionOperator, AbapBuilder builder) {
        builder.Append(filterExpressionOperator switch {
            SqlFilterOperator.Plus => "+",
            SqlFilterOperator.Minus => "-",
            SqlFilterOperator.Multiply => "*",
            SqlFilterOperator.Divide => "/",
            SqlFilterOperator.Modulo => "MOD",

            SqlFilterOperator.Equals => "EQ",
            SqlFilterOperator.NotEquals => "NE",
            SqlFilterOperator.GreaterThan => "GT",
            SqlFilterOperator.GreaterThanOrEquals => "GE",
            SqlFilterOperator.SmallerThan => "LT",
            SqlFilterOperator.SmallerThanOrEquals => "LE",

            SqlFilterOperator.And => "AND",
            SqlFilterOperator.Or => "OR",

            _ => throw new ArgumentOutOfRangeException(nameof(filterExpressionOperator), filterExpressionOperator, null)
        });
    }
}