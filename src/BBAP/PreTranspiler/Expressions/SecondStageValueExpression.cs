using BBAP.Parser.Expressions;
using BBAP.Types;

namespace BBAP.PreTranspiler.Expressions; 

public record SecondStageValueExpression(int Line, IType Type, IExpression Value): ISecondStageValue;