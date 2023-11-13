using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Lexer.Tokens.Others;
using BBAP.Lexer.Tokens.Setting;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler;
using BBAP.Results;
using BBAP.Types;
using Error = BBAP.Results.Error;

namespace BBAP.Parser.SubParsers; 

public class SetStructParser {
    public static Result<IExpression> Run(ParserState state, VariableExpression variableToSet) {
        Result<UnknownWordToken> structNameResult = state.Next<UnknownWordToken>();
        if (!structNameResult.TryGetValue(out UnknownWordToken structName)) {
            return structNameResult.ToErrorResult();
        }

        Result<IToken> tempResult = state.Next(typeof(OpeningCurlyBracketToken));
        if (!tempResult.IsSuccess) {
            return tempResult.ToErrorResult();
        }

        Result<ImmutableArray<SetExpression>> fieldsResult = GetFields(state, variableToSet);
        if (!fieldsResult.TryGetValue(out ImmutableArray<SetExpression> fields)) {
            return fieldsResult.ToErrorResult();
        }

        var structSetExpression = new StructSetExpression(structName.Line, variableToSet, fields);
        return Ok<IExpression>(structSetExpression);
    }

    private static Result<ImmutableArray<SetExpression>> GetFields(ParserState state, VariableExpression variableToSet) {
        Result<UnknownWordToken> structNameResult;
        Result<IToken> tempResult;
        var fieldSets = new List<SetExpression>();
        IToken? valueEndToken = null;
        while (valueEndToken is not ClosingCurlyBracketToken) {
            Result<UnknownWordToken> fieldNameResult = state.Next<UnknownWordToken>();
            if (!fieldNameResult.TryGetValue(out UnknownWordToken fieldName)) {
                return fieldNameResult.ToErrorResult();
            }

            tempResult = state.Next(typeof(SetToken));
            if (!tempResult.IsSuccess) {
                return tempResult.ToErrorResult();
            }

            Result<IExpression> valueResult = ValueParser.FullExpression(state, out valueEndToken,
                                                                         typeof(ClosingCurlyBracketToken),
                                                                         typeof(CommaToken));

            if (!valueResult.TryGetValue(out IExpression? value)) {
                return valueResult.ToErrorResult();
            }

            if (value is EmptyExpression) {
                if (valueEndToken is CommaToken) {
                    return Error(valueEndToken.Line, "Invalid Token ',', expected '}'");
                }

                break;
            }

            var variable = new FieldVariable(new UnknownType(), fieldName.Value, variableToSet.Variable);
            var varExpression = new VariableExpression(fieldName.Line, variable);
            var fieldSetExpression = new SetExpression(fieldName.Line, varExpression, SetType.Generic, value);
            fieldSets.Add(fieldSetExpression);
        }

        return Ok(fieldSets.ToImmutableArray());
    }
}