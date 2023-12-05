namespace BBAP;

public static class EnumerableExtensionMethods {
    public static IEnumerable<T> Remove<T>(this IEnumerable<T> enumerable, T element) {
        foreach (T e in enumerable) {
            if (e is null) {
                if (element is null) yield return e;
                continue;
            }

            if (!e.Equals(element)) yield return e;
        }
    }
}