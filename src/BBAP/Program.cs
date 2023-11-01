using System.Collections.Immutable;
using BBAP.Lexer.Tokens;
using BBAP.Parser.Expressions;
using BBAP.Results;
using Newtonsoft.Json;

namespace BBAP;

internal class Program {
    private static int Main(string[] args) {
        Config.WriteDebugOutput = true;
        
        if (args.Length < 2) {
            PrintHelp();
            return 0;
        }

        string inputPath = Path.GetFullPath(args[0]);
        string outputPath = Path.GetFullPath(args[1]);

        if (!File.Exists(inputPath)) {
            Console.WriteLine($"The file '{inputPath}' does not exist.");
            return 1;
        }

        if (!Path.HasExtension(outputPath)) outputPath += $"{Path.GetFileNameWithoutExtension(inputPath)}.abap";

        string inputString = File.ReadAllText(inputPath);

        var startTime = Environment.TickCount64;
        
        var lexer = new Lexer.Lexer();
        Result<ImmutableArray<IToken>> tokensResult = lexer.Run(inputString);
        if (!tokensResult.TryGetValue(out ImmutableArray<IToken> tokens)) {
            Error error = tokensResult.Error;
            Console.WriteLine($"Error at line {error.Line}: {error.Text}");
            if(Config.WriteDebugOutput)
                Console.WriteLine(error.Stack);
            return 1;
        }

        var parser = new Parser.Parser();
        Result<ImmutableArray<IExpression>> treeResult = parser.Run(tokens);
        if (!treeResult.TryGetValue(out ImmutableArray<IExpression> tree)) {
            Error error = treeResult.Error;
            Console.WriteLine($"Error at line {error.Line}: {error.Text}");
            if(Config.WriteDebugOutput)
                Console.WriteLine(error.Stack);
            return 1;
        }

        var preTranspiler = new PreTranspiler.PreTranspiler();
        Result<ImmutableArray<IExpression>> preTranspiledTreeResult = preTranspiler.Run(tree);
        if (!preTranspiledTreeResult.TryGetValue(out ImmutableArray<IExpression> preTranspiledTree)) {
            Error error = preTranspiledTreeResult.Error;
            Console.WriteLine($"Error at line {error.Line}: {error.Text}");
            if(Config.WriteDebugOutput)
                Console.WriteLine(error.Stack);
            return 1;
        }

        var transpiler = new Transpiler.Transpiler();
        Result<string> outputResult = transpiler.Run(preTranspiledTree);
        if (!outputResult.TryGetValue(out string? output)) {
            Error error = outputResult.Error;
            Console.WriteLine($"Error at line {error.Line}: {error.Text}");
            if(Config.WriteDebugOutput)
                Console.WriteLine(error.Stack);
            return 1;
        }

        Console.WriteLine(output);

        var compileTimeMs = Environment.TickCount64 - startTime;
        var compileTime = TimeSpan.FromMilliseconds(compileTimeMs);

        Console.WriteLine();
        Console.WriteLine($"In {compileTime.Hours:00}:{compileTime.Minutes:00}:{compileTime.Seconds:00}.{compileTime.Milliseconds:000}");
        
        return 0;
    }

    private static void PrintHelp() {
        Console.WriteLine("Syntax:");
        Console.WriteLine("bbap [input file] [output file]");
    }
}