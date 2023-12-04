using System.Collections.Immutable;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Lexer.Tokens.Others;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Calculations;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.Parser.SubParsers;

public static class IncrementParser {
    public static Result<IExpression> Run(ParserState state,
        VariableExpression variableExpression,
        IncrementType incrementType) {
        int line = variableExpression.Line;

        Result<IToken> result = state.Next(typeof(SemicolonToken));
        if (!result.IsSuccess) {
            return Error(line, "Inline Incrementor are currently not supported.");
        }
        
        var incrementExpression = new IncrementExpression(line, variableExpression, incrementType);

        return Ok<IExpression>(incrementExpression);
    }
}