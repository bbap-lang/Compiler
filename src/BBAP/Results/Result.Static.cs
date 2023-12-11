namespace BBAP.Results;

public static class Result {
    private static Queue<Warning> _warnings = new();
    
    public static Result<T> Ok<T>(T value) {
        return new Result<T>(value);
    }

    public static Result<int> Ok() {
        return new Result<int>(0);
    }

    public static Result<ErrorResult> Error(Error error) {
        string stack = string.IsNullOrWhiteSpace(error.Stack) ? GetStack() : error.Stack;
        return new Result<ErrorResult>(error with { Stack = stack });
    }

    public static Result<ErrorResult> Error(int line, string text) {
        string stack = GetStack();
        return new Result<ErrorResult>(new Error(line, text, stack));
    }

    public static void Warn(int line, string text) {
        _warnings.Enqueue(new Warning(line, text));
    }
    
    public static bool TryGetNextWarning(out Warning? warning) {
        if (_warnings.Count == 0) {
            warning = null;
            return false;
        }

        warning = _warnings.Dequeue();
        return true;
    }

    private static string GetStack() {
        string rawStack = Environment.StackTrace;
        string[] splitted = rawStack.Split('\n');
        return string.Join('\n', splitted[3..]);
    }


    // ExtensionMethods
    public static Result<T[]> Wrap<T>(this Result<T>[] results) {
        var values = new T[results.Length];

        for (int i = 0; i < results.Length; i++) {
            if (!results[i].TryGetValue(out T? value)) return results[i].ToErrorResult();

            values[i] = value;
        }

        return Ok(values);
    }

    public static Result<T[]> Wrap<T>(this IEnumerable<Result<T>> results) {
        return results.ToArray().Wrap();
    }
}