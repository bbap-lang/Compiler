using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Lexer.Tokens.Keywords;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Results;

namespace BBAP.Parser.SubParsers;

public class ExtendParser {
    public static Result<IExpression> Run(ParserState state, int line) {
        Result<TypeExpression> typeResult = TypeParser.Run(state);
        if (!typeResult.TryGetValue(out TypeExpression? type)) return typeResult.ToErrorResult();

        Result<OpeningCurlyBracketToken> openingBracketResult = state.Next<OpeningCurlyBracketToken>();
        if (!openingBracketResult.IsSuccess) return openingBracketResult.ToErrorResult();

        var functions = new List<FunctionExpression>();
        while (true) {
            Result<StaticToken> staticResult = state.Next<StaticToken>();
            if (!staticResult.IsSuccess) state.Revert();

            bool isStatic = staticResult.IsSuccess;
            
            Result<FunctionToken> functionTokenResult = state.Next<FunctionToken>();
            if (!functionTokenResult.TryGetValue(out FunctionToken? functionToken))
                return functionTokenResult.ToErrorResult();

            Result<IExpression> functionResult = FunctionParser.Run(state, functionToken.Line);
            if (!functionResult.TryGetValue(out IExpression? functionExpression)) return functionResult.ToErrorResult();

            if (functionExpression is not FunctionExpression function) throw new UnreachableException();

            if (isStatic)
                function = new StaticFunctionExpression(function.Line, function.Name, function.IsReadOnly, function.Parameters,
                                                        function.OutputTypes, function.BlockContent);

            functions.Add(function);

            Result<ClosingCurlyBracketToken> closingBracketResult = state.Next<ClosingCurlyBracketToken>();
            if (closingBracketResult.IsSuccess) break;
            state.Revert();
        }

        var extendExpression = new ExtendExpression(line, type, functions.ToImmutableArray());
        return Ok<IExpression>(extendExpression);
    }
}