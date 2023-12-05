﻿using System.Diagnostics;
using BBAP.Parser.Expressions;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.PreTranspiler.SubPreTranspiler; 

public static class AliasPreTranspiler {
    public static Result<int> Create(AliasExpression aliasExpression, PreTranspilerState state) {
        var aliasType = new OnlyNameType(aliasExpression.Name);
        
        Result<int> addTypeResult = state.Types.Add(aliasType, aliasExpression.Line);
        if (!addTypeResult.IsSuccess) {
            return addTypeResult.ToErrorResult();
        }
        
        return Ok();
    }
    
    public static Result<int> PostCreate(AliasExpression aliasExpression, PreTranspilerState state) {
        IType sourceType = aliasExpression.SourceType.Type;

        Result<IType> sourceTypeResult
            = TypePreTranspiler.Run(aliasExpression.SourceType.Type, state, aliasExpression.SourceType.Line);

        if (!sourceTypeResult.TryGetValue(out IType? newSourceType)) {
            return sourceTypeResult.ToErrorResult();
        }

        Result<IType> oldTypeResult = state.Types.Get(aliasExpression.Line, aliasExpression.Name);
        if(!oldTypeResult.TryGetValue(out IType? oldType)) {
            throw new UnreachableException();
        }
        
        state.Types.Remove(oldType);
        
        var aliasType = new AliasType(aliasExpression.Name, newSourceType , aliasExpression.IsPublic);
        Result<int> addTypeResult = state.Types.Add(aliasType, aliasExpression.Line);
        if (!addTypeResult.IsSuccess) {
            throw new UnreachableException();
        }
        
        return Ok();
    }
    
    
    public static Result<IExpression[]> Replace(AliasExpression aliasExpression, PreTranspilerState state) {
        if (!aliasExpression.IsPublic) {
            return new Result<IExpression[]>(Array.Empty<IExpression>());
        }
        
        Result<IType> savedTypeResult = state.Types.Get(aliasExpression.Line, aliasExpression.Name);
        if (!savedTypeResult.TryGetValue(out IType? savedType)) {
            throw new UnreachableException();
        }
        
        if(savedType is not AliasType aliasType) {
            throw new UnreachableException();
        }

        AliasExpression newAliasExpression = aliasExpression with { SourceType = aliasExpression.SourceType with { Type = aliasType.SourceType } };
        return new Result<IExpression[]>(new IExpression[]{ newAliasExpression });

    }
}