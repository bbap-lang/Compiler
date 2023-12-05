using BBAP.Lexer.Tokens.Others;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Calculations;
using BBAP.Parser.Expressions.Values;
using BBAP.Results;

namespace BBAP.Parser.SubParsers;

public static class IncrementParser {
    public static Result<IExpression> Run(ParserState state,
        VariableExpression variableExpression,
        IncrementType incrementType) {
        int line = variableExpression.Line;

        Result<SemicolonToken> semicolonResult = state.Next<SemicolonToken>();
        if (!semicolonResult.IsSuccess) return Error(line, "Inline Incrementor are currently not supported.");

        var incrementExpression = new IncrementExpression(line, variableExpression, incrementType);

        return Ok<IExpression>(incrementExpression);
    }
}