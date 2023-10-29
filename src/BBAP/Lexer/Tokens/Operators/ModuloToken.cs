namespace BBAP.Lexer.Tokens.Operators;

public class ModuloToken : IToken {
    public ModuloToken(int line) {
        Line = line;
    }

    public int Line { get; }
}