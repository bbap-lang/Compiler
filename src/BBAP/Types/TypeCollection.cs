using BBAP.Results;

namespace BBAP.Types;

public class TypeCollection {
    private Dictionary<string, IType> _types;

    public TypeCollection() {
        var typeAny = new AnyType();

        var typeString = new DefaultType(Keywords.String, "STRING", null,
                                         SupportedOperator.Plus
                                       | SupportedOperator.Equals
                                       | SupportedOperator.NotEquals);
        
        var typeDouble = new DefaultType(Keywords.Double, "D", typeString,
                                         SupportedOperator.AllMath | SupportedOperator.AllComparison);
        
        var typeFloat = new DefaultType(Keywords.Float, "F", typeDouble,
                                        SupportedOperator.AllMath | SupportedOperator.AllComparison);
        
        var typeLong = new DefaultType(Keywords.Long, "L", typeFloat,
                                       SupportedOperator.AllMath | SupportedOperator.AllComparison);
        
        var typeInt = new DefaultType(Keywords.Int, "I", typeLong,
                                      SupportedOperator.AllMath | SupportedOperator.AllComparison);

        var typeBool = new DefaultType(Keywords.Boolean, "ABAP_BOOL", null, SupportedOperator.AllBoolean);

        _types = new Dictionary<string, IType>() {
            { "ANY", typeAny },
            { Keywords.String, typeString },
            { Keywords.Double, typeDouble },
            { Keywords.Float, typeFloat },
            { Keywords.Long, typeLong },
            { Keywords.Int, typeInt },
            { Keywords.Boolean, typeBool },
        };
    }

    public Result<IType> Get(int line, string name) {
        if (_types.TryGetValue(name, out IType? type)) {
            return Ok(type);
        }

        return Error(line, $"Type {name} was not defined");
    }

    public Result<int> Add(IType type, int line) {
        if (_types.ContainsKey(type.Name)) {
            return Error(line, $"The type {type.Name} already exists.");
        }

        _types.Add(type.Name, type);
        
        return Ok(0);
    }
}