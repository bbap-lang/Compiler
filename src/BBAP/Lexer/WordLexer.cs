using System.Text;
using BBAP.ExtensionMethods;
using BBAP.Lexer.Tokens;
using BBAP.Lexer.Tokens.Boolean;
using BBAP.Lexer.Tokens.Keywords;
using BBAP.Lexer.Tokens.Setting;
using BBAP.Lexer.Tokens.Sql;
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
        state.Revert();
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
            
            Keywords.Not => new NotToken(state.Line),

            Keywords.Select => new SelectToken(state.Line),
            Keywords.From => new FromToken(state.Line),
            Keywords.Where => new WhereToken(state.Line),
            Keywords.Join => new JoinToken(state.Line),
            Keywords.Full => new FullToken(state.Line),
            Keywords.Outer => new OuterToken(state.Line),
            Keywords.Inner => new InnerToken(state.Line),
            Keywords.Left => new LeftToken(state.Line),
            Keywords.Right => new RightToken(state.Line),
            Keywords.On => new OnToken(state.Line),
            Keywords.Order => new OrderToken(state.Line),
            Keywords.By => new ByToken(state.Line),
            Keywords.Ascending => new AscendingToken(state.Line),
            Keywords.Descending => new DescendingToken(state.Line),
            Keywords.Limit => new LimitToken(state.Line),
            Keywords.Like => new LikeToken(state.Line),
            
            _ => new UnknownWordToken(word, state.Line)
        };

        return Ok(token);
    }
}