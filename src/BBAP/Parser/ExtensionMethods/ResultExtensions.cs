using BBAP.Results;

namespace BBAP.Parser.ExtensionMethods;

public static class ResultExtensions {
    /*
    public static Result<T> ParseErrors<T>(this Result<T> sourceResult) {
        if (sourceResult.IsSuccess) {
            return sourceResult;
        }

        return sourceResult.Error switch {
            NoMoreDataError noMoreDataError => Error(noMoreDataError.Line, "Invalid end of file."),
            InvalidTokenError wrongToken => Error(wrongToken.Line, $"Invalid Token '{wrongToken.Token}'"),
            _ => sourceResult
        };
    }*/
}