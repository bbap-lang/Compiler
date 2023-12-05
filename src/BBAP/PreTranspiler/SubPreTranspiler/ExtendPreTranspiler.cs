using BBAP.Parser.Expressions;
using BBAP.PreTranspiler.Expressions;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.PreTranspiler.SubPreTranspiler;

public class ExtendPreTranspiler {
    public static Result<SecondStageFunctionExpression[]>
        Create(ExtendExpression expression, PreTranspilerState state) {
        Result<IType> typeResult = state.Types.Get(expression.Type.Line, expression.Type.Type.Name);
        if (!typeResult.TryGetValue(out IType? type)) return typeResult.ToErrorResult();

        Result<SecondStageFunctionExpression[]> newFunctionsResult = expression
                                                                     .Functions
                                                                     .Select(func
                                                                                 => FunctionPreTranspiler
                                                                                     .Create(func, type, state))
                                                                     .Wrap();

        if (!newFunctionsResult.TryGetValue(out SecondStageFunctionExpression[]? newFunctions))
            return newFunctionsResult.ToErrorResult();

        return Ok(newFunctions);
    }

    public static Result<IExpression[]> Replace(ExtendExpression expression, PreTranspilerState state) {
        Result<IType> typeResult = state.Types.Get(expression.Type.Line, expression.Type.Type.Name);
        if (!typeResult.TryGetValue(out IType? type)) return typeResult.ToErrorResult();

        Result<IExpression[][]> newFunctionsResult = expression
                                                     .Functions
                                                     .Select(func => FunctionPreTranspiler.Replace(func, type, state))
                                                     .Wrap();

        if (!newFunctionsResult.TryGetValue(out IExpression[][]? abstractNewFunction))
            return newFunctionsResult.ToErrorResult();

        IExpression[] newFunctions = abstractNewFunction.SelectMany(x => x).ToArray();

        return Ok(newFunctions);
    }
}