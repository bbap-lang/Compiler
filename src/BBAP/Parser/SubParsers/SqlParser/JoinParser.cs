using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Others;
using BBAP.Lexer.Tokens.Sql;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions.Sql;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Variables;
using BBAP.Results;
using BBAP.Types.Types.ParserTypes;

namespace BBAP.Parser.SubParsers.SqlParser;

public static class JoinParser {
    public static Result<ImmutableArray<JoinExpression>> Run(ParserState state) {
        List<JoinExpression> joins = new();
        while (true) {
            Result<IToken> joinTypeResult
                = state.Next(typeof(LeftToken), typeof(RightToken), typeof(InnerToken), typeof(OuterToken));
            var joinType = JoinType.Outer;
            int line;
            if (joinTypeResult.TryGetValue(out IToken? joinTypeToken)) {
                joinType = joinTypeToken switch {
                    LeftToken => JoinType.Left,
                    RightToken => JoinType.Right,
                    InnerToken => JoinType.Inner,
                    OuterToken => JoinType.Outer,
                    _ => throw new UnreachableException()
                };

                Result<JoinToken> joinResult = state.Next<JoinToken>();
                if (!joinResult.IsSuccess) return Error(joinTypeToken.Line, "Expected 'JOIN' keyword");

                line = joinTypeToken.Line;
            } else {
                state.Revert();
                Result<JoinToken> joinResult = state.Next<JoinToken>();
                if (!joinResult.TryGetValue(out JoinToken? joinToken)) break;
                line = joinToken.Line;
            }

            Result<UnknownWordToken> tableResult = state.Next<UnknownWordToken>();
            if (!tableResult.TryGetValue(out UnknownWordToken? tableToken)) return tableResult.ToErrorResult();

            var table = new VariableExpression(tableToken.Line, new Variable(new UnknownType(), tableToken.Value, MutabilityType.Mutable));

            Result<OnToken> onResult = state.Next<OnToken>();
            if (!onResult.IsSuccess) return onResult.ToErrorResult();

            Result<SqlFilterExpression> filterExpressionResult
                = FilterParser.Run(state, out _, typeof(SemicolonToken), typeof(WhereToken), typeof(OrderToken),
                                   typeof(LimitToken));
            if (!filterExpressionResult.TryGetValue(out SqlFilterExpression? filterExpression))
                return filterExpressionResult.ToErrorResult();


            var joinExpression = new JoinExpression(line, table, joinType, filterExpression);
            joins.Add(joinExpression);
        }

        state.Revert();
        return Ok(joins.ToImmutableArray());
    }
}