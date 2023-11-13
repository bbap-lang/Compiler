using System.Runtime.InteropServices.JavaScript;

namespace BBAP.Results;

public static class Result {
    public static Result<T> Ok<T>(T value) {
        return new Result<T>(value);
    }

    public static Result<ErrorResult> Error(Error error) {
        string stack = string.IsNullOrWhiteSpace(error.Stack) ? GetStack() : error.Stack;
        return new Result<ErrorResult>(error with {Stack = stack});
    }

    public static Result<ErrorResult> Error(int line, string text) {
        string stack = GetStack();
        return new Result<ErrorResult>(new Error(line, text, stack));
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
            if (!results[i].TryGetValue(out T? value)) {
                return results[i].ToErrorResult();
            }

            values[i] = value;
        }

        return Ok(values);
    }
}