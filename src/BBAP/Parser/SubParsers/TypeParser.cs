using System.Diagnostics;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Comparing;
using BBAP.Lexer.Tokens.Grouping;
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

        var addtionalDataResult = state.Next(typeof(LessThenToken), typeof(OpeningSquareBracketToken));
        
        if(!addtionalDataResult.TryGetValue(out var addtionalData)) {
            state.Revert();
            var type = new OnlyNameType(typeNameToken.Value);
            var typeExpression = new TypeExpression(typeNameToken.Line, type);
            return Ok(typeExpression);
        }

        return addtionalData switch {
            LessThenToken => RunGeneric(state, typeNameToken),
            OpeningSquareBracketToken => RunLength(state, typeNameToken),
            _ => throw new UnreachableException()
        };
    }

    private static Result<TypeExpression> RunLength(ParserState state, UnknownWordToken typeNameToken) {
        Result<IntValueToken> lengthResult = state.Next<IntValueToken>();
        if (!lengthResult.TryGetValue(out IntValueToken? lengthToken)) {
            return lengthResult.ToErrorResult();
        }

        Result<ClosingSquareBracketToken> closingBracketResult = state.Next<ClosingSquareBracketToken>();
        if (!closingBracketResult.IsSuccess) {
            return closingBracketResult.ToErrorResult();
        }

        var lengthType = new OnlyNameLengthType(typeNameToken.Value, lengthToken.Value);
        var lengthTypeExpression = new TypeExpression(typeNameToken.Line, lengthType);
        return Ok(lengthTypeExpression);
    }

    private static Result<TypeExpression> RunGeneric(ParserState state, UnknownWordToken typeNameToken) {
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