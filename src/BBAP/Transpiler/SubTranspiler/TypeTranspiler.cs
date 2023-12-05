using BBAP.Parser.Expressions;
using BBAP.Types;

namespace BBAP.Transpiler.SubTranspiler; 

public class TypeTranspiler {
    public static void Run(TypeExpression typeExpression, AbapBuilder builder) => Run(typeExpression.Type, builder);
    
    public static void Run(IType type, AbapBuilder builder) {
        builder.Append(type.DeclareKeyWord);
        builder.Append(' ');
        builder.Append(type.AbapName);
    }
}