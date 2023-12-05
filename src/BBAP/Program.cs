using System.Collections.Immutable;
using System.Text.Json;
using BBAP.Config;
using BBAP.Lexer.Tokens;
using BBAP.Parser.Expressions;
using BBAP.PreTranspiler;
using BBAP.Results;

namespace BBAP;

internal class Program {
    private static int Main(string[] args) {
        if (args.Length < 2) {
            PrintHelp();
            return 0;
        }

        string inputPath = Path.GetFullPath(args[0]);
        string outputPath = Path.GetFullPath(args[1]);

        string sourceDirectory;

        ConfigData config;
        if (Directory.Exists(inputPath)) {
            sourceDirectory = inputPath;
            string configPath = Path.Combine(sourceDirectory, "config.json");
            config = File.Exists(configPath)
                ? JsonSerializer
                    .Deserialize<ConfigData>(File.ReadAllText(configPath))!
                : // TODO: Add error handling
                CreateDefaultConfig();

            inputPath = Path.Combine(config.StartFile);
        } else {
            sourceDirectory = Path.GetDirectoryName(inputPath)!;
            string configPath = Path.Combine(sourceDirectory, "config.json");
            config = File.Exists(configPath)
                ? JsonSerializer.Deserialize<ConfigData>(File.ReadAllText(configPath))!
                : CreateDefaultConfig(); // TODO: Add error handling
            config = config with { StartFile = Path.Combine(sourceDirectory, Path.GetFileName(inputPath)) };
        }

        if (!File.Exists(inputPath)) {
            Console.WriteLine($"The file '{inputPath}' does not exist.");
            return 1;
        }

        if (!Path.HasExtension(outputPath)) outputPath += $"{Path.GetFileNameWithoutExtension(inputPath)}.abap";

        string inputString = File.ReadAllText(inputPath);

        long startTime = Environment.TickCount64;

        var lexer = new Lexer.Lexer();
        Result<ImmutableArray<IToken>> tokensResult = lexer.Run(inputString);
        if (!tokensResult.TryGetValue(out ImmutableArray<IToken> tokens)) {
            PrintError(tokensResult, config, inputPath);
            return 1;
        }

        var parser = new Parser.Parser();
        Result<ImmutableArray<IExpression>> treeResult = parser.Run(tokens);
        if (!treeResult.TryGetValue(out ImmutableArray<IExpression> tree)) {
            PrintError(treeResult, config, inputPath);
            return 1;
        }

        string[] abapIncludePaths = config.AbapDefaults ?? Array.Empty<string>();
        abapIncludePaths = abapIncludePaths
                           .Select(path => Path.HasExtension(path) ? path : path + ".bbap")
                           .Select(path => Path.IsPathRooted(path) ? path : Path.Combine(sourceDirectory, path))
                           .ToArray();

        var preTranspilerState = new PreTranspilerState();
        foreach (string path in abapIncludePaths) {
            Result<int> includeResult = RunAbapInclude(path, preTranspilerState);

            if (!includeResult.IsSuccess) {
                PrintError(includeResult, config, path);
                return 1;
            }
        }

        preTranspilerState.FinishInitialization(config.UseScopes);
        var preTranspiler = new PreTranspiler.PreTranspiler();
        Result<ImmutableArray<IExpression>> preTranspiledTreeResult = preTranspiler.Run(tree, preTranspilerState);
        if (!preTranspiledTreeResult.TryGetValue(out ImmutableArray<IExpression> preTranspiledTree)) {
            PrintError(preTranspiledTreeResult, config, inputPath);
            return 1;
        }

        var transpiler = new Transpiler.Transpiler();
        Result<string> outputResult = transpiler.Run(preTranspiledTree);
        if (!outputResult.TryGetValue(out string? output)) {
            PrintError(outputResult, config, inputPath);
            return 1;
        }

        Console.WriteLine(output);

        long compileTimeMs = Environment.TickCount64 - startTime;
        TimeSpan compileTime = TimeSpan.FromMilliseconds(compileTimeMs);

        Console.WriteLine();
        Console.WriteLine($"In {compileTime.Hours:00}:{compileTime.Minutes:00}:{compileTime.Seconds:00}.{compileTime.Milliseconds:000}");

        return 0;
    }

    private static void PrintError<T>(Result<T> errorResult, ConfigData config, string path) {
        Error error = errorResult.Error;
        Console.WriteLine($"Error in '{path}' at line {error.Line}: {error.Text}");
        if (config.Debug) Console.WriteLine(error.Stack);
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

    private static ConfigData CreateDefaultConfig() {
        return new ConfigData();
    }

    private static void PrintHelp() {
        Console.WriteLine("Syntax:");
        Console.WriteLine("bbap [input file] [output file]");
    }
}