using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Values;
using BBAP.Results;

namespace BBAP.Lexer;

public static class NumberLexer {
    private static int ParseInt(char c) {
        return c - '0';
    }

    public static Result<IToken> Run(LexerState state, char num) {
        int beforeDot = ParseInt(num);
        int afterDot = 0;
        bool includesDot = false;

        while (state.TryNext(out char nextChar)) {
            switch (nextChar) {
                case >= '0' and <= '9':
                    AddToNumber(nextChar, includesDot, ref afterDot, ref beforeDot);
                    break;
                case '.':
                    if (includesDot) return Error(state.Line, "Unexpected second '.' in number.");

                    includesDot = true;
                    break;
                case '_':
                // Underscores get ignored inside a number
                default:
                    goto EndParseNumber;
            }
        }

        EndParseNumber:
        state.SkipNext();

        IToken token = includesDot
            ? new FloatValueToken(beforeDot + CalculateAfterDot(afterDot), state.Line)
            : new IntValueToken(beforeDot, state.Line);

        return Ok(token);
    }

    private static double CalculateAfterDot(int afterDot) {
        int digits = 0;

        int num = 0;
        while (num != 0) {
            digits++;
            num /= 10;
        }

        double value = 0;
        for (int i = 0; i < digits; i++) {
            value += (afterDot % 10) * Math.Pow(10, i + 1);

            afterDot /= 10;
        }

        return value;
    }
    
    

    private static void AddToNumber(char nextChar, bool includesDot, ref int afterDot, ref int beforeDot) {
        int adder = ParseInt(nextChar);
        if (includesDot) {
            afterDot *= 10;
            afterDot += adder;
        } else {
            beforeDot *= 10;
            beforeDot += adder;
        }
    }
}