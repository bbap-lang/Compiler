using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Lexer.Tokens.Others;
using BBAP.Lexer.Tokens.Setting;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.Results;
using BBAP.Types.Types.ParserTypes;

namespace BBAP.Parser.SubParsers; 

public class EnumParser {
    public static Result<IExpression> Run(ParserState state, int line) {
        Result<UnknownWordToken> nameResult = state.Next<UnknownWordToken>();
        if (!nameResult.TryGetValue(out UnknownWordToken? nameToken)) return nameResult.ToErrorResult();

        TypeExpression type;
        bool requiresValue = false;
        
        Result<ColonToken> colonResult = state.Next<ColonToken>();
        if (colonResult.IsSuccess) {
            Result<TypeExpression> typeResult = TypeParser.Run(state);
            if (!typeResult.TryGetValue(out type)) return typeResult.ToErrorResult();
            requiresValue = true;
        } else {
            state.Revert();
            type = new TypeExpression(line, new OnlyNameType(Keywords.Int));
        }
        
        Result<OpeningCurlyBracketToken> openingBracketResult = state.Next<OpeningCurlyBracketToken>();
        if (!openingBracketResult.TryGetValue(out OpeningCurlyBracketToken? openingBracketToken)) return openingBracketResult.ToErrorResult();
        
        var values = new Dictionary<string, IExpression>();

        while (true) {
            Result<UnknownWordToken> valueNameResult = state.Next<UnknownWordToken>();
            if (!valueNameResult.TryGetValue(out UnknownWordToken? valueName)) return valueNameResult.ToErrorResult();
            
            Result<SetToken> setTokenResult = state.Next<SetToken>();
            if (!setTokenResult.IsSuccess && requiresValue)
                return Error(valueName.Line, "Value can't be attained from usage.");
            
            if (setTokenResult.IsSuccess) {
                requiresValue = true;
                Result<IExpression> valueResult = ValueParser.FullExpression(state, out IToken lastToken, typeof(CommaToken), typeof(ClosingCurlyBracketToken));
                if (!valueResult.TryGetValue(out IExpression? value)) return valueResult.ToErrorResult();
                
                values.Add(valueName.Value, value);
                
                if (lastToken is ClosingCurlyBracketToken) break;
            } else {
                state.Revert();
                values.Add(valueName.Value, new IntExpression(valueName.Line, values.Count));
                
                Result<ClosingCurlyBracketToken> closingBracketResult = state.Next<ClosingCurlyBracketToken>();
                if (closingBracketResult.IsSuccess) break;
                
                state.Revert();
                Result<CommaToken> commaResult = state.Next<CommaToken>();
                if (!commaResult.IsSuccess) return commaResult.ToErrorResult();
            }
        }
        
        return Ok<IExpression>(new EnumExpression(line, nameToken.Value, type, values));
    }
}