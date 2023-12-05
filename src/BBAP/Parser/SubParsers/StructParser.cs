using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Lexer.Tokens.Others;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Variables;
using BBAP.Results;

namespace BBAP.Parser.SubParsers;

public static class StructParser {
    public static Result<IExpression> Run(ParserState state, int line) {
        Result<UnknownWordToken> nameResult = state.Next<UnknownWordToken>();
        if (!nameResult.TryGetValue(out UnknownWordToken? name)) return nameResult.ToErrorResult();

        Result<OpeningCurlyBracketToken> openingCurlyBracketResult = state.Next<OpeningCurlyBracketToken>();
        if (!openingCurlyBracketResult.IsSuccess) return openingCurlyBracketResult.ToErrorResult();

        List<VariableExpression> fields = new();
        while (true) {
            Result<IToken> nextTokenResult = state.Next(typeof(UnknownWordToken), typeof(ClosingCurlyBracketToken));
            if (!nextTokenResult.TryGetValue(out IToken? nextToken)) return nextTokenResult.ToErrorResult();

            if (nextToken is ClosingCurlyBracketToken) break;

            if (nextToken is not UnknownWordToken fieldName) throw new UnreachableException();

            Result<ColonToken> colonResult = state.Next<ColonToken>();
            if (!colonResult.TryGetValue(out _)) return colonResult.ToErrorResult();

            Result<TypeExpression> typeResult = TypeParser.Run(state);
            if (!typeResult.TryGetValue(out TypeExpression? type)) return typeResult.ToErrorResult();

            var newField = new VariableExpression(fieldName.Line, new Variable(type.Type, fieldName.Value));
            fields.Add(newField);

            Result<IToken> endTokenResult = state.Next(typeof(CommaToken), typeof(ClosingCurlyBracketToken));
            if (!endTokenResult.TryGetValue(out IToken? endToken)) return endTokenResult.ToErrorResult();

            if (endToken is ClosingCurlyBracketToken) break;
        }

        var structExpression = new StructExpression(line, name.Value, fields.ToImmutableArray());
        return Ok<IExpression>(structExpression);
    }
}