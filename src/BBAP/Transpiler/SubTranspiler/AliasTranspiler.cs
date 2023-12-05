using BBAP.Parser.Expressions;

namespace BBAP.Transpiler.SubTranspiler;

public class AliasTranspiler {
    public static void Run(AliasExpression aliasExpression, TranspilerState state) {
        AbapBuilder builder = state.Builder;
        builder.Append("ALIASES ");
        builder.Append(aliasExpression.Name);
        builder.Append(" FOR ");
        TypeTranspiler.Run(aliasExpression.SourceType, builder);
        builder.AppendLine('.');
    }
}