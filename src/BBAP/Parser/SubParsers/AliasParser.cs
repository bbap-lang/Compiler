using System.Diagnostics;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Others;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions;
using BBAP.Results;

namespace BBAP.Parser.SubParsers; 

public class AliasParser {
    public static Result<IExpression> Run(ParserState state) {
        Result<UnknownWordToken> nameResult = state.Next<UnknownWordToken>();
        if (!nameResult.TryGetValue(out UnknownWordToken? name)) {
            return nameResult.ToErrorResult();
        }
        

        Result<ColonToken> tmpTokenResult = state.Next<ColonToken>();

        if (!tmpTokenResult.TryGetValue(out _)) {
            return tmpTokenResult.ToErrorResult();
        }

        Result<TypeExpression> typeResult = TypeParser.Run(state);
        
        if(!typeResult.TryGetValue(out TypeExpression? typeExpression)) {
            return typeResult.ToErrorResult();
        }
        
        var aliasExpression = new AliasExpression(name.Line, name.Value, typeExpression);

        state.SkipSemicolon();
        
        return Ok<IExpression>(aliasExpression);
    }
}