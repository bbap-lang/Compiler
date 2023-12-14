using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Lexer.Tokens.Keywords;
using BBAP.Lexer.Tokens.Others;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions;
using BBAP.Parser.SubParsers;
using BBAP.Results;

namespace BBAP.Parser;

public class Parser {
    public Result<ImmutableArray<IExpression>> Run(ImmutableArray<IToken> tokens) {
        var expressions = new List<IExpression>();

        var state = new ParserState(tokens);
        while (true) {
            Result<IExpression[]> result = ParseNextStatement(state);

            if (!result.TryGetValue(out IExpression[]? expression)) {
                if (result.Error is NoMoreDataError) break;

                if (result.Error is null) {
                    state.Revert();
                    Result<EmptyToken> tokenResult = state.Next<EmptyToken>();
                    return Error(tokenResult.Error.Line,
                                 "Unknown error, the line is only a approximation (maybe a bug in the parser. If so, please report on github)");
                }

                return result.ToErrorResult();
            }

            expressions.AddRange(expression);
        }

        return Ok(expressions.ToImmutableArray());
    }

    public static Result<IExpression[]> ParseNextStatement(ParserState state) {
        Result<IToken> tokenResult = state.Next(typeof(UnknownWordToken), typeof(IfToken), typeof(ForToken),
                                                typeof(WhileToken),
                                                typeof(DoToken), typeof(LetToken), typeof(FunctionToken),
                                                typeof(ReturnToken), typeof(OpeningGenericBracketToken),
                                                typeof(AliasToken), typeof(StructToken), typeof(ExtendToken),
                                                typeof(PublicToken), typeof(EnumToken), typeof(BreakToken), typeof(ContinueToken), typeof(SwitchToken), typeof(ConstToken));

        if (!tokenResult.TryGetValue(out IToken? token)) return tokenResult.ToErrorResult();

        Result<IExpression>? result = token switch {
            UnknownWordToken unknownWordToken => UnknownWordParser.RunRoot(state, unknownWordToken),
            IfToken => IfParser.Run(state, token.Line),
            ForToken => ForParser.Run(state),
            WhileToken => WhileParser.Run(state, token.Line),
            DoToken => DoParser.Run(state),
            FunctionToken => FunctionParser.Run(state, token.Line),
            ReturnToken => ReturnParser.Run(state, token.Line),
            PublicToken => PublicParser.Run(state),
            EnumToken => EnumParser.Run(state, token.Line),
            SwitchToken => SwitchParser.Run(state),

            AliasToken => AliasParser.Run(state, false),
            StructToken => StructParser.Run(state, token.Line),
            ExtendToken => ExtendParser.Run(state, token.Line),
            
            BreakToken => LoopAdditionExpression.RunBreak(state, token.Line),
            ContinueToken => LoopAdditionExpression.RunContinue(state, token.Line),

            LetToken => DeclareParser.Run(state, token.Line, false),
            ConstToken => DeclareParser.Run(state, token.Line, true),
            OpeningGenericBracketToken => FunctionCallParser.RunFull(state, token.Line),
            _ => null
        };

        if (result is not null) {
            if (result.Value.TryGetValue(out var value)) {
                return Ok(new[] {value});
            }

            if (result.Value.Error is NoMoreDataError) {
                return Error(result.Value.Error.Line, "Unexpected end of file.");
            }
            
            return result.Value.ToErrorResult();
        }
        
        Result<IExpression[]> arrayResult = token switch {
            _ => throw new UnreachableException()
        };
        
        return arrayResult;
    }

    public static Result<ImmutableArray<IExpression>> ParseBlock(ParserState state, bool includeOpeningBracket) {
        var blockContent = new List<IExpression>();

        if (includeOpeningBracket) {
            Result<OpeningCurlyBracketToken> openingCurlyBracketResult = state.Next<OpeningCurlyBracketToken>();
            if (!openingCurlyBracketResult.IsSuccess) return openingCurlyBracketResult.ToErrorResult();
        }

        while (true) {
            Result<IExpression[]> expressionResult = ParseNextStatement(state);

            if (!expressionResult.TryGetValue(out IExpression[]? expression)) {
                if (expressionResult.Error is InvalidTokenError { Token: ClosingCurlyBracketToken }) break;

                return expressionResult.ToErrorResult();
            }

            blockContent.AddRange(expression);
        }

        return Ok(blockContent.ToImmutableArray());
    }
}