using System.Diagnostics;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Others;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions;
using BBAP.Results;

namespace BBAP.Parser.SubParsers; 

public class AliasParser {
    public static Result<IExpression> Run(ParserState state) {
        Result<IToken> nameResult = state.Next(typeof(UnknownWordToken));
        if (!nameResult.TryGetValue(out IToken? nameToken)) {
            return nameResult.ToErrorResult();
        }
        
        if(nameToken is not UnknownWordToken name) {
            throw new UnreachableException();
        }

        Result<IToken> tmpTokenResult = state.Next(typeof(ColonToken));

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