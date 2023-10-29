using BBAP.Parser.Expressions;
using BBAP.Types;

namespace BBAP.PreTranspiler.Expressions; 

public interface ISecondStageValues: IExpression {
    public IType Type { get; }
}