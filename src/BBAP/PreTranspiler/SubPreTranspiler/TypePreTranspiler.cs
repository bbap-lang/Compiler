﻿using System.Diagnostics;
using BBAP.Results;
using BBAP.Types;
using BBAP.Types.Types.ParserTypes;

namespace BBAP.PreTranspiler.SubPreTranspiler;

public class TypePreTranspiler {
    public static Result<IType> Run(IType rawType, PreTranspilerState state, int line) {
        return rawType switch {
            OnlyNameType onlyNameType => state.Types.Get(line, onlyNameType.Name),
            OnlyNameGenericType onlyNameGenericType => state.Types.GetGenericType(line, onlyNameGenericType.Name,
                                                                                onlyNameGenericType.GenericType.Name),
            OnlyNameLengthType typeWithLength => state.Types.GetLengthType(line, typeWithLength.Name,
                                                                           typeWithLength.Length),
            _ => throw new UnreachableException()
        };
    }
}