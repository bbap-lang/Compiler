using System.Collections.Immutable;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Lexer.Tokens.Keywords;
using BBAP.Lexer.Tokens.Sql;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Parser.Expressions.Values;
using BBAP.Results;

namespace BBAP.Parser.SubParsers;

public class ForeachParser {
    public static Result<IExpression> Run(ParserState state, int line) {

        Result<OpeningGenericBracketToken> openBracketResult = state.Next<OpeningGenericBracketToken>();
        if(!openBracketResult.IsSuccess) return openBracketResult.ToErrorResult();

        Result<LetToken> declareResult = state.Next<LetToken>();
        if(!declareResult.IsSuccess) state.Revert();

        Result<VariableExpression> variableResult = VariableParser.ParseRaw(state);
        if(!variableResult.TryGetValue(out VariableExpression? variable)) return variableResult.ToErrorResult();
        
        Result<OnToken> onResult = state.Next<OnToken>();
        if(!onResult.IsSuccess) return onResult.ToErrorResult();
        
        Result<VariableExpression> tableResult = VariableParser.ParseRaw(state);
        if(!tableResult.TryGetValue(out VariableExpression? table)) return tableResult.ToErrorResult();
        
        Result<ClosingGenericBracketToken> closeBracketResult = state.Next<ClosingGenericBracketToken>();
        if(!closeBracketResult.IsSuccess) return closeBracketResult.ToErrorResult();

        Result<ImmutableArray<IExpression>> blockResult = Parser.ParseBlock(state, true);
        if(!blockResult.TryGetValue(out ImmutableArray<IExpression> block)) return blockResult.ToErrorResult();

        var foreachExpression = new ForeachExpression(line, declareResult.IsSuccess, variable, table, block);
        return Ok<IExpression>(foreachExpression);
    }
}