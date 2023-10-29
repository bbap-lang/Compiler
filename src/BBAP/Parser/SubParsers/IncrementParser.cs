using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Lexer.Tokens.Others;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Calculations;
using BBAP.Parser.Expressions.Values;
using BBAP.Results;

namespace BBAP.Parser.SubParsers; 

public static class IncrementParser {
    public static Result<IExpression> Run(ParserState state, UnknownWordToken variableToken, IncrementType incrementType) {
        Result<IToken> result = state.Next(typeof(SemicolonToken));
        if (!result.IsSuccess) {
            return Error(variableToken.Line, "Inline Incrementor are currently not supported.");
        }

        var variable = new VariableExpression(variableToken.Line, variableToken.Value);
        var incrementExpression = new IncrementExpression(variable.Line, variable, incrementType);
        
        return Ok<IExpression>(incrementExpression);
    }
}