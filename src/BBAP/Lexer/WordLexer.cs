using System.Text;
using BBAP.ExtensionMethods;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Keywords;
using BBAP.Lexer.Tokens.Values;
using BBAP.Results;

namespace BBAP.Lexer;

public static class WordLexer {
    public static Result<IToken> Run(LexerState state, char nextChar) {
        var wordBuilder = new StringBuilder();
        wordBuilder.AppendNormalized(nextChar);

        while (state.TryNext(out nextChar)) {
            switch (nextChar) {
                case (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or (>= '0' and <= '9') or '_':
                    wordBuilder.AppendNormalized(nextChar);
                    break;
                default:
                    goto EndWordRead;
            }
        }

        EndWordRead:
        state.SkipNext();
        string word = wordBuilder.ToString();

        IToken token = word switch {
            DoToken.Name => new DoToken(state.Line),
            ForToken.Name => new ForToken(state.Line),
            IfToken.Name => new IfToken(state.Line),
            LetToken.Name => new LetToken(state.Line),
            WhileToken.Name => new WhileToken(state.Line),
            FunctionToken.Name => new FunctionToken(state.Line),

            BooleanValueToken.NameTrue => new BooleanValueToken(state.Line, true),
            BooleanValueToken.NameFalse => new BooleanValueToken(state.Line, false),

            _ => new UnknownWordToken(word, state.Line)
        };

        return Ok(token);
    }
}