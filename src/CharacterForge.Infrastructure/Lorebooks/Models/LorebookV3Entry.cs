using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace CharacterForge.Infrastructure.Lorebooks;

/// <summary>
/// Represents a lorebook entry adhering to the Version 3.0 specification.
/// </summary>
[PublicAPI]
public sealed record class LorebookV3Entry
{
    [JsonPropertyName("keys")]
    public required ImmutableArray<string> Keys { get; init; }

    [JsonPropertyName("content")]
    public required string Content { get; init; }

    [JsonPropertyName("extensions")]
    public required IReadOnlyDictionary<string, JsonElement> Extensions { get; init; }

    [JsonPropertyName("enabled")]
    public required bool Enabled { get; init; }

    [JsonPropertyName("insertion_order")]
    public required int InsertionOrder { get; init; }

    [JsonPropertyName("case_sensitive")]
    public bool? CaseSensitive { get; init; }

    [JsonPropertyName("use_regex")]
    public required bool UseRegex { get; init; }

    [JsonPropertyName("constant")]
    public bool? Constant { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("priority")]
    public int? Priority { get; init; }

    [JsonPropertyName("id")]
    public dynamic? Id { get; init; }

    [JsonPropertyName("comment")]
    public string? Comment { get; init; }

    [JsonPropertyName("selective")]
    public bool? Selective { get; init; }

    [JsonPropertyName("secondary_keys")]
    public ImmutableArray<string>? SecondaryKeys { get; init; }

    [JsonPropertyName("position")]
    public string? Position { get; init; }
}
