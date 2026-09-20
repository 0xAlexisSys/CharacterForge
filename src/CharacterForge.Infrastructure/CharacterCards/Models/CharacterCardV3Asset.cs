using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace CharacterForge.Infrastructure.CharacterCards;

/// <summary>
/// Represents a character card asset adhering to the Version 3.0 specification.
/// </summary>
[PublicAPI]
public sealed record class CharacterCardV3Asset
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("uri")]
    public required string Uri { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("ext")]
    public required string Extension { get; set; }
}
