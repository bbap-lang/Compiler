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
    
    public static (T first, T second) GetFirstAndSecond<T>(this IEnumerable<T> inputs) {
        using IEnumerator<T> enumerator = inputs.GetEnumerator();

        enumerator.MoveNext();
        T first = enumerator.Current;
        enumerator.MoveNext();
        T second = enumerator.Current;
        
        return (first, second);
    }
}