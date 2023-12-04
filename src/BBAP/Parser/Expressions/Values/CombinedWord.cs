using System.Collections.Immutable;
using System.Diagnostics;

namespace BBAP.Parser.Expressions.Values;

public record CombinedWord(int Line, ImmutableArray<string> NameSpace, ImmutableArray<string> Variable) : IExpression {
    public CombinedWordType GetCombinedWordType() {
        if (Variable.Length >= 1) {
            return CombinedWordType.VariableOrFunction;
        }

        return CombinedWordType.TypeOrStaticFunction;
    }
}

public enum CombinedWordType {
    VariableOrFunction,
    TypeOrStaticFunction
}