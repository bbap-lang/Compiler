namespace BBAP.Results;

public static class Result {
    public static Result<T> Ok<T>(T value) {
        return new Result<T>(value);
    }

    public static Result<ErrorResult> Error(Error error) {
        return new Result<ErrorResult>(error);
    }

    public static Result<ErrorResult> Error(int line, string text) {
        return new Result<ErrorResult>(new Error(line, text));
    }
}