using BBAP.Results;

namespace BBAP.Types;

public class TypeCollection {
    private Dictionary<string, IType> _types;

    public TypeCollection() {
        var typeAny = new AnyType();
        
        var typeString = new DefaultType("STRING", "STRING", null, SupportedOperator.Plus | SupportedOperator.Equals | SupportedOperator.NotEquals);
        var typeDouble = new DefaultType("DOUBLE", "D", typeString, SupportedOperator.AllMath | SupportedOperator.AllComparison);
        var typeFloat = new DefaultType("FLOAT", "F", typeDouble, SupportedOperator.AllMath | SupportedOperator.AllComparison);
        var typeLong = new DefaultType("LONG", "L", typeFloat, SupportedOperator.AllMath | SupportedOperator.AllComparison);
        var typeInt = new DefaultType("INT", "I", typeLong, SupportedOperator.AllMath | SupportedOperator.AllComparison);
        
        var typeBool = new DefaultType("BOOL", "ABAP_BOOL", null, SupportedOperator.AllBoolean);

        _types = new Dictionary<string, IType>() {
            { "ANY", typeAny },
            { "STRING", typeString },
            { "DOUBLE", typeDouble },
            { "FLOAT", typeFloat },
            { "LONG", typeLong },
            { "INT", typeInt },
            { "BOOL", typeBool },
        };
    }
    
    public Result<IType> Get(int line, string name) {
        if (_types.TryGetValue(name, out IType? type)) {
            return Ok(type);
        }

        return Error(line, $"Type {name} was not defined");
    }

}