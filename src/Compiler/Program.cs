using System.Text.Json;
using BBAP;
using BBAP.Config;
using BBAP.Results;

if (args.Length < 2) {
    PrintHelp();
    return 0;
}

string inputPath = Path.GetFullPath(args[0]);
string outputPath = Path.GetFullPath(args[1]);

outputPath = InitOutputPath(outputPath);

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

Result<string> compilerResult = Compiler.Run(inputString, sourceDirectory, config);

if (!compilerResult.TryGetValue(out string? output)) {
    PrintError(compilerResult, config, inputPath);
    return 1;
}

long compileTimeMs = Environment.TickCount64 - startTime;
TimeSpan compileTime = TimeSpan.FromMilliseconds(compileTimeMs);

Console.WriteLine($"Successfully compiled In {compileTime.Hours:00}:{compileTime.Minutes:00}:{compileTime.Seconds:00}.{compileTime.Milliseconds:000}");


if (config.Debug) {
    Console.WriteLine(output);
} else {
    File.WriteAllText(outputPath, output);
}

return 0;

string InitOutputPath(string outputPath) {
    var directory = new DirectoryInfo(Path.GetDirectoryName(outputPath)!);

    if (!directory.Exists) directory.Create();

    return outputPath;
}

void PrintError<T>(Result<T> errorResult, ConfigData config, string path) {
    Error error = errorResult.Error;
    Console.WriteLine($"Error in '{path}' at line {error.Line}: {error.Text}");
    if (config.Debug) Console.WriteLine(error.Stack);
}

ConfigData CreateDefaultConfig() {
    return new ConfigData();
}

void PrintHelp() {
    Console.WriteLine("Syntax:");
    Console.WriteLine("bbap [input file] [output file]");
}