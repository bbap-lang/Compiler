using BBAP.Lexer.Tokens.Others;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.Parser.SubParsers; 

public static class SetParser {
    public static Result<IExpression> Run(ParserState state, UnknownWordToken variableToken, SetType setType) {
        var valueResult = ValueParser.FullExpression(state, out var endToken, typeof(SemicolonToken));

        if (!valueResult.TryGetValue(out IExpression value)) {
            return valueResult;
        }
        
        var variable = new VariableExpression(variableToken.Line, variableToken.Value, new UnknownType());
        
        return Ok<IExpression>(new SetExpression(variable.Line, variable, setType, value));
    }
}