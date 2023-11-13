namespace BBAP.Types; 

public partial class TypeCollection {
    
    public static IType StringType = new DefaultType(Keywords.String, "STRING", null,
                                     SupportedOperator.Plus
                                   | SupportedOperator.Equals
                                   | SupportedOperator.NotEquals);
}