using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Variables;
using BBAP.Results;
using BBAP.Types.Types.ParserTypes;

namespace BBAP.Parser.SubParsers;

public class VariableParser {
    public static VariableExpression Run(CombinedWord combinedWord) {
        //TODO: Add Namespace Support
        VariableExpression? variableExpression = null;
        IVariable variable = null;

        if(combinedWord.Variable.Length == 0) {
            if (combinedWord.NameSpace.Length != 2) {
                throw new NotImplementedException("Namespace support is not implemented yet.");
            }
            
            string typeName = combinedWord.NameSpace[0];
            string variableName = combinedWord.NameSpace[1];
            
            variable = new StaticVariable(new UnknownType(), variableName, new OnlyNameType(typeName), MutabilityType.Mutable);
            return new VariableExpression(combinedWord.Line, variable);
        }
        
        foreach (string wordToken in combinedWord.Variable) {
            if (variable is not null)
                variable = new FieldVariable(new UnknownType(), wordToken, variable);
            else
                variable = new Variable(new UnknownType(), wordToken, MutabilityType.Mutable);
        }

        variableExpression = new VariableExpression(combinedWord.Line, variable);

        return variableExpression;
    }

    /*
    public static VariableExpression Run(ImmutableArray<UnknownWordToken> variableTokens) {
        int line = variableTokens.Last().Line;
        VariableExpression? variableExpression = null;
        IVariable variable = null;

        foreach (UnknownWordToken variableToken in variableTokens) {
            if (variable is not null) {
                variable = new FieldVariable(new UnknownType(), variableToken.Value, variable);
            } else {
                variable = new Variable(new UnknownType(), variableToken.Value);
            }
        }

        variableExpression = new VariableExpression(line, variable);

        return variableExpression;
    }
*/
    public static Result<VariableExpression> ParseRaw(ParserState state) {
        //Use UnknownWordParser.ParseWord
        Result<CombinedWord> combinedWordResult = UnknownWordParser.ParseWord(state);
        if (!combinedWordResult.TryGetValue(out CombinedWord? combinedWord)) return combinedWordResult.ToErrorResult();

        if (combinedWord.GetCombinedWordType() == CombinedWordType.TypeOrStaticFunction)
            return Error(combinedWord.Line, "Unexpected type expression, expected variable.");

        VariableExpression variableExpression = Run(combinedWord);
        return Ok(variableExpression);
    }
}