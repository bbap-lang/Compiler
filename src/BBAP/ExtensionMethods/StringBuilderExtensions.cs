using System.Text;

namespace BBAP.ExtensionMethods;

public static class StringBuilderExtensions {
    public static void AppendNormalized(this StringBuilder stringBuilder, char c) {
        stringBuilder.Append(c.Normalize());
    }
}