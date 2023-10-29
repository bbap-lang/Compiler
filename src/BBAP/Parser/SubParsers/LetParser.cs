using System.Diagnostics;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Others;
using BBAP.Lexer.Tokens.Setting;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.Parser.SubParsers;

public class LetParser {
    public static Result<IExpression> Run(ParserState state) {
        Result<IToken> variableNameResult = state.Next(typeof(UnknownWordToken));
        if (!variableNameResult.TryGetValue(out IToken? variableNameToken)) {
            return variableNameResult.ToErrorResult();
        }
        if (variableNameToken is not UnknownWordToken variableName) {
            throw new UnreachableException();
        }

        Result<IToken> result = state.Next(typeof(SetToken), typeof(ColonToken));

        if (!result.TryGetValue(out IToken? token)) {
            return variableNameResult.ToErrorResult();
        }

        IType type;
        int typeLine = token.Line;
        TypeExpression? typeExpression;
        VariableExpression? variableExpression;
        DeclareExpression? declareExpression;
        if (token is ColonToken) {
            result = state.Next(typeof(UnknownWordToken));

            if (!result.TryGetValue(out token) || token is not UnknownWordToken typeToken) {
                return result.ToErrorResult();
            }

            type = new GeneralType(typeToken.Value);
            typeLine = typeToken.Line;
            

            result = state.Next(typeof(SetToken), typeof(SemicolonToken));

            if (!result.TryGetValue(out token)) {
                return variableNameResult.ToErrorResult();
            }

            if (token is SemicolonToken) {
                typeExpression = new TypeExpression( typeLine, type);
                variableExpression = new VariableExpression(variableName.Line, variableName.Value);
                declareExpression = new DeclareExpression(variableName.Line, variableExpression, typeExpression, null);
                return Ok<IExpression>(declareExpression);
            }
        } else {
            type = new UnknownType();
        }
        
        Result<IExpression> valueResult = ValueParser.FullExpression(state, out _, typeof(SemicolonToken));
        if (!valueResult.TryGetValue(out IExpression? value)) {
            return valueResult;
        }
        
        typeExpression = new TypeExpression( typeLine , type);
        variableExpression = new VariableExpression(variableName.Line, variableName.Value);
        var setExpression = new SetExpression(value.Line, variableExpression, SetType.Generic, value);
        declareExpression = new DeclareExpression(variableName.Line, variableExpression, typeExpression, setExpression);
        return Ok<IExpression>(declareExpression);
    }
}