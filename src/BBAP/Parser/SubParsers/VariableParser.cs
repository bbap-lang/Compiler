using System.Collections.Immutable;
using BBAP.Lexer.Tokens.Others;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.Parser.SubParsers; 

public class VariableParser {
    public static VariableExpression Run(ImmutableArray<UnknownWordToken> variableTokens) {
        int line = variableTokens.Last().Line;
        VariableExpression? variableExpression = null;
        IVariable variable = null;

        foreach (UnknownWordToken variableToken in variableTokens) {
            if (variable is not null) {
                variable = new FieldVariable(new UnknownType(), variableToken.Value, variable);
            } else {
                variable = new Variable(new UnknownType(), variableToken.Value);
            }
        }
        
        variableExpression = new VariableExpression(line, variable);

        return variableExpression;
    }

    public static Result<VariableExpression> ParseRaw(ParserState state) {
        List<UnknownWordToken> variableTokens = new();
        Result<DotToken> dotResult;
        do {
            Result<UnknownWordToken> tokenResult = state.Next<UnknownWordToken>();
            if (!tokenResult.TryGetValue(out UnknownWordToken? variableName)) {
                return tokenResult.ToErrorResult();
            }
                
            variableTokens.Add(variableName);
            
            dotResult = state.Next<DotToken>();
        } while (dotResult.IsSuccess);
        state.Revert();

        VariableExpression variableExpression = Run(variableTokens.ToImmutableArray());
        return Ok(variableExpression);
    }
}