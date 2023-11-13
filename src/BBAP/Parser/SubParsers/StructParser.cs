using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Grouping;
using BBAP.Lexer.Tokens.Others;
using BBAP.Lexer.Tokens.Values;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Blocks;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler;
using BBAP.Results;

namespace BBAP.Parser.SubParsers; 

public static class StructParser {
    public static Result<IExpression> Run(ParserState state, int line) {
        Result<IToken> nameResult = state.Next(typeof(UnknownWordToken));
        if (!nameResult.TryGetValue(out IToken? nameToken)) {
            return nameResult.ToErrorResult();
        }
        
        if(nameToken is not UnknownWordToken name) {
            throw new UnreachableException();
        }

        Result<IToken> tmpTokenResult = state.Next(typeof(OpeningCurlyBracketToken));
        if (!tmpTokenResult.TryGetValue(out _)) {
            return tmpTokenResult.ToErrorResult();
        }

        List<VariableExpression> fields = new();
        while (true) {
            Result<IToken> nextTokenResult = state.Next(typeof(UnknownWordToken), typeof(ClosingCurlyBracketToken));
            if (!nextTokenResult.TryGetValue(out IToken? nextToken)) {
                return nextTokenResult.ToErrorResult();
            }
            
            if(nextToken is ClosingCurlyBracketToken) {
                break;
            }
            
            if(nextToken is not UnknownWordToken fieldName) {
                throw new UnreachableException();
            }
            
            Result<IToken> colonResult = state.Next(typeof(ColonToken));
            if (!colonResult.TryGetValue(out _)) {
                return colonResult.ToErrorResult();
            }
            
            Result<TypeExpression> typeResult = TypeParser.Run(state);
            if (!typeResult.TryGetValue(out TypeExpression? type)) {
                return typeResult.ToErrorResult();
            }
            
            var newField = new VariableExpression(fieldName.Line, new Variable(type.Type, fieldName.Value));
            fields.Add(newField);

            Result<IToken> endTokenResult = state.Next(typeof(CommaToken), typeof(ClosingCurlyBracketToken));
            if (!endTokenResult.TryGetValue(out IToken? endToken)) {
                return endTokenResult.ToErrorResult();
            }
            
            if(endToken is ClosingCurlyBracketToken) {
                break;
            }
            
        }
        
        var structExpression = new StructExpression(line, name.Value, fields.ToImmutableArray());
        return Ok<IExpression>(structExpression);
    }
}