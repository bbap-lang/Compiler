namespace BBAP.ExtensionMethods; 

public static class TypeExtensions {
    public static bool Implements(this Type sourceType, Type interfaceType) {
        return interfaceType.IsAssignableFrom(sourceType);
    }
}