using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using BBAP.ExtensionMethods;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Others;
using BBAP.Parser.Expressions;
using BBAP.Results;

namespace BBAP.Parser; 

public class ParserState {
    private ImmutableArray<IToken> _tokens;
    private int _index = -1;
    
    public ParserState(ImmutableArray<IToken> tokens) {
        _tokens = tokens;
    }

        public Result<T> Next<T>() where T: IToken {
            var nextTokenResult = Next(typeof(T));
            if (!nextTokenResult.TryGetValue(out IToken? token)) {
                return nextTokenResult.ToErrorResult();
            }

            if (token is not T typeToken) {
                throw new UnreachableException();
            }

            return Ok(typeToken);
        }
    
    public Result<IToken> Next(params Type[] validTokenTypes) {
        if (!validTokenTypes.Any(x => x.Implements(typeof(IToken)))) {
            throw new UnreachableException();
        }
        
        _index++;
        if (_index >= _tokens.Length) {
            return Error(new NoMoreDataError());
        }

        IToken token = _tokens[_index];
        Type tokenType = token.GetType();
        return validTokenTypes.Contains(tokenType) 
            ? Ok(token) 
            : Error(new InvalidTokenError(token));
    }

    public void Revert() {
        _index--;
    }

    public bool SkipSemicolon() {
        Result<IToken> semicolonResult = Next(typeof(SemicolonToken));
        if (semicolonResult.IsSuccess) return false;
        Revert();
        return true;

    }
}