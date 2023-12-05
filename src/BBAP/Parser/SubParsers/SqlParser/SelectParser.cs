using System.Collections.Immutable;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Operators;
using BBAP.Lexer.Tokens.Others;
using BBAP.Lexer.Tokens.Sql;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Sql;
using BBAP.Parser.Expressions.Values;
using BBAP.Results;

namespace BBAP.Parser.SubParsers.SqlParser;

public static class SelectParser {
    public static Result<IExpression> Run(ParserState state, out IToken lastToken, Type[] endTokenTypes, int line) {
        var returnFields = new HashSet<VariableExpression>();
        Result<MultiplyToken> fieldTokenResult = state.Next<MultiplyToken>();
        if (fieldTokenResult.TryGetValue(out MultiplyToken? starToken)) {
            returnFields.Add(new VariableExpression(starToken.Line, new AllFields()));
        } else {
            state.Revert();
            while (true) {
                Result<VariableExpression> variableExpressionResult = VariableParser.ParseRaw(state);
                if (!variableExpressionResult.TryGetValue(out VariableExpression? variableExpression)) {
                    lastToken = null;
                    return variableExpressionResult.ToErrorResult();
                }

                returnFields.Add(variableExpression);

                Result<CommaToken> commaResult = state.Next<CommaToken>();
                if (!commaResult.IsSuccess) break;
            }

            state.Revert();

            if (returnFields.Count == 0) Error(line, "Expected at least one field to return");
        }

        Result<FromToken> fromResult = state.Next<FromToken>();
        if (!fromResult.IsSuccess) {
            lastToken = null;
            return fromResult.ToErrorResult();
        }

        Result<VariableExpression> fromTableResult = VariableParser.ParseRaw(state);
        if (!fromTableResult.TryGetValue(out VariableExpression? fromTable)) {
            lastToken = null;
            return fromTableResult.ToErrorResult();
        }

        Result<ImmutableArray<JoinExpression>> joinsResult = JoinParser.Run(state);
        if (!joinsResult.TryGetValue(out ImmutableArray<JoinExpression> joins)) {
            lastToken = null;
            return joinsResult.ToErrorResult();
        }

        Result<SqlFilterExpression?> whereResult = WhereParser.Run(state, out lastToken, endTokenTypes);
        if (!whereResult.TryGetValue(out SqlFilterExpression? whereExpression)) {
            lastToken = null;
            return whereResult.ToErrorResult();
        }

        state.Revert();

        Result<ImmutableArray<VariableExpression>> orderByResult = OrderByParser.Run(state);
        if (!orderByResult.TryGetValue(out ImmutableArray<VariableExpression> orderByExpressions)) {
            lastToken = null;
            return orderByResult.ToErrorResult();
        }

        Result<long?> limitResult = LimitParser.Run(state);
        if (!limitResult.TryGetValue(out long? limit)) {
            lastToken = null;
            return limitResult.ToErrorResult();
        }

        Result<IToken> lastTokenResult = state.Next(endTokenTypes);
        if (!lastTokenResult.TryGetValue(out lastToken)) return lastTokenResult.ToErrorResult();

        var selectExpression = new SqlSelectExpression(line, returnFields.ToImmutableArray(), fromTable, joins,
                                                       whereExpression, orderByExpressions, limit);

        return Ok<IExpression>(selectExpression);
    }
}