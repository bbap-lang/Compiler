using System.Diagnostics;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Lexer.Tokens.Others;
using BBAP.Lexer.Tokens.Setting;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Calculations;
using BBAP.Parser.Expressions.Values;
using BBAP.Parser.ExtensionMethods;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.Parser.SubParsers;

public static class UnknownWordParser {
    public static Result<IExpression> RunRoot(ParserState state, UnknownWordToken unknownWordToken) {
        Result<IToken> nextTokenResult = state.Next(typeof(OpeningGenericBracketToken), typeof(SetToken),
            typeof(PlusEqualsToken), typeof(MinusEqualsToken), typeof(MultiplyEqualsToken),
            typeof(DivideEqualsToken), typeof(ModuloEqualsToken), typeof(IncrementToken),
            typeof(DecrementToken));

        if (!nextTokenResult.TryGetValue(out IToken? nextToken)) {
            return nextTokenResult.ToErrorResult();
        }

        Result<IExpression> result = nextToken switch {
            OpeningGenericBracketToken openingGenericBracket => FunctionCallParser.Run(state, unknownWordToken),
            SetToken setToken => SetParser.Run(state, unknownWordToken, SetType.Generic),
            PlusEqualsToken setToken => SetParser.Run(state, unknownWordToken, SetType.Plus),
            MinusEqualsToken setToken => SetParser.Run(state, unknownWordToken, SetType.Minus),
            MultiplyEqualsToken setToken => SetParser.Run(state, unknownWordToken, SetType.Multiplication),
            DivideEqualsToken setToken => SetParser.Run(state, unknownWordToken, SetType.Devide),
            ModuloEqualsToken setToken => SetParser.Run(state, unknownWordToken, SetType.Modulo),
            IncrementToken setToken => IncrementParser.Run(state, unknownWordToken, IncrementType.Plus),
            DecrementToken setToken =>
                IncrementParser.Run(state, unknownWordToken, IncrementType.Minus),
            _ => throw new UnreachableException()
        };

        if (!result.IsSuccess) {
            return result;
        }

        state.SkipSemicolon();

        return result;
    }

    public static Result<IExpression> RunValue(ParserState state, UnknownWordToken unknownWordToken) {
        Result<IToken> nextTokenResult = state.Next(typeof(OpeningGenericBracketToken));

        if (!nextTokenResult.TryGetValue(out IToken? nextToken) && nextTokenResult.Error is not InvalidTokenError) {
            return nextTokenResult.ToErrorResult();
        }

        if (nextTokenResult.Error is InvalidTokenError) {
            state.Revert();
        }

        Result<IExpression> result = nextToken switch {
            OpeningGenericBracketToken openingGenericBracket => FunctionCallParser.Run(state, unknownWordToken),
            
            _ => Ok<IExpression>(new VariableExpression(unknownWordToken.Line, unknownWordToken.Value, new UnknownType()))
        };

        return result;
    }
}