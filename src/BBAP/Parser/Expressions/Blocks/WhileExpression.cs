using System.Collections.Immutable;
using BBAP.Parser.Expressions;

namespace BBAP.Parser.Expressions.Blocks; 

public record WhileExpression(int Line, IExpression Condition, ImmutableArray<IExpression> BlockContent) : IExpression;