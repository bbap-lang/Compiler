using BBAP.Types.Types.FullTypes;

namespace BBAP.Types;

public partial class TypeCollection {
    public static IType AnyType = new AnyType();
    public static IType AnyTableType = new AnyTableType(AnyType);
    
    public static IType StringType = new DefaultType(Keywords.String, "STRING", AnyType,
                                                     SupportedOperator.Plus
                                                   | SupportedOperator.Equals
                                                   | SupportedOperator.NotEquals);

    public static IType BaseCharType = new BaseCharType(StringType);
}