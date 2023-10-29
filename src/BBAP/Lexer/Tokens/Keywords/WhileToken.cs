namespace BBAP.Lexer.Tokens.Keywords;

public class WhileToken : IToken {
    public const string Name = "WHILE";

    public WhileToken(int line) {
        Line = line;
    }

    public int Line { get; }
}