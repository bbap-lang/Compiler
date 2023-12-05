using System.Text.Json.Serialization;

namespace BBAP.Config;

public record ConfigData(
    [property: JsonPropertyName("debug")] bool Debug = false,
    [property: JsonPropertyName("start-file")]
    string StartFile = "main.bbap",
    [property: JsonPropertyName("abap-defaults")]
    string[]? AbapDefaults = null,
    [property: JsonPropertyName("use-stack")]
    bool UseStack = true
);