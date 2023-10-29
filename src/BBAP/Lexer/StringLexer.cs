using System.Text;
using BBAP.ExtensionMethods;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Values;
using BBAP.Results;

namespace BBAP.Lexer;

public static class StringLexer {
    public static Result<IToken> Run(LexerState state, char quotationMark) {
        var wordBuilder = new StringBuilder();

        while (state.TryNext(out char nextChar)) {
            switch (nextChar) {
                case '\\':
                    Result<char> escapeResult = GetEscapeChar(state, quotationMark);
                    if (!escapeResult.TryGetValue(out char escapedChar)) return escapeResult.ToErrorResult();

                    wordBuilder.Append(escapedChar);
                    break;
                case '\'':
                    if (quotationMark == '\'') goto EndStringRead;

                    wordBuilder.Append(nextChar);
                    break;
                case '"':
                    if (quotationMark == '"') goto EndStringRead;

                    wordBuilder.Append(nextChar);
                    break;


                case '\n' or '\r' or '\b': // Invalid characters
                    return Error(state.Line, $"Invalid character in string '{nextChar.Escape()}'");

                default:
                    wordBuilder.Append(nextChar);
                    break;
            }
        }

        if (state.AtEnd) return Error(state.Line, $"Invalid end of file, expected: ' {quotationMark} '");

        EndStringRead:

        return Ok<IToken>(new StringValueToken(wordBuilder.ToString(), state.Line));
    }

    private static Result<char> GetEscapeChar(LexerState state, char quotationMark) {
        if (!state.TryNext(out char escapeChar))
            return Error(state.Line, $"Invalid end of file, expected: ' {quotationMark} '");

        char resultChar;
        switch (escapeChar) {
            case 'n':
                resultChar = '\n';
                break;
            case 'r':
                resultChar = '\r';
                break;
            case 't':
                resultChar = '\t';
                break;
            case '0':
                resultChar = '\0';
                break;
            case '\b':
                resultChar = '\b';
                break;

            case '\'' or '"' or '\\':
                resultChar = escapeChar;
                break;

            default:
                return Error(state.Line, $"Invalid escape sequence: '\\{escapeChar}'");
        }

        return Ok(resultChar);
    }
}