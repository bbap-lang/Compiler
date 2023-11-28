using BBAP.Parser.Expressions;
using BBAP.Types;

namespace BBAP.PreTranspiler.Expressions; 

public record SecondStageValueExpression(int Line, TypeExpression Type, IExpression Value): ISecondStageValue;