namespace BBAP.Lexer;

public class LexerState {
    private readonly string _text;
    private int _position = -1;

    public LexerState(string text) {
        _text = text;
    }

    public bool AtEnd { get; private set; }

    public int Line { get; private set; } = 1;

    public bool TryNext(out char nextChar) {
        _position++;

        if (_position >= _text.Length) {
            nextChar = '\0';
            AtEnd = true;
            return false;
        }

        nextChar = _text[_position];

        if (nextChar == '\n') Line++;

        return true;
    }

    public void Revert() {
        _position--;
    }
}