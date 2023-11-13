using System.Collections.Immutable;
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
using BBAP.PreTranspiler;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.Parser.SubParsers;

public static class UnknownWordParser {
    public static Result<IExpression> RunRoot(ParserState state, UnknownWordToken unknownWordToken) {
        IToken? nextToken;
        
        List<UnknownWordToken> wordList = new();
        do {
            if (wordList.Count == 0) {
                wordList.Add(unknownWordToken);
            } else {
                Result<UnknownWordToken> nextWordResult = state.Next<UnknownWordToken>();

                if (!nextWordResult.TryGetValue(out UnknownWordToken? nextWord)) {
                    return nextWordResult.ToErrorResult();
                }
                
                wordList.Add(nextWord);
            }
            
            Result<IToken> nextTokenResult = state.Next(typeof(OpeningGenericBracketToken), typeof(SetToken),
                                                        typeof(PlusEqualsToken), typeof(MinusEqualsToken), typeof(MultiplyEqualsToken),
                                                        typeof(DivideEqualsToken), typeof(ModuloEqualsToken), typeof(IncrementToken),
                                                        typeof(DecrementToken), typeof(DotToken));

            if (!nextTokenResult.TryGetValue(out nextToken) && nextTokenResult.Error is not InvalidTokenError) {
                return nextTokenResult.ToErrorResult();
            }

        } while (nextToken is DotToken);

        var words = wordList.ToImmutableArray();

        Result<IExpression> result = nextToken switch {
            OpeningGenericBracketToken openingGenericBracket => FunctionCallParser.Run(state, words),
            SetToken setToken => SetParser.Run(state, words, SetType.Generic),
            PlusEqualsToken setToken => SetParser.Run(state, words, SetType.Plus),
            MinusEqualsToken setToken => SetParser.Run(state, words, SetType.Minus),
            MultiplyEqualsToken setToken => SetParser.Run(state, words, SetType.Multiplication),
            DivideEqualsToken setToken => SetParser.Run(state, words, SetType.Devide),
            ModuloEqualsToken setToken => SetParser.Run(state, words, SetType.Modulo),
            IncrementToken setToken => IncrementParser.Run(state, words, IncrementType.Plus),
            DecrementToken setToken =>
                IncrementParser.Run(state, words, IncrementType.Minus),
            _ => throw new UnreachableException()
        };

        if (!result.IsSuccess) {
            return result;
        }

        state.SkipSemicolon();

        return result;
    }

    public static Result<IExpression> RunValue(ParserState state, UnknownWordToken unknownWordToken) {
        Result<IToken> nextTokenResult;
        IToken? nextToken;
        
        List<UnknownWordToken> wordList = new();
        do {
            if (wordList.Count == 0) {
                wordList.Add(unknownWordToken);
            } else {
                Result<UnknownWordToken> nextWordResult = state.Next<UnknownWordToken>();

                if (!nextWordResult.TryGetValue(out UnknownWordToken? nextWord)) {
                    return nextWordResult.ToErrorResult();
                }
                
                wordList.Add(nextWord);
            }
            
            nextTokenResult = state.Next(typeof(OpeningGenericBracketToken), typeof(DotToken));

            if (!nextTokenResult.TryGetValue(out nextToken) && nextTokenResult.Error is not InvalidTokenError) {
                return nextTokenResult.ToErrorResult();
            }

        } while (nextToken is DotToken);
        
        ImmutableArray<UnknownWordToken> words = wordList.ToImmutableArray();

        if (nextTokenResult.Error is InvalidTokenError) {
            state.Revert();
        }

        Result<IExpression> result = nextToken switch {
            OpeningGenericBracketToken openingGenericBracket => FunctionCallParser.Run(state, words),
            
            _ => Ok<IExpression>(VariableParser.Run(words))
            
        };

        return result;
    }
}