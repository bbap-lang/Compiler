using System.Diagnostics.CodeAnalysis;

namespace BBAP.Results;

public readonly struct Result<T> {
    public static implicit operator Result<T>(Result<ErrorResult> result) {
        return new Result<T>(result.Error);
    }

    private readonly T _value;
    public Error Error { get; }

    public bool IsSuccess { get; }

    public Result(T value) {
        _value = value;
        IsSuccess = true;
    }

    public Result(Error error) {
        Error = error;
        IsSuccess = false;
    }


    public bool TryGetValue([NotNullWhen(true)]out T? value) {
        if (IsSuccess && _value is not null) {
            value = _value;
            return true;
        }

        value = default;
        return false;
    }
    public bool TryGetValue([NotNullWhen(true)]out T? value, out Error error) {
        if (IsSuccess && _value is not null) {
            value = _value;
            error = default;
            return true;
        }

        value = default;
        error = Error;
        return false;
    }

    public Result<ErrorResult> ToErrorResult() {
        return Error(Error);
    }
}