using System.Collections.Immutable;
using BBAP.Lexer.Tokens.Sql;
using BBAP.Parser.Expressions.Values;
using BBAP.Results;

namespace BBAP.Parser.SubParsers;

public class OrderByParser {
    public static Result<ImmutableArray<VariableExpression>> Run(ParserState state) {
        List<VariableExpression> variables = new();
        while (true) {
            Result<OrderToken> orderResult = state.Next<OrderToken>();
            if (!orderResult.IsSuccess) {
                break;
            }
            
            Result<ByToken> byResult = state.Next<ByToken>();
            if (!byResult.IsSuccess) {
                return byResult.ToErrorResult();
            }

            Result<VariableExpression> variableResult = VariableParser.ParseRaw(state);
            if (!variableResult.TryGetValue(out VariableExpression? variableExpression)) {
                return variableResult.ToErrorResult();
            }
            variables.Add(variableExpression);
        }
        state.Revert();
        
        return Ok(variables.ToImmutableArray());
    }
}