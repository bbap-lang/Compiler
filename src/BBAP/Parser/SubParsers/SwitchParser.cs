using System.Collections.Immutable;
using System.Linq.Expressions;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Lexer.Tokens.Keywords;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Parser.Expressions.Values;
using BBAP.Results;
using SwitchExpression = BBAP.Parser.Expressions.Blocks.SwitchExpression;

namespace BBAP.Parser.SubParsers;

public class SwitchParser {
    public static Result<IExpression> Run(ParserState state) {
        Result<VariableExpression> variableResult = VariableParser.ParseRaw(state);
        if (!variableResult.TryGetValue(out VariableExpression? variable)) return variableResult.ToErrorResult();

        var openCurlyBracketResult = state.Next<OpeningCurlyBracketToken>();
        if (!openCurlyBracketResult.TryGetValue(out OpeningCurlyBracketToken? openCurlyBracket))
            return openCurlyBracketResult.ToErrorResult();

        ImmutableArray<IExpression>? defaultCase = null;
        List<CaseExpression> cases = new();
        while (true) {
            Result<DefaultToken> defaultResult = state.Next<DefaultToken>();
            if (defaultResult.TryGetValue(out DefaultToken? defaultToken)) {
                if (defaultCase is not null) {
                    return Error(defaultToken.Line, "Default case already defined.");
                }
                
                Result<ImmutableArray<IExpression>> defaultBlockResult = Parser.ParseBlock(state, true);
                if (!defaultBlockResult.TryGetValue(out ImmutableArray<IExpression> defaultBlock)) return defaultBlockResult.ToErrorResult();
                defaultCase = defaultBlock;
            } else {
                state.Revert();

                Result<CaseToken> caseResult = state.Next<CaseToken>();
                if (!caseResult.TryGetValue(out CaseToken? caseToken)) return caseResult.ToErrorResult();

                Result<IExpression> valueResult
                    = ValueParser.FullExpression(state, out _, typeof(OpeningCurlyBracketToken));
                if (!valueResult.TryGetValue(out IExpression? value)) return valueResult;

                Result<ImmutableArray<IExpression>> blockResult = Parser.ParseBlock(state, false);
                if (!blockResult.TryGetValue(out ImmutableArray<IExpression> block)) return blockResult.ToErrorResult();

                var caseExpression = new CaseExpression(caseToken.Line, value, block);
                cases.Add(caseExpression);
            }

            Result<ClosingCurlyBracketToken> nextTokenResult = state.Next<ClosingCurlyBracketToken>();
            if (nextTokenResult.IsSuccess) break;
            state.Revert();
        }
        
        var switchExpression = new SwitchExpression(variable.Line, variable, cases.ToImmutableArray(), defaultCase);
        return Ok<IExpression>(switchExpression);
    }
}