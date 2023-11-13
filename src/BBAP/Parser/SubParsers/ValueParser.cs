using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Boolean;
using BBAP.Lexer.Tokens.Comparing;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Lexer.Tokens.Operators;
using BBAP.Lexer.Tokens.Others;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Calculations;
using BBAP.Parser.Expressions.Values;
using BBAP.Parser.ExtensionMethods;
using BBAP.Results;

namespace BBAP.Parser.SubParsers;

public class ValueParser {
    private static Result<IExpression> NextValue(ParserState state, Type[] endTokenTypes) {
        var nextTokenResult = state.Next(typeof(OpeningGenericBracketToken),
            typeof(UnknownWordToken),
            typeof(IntValueToken), typeof(FloatValueToken), typeof(StringValueToken), typeof(NotToken), typeof(MinusToken), typeof(BooleanValueToken));

        if (!nextTokenResult.TryGetValue(out IToken? nextToken)) {
            return nextTokenResult.ToErrorResult();
        }

        Result<IExpression> result = nextToken switch {
            OpeningGenericBracketToken => FullExpression(state, out _, typeof(ClosingGenericBracketToken)),
            UnknownWordToken unknownWordToken => UnknownWordParser.RunValue(state, unknownWordToken),
            IntValueToken intToken => Ok<IExpression>(new IntExpression(nextToken.Line, intToken.Value)),
            FloatValueToken floatToken => Ok<IExpression>(new FloatExpression(nextToken.Line, floatToken.Value)),
            StringValueToken stringToken => Ok<IExpression>(new StringExpression(nextToken.Line,
                stringToken.Value)),
            BooleanValueToken booleanValueToken => Ok<IExpression>(new BooleanValueExpression(nextToken.Line, booleanValueToken.Value)),
            
            NotToken => NextValue(state, endTokenTypes).TryGetValue(out IExpression? expression, out Error error)
                ? Ok<IExpression>(new NotExpression(nextToken.Line, expression))
                : Error(error),
            MinusToken => NextValue(state, endTokenTypes).TryGetValue(out IExpression? expression, out Error error)
                ? Ok<IExpression>(new NegativeExpression(nextToken.Line, expression))
                : Error(error),
            _ => throw new UnreachableException()
        };

        return result;
    }

    private static Result<IExpression> NextOperator(ParserState state) {
        var nextTokenResult = state.Next(typeof(BitAndToken), typeof(BitOrToken), typeof(DivideToken),
            typeof(MinusToken), typeof(ModuloToken), typeof(MultiplyToken), typeof(PlusToken), typeof(EqualsToken),
            typeof(NotEqualsToken), typeof(MoreThenToken),
            typeof(MoreThenOrEqualsToken), typeof(LessThenToken), typeof(LessThenOrEqualsToken), typeof(AndToken),
            typeof(OrToken), typeof(XorToken));

        if (!nextTokenResult.TryGetValue(out IToken? nextToken)) {
            return nextTokenResult.ToErrorResult();
        }

        Result<IExpression> result = nextToken switch {
            BitAndToken => Ok<IExpression>(
                new EmptyMathCalculationExpression(nextToken.Line, CalculationType.BitwiseAnd)),
            BitOrToken => Ok<IExpression>(
                new EmptyMathCalculationExpression(nextToken.Line, CalculationType.BitwiseOr)),
            DivideToken => Ok<IExpression>(
                new EmptyMathCalculationExpression(nextToken.Line, CalculationType.Divide)),
            MinusToken => Ok<IExpression>(
                new EmptyMathCalculationExpression(nextToken.Line, CalculationType.Minus)),
            ModuloToken => Ok<IExpression>(
                new EmptyMathCalculationExpression(nextToken.Line, CalculationType.Modulo)),
            MultiplyToken => Ok<IExpression>(
                new EmptyMathCalculationExpression(nextToken.Line, CalculationType.Multiply)),
            PlusToken => Ok<IExpression>(
                new EmptyMathCalculationExpression(nextToken.Line, CalculationType.Plus)),

            EqualsToken => Ok<IExpression>(new EmptyComparisonExpression(nextToken.Line, ComparisonType.Equals)),
            NotEqualsToken => Ok<IExpression>(new EmptyComparisonExpression(nextToken.Line,
                ComparisonType.NotEquals)),
            MoreThenToken => Ok<IExpression>(new EmptyComparisonExpression(nextToken.Line,
                ComparisonType.GreaterThen)),
            MoreThenOrEqualsToken => Ok<IExpression>(
                new EmptyComparisonExpression(nextToken.Line, ComparisonType.GreaterThenOrEquals)),
            LessThenToken => Ok<IExpression>(new EmptyComparisonExpression(nextToken.Line,
                ComparisonType.SmallerThen)),
            LessThenOrEqualsToken => Ok<IExpression>(
                new EmptyComparisonExpression(nextToken.Line, ComparisonType.SmallerThenOrEquals)),

            AndToken => Ok<IExpression>(new EmptyBooleanExpression(nextToken.Line, BooleanType.And)),
            OrToken => Ok<IExpression>(new EmptyBooleanExpression(nextToken.Line, BooleanType.Or)),
            XorToken => Ok<IExpression>(new EmptyBooleanExpression(nextToken.Line, BooleanType.Xor)),

            _ => throw new UnreachableException(),
        };

        return result;
    }

    public static Result<IExpression> FullExpression(ParserState state, out IToken lastToken, params Type[] endTokenTypes) {
        var expressions = new List<IExpression>();

        while (true) {
            Result<IExpression> valueResult = NextValue(state, endTokenTypes);
            if (!valueResult.TryGetValue(out IExpression? value)) {
                if (expressions.Count == 0 && valueResult.Error is InvalidTokenError invalidTokenError && endTokenTypes.Contains(invalidTokenError.Token.GetType())) {
                    lastToken = invalidTokenError.Token;
                    return Ok<IExpression>(new EmptyExpression(lastToken.Line));
                }
                
                lastToken = default;
                return valueResult;
            }

            expressions.Add(value);

            Result<IExpression> opResult = NextOperator(state);
            if (!opResult.TryGetValue(out IExpression? op)) {
                if (opResult.Error is InvalidTokenError invalidTokenError && endTokenTypes.Contains(invalidTokenError.Token.GetType())) {
                    lastToken = invalidTokenError.Token;
                    break;
                }

                lastToken = default;
                return opResult;
            }

            expressions.Add(op);
        }

        if (expressions.Count == 1) {
            return Ok(expressions.First());
        }

        CombineMath(expressions, CalculationType.Multiply, CalculationType.Divide, CalculationType.Modulo);
        CombineMath(expressions, CalculationType.Plus, CalculationType.Minus);
        CombineMath(expressions, CalculationType.BitwiseAnd);
        CombineMath(expressions, CalculationType.BitwiseOr);

        CombineComparison(expressions, ComparisonType.SmallerThen, ComparisonType.GreaterThen, ComparisonType.SmallerThenOrEquals, ComparisonType.GreaterThenOrEquals);
        CombineComparison(expressions, ComparisonType.Equals);
        CombineComparison(expressions, ComparisonType.NotEquals);

        CombineBoolean(expressions, BooleanType.Xor);
        CombineBoolean(expressions, BooleanType.And);
        CombineBoolean(expressions, BooleanType.Or);

        if (expressions.Count != 1) {
            throw new UnreachableException();
        }

        return Ok(expressions.First());
    }

    private static void CombineMath(IList<IExpression> expressions, params CalculationType[] type) {
        for (int i = 1; i < expressions.Count; i++) {
            if (expressions[i] is not EmptyMathCalculationExpression emptyMathCalculationExpression) continue;
            if (!type.Contains(emptyMathCalculationExpression.CalculationType)) continue;

            var left = expressions[i - 1];
            var right = expressions[i + 1];

            // remove all previous expressions
            expressions.RemoveAt(i - 1);
            expressions.RemoveAt(i - 1);
            expressions.RemoveAt(i - 1);

            var newExpression = new MathCalculationExpression(emptyMathCalculationExpression.Line, emptyMathCalculationExpression.CalculationType, left, right);
            expressions.Insert(i - 1, newExpression);
        }
    }

    private static void CombineComparison(IList<IExpression> expressions, params ComparisonType[] type) {
        for (int i = 1; i < expressions.Count; i++) {
            if (expressions[i] is not EmptyComparisonExpression emptyComparisonExpression) continue;
            if (!type.Contains(emptyComparisonExpression.ComparisonType)) continue;

            IExpression left = expressions[i - 1];
            IExpression right = expressions[i + 1];

            // remove all previous expressions
            expressions.RemoveAt(i - 1);
            expressions.RemoveAt(i - 1);
            expressions.RemoveAt(i - 1);

            var newExpression = new ComparisonExpression(emptyComparisonExpression.Line, emptyComparisonExpression.ComparisonType, left, right);
            expressions.Insert(i - 1, newExpression);
        }
    }

    private static void CombineBoolean(IList<IExpression> expressions, BooleanType type) {
        for (int i = 1; i < expressions.Count; i++) {
            if (expressions[i] is not EmptyBooleanExpression emptyBooleanExpression) continue;
            if (emptyBooleanExpression.BooleanType != type) continue;

            IExpression left = expressions[i - 1];
            IExpression right = expressions[i + 1];

            // remove all previous expressions
            expressions.RemoveAt(i - 1);
            expressions.RemoveAt(i - 1);
            expressions.RemoveAt(i - 1);

            var newExpression = new BooleanExpression(emptyBooleanExpression.Line, type, left, right);
            expressions.Insert(i - 1, newExpression);
        }
    }
}