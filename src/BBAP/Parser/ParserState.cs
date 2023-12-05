using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.ExtensionMethods;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Others;
using BBAP.Results;

namespace BBAP.Parser;

public class ParserState {
    private int _index = -1;
    private readonly ImmutableArray<IToken> _tokens;

    public ParserState(ImmutableArray<IToken> tokens) {
        _tokens = tokens;
    }

    public Result<T> Next<T>() where T : IToken {
        Result<IToken> nextTokenResult = Next(typeof(T));
        if (!nextTokenResult.TryGetValue(out IToken? token)) {
            Error error = nextTokenResult.Error;
            if (error is not InvalidTokenError invalidTokenError) throw new UnreachableException();

            return Error(invalidTokenError with { CorrectToken = typeof(T) });
        }

        if (token is not T typeToken) throw new UnreachableException();

        return Ok(typeToken);
    }

    public Result<IToken> Next(params Type[] validTokenTypes) {
        if (!validTokenTypes.Any(x => x.Implements(typeof(IToken)))) throw new UnreachableException();

        _index++;
        if (_index >= _tokens.Length) return Error(new NoMoreDataError());

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