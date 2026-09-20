using System.Collections.Frozen;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CharacterForge.Infrastructure;

internal static class Constants
{
    public static readonly FrozenDictionary<bool, JsonSerializerOptions> IndentableJsonSerializerOptions = new Dictionary<bool, JsonSerializerOptions>
    {
        [true] = CreateIndentableJsonSerializerOptions(true),
        [false] = CreateIndentableJsonSerializerOptions(false),
    }.ToFrozenDictionary();

    private static JsonSerializerOptions CreateIndentableJsonSerializerOptions(bool writeIndented) => new(JsonSerializerDefaults.Strict)
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = writeIndented,
    };
}
