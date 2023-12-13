namespace BBAP.Results;

public record NoMoreDataError(int line) : Error(line, string.Empty) {
    public override string ToString() {
        return "Unexpected end of file";
    }
}