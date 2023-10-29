﻿using System.Collections.Immutable;
using System.Diagnostics;
using System.Drawing;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Lexer.Tokens.Keywords;
using BBAP.Lexer.Tokens.Setting;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions;
using BBAP.Parser.ExtensionMethods;
using BBAP.Parser.SubParsers;
using BBAP.Results;

namespace BBAP.Parser;

public class Parser {
    public Result<ImmutableArray<IExpression>> Run(ImmutableArray<IToken> tokens) {
        var expressions = new List<IExpression>();

        var state = new ParserState(tokens);
        while (true) {
            Result<IExpression> result = ParseNextStatement(state);

            if (!result.TryGetValue(out IExpression? expression)) {
                if (result.Error is NoMoreDataError) {
                    break;
                }

                return result.ToErrorResult();
            }

            expressions.Add(expression);
        }

        return Ok(expressions.ToImmutableArray());
    }

    public static Result<IExpression> ParseNextStatement(ParserState state) {
        Result<IToken> tokenResult = state.Next(typeof(UnknownWordToken), typeof(IfToken), typeof(ForToken),
            typeof(WhileToken),
            typeof(DoToken), typeof(LetToken), typeof(FunctionToken));

        if (!tokenResult.TryGetValue(out IToken? token)) {
            return tokenResult.ToErrorResult();
        }

        Result<IExpression> result = token switch {
            UnknownWordToken unknownWordToken => UnknownWordParser.RunRoot(state, unknownWordToken),
            IfToken => IfParser.Run(state, token.Line),
            ForToken => ForParser.Run(state),
            WhileToken => WhileParser.Run(state, token.Line),
            DoToken => DoParser.Run(state),
            LetToken => LetParser.Run(state),
            FunctionToken => FunctionParser.Run(state, token.Line),
            _ => throw new UnreachableException()
        };

         return result;
    }

    public static Result<ImmutableArray<IExpression>> ParseBlock(ParserState state, bool includeOpeningBracket) {
        var blockContent = new List<IExpression>();

        if (includeOpeningBracket) {
            Result<IToken> result = state.Next(typeof(OpeningCurlyBracketToken));
            if (!result.IsSuccess) {
                return result.ToErrorResult();
            }
        }
        
        while(true) {
            Result<IExpression> expressionResult = ParseNextStatement(state);

            if (!expressionResult.TryGetValue(out IExpression? expression)) {
                if (expressionResult.Error is InvalidTokenError { Token: ClosingCurlyBracketToken }) {
                    break;
                }

                return expressionResult.ToErrorResult();
            }

            blockContent.Add(expression);
        }

        return Ok(blockContent.ToImmutableArray());
    }
}