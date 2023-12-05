using BBAP.Lexer.Tokens.Keywords;
using BBAP.Lexer.Tokens.Others;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.Results;

namespace BBAP.Parser.SubParsers;

public static class SetParser {
    public static Result<IExpression> Run(ParserState state, VariableExpression variableExpression, SetType setType) {
        Result<NewToken> checkNewResult = state.Next<NewToken>();
        if (checkNewResult.IsSuccess) return SetStructParser.Run(state, variableExpression);

        state.Revert();

        Result<IExpression> valueResult = ValueParser.FullExpression(state, out _, typeof(SemicolonToken));

        if (!valueResult.TryGetValue(out IExpression value)) return valueResult;


        return Ok<IExpression>(new SetExpression(variableExpression.Line, variableExpression, setType, value));
    }
}