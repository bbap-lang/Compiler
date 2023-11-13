using BBAP.Parser.Expressions;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.PreTranspiler.SubPreTranspiler; 

public static class AliasPreTranspiler {
    public static Result<IExpression[]> Run(AliasExpression aliasExpression, PreTranspilerState state) {
        IType sourceType = aliasExpression.SourceType.Type;
        
        Result<IType> sourceTypeResult = state.Types.Get(aliasExpression.SourceType.Line, sourceType.Name);

        if (!sourceTypeResult.TryGetValue(out IType? newSourceType)) {
            return sourceTypeResult.ToErrorResult();
        }
        
        var newType = new AliasType(aliasExpression.Name, newSourceType , false);
        state.Types.Add(newType, aliasExpression.Line);

        return new Result<IExpression[]>(Array.Empty<IExpression>());
    }
}