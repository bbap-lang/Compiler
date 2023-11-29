using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using BBAP.ExtensionMethods;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Values;
using BBAP.Results;

namespace BBAP.Lexer;

public static class NumberLexer {
    private static int ParseInt(char c) {
        if(c <= '9')
            return c - '0';
        if (c >= 'A') return c - 'A' + 10;

        throw new UnreachableException();
    }

    public static Result<IToken> Run(LexerState state, char num) {
        long beforeDot = ParseInt(num);
        long afterDot = 0;
        bool includesDot = false;

        var numberType = NumberType.Decimal;

        while (state.TryNext(out char nextChar)) {
            nextChar = nextChar.Normalize();
            switch (nextChar) {
                case 'X':
                    if (beforeDot != 0 || numberType != NumberType.Decimal) {
                        return Error(state.Line,
                                     "Unexpected 'x' in number. If you want to use hexadecimal numbers, use '0x' as prefix.");
                    }

                    numberType = NumberType.Hexadecimal;
                    break;
                
                case >= '0' and <= '9' or >= 'A' and <= 'F':
                    if (numberType == NumberType.Decimal && nextChar == 'B') {
                        if (beforeDot != 0) {
                            return Error(state.Line,
                                         "Unexpected 'b' in number. If you want to use binary numbers, use '0b' as prefix.");
                        }
                        
                        numberType = NumberType.Binary;
                        break;
                    }
                    
                    if (numberType == NumberType.Binary && nextChar > '1') {
                        return Error(state.Line,
                                     $"Unexpected character '{nextChar}'. Number was defined as binary representation.");
                    }
                    
                    if (numberType == NumberType.Decimal && nextChar > '9') {
                        return Error(state.Line,
                                     $"Unexpected character '{nextChar}'. Number was defined as decimal representation.");
                    }

                    AddToNumber(nextChar, includesDot, ref afterDot, ref beforeDot, numberType);
                    break;
                case '.':
                    if (includesDot) return Error(state.Line, "Unexpected second '.' in number.");

                    includesDot = true;
                    break;
                case '_':
                // Underscores get ignored inside a number
                break;
                default:
                    goto EndParseNumber;
            }
        }

        EndParseNumber:
        state.Revert();

        if (numberType != NumberType.Decimal && includesDot) {
            return Error(state.Line,
                         "Unexpected '.' in number. Hexadecimals and binary representations are only supported for Integer.");
        }

        IToken token = includesDot
            ? new FloatValueToken(beforeDot + CalculateAfterDot(afterDot), state.Line)
            : new IntValueToken(beforeDot, state.Line);

        return Ok(token);
    }
    
    private static double CalculateAfterDot(long afterDot) {
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


    private static void AddToNumber(char nextChar,
        bool includesDot,
        ref long afterDot,
        ref long beforeDot,
        NumberType numberType) {
        int adder = ParseInt(nextChar);

        int multiplier = numberType switch {
            NumberType.Binary => 2,
            NumberType.Decimal => 10,
            NumberType.Hexadecimal => 16,
            _ => throw new UnreachableException()
        };
        
        if (includesDot) {
            afterDot *= multiplier;
            afterDot += adder;
        } else {
            beforeDot *= multiplier;
            beforeDot += adder;
        }
    }

    private enum NumberType {
        Decimal,
        Hexadecimal,
        Binary,
    }
}