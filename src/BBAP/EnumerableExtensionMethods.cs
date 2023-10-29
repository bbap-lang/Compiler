namespace BBAP; 

public static class EnumerableExtensionMethods {
    public static IEnumerable<T> Remove<T>(this IEnumerable<T> enumerable, T element) {
        foreach (var e in enumerable) {
            if (!e.Equals(element)) {
                yield return e;
            }
        }
    }
}