using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace CharacterForge.Infrastructure.Lorebooks;

/// <summary>
/// Represents a lorebook adhering to the Version 3.0 specification.
/// </summary>
[PublicAPI]
public sealed record class LorebookV3
{
    public const string TypicalSpecification = "lorebook_v3";

    [JsonPropertyName("spec")]
    public required string Specification { get; init; }

    [JsonPropertyName("data")]
    public required CoreProperties Properties { get; init; }

    [PublicAPI]
    public sealed record class CoreProperties
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("scan_depth")]
        public int? ScanDepth { get; init; }

        [JsonPropertyName("token_budget")]
        public int? TokenBudget { get; init; }

        [JsonPropertyName("recursive_scanning")]
        public bool? RecursiveScanning { get; init; }

        [JsonPropertyName("extensions")]
        public required IReadOnlyDictionary<string, JsonElement> Extensions { get; init; }

        [JsonPropertyName("entries")]
        public required ImmutableArray<LorebookV3Entry> Entries { get; init; }
    }
}
