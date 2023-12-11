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
using BBAP.Results;

namespace BBAP.Parser.SubParsers;

public static class UnknownWordParser {
    public static Result<IExpression> RunRoot(ParserState state, UnknownWordToken unknownWordToken) {
        Result<CombinedWord> combinedWordResult = ParseWord(state, unknownWordToken);
        if (!combinedWordResult.TryGetValue(out CombinedWord? combinedWord)) return combinedWordResult.ToErrorResult();

        Result<IToken> nextTokenResult;
        IToken? nextToken;
        if (combinedWord.GetCombinedWordType() == CombinedWordType.VariableOrFunction) {
            nextTokenResult = state.Next(typeof(OpeningGenericBracketToken), typeof(SetToken),
                                         typeof(PlusEqualsToken), typeof(MinusEqualsToken),
                                         typeof(MultiplyEqualsToken), typeof(DivideEqualsToken),
                                         typeof(ModuloEqualsToken), typeof(IncrementToken), typeof(DecrementToken));

            if (!nextTokenResult.TryGetValue(out nextToken)) return nextTokenResult.ToErrorResult();

            if (nextToken is OpeningGenericBracketToken) {
                Result<FunctionCallExpression> functionCallResult = FunctionCallParser.Run(state, combinedWord);
                if (!functionCallResult.TryGetValue(out FunctionCallExpression? functionCall)) return functionCallResult.ToErrorResult();

                state.SkipSemicolon();
                return Ok<IExpression>(functionCall);
            }

            VariableExpression variable = VariableParser.Run(combinedWord);

            Result<IExpression> result = nextToken switch {
                SetToken => SetParser.Run(state, variable, SetType.Generic),
                PlusEqualsToken => SetParser.Run(state, variable, SetType.Plus),
                MinusEqualsToken => SetParser.Run(state, variable, SetType.Minus),
                MultiplyEqualsToken => SetParser.Run(state, variable, SetType.Multiplication),
                DivideEqualsToken => SetParser.Run(state, variable, SetType.Devide),
                ModuloEqualsToken => SetParser.Run(state, variable, SetType.Modulo),
                IncrementToken => IncrementParser.Run(state, variable, IncrementType.Plus),
                DecrementToken => IncrementParser.Run(state, variable, IncrementType.Minus),
                _ => throw new UnreachableException()
            };
            if (!result.IsSuccess) return result;

            state.SkipSemicolon();
            return result;
        }

        nextTokenResult = state.Next(typeof(OpeningGenericBracketToken));

        if (!nextTokenResult.TryGetValue(out nextToken)) return nextTokenResult.ToErrorResult();

        if (nextToken is OpeningGenericBracketToken) {
            Result<FunctionCallExpression> functionCallResult = FunctionCallParser.Run(state, combinedWord);
            if (!functionCallResult.TryGetValue(out var functionCall)) return functionCallResult.ToErrorResult();
            state.SkipSemicolon();
            return Ok<IExpression>(functionCall);
        }

        throw new UnreachableException();
    }

    public static Result<IExpression> RunValue(ParserState state, UnknownWordToken unknownWordToken) {
        Result<CombinedWord> combinedWordResult = ParseWord(state, unknownWordToken);
        if (!combinedWordResult.TryGetValue(out CombinedWord? combinedWord)) return combinedWordResult.ToErrorResult();

        Result<OpeningGenericBracketToken> bracketOpenResult = state.Next<OpeningGenericBracketToken>();

        if (!bracketOpenResult.IsSuccess && bracketOpenResult.Error is not InvalidTokenError)
            return bracketOpenResult.ToErrorResult();

        if (bracketOpenResult.Error is InvalidTokenError) state.Revert();

        if (bracketOpenResult.IsSuccess) {
            Result<FunctionCallExpression> functionCallResult = FunctionCallParser.Run(state, combinedWord);
            if (!functionCallResult.TryGetValue(out FunctionCallExpression? functionCall)) return functionCallResult.ToErrorResult();
            
            return Ok<IExpression>(functionCall);
        }

        return Ok<IExpression>(VariableParser.Run(combinedWord));
    }

    public static Result<CombinedWord> ParseWord(ParserState state, UnknownWordToken unknownWord) {
        int line = unknownWord.Line;

        List<string> nameSpace = new();
        List<string> variables = new();

        var currentStep = Step.Init;
        IToken? nextToken;
        do {
            if (currentStep != Step.Init) {
                Result<UnknownWordToken> nextWordResult = state.Next<UnknownWordToken>();

                if (!nextWordResult.TryGetValue(out unknownWord)) return nextWordResult.ToErrorResult();
            }

            Result<IToken> nextTokenResult = state.Next(typeof(DotToken), typeof(DoubleColonToken));

            if (!nextTokenResult.TryGetValue(out nextToken) && nextTokenResult.Error is not InvalidTokenError)
                return nextTokenResult.ToErrorResult();

            if (nextToken is DotToken) {
                variables.Add(unknownWord.Value);
                if (currentStep == Step.Init || currentStep == Step.Namespace) currentStep = Step.Variable;
            } else if (nextToken is DoubleColonToken) {
                if (currentStep == Step.Init) currentStep = Step.Namespace;

                if (currentStep == Step.Variable)
                    return Error(nextToken.Line, "A double colon can only be used for namespaces");

                nameSpace.Add(unknownWord.Value);
            } else {
                if (currentStep == Step.Init) currentStep = Step.Variable;

                if (currentStep == Step.Variable) variables.Add(unknownWord.Value);

                if (currentStep == Step.Namespace) nameSpace.Add(unknownWord.Value);

                state.Revert();
                break;
            }
        } while (nextToken is DotToken or DoubleColonToken);

        return Ok(new CombinedWord(line, nameSpace.ToImmutableArray(), variables.ToImmutableArray()));
    }

    public static Result<CombinedWord> ParseWord(ParserState state) {
        Result<UnknownWordToken> unknownWordTokenResult = state.Next<UnknownWordToken>();
        if (!unknownWordTokenResult.TryGetValue(out UnknownWordToken? unknownWordToken))
            return unknownWordTokenResult.ToErrorResult();

        return ParseWord(state, unknownWordToken);
    }

    private enum Step {
        Init,
        Namespace,
        Variable
    }
}