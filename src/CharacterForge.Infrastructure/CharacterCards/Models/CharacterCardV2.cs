using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using CharacterForge.Infrastructure.Lorebooks;
using JetBrains.Annotations;

namespace CharacterForge.Infrastructure.CharacterCards;

/// <summary>
/// Represents a character card adhering to the Version 2.0 specification.
/// </summary>
[PublicAPI]
public sealed record class CharacterCardV2
{
    public const string TypicalSpecification = "chara_card_v2";
    public const string TypicalSpecificationVersionString = "2.0";

    [JsonPropertyName("spec")]
    public required string Specification { get; init; }

    [JsonPropertyName("spec_version")]
    public required string SpecificationVersionString { get; init; }

    [JsonPropertyName("data")]
    public required CoreProperties Properties { get; init; }

    /// <summary>
    /// Converts this model to a <see cref="CharacterCardV3"/> representation.
    /// </summary>
    /// <returns>
    /// A new <see cref="CharacterCardV3"/> model with transferred properties.
    /// </returns>
    public CharacterCardV3 ToV3() => new()
    {
        Specification = CharacterCardV3.TypicalSpecification,
        SpecificationVersionString = new SemanticVersion(SpecificationVersionString).Major < 3U ? CharacterCardV3.TypicalSpecificationVersionString : SpecificationVersionString,
        Properties = new()
        {
            Name = Properties.Name,
            Description = Properties.Description,
            Tags = Properties.Tags,
            CreatorName = Properties.CreatorName,
            Version = Properties.Version,
            ExampleMessages = Properties.ExampleMessages,
            Extensions = Properties.Extensions,
            SystemPrompt = Properties.SystemPrompt,
            PostHistoryInstructions = Properties.PostHistoryInstructions,
            FirstGreeting = Properties.FirstGreeting,
            AlternateGreetings = Properties.AlternateGreetings,
            Personality = Properties.Personality,
            Scenario = Properties.Scenario,
            Lorebook = Properties.Lorebook?.ToV3().Properties,
            LegacyCreatorNotes = Properties.CreatorNotes,
            CreatorNotes = Properties.CreatorNotes.Length != 0 ? new Dictionary<string, string> {["en"] = Properties.CreatorNotes} : null,
        },
    };

    [PublicAPI]
    public sealed record class CoreProperties
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

        [JsonPropertyName("creator_notes")]
        public required string CreatorNotes { get; init; }

        [JsonPropertyName("system_prompt")]
        public required string SystemPrompt { get; init; }

        [JsonPropertyName("post_history_instructions")]
        public required string PostHistoryInstructions { get; init; }

        [JsonPropertyName("alternate_greetings")]
        public required ImmutableArray<string> AlternateGreetings { get; init; }

        [JsonPropertyName("character_book")]
        public LorebookV2? Lorebook { get; init; }

        [JsonPropertyName("tags")]
        public required ImmutableArray<string> Tags { get; init; }

        [JsonPropertyName("creator")]
        public required string CreatorName { get; init; }

        [JsonPropertyName("character_version")]
        public required string Version { get; init; }

        [JsonPropertyName("extensions")]
        public required IReadOnlyDictionary<string, JsonElement> Extensions { get; init; }
    }
}
