using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace CharacterForge.Infrastructure.CharacterCards;

/// <summary>
/// Represents a character card adhering to the Version 1.0 specification.
/// </summary>
[PublicAPI]
public sealed record class CharacterCardV1
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("personality")]
    public required string Personality { get; init; }

    [JsonPropertyName("scenario")]
    public required string Scenario { get; init; }

    [JsonPropertyName("first_mes")]
    public required string FirstGreeting { get; init; }

    [JsonPropertyName("mes_example")]
    public required string ExampleMessages { get; init; }

    /// <summary>
    /// Converts this model to a <see cref="CharacterCardV3"/> representation.
    /// </summary>
    /// <returns>
    /// A new <see cref="CharacterCardV3"/> model with transferred properties.
    /// </returns>
    public CharacterCardV3 ToV3() => new()
    {
        Specification = CharacterCardV3.TypicalSpecification,
        SpecificationVersionString = CharacterCardV3.TypicalSpecificationVersionString,
        Properties = new()
        {
            Name = Name,
            Description = Description,
            Tags = [],
            CreatorName = string.Empty,
            Version = string.Empty,
            ExampleMessages = ExampleMessages,
            Extensions = new Dictionary<string, JsonElement>(),
            SystemPrompt = string.Empty,
            PostHistoryInstructions = string.Empty,
            FirstGreeting = FirstGreeting,
            AlternateGreetings = [],
            Personality = Personality,
            Scenario = Scenario,
            LegacyCreatorNotes = string.Empty,
        },
    };
}
