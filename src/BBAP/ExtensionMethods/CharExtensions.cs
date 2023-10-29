using Microsoft.CodeAnalysis.CSharp;

namespace BBAP.ExtensionMethods;

public static class CharExtensions {
    public static char Normalize(this char c) {
        return char.ToUpper(c);
    }

    public static string Escape(this char c) {
        return SymbolDisplay.FormatLiteral(c.ToString(), false);
    }
}