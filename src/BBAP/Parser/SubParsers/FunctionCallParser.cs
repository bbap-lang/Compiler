using System.Collections.Immutable;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Lexer.Tokens.Others;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.Results;

namespace BBAP.Parser.SubParsers;

public static class FunctionCallParser {
    public static Result<IExpression> Run(ParserState state, UnknownWordToken nameToken) {
        string functionName = nameToken.Value;
        var parameters = new List<IExpression>();

        IToken? lastToken = null;
        
        while (lastToken is not ClosingGenericBracketToken) {
            Result<IExpression> parameterResult = ValueParser.FullExpression(state, out lastToken, typeof(ClosingGenericBracketToken),
                typeof(CommaToken));

            if (!parameterResult.TryGetValue(out IExpression? parameter)) {
                return parameterResult;
            }

            if (parameter is EmptyExpression) {
                if (lastToken is CommaToken) {
                    return Error(lastToken.Line, "Unexpected Symbol ',' expected ')'");
                }
                
                break;
            }

            parameters.Add(parameter);
        }

        return Ok<IExpression>( new FunctionCallExpression(nameToken.Line, functionName, parameters.ToImmutableArray()));
    }
}