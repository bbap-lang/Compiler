using BBAP.Parser.Expressions;
using BBAP.Types;

namespace BBAP.PreTranspiler.Expressions; 

public interface ISecondStageValue: IExpression {
    public IType Type { get; }
}