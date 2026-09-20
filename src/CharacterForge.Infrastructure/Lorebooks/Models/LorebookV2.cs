using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace CharacterForge.Infrastructure.Lorebooks;

/// <summary>
/// Represents a lorebook adhering to the Version 2.0 specification.
/// </summary>
[PublicAPI]
public sealed record class LorebookV2
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
    public required ImmutableArray<LorebookV2Entry> Entries { get; init; }

    /// <summary>
    /// Converts this model to a <see cref="LorebookV3"/> representation.
    /// </summary>
    /// <returns>
    /// A new <see cref="LorebookV3"/> model with transferred properties.
    /// </returns>
    public LorebookV3 ToV3()
    {
        ImmutableArray<LorebookV3Entry>.Builder? convertedEntriesBuilder = Entries.Length != 0 ? ImmutableArray.CreateBuilder<LorebookV3Entry>(Entries.Length) : null;
        if (convertedEntriesBuilder is not null)
        {
            foreach (LorebookV2Entry entry in Entries) convertedEntriesBuilder.Add(entry.ToV3());
        }

        return new()
        {
            Specification = LorebookV3.TypicalSpecification,
            Properties = new()
            {
                Name = Name,
                Description = Description,
                ScanDepth = ScanDepth,
                TokenBudget = TokenBudget,
                RecursiveScanning = RecursiveScanning,
                Extensions = Extensions,
                Entries = convertedEntriesBuilder?.DrainToImmutable() ?? [],
            },
        };
    }
}
