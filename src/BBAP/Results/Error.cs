namespace BBAP.Results;

public record Error(int Line, string Text, string Stack = "");