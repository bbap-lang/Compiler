namespace BBAP.Results;

public record NoMoreDataError() : Error(0, string.Empty) {
    public override string ToString() => $"Unexpected end of file";
}