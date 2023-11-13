using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Boolean;
using BBAP.Lexer.Tokens.Comparing;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Lexer.Tokens.Operators;
using BBAP.Lexer.Tokens.Others;
using BBAP.Lexer.Tokens.Setting;
using BBAP.Results;

namespace BBAP.Lexer;

public class Lexer {
    public Result<ImmutableArray<IToken>> Run(string inputString) {
        var tokens = new List<IToken>();

        var state = new LexerState(inputString);

        Result<IToken> result;
        IToken? token;
        while (state.TryNext(out char nextChar)) {
            switch (nextChar) {
                case ' ' or '\t' or '\n' or '\r':
                    break;

                case ';':
                    tokens.Add(new SemicolonToken(state.Line));
                    break;

                case '+' or '-' or '*' or '/' or '%':
                    result = MathOperator(state, nextChar);
                    if (!result.TryGetValue(out token)) return result.ToErrorResult();

                    tokens.Add(token);
                    break;

                case >= '0' and <= '9':
                    result = NumberLexer.Run(state, nextChar);
                    if (!result.TryGetValue(out token)) return result.ToErrorResult();

                    tokens.Add(token);
                    break;

                case (>= 'a' and <= 'z') or (>= 'A' and <= 'Z'):
                    result = WordLexer.Run(state, nextChar);
                    if (!result.TryGetValue(out token)) return result.ToErrorResult();

                    tokens.Add(token);
                    break;

                case '=':
                    result = EqualsOperator(state);
                    if (!result.TryGetValue(out token)) return result.ToErrorResult();

                    tokens.Add(token);
                    break;
                case '>' or '<' or '!':
                    result = CompareOperator(state, nextChar);
                    if (!result.TryGetValue(out token)) return result.ToErrorResult();

                    tokens.Add(token);
                    break;

                case '"' or '\'':
                    result = StringLexer.Run(state, nextChar);
                    if (!result.TryGetValue(out token)) return result.ToErrorResult();

                    tokens.Add(token);
                    break;

                case '(':
                    tokens.Add(new OpeningGenericBracketToken(state.Line));
                    break;
                case ')':
                    tokens.Add(new ClosingGenericBracketToken(state.Line));
                    break;
                case '{':
                    tokens.Add(new OpeningCurlyBracketToken(state.Line));
                    break;
                case '}':
                    tokens.Add(new ClosingCurlyBracketToken(state.Line));
                    break;
                case '[':
                    tokens.Add(new OpeningSquareBracketToken(state.Line));
                    break;
                case ']':
                    tokens.Add(new ClosingCurlyBracketToken(state.Line));
                    break;
                case ',':
                    tokens.Add(new CommaToken(state.Line));
                    break;

                case ':':
                    tokens.Add(new ColonToken(state.Line));
                    break;

                case '.':
                    tokens.Add(new DotToken(state.Line));
                    break;

                case '&':
                    result = BooleanOperatorLexer.RunAnd(state);
                    if (!result.TryGetValue(out token)) return result.ToErrorResult();

                    tokens.Add(token);
                    break;

                case '|':
                    result = BooleanOperatorLexer.RunOr(state);
                    if (!result.TryGetValue(out token)) return result.ToErrorResult();

                    tokens.Add(token);
                    break;

                case '^':
                    tokens.Add(new XorToken(state.Line));
                    break;

                default:
                    return Error(state.Line, $"Unknown character: '${nextChar}'");
            }
        }

        return Ok(tokens.Where(x => x is not EmptyToken).ToImmutableArray());
    }

    private Result<IToken> EqualsOperator(LexerState state) {
        if (!state.TryNext(out char nextChar)) return Error(state.Line, "Unexpected end of file");

        if (nextChar == '=') return Ok<IToken>(new EqualsToken(state.Line));

        state.Revert();
        return Ok<IToken>(new SetToken(state.Line));
    }

    private Result<IToken> CompareOperator(LexerState state, char op) {
        if (!state.TryNext(out char nextChar)) return Error(state.Line, "Unexpected end of file");

        if (nextChar == '=')
            return op switch {
                '>' => Ok<IToken>(new MoreThenOrEqualsToken(state.Line)),
                '<' => Ok<IToken>(new LessThenOrEqualsToken(state.Line)),
                '!' => Ok<IToken>(new NotEqualsToken(state.Line)),
                _ => throw new UnreachableException("This should never happen")
            };

        state.Revert();
        return op switch {
            '>' => Ok<IToken>(new MoreThenToken(state.Line)),
            '<' => Ok<IToken>(new LessThenToken(state.Line)),
            '!' => Ok<IToken>(new NotToken(state.Line)),
            _ => throw new UnreachableException("This should never happen")
        };
    }

    private Result<IToken> MathOperator(LexerState state, char op) {
        if (!state.TryNext(out char nextChar)) return Error(state.Line, "Unexpected end of file");

        if (op == '/' && nextChar is '/' or '*') return Comment(state, nextChar);

        if (nextChar == '=')
            return op switch {
                '+' => Ok<IToken>(new PlusEqualsToken(state.Line)),
                '-' => Ok<IToken>(new MinusEqualsToken(state.Line)),
                '*' => Ok<IToken>(new MultiplyEqualsToken(state.Line)),
                '/' => Ok<IToken>(new DivideEqualsToken(state.Line)),
                '%' => Ok<IToken>(new ModuloEqualsToken(state.Line)),
                _ => throw new UnreachableException("This should never happen")
            };

        if (op == '+' && nextChar == '+') return Ok<IToken>(new IncrementToken(state.Line));

        if (op == '-' && nextChar == '-') return Ok<IToken>(new DecrementToken(state.Line));

        state.Revert();
        return op switch {
            '+' => Ok<IToken>(new PlusToken(state.Line)),
            '-' => Ok<IToken>(new MinusToken(state.Line)),
            '*' => Ok<IToken>(new MultiplyToken(state.Line)),
            '/' => Ok<IToken>(new DivideToken(state.Line)),
            '%' => Ok<IToken>(new ModuloToken(state.Line)),
            _ => throw new UnreachableException("This should never happen")
        };
    }

    private static Result<IToken> Comment(LexerState state, char commentType) {
        switch (commentType) {
            case '/': {
                while (state.TryNext(out char nextChar) && nextChar != '\n') { }

                break;
            }
            case '*': {
                char lastChar = '\0';
                while (state.TryNext(out char nextChar) && !(lastChar == '*' && nextChar == '/')) {
                    lastChar = nextChar;
                }

                if (state.AtEnd)
                    return Error(state.Line,
                                 "File ends inside a block comment. Please make sure all open block comments are closed!");

                break;
            }

            default:
                throw new UnreachableException("Something went really wrong!");
        }

        return Ok<IToken>(new EmptyToken());
    }
}