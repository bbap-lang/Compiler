using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Boolean;
using BBAP.Lexer.Tokens.Operators;
using BBAP.Results;

namespace BBAP.Lexer;

public class BooleanOperatorLexer {
    public static Result<IToken> RunAnd(LexerState state) {
        if (!state.TryNext(out char nextChar)) return Error(state.Line, "Unexpected end of file");

        if (nextChar == '&') return Ok<IToken>(new AndToken(state.Line));

        state.Revert();
        return Ok<IToken>(new BitAndToken(state.Line));
    }

    public static Result<IToken> RunOr(LexerState state) {
        if (!state.TryNext(out char nextChar)) return Error(state.Line, "Unexpected end of file");


        if (nextChar == '|') return Ok<IToken>(new OrToken(state.Line));

        state.Revert();
        return Ok<IToken>(new BitOrToken(state.Line));
    }
}