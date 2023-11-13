using System.Diagnostics;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Others;
using BBAP.Lexer.Tokens.Setting;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.Parser.SubParsers;

public static class DeclareParser {
    public static Result<IExpression> Run(ParserState state) {
        Result<IToken> variableNameResult = state.Next(typeof(UnknownWordToken));
        if (!variableNameResult.TryGetValue(out IToken? variableNameToken)) {
            return variableNameResult.ToErrorResult();
        }
        if (variableNameToken is not UnknownWordToken variableName) {
            throw new UnreachableException();
        }

        Result<IToken> result = state.Next(typeof(SetToken), typeof(ColonToken), typeof(SemicolonToken));

        if (!result.TryGetValue(out IToken? token)) {
            return result.ToErrorResult();
        }

        TypeExpression? typeExpression;
        VariableExpression? variableExpression;
        DeclareExpression? declareExpression;
        if (token is ColonToken) {
            Result<TypeExpression> typeResult = TypeParser.Run(state);
            if (!typeResult.TryGetValue(out typeExpression)) {
                return typeResult.ToErrorResult();
            }
            
            result = state.Next(typeof(SetToken), typeof(SemicolonToken));

            if (!result.TryGetValue(out token)) {
                return variableNameResult.ToErrorResult();
            }

            if (token is SemicolonToken) {
                variableExpression = new VariableExpression(variableName.Line, new Variable(new UnknownType(), variableName.Value));
                declareExpression = new DeclareExpression(variableName.Line, variableExpression, typeExpression, null);
                return Ok<IExpression>(declareExpression);
            }
        } else {
            typeExpression = new TypeExpression(variableName.Line, new UnknownType());
        }
        
        Result<IExpression> valueResult = ValueParser.FullExpression(state, out _, typeof(SemicolonToken));
        if (!valueResult.TryGetValue(out IExpression? value)) {
            return valueResult;
        }
        
        variableExpression = new VariableExpression(variableName.Line, new Variable(new UnknownType(), variableName.Value));
        var setExpression = new SetExpression(value.Line, variableExpression, SetType.Generic, value);
        declareExpression = new DeclareExpression(variableName.Line, variableExpression, typeExpression, setExpression);
        return Ok<IExpression>(declareExpression);
    }
}