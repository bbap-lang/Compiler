namespace BBAP.Lexer.Tokens.Keywords;

public class ForToken : IToken {
    public const string Name = "FOR";

    public ForToken(int line) {
        Line = line;
    }

    public int Line { get; }
}