using System.Text.Json.Serialization;

namespace BBAP.Config;

public record ConfigData(
    [property: JsonPropertyName("debug")] bool Debug = false,
    [property: JsonPropertyName("start-file")]
    string StartFile = "main.bbap",
    [property: JsonPropertyName("abap-dependencies")]
    string[]? AbapDependencies = null,
    [property: JsonPropertyName("use-scopes")]
    bool UseScopes = true
);