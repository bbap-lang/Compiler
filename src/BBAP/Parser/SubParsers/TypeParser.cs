using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Comparing;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.Parser.SubParsers; 

public static class TypeParser {
    public static Result<TypeExpression> Run(ParserState state) {
        Result<UnknownWordToken> nameTokenResult = state.Next<UnknownWordToken>();

        if (!nameTokenResult.TryGetValue(out UnknownWordToken? typeNameToken)) {
            return nameTokenResult.ToErrorResult();
        }

        Result<LessThenToken> startGenericResult = state.Next<LessThenToken>();
        if (!startGenericResult.IsSuccess) {
            state.Revert();
            var type = new OnlyNameType(typeNameToken.Value);
            var typeExpression = new TypeExpression(typeNameToken.Line, type);
            return Ok(typeExpression);
        }

        Result<UnknownWordToken> genericTypeNameResult = state.Next<UnknownWordToken>();
        if(!genericTypeNameResult.TryGetValue(out UnknownWordToken? genericTypeName)) {
            return genericTypeNameResult.ToErrorResult();
        }
        
        Result<MoreThenToken> endGenericResult = state.Next<MoreThenToken>();
        if (!endGenericResult.IsSuccess) {
            return endGenericResult.ToErrorResult();
        }

        var genericType = new OnlyNameType(genericTypeName.Value);
        var fullType = new OnlyNameGenericType(typeNameToken.Value, genericType);
        var genericTypeExpression = new TypeExpression(typeNameToken.Line, fullType);
        return Ok(genericTypeExpression);
    }
}