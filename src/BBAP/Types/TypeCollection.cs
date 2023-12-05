using System.Diagnostics;
using BBAP.Parser.Expressions;
using BBAP.Results;

namespace BBAP.Types;

public partial class TypeCollection {
    private Dictionary<string, IType> _types;

    public TypeCollection() {
        var typeAny = new AnyType();

        IType typeString = StringType;
        IType typeBaseChar = new BaseCharType(typeString);

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
            { Keywords.Char, typeBaseChar},
            { Keywords.Double, typeDouble },
            { Keywords.Float, typeFloat },
            { Keywords.Long, typeLong },
            { Keywords.Int, typeInt },
            { Keywords.Boolean, typeBool },
        };
    }
    
    public Result<IType> GetTableType(int line, string tableName, string genericTypeName) {
        Result<IType> genericTypeResult = Get(line, genericTypeName);
        if (!genericTypeResult.TryGetValue(out IType? genericType)) {
            return genericTypeResult;
        }

        Result<TableTypes> tableTypeResult = tableName switch {
            "TABLE" or "STANDARDTABLE" => Ok(TableTypes.StandardTable),
            "HASHEDTABLE" => Ok(TableTypes.HashedTable),
            "SORTEDTABLE" => Ok(TableTypes.SortedTable),
            _ => Error(line, $"Unknown table type {tableName}"),
        };
        
        if (!tableTypeResult.TryGetValue(out TableTypes tableType)) {
            return tableTypeResult.ToErrorResult();
        }

        return Ok<IType>(new TableType(genericType, tableType));
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

    public void Remove(IType type) {
        if (!_types.ContainsKey(type.Name)) {
            throw new UnreachableException();
        }
        
        _types.Remove(type.Name);
    }
    
    public Result<IType> GetLengthType(int line, string name, long length) {
        switch (name) {
            case Keywords.Char:
                Result<IType> baseCharResult = Get(line, Keywords.Char);
                if (!baseCharResult.TryGetValue(out IType? baseChar)) throw new UnreachableException();
                var charType = new CharType(baseChar, length);
                return Ok<IType>(charType);
            
            default:
                return Error(line, $"The type {name} does not support a length."); //TODO: Support arrays of all kinds
        }
    }
}