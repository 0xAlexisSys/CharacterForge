using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using CharacterForge.Infrastructure.Lorebooks;
using JetBrains.Annotations;

namespace CharacterForge.Infrastructure.CharacterCards;

/// <summary>
/// Represents a character card adhering to the Version 3.0 specification.
/// </summary>
[PublicAPI]
public sealed record class CharacterCardV3
{
    public const string TypicalSpecification = "chara_card_v3";
    public const string TypicalSpecificationVersionString = "3.0";

    [JsonPropertyName("spec")]
    public required string Specification { get; init; }

    [JsonPropertyName("spec_version")]
    public required string SpecificationVersionString { get; init; }

    [JsonPropertyName("data")]
    public required CoreProperties Properties { get; init; }

    [PublicAPI]
    public sealed record class CoreProperties
    {
        [JsonPropertyName("name")]
        public required string Name { get; init; }

        [JsonPropertyName("description")]
        public required string Description { get; init; }

        [JsonPropertyName("tags")]
        public required ImmutableArray<string> Tags { get; init; }

        [JsonPropertyName("creator")]
        public required string CreatorName { get; init; }

        [JsonPropertyName("character_version")]
        public required string Version { get; init; }

        [JsonPropertyName("mes_example")]
        public required string ExampleMessages { get; init; }

        [JsonPropertyName("extensions")]
        public required IReadOnlyDictionary<string, JsonElement> Extensions { get; init; }

        [JsonPropertyName("system_prompt")]
        public required string SystemPrompt { get; init; }

        [JsonPropertyName("post_history_instructions")]
        public required string PostHistoryInstructions { get; init; }

        [JsonPropertyName("first_mes")]
        public required string FirstGreeting { get; init; }

        [JsonPropertyName("alternate_greetings")]
        public required ImmutableArray<string> AlternateGreetings { get; init; }

        [JsonPropertyName("personality")]
        public required string Personality { get; init; }

        [JsonPropertyName("scenario")]
        public required string Scenario { get; init; }

        [JsonPropertyName("creator_notes")]
        public required string LegacyCreatorNotes { get; init; }

        [JsonPropertyName("character_book")]
        public LorebookV3.CoreProperties? Lorebook { get; init; }

        [JsonPropertyName("assets")]
        public List<CharacterCardV3Asset>? Assets { get; set; }

        [JsonPropertyName("nickname")]
        public string? Nickname { get; init; }

        [JsonPropertyName("creator_notes_multilingual")]
        public IReadOnlyDictionary<string, string>? CreatorNotes { get; init; }

        [JsonPropertyName("source")]
        public ImmutableArray<string>? Source { get; init; }

        // While group_only_greetings is not marked as nullable in the specification,
        // some CCv3-supporting apps omit this property for exports so it is not required.
        [JsonPropertyName("group_only_greetings")]
        public ImmutableArray<string> GroupOnlyGreetings { get; init; } = [];

        [JsonPropertyName("creation_date")]
        public long? CreationDate { get; init; }

        [JsonPropertyName("modification_date")]
        public long? ModificationDate { get; init; }
    }
}
