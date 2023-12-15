using System.Collections.Immutable;
using System.Diagnostics;
using BBAP.PreTranspiler.Variables;
using BBAP.Results;
using BBAP.Types.Types.FullTypes;

namespace BBAP.Types;

public partial class TypeCollection {
    private readonly Dictionary<string, IType> _types;

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

        _types = new Dictionary<string, IType> {
            { "ANY", typeAny },
            { Keywords.String, typeString },
            { Keywords.Char, typeBaseChar },
            { Keywords.Double, typeDouble },
            { Keywords.Float, typeFloat },
            { Keywords.Long, typeLong },
            { Keywords.Int, typeInt },
            { Keywords.Boolean, typeBool }
        };
    }

    public Result<IType> GetTableType(int line, string tableName, string genericTypeName) {
        Result<IType> genericTypeResult = Get(line, genericTypeName);
        if (!genericTypeResult.TryGetValue(out IType? genericType)) return genericTypeResult;

        Result<TableTypes> tableTypeResult = tableName switch {
            "TABLE" or "STANDARDTABLE" => Ok(TableTypes.StandardTable),
            "HASHEDTABLE" => Ok(TableTypes.HashedTable),
            "SORTEDTABLE" => Ok(TableTypes.SortedTable),
            _ => Error(line, $"Unknown table type {tableName}")
        };

        if (!tableTypeResult.TryGetValue(out TableTypes tableType)) return tableTypeResult.ToErrorResult();

        return Ok<IType>(new TableType(genericType, tableType));
    }

    public Result<IType> Get(int line, string name) {
        if (name.Contains('<')) {
            string[] splittedName = name.Split('<');
            string genericTypeName = splittedName[1].TrimEnd('>').ToUpper();
            string tableName = splittedName[0].ToUpper();
            
            return GetTableType(line, tableName, genericTypeName);
        }
        
        if (_types.TryGetValue(name, out IType? type)) return Ok(type);

        return Error(line, $"Type {name} was not defined");
    }

    public Result<int> Add(IType type, int line) {
        if (_types.ContainsKey(type.Name)) return Error(line, $"The type {type.Name} already exists.");

        _types.Add(type.Name, type);

        return Ok(0);
    }

    public void Replace(IType oldType, IType newType) {
        if (newType is TableType) return;

        if (!_types.ContainsKey(oldType.Name) && newType is not TableType) throw new UnreachableException();

        ReplaceAliases(oldType, newType);
        ReplaceStructs(oldType, newType);
        ReplaceTableTypes(oldType, newType);

        _types[oldType.Name] = newType;
    }

    private void ReplaceAliases(IType oldType, IType newType) {
        foreach (AliasType oldAlias in _types.Values.OfType<AliasType>()) {
            ReplaceAlias(oldAlias, oldType, newType);
        }
    }

    private IType? ReplaceStruct(StructType structType, IType oldType, IType? newType) {
        var fieldsToReplace = new List<Variable>();
        foreach (Variable field in structType.Fields) {
            IType? fieldType;
            switch (field.Type) {
                case AliasType aliasType:
                    fieldType = ReplaceAlias(aliasType, oldType, newType);
                    break;
                case StructType innerStruct:
                    fieldType = ReplaceStruct(innerStruct, oldType, newType);
                    break;
                default:
                    continue;
            }

            if (fieldType is null) continue;

            Variable newField = field with { Type = newType };
            fieldsToReplace.Add(newField);
        }

        if (fieldsToReplace.Count == 0) return null;

        ImmutableArray<Variable> newFields = structType.Fields
                                                       .Select(field
                                                                   => fieldsToReplace.FirstOrDefault(x => x.Name
                                                                       == field.Name)
                                                                   ?? field)
                                                       .ToImmutableArray();

        StructType newStruct = structType with { Fields = newFields };
        Replace(structType, newStruct);
        return newStruct;
    }

    private IType? ReplaceAlias(AliasType alias, IType oldType, IType? newType) {
        if (alias.SourceType != oldType)
            switch (alias.SourceType) {
                case AliasType aliasType:
                    newType = ReplaceAlias(aliasType, oldType, newType);
                    break;
                case StructType structType:
                    newType = ReplaceStruct(structType, oldType, newType);
                    break;
                case TableType tableType:
                    newType = ReplaceTableType(tableType, oldType, newType);
                    break;
                default:
                    return null;
            }

        if (newType is null) return null;

        AliasType newAlias = alias with { SourceType = newType };
        Replace(newAlias, newAlias);
        return newAlias;
    }

    private void ReplaceStructs(IType oldType, IType newType) {
        foreach (StructType oldStruct in _types.Values.OfType<StructType>()) {
            ReplaceStruct(oldStruct, oldType, newType);
        }
    }

    private void ReplaceTableTypes(IType oldType, IType newType) {
        foreach (TableType oldTable in _types.Values.OfType<TableType>()) {
            ReplaceTableType(oldTable, oldType, newType);
        }
    }

    private IType? ReplaceTableType(TableType tableType, IType oldType, IType? newType) {
        if (tableType.ContentType != oldType)
            switch (tableType.ContentType) {
                case AliasType aliasType:
                    newType = ReplaceAlias(aliasType, oldType, newType);
                    break;
                case StructType structType:
                    newType = ReplaceStruct(structType, oldType, newType);
                    break;
                default:
                    return null;
            }

        if (newType is null) return null;

        TableType newTable = tableType with { ContentType = newType };
        Replace(tableType, newTable);
        return newTable;
    }

/*
    public void Remove(IType type) {
        if (!_types.ContainsKey(type.Name)) {
            throw new UnreachableException();
        }

        _types.Remove(type.Name);
    }
  */
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