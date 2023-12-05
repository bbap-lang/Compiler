namespace BBAP.Results;

public record NoMoreDataError() : Error(0, string.Empty) {
    public override string ToString() {
        return "Unexpected end of file";
    }
}