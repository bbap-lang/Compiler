using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Lexer.Tokens.Others;
using BBAP.Lexer.Tokens.Setting;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.Parser.SubParsers;

public static class FunctionCallParser {
    public static Result<IExpression> Run(ParserState state, ImmutableArray<UnknownWordToken> names) {
        int line = names.Last().Line;
        string functionName = string.Join('.', names.Select(x => x.Value));
        var parameters = new List<IExpression>();

        IToken? lastToken = null;

        while (lastToken is not ClosingGenericBracketToken) {
            Result<IExpression> parameterResult = ValueParser.FullExpression(state, out lastToken,
                                                                             typeof(ClosingGenericBracketToken),
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

        return Ok<IExpression>(new FunctionCallExpression(line, functionName, parameters.ToImmutableArray()));
    }

    public static Result<IExpression> RunFull(ParserState state, int valueLine) {
        var variables = new List<VariableExpression>();

        IToken endToken;
        do {
            Result<IToken> nameResult = state.Next(typeof(UnknownWordToken));
            if (!nameResult.TryGetValue(out IToken? nameToken)) {
                return nameResult.ToErrorResult();
            }

            if (nameToken is not UnknownWordToken wordToken) {
                throw new UnreachableException();
            }

            var newVariable = new VariableExpression(nameToken.Line, new Variable(new UnknownType(), wordToken.Value));
            variables.Add(newVariable);

            Result<IToken> commaResult = state.Next(typeof(CommaToken), typeof(ClosingGenericBracketToken));
            if (!commaResult.TryGetValue(out endToken)) {
                return commaResult.ToErrorResult();
            }
        } while (endToken is not ClosingGenericBracketToken);

        Result<IToken> tempResult = state.Next(typeof(SetToken));

        if (!tempResult.IsSuccess) {
            return tempResult.ToErrorResult();
        }


        var nameTokenList = new List<UnknownWordToken>();

        while (true) {
            Result<UnknownWordToken> functionNameResult = state.Next<UnknownWordToken>();
            if (!functionNameResult.TryGetValue(out UnknownWordToken? nameToken)) {
                return functionNameResult.ToErrorResult();
            }

            nameTokenList.Add(nameToken);

            Result<DotToken> dotTokenResult = state.Next<DotToken>();
            if (!dotTokenResult.IsSuccess) {
                state.Revert();
                break;
            }
        }

        ImmutableArray<UnknownWordToken> nameTokens = nameTokenList.ToImmutableArray();

        tempResult = state.Next(typeof(OpeningGenericBracketToken));
        if (!tempResult.IsSuccess) {
            return tempResult.ToErrorResult();
        }

        Result<IExpression> functionCallResult = Run(state, nameTokens);

        if (!functionCallResult.TryGetValue(out IExpression? tempExpression)) {
            return functionCallResult.ToErrorResult();
        }

        var res = state.SkipSemicolon();

        if (tempExpression is not FunctionCallExpression functionCallExpression) {
            throw new UnreachableException();
        }

        var newFunctionCall
            = new FunctionCallSetExpression(functionCallExpression.Line, functionCallExpression.Name,
                                            functionCallExpression.Parameters, variables.ToImmutableArray());

        state.SkipSemicolon();
        return Ok<IExpression>(newFunctionCall);
    }
}