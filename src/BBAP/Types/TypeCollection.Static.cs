using BBAP.Types.Types.FullTypes;

namespace BBAP.Types;

public partial class TypeCollection {
    public static IType StringType = new DefaultType(Keywords.String, "STRING", null,
                                                     SupportedOperator.Plus
                                                   | SupportedOperator.Equals
                                                   | SupportedOperator.NotEquals);

    public static IType BaseCharType = new BaseCharType(StringType);
}