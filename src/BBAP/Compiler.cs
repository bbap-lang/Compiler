using System.Collections.Immutable;
using System.Text.Json;
using BBAP.Config;
using BBAP.Lexer.Tokens;
using BBAP.Parser.Expressions;
using BBAP.PreTranspiler;
using BBAP.Results;

namespace BBAP;

public class Compiler {
    public static Result<string> Run(string code, string sourceDirectory, ConfigData config) {
        var lexer = new Lexer.Lexer();
        Result<ImmutableArray<IToken>> tokensResult = lexer.Run(code);
        if (!tokensResult.TryGetValue(out ImmutableArray<IToken> tokens)) {
            return tokensResult.ToErrorResult();
        }

        var parser = new Parser.Parser();
        Result<ImmutableArray<IExpression>> treeResult = parser.Run(tokens);
        if (!treeResult.TryGetValue(out ImmutableArray<IExpression> tree)) {
            return treeResult.ToErrorResult();
        }

        string[] abapIncludePaths = config.AbapDependencies ?? Array.Empty<string>();
        abapIncludePaths = abapIncludePaths
                           .Select(path => Path.HasExtension(path) ? path : path + ".bbap")
                           .Select(path => Path.IsPathRooted(path) ? path : Path.Combine(sourceDirectory, path))
                           .ToArray();

        var preTranspilerState = new PreTranspilerState();
        foreach (string path in abapIncludePaths) {
            Result<int> includeResult = RunAbapInclude(path, preTranspilerState);

            if (!includeResult.IsSuccess) {
                return includeResult.ToErrorResult();
            }
        }

        preTranspilerState.FinishInitialization(config.UseScopes);
        var preTranspiler = new PreTranspiler.PreTranspiler();
        Result<ImmutableArray<IExpression>> preTranspiledTreeResult = preTranspiler.Run(tree, preTranspilerState);
        if (!preTranspiledTreeResult.TryGetValue(out ImmutableArray<IExpression> preTranspiledTree)) {
            return preTranspiledTreeResult.ToErrorResult();
        }

        var transpiler = new Transpiler.Transpiler();
        Result<string> outputResult = transpiler.Run(preTranspiledTree);

        return outputResult; 
    }

    private static Result<int> RunAbapInclude(string path, PreTranspilerState state) {
        if (!File.Exists(path)) return Error(0, $"The file '{path}' does not exist.");

        string inputString = File.ReadAllText(path);

        var lexer = new Lexer.Lexer();
        Result<ImmutableArray<IToken>> tokensResult = lexer.Run(inputString);
        if (!tokensResult.TryGetValue(out ImmutableArray<IToken> tokens)) return tokensResult.ToErrorResult();

        var parser = new Parser.Parser();
        Result<ImmutableArray<IExpression>> treeResult = parser.Run(tokens);
        if (!treeResult.TryGetValue(out ImmutableArray<IExpression> tree)) return treeResult.ToErrorResult();

        var preTranspiler = new PreTranspiler.PreTranspiler();
        Result<ImmutableArray<IExpression>> preTranspiledTreeResult = preTranspiler.Run(tree, state);
        if (!preTranspiledTreeResult.TryGetValue(out _)) return preTranspiledTreeResult.ToErrorResult();

        return Ok();
    }
    
}