using BBAP.Results;

namespace BBAP.Types;

public class TypeCollection {
    private Dictionary<string, IType> _types = new() {
        { "ANY", new AnyType() },
        {"INT", new DefaultType("INT", "I")},
        {"DOUBLE", new DefaultType("DOUBLE", "D")},
        {"FLOAT", new DefaultType("FLOAT", "F")},
        {"LONG", new DefaultType("LONG", "L")},
        {"BOOL", new DefaultType("BOOL", "ABAP_BOOL")},
        
        {"STRING", new GeneralType("STRING")},
    };

    public Result<IType> Get(int line, string name) {
        if (_types.TryGetValue(name, out IType? type)) {
            return Ok(type);
        }

        return Error(line, $"Type {name} was not defined");
    }

    public Result<IType> Create(int line, string name) {
        if (_types.ContainsKey(name)) {
            return Error(line, $"Type {name} was already defined, please give it another name.");
        }

        var newType = new GeneralType(name);
        _types.Add(name, newType);
        return Ok<IType>(newType);
    }
}