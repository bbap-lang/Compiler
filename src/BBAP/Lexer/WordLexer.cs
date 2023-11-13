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
            Keywords.Do => new DoToken(state.Line),
            Keywords.For => new ForToken(state.Line),
            Keywords.If => new IfToken(state.Line),
            Keywords.Else => new ElseToken(state.Line),
            Keywords.Let => new LetToken(state.Line),
            Keywords.While => new WhileToken(state.Line),
            Keywords.Function => new FunctionToken(state.Line),
            
            Keywords.Alias => new AliasToken(state.Line),
            Keywords.Struct => new StructToken(state.Line),
            Keywords.New => new NewToken(state.Line),
            
            Keywords.Return => new ReturnToken(state.Line),

            Keywords.True => new BooleanValueToken(state.Line, true),
            Keywords.False => new BooleanValueToken(state.Line, false),

            _ => new UnknownWordToken(word, state.Line)
        };

        return Ok(token);
    }
}