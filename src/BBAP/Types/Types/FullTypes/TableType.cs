﻿using System.Diagnostics;

namespace BBAP.Types.Types.FullTypes;

public record TableType(IType ContentType, TableTypes Type) : IType {
    public string Name => $"{Type}<{ContentType.Name}>";
    public string AbapName => ContentType.Name;
    public IType? InheritsFrom => TypeCollection.AnyTableType;
    public SupportedOperator SupportedOperators => SupportedOperator.None;

    public string DeclareKeyWord => Type switch {
        TableTypes.StandardTable => "TYPE STANDARD TABLE OF",
        TableTypes.SortedTable => "TYPE SORTED TABLE OF",
        TableTypes.HashedTable => "TYPE STANDARD TABLE OF",
        _ => throw new UnreachableException()
    };
}

public enum TableTypes {
    StandardTable,
    SortedTable,
    HashedTable
}