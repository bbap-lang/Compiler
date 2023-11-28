using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Calculations;
using BBAP.Parser.Expressions.Values;
using BBAP.Types;

namespace BBAP.PreTranspiler.Expressions; 

public record SecondStageCalculationExpression(int Line, TypeExpression Type, SecondStageCalculationType CalculationType, IExpression Left, IExpression Right): ISecondStageValue;