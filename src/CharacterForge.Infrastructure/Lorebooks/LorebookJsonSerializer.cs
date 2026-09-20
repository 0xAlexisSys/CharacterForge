using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CharacterForge.Infrastructure.Lorebooks;

/// <summary>
/// Provides static methods for serializing and deserializing lorebook JSON.
/// </summary>
[PublicAPI]
public static class LorebookJsonSerializer
{
    /// <summary>
    /// Serializes a <see cref="LorebookV3"/> model to a JSON string.
    /// </summary>
    /// <param name="lorebook">
    /// The <see cref="LorebookV3"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON string with indentation.
    /// </param>
    /// <returns>
    /// A JSON string representation of the <see cref="LorebookV3"/> model.
    /// </returns>
    public static string Serialize(LorebookV3 lorebook, bool writeIndented = true) => JsonSerializer.Serialize(lorebook, Constants.IndentableJsonSerializerOptions[writeIndented]);

    /// <summary>
    /// Serializes a <see cref="LorebookV3"/> model to a stream as JSON.
    /// </summary>
    /// <param name="stream">
    /// The stream to write to.
    /// </param>
    /// <param name="lorebook">
    /// The <see cref="LorebookV3"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    public static void Serialize(Stream stream, LorebookV3 lorebook, bool writeIndented = true) => JsonSerializer.Serialize(stream, lorebook, Constants.IndentableJsonSerializerOptions[writeIndented]);

    /// <summary>
    /// Serializes a <see cref="LorebookV3"/> model to a <see cref="Utf8JsonWriter"/>.
    /// </summary>
    /// <param name="jsonWriter">
    /// The <see cref="Utf8JsonWriter"/> to write to.
    /// </param>
    /// <param name="lorebook">
    /// The <see cref="LorebookV3"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    public static void Serialize(Utf8JsonWriter jsonWriter, LorebookV3 lorebook, bool writeIndented = true) => JsonSerializer.Serialize(jsonWriter, lorebook, Constants.IndentableJsonSerializerOptions[writeIndented]);

    /// <summary>
    /// Asynchronously serializes a <see cref="LorebookV3"/> model to a stream as JSON.
    /// </summary>
    /// <param name="stream">
    /// The stream to write to.
    /// </param>
    /// <param name="lorebook">
    /// The <see cref="LorebookV3"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation requests.
    /// </param>
    public static Task SerializeAsync(Stream stream, LorebookV3 lorebook, bool writeIndented = true, CancellationToken cancellationToken = default) => JsonSerializer.SerializeAsync(stream, lorebook, Constants.IndentableJsonSerializerOptions[writeIndented], cancellationToken);

    /// <summary>
    /// Asynchronously serializes a <see cref="LorebookV3"/> model to a <see cref="PipeWriter"/>.
    /// </summary>
    /// <param name="jsonWriter">
    /// The <see cref="PipeWriter"/> to write to.
    /// </param>
    /// <param name="lorebook">
    /// The <see cref="LorebookV3"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation requests.
    /// </param>
    public static Task SerializeAsync(PipeWriter jsonWriter, LorebookV3 lorebook, bool writeIndented = true, CancellationToken cancellationToken = default) => JsonSerializer.SerializeAsync(jsonWriter, lorebook, Constants.IndentableJsonSerializerOptions[writeIndented], cancellationToken);

    /// <summary>
    /// Serializes a <see cref="LorebookV3"/> model to a UTF-8 encoded <see cref="byte"/>[].
    /// </summary>
    /// <param name="lorebook">
    /// The <see cref="LorebookV3"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    /// <returns>
    /// A UTF-8 encoded <see cref="byte"/>[] representing the lorebook JSON.
    /// </returns>
    public static byte[] SerializeToUtf8Bytes(LorebookV3 lorebook, bool writeIndented = false) => JsonSerializer.SerializeToUtf8Bytes(lorebook, Constants.IndentableJsonSerializerOptions[writeIndented]);

    /// <summary>
    /// Serializes a <see cref="LorebookV2"/> model to a JSON string.
    /// </summary>
    /// <param name="lorebook">
    /// The <see cref="LorebookV2"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON string with indentation.
    /// </param>
    /// <returns>
    /// A JSON string representation of the <see cref="LorebookV2"/> model.
    /// </returns>
    public static string Serialize(LorebookV2 lorebook, bool writeIndented = true) => JsonSerializer.Serialize(lorebook, Constants.IndentableJsonSerializerOptions[writeIndented]);

    /// <summary>
    /// Serializes a <see cref="LorebookV2"/> model to a stream as JSON.
    /// </summary>
    /// <param name="stream">
    /// The stream to write to.
    /// </param>
    /// <param name="lorebook">
    /// The <see cref="LorebookV2"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    public static void Serialize(Stream stream, LorebookV2 lorebook, bool writeIndented = true) => JsonSerializer.Serialize(stream, lorebook, Constants.IndentableJsonSerializerOptions[writeIndented]);

    /// <summary>
    /// Serializes a <see cref="LorebookV2"/> model to a <see cref="Utf8JsonWriter"/>.
    /// </summary>
    /// <param name="jsonWriter">
    /// The <see cref="Utf8JsonWriter"/> to write to.
    /// </param>
    /// <param name="lorebook">
    /// The <see cref="LorebookV2"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    public static void Serialize(Utf8JsonWriter jsonWriter, LorebookV2 lorebook, bool writeIndented = true) => JsonSerializer.Serialize(jsonWriter, lorebook, Constants.IndentableJsonSerializerOptions[writeIndented]);

    /// <summary>
    /// Asynchronously serializes a <see cref="LorebookV2"/> model to a stream as JSON.
    /// </summary>
    /// <param name="stream">
    /// The stream to write to.
    /// </param>
    /// <param name="lorebook">
    /// The <see cref="LorebookV2"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation requests.
    /// </param>
    public static Task SerializeAsync(Stream stream, LorebookV2 lorebook, bool writeIndented = true, CancellationToken cancellationToken = default) => JsonSerializer.SerializeAsync(stream, lorebook, Constants.IndentableJsonSerializerOptions[writeIndented], cancellationToken);

    /// <summary>
    /// Asynchronously serializes a <see cref="LorebookV2"/> model to a <see cref="PipeWriter"/>.
    /// </summary>
    /// <param name="jsonWriter">
    /// The <see cref="PipeWriter"/> to write to.
    /// </param>
    /// <param name="lorebook">
    /// The <see cref="LorebookV2"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation requests.
    /// </param>
    public static Task SerializeAsync(PipeWriter jsonWriter, LorebookV2 lorebook, bool writeIndented = true, CancellationToken cancellationToken = default) => JsonSerializer.SerializeAsync(jsonWriter, lorebook, Constants.IndentableJsonSerializerOptions[writeIndented], cancellationToken);

    /// <summary>
    /// Serializes a <see cref="LorebookV2"/> model to a UTF-8 encoded <see cref="byte"/>[].
    /// </summary>
    /// <param name="lorebook">
    /// The <see cref="LorebookV2"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    /// <returns>
    /// A UTF-8 encoded <see cref="byte"/>[] representing the lorebook JSON.
    /// </returns>
    public static byte[] SerializeToUtf8Bytes(LorebookV2 lorebook, bool writeIndented = false) => JsonSerializer.SerializeToUtf8Bytes(lorebook, Constants.IndentableJsonSerializerOptions[writeIndented]);

    /// <summary>
    /// Deserializes a JSON string into a <see cref="LorebookV3"/> model.
    /// </summary>
    /// <param name="jsonString">
    /// The JSON string to parse.
    /// </param>
    /// <returns>
    /// The deserialized <see cref="LorebookV3"/> model.
    /// </returns>
    public static LorebookV3 Deserialize(string jsonString)
    {
        using JsonDocument document = JsonDocument.Parse(jsonString);
        return Deserialize(document);
    }

    /// <summary>
    /// Deserializes a <see cref="byte"/> sequence into a <see cref="LorebookV3"/> model.
    /// </summary>
    /// <param name="sequence">
    /// The <see cref="byte"/> sequence to parse.
    /// </param>
    /// <returns>
    /// The deserialized <see cref="LorebookV3"/> model.
    /// </returns>
    public static LorebookV3 Deserialize(ReadOnlySequence<byte> sequence)
    {
        using JsonDocument document = JsonDocument.Parse(sequence);
        return Deserialize(document);
    }

    /// <summary>
    /// Deserializes <see cref="byte"/>s from memory into a <see cref="LorebookV3"/> model.
    /// </summary>
    /// <param name="memory">
    /// The <see cref="byte"/> memory buffer to parse.
    /// </param>
    /// <returns>
    /// The deserialized <see cref="LorebookV3"/> model.
    /// </returns>
    public static LorebookV3 Deserialize(ReadOnlyMemory<byte> memory)
    {
        using JsonDocument document = JsonDocument.Parse(memory);
        return Deserialize(document);
    }

    /// <summary>
    /// Deserializes <see cref="char"/>s from memory into a <see cref="LorebookV3"/> model.
    /// </summary>
    /// <param name="memory">
    /// The <see cref="char"/> memory buffer to parse.
    /// </param>
    /// <returns>
    /// The deserialized <see cref="LorebookV3"/> model.
    /// </returns>
    public static LorebookV3 Deserialize(ReadOnlyMemory<char> memory)
    {
        using JsonDocument document = JsonDocument.Parse(memory);
        return Deserialize(document);
    }

    /// <summary>
    /// Deserializes a JSON stream into a <see cref="LorebookV3"/> model.
    /// </summary>
    /// <param name="stream">
    /// The stream containing JSON to parse.
    /// </param>
    /// <returns>
    /// The deserialized <see cref="LorebookV3"/> model.
    /// </returns>
    public static LorebookV3 Deserialize(Stream stream)
    {
        using JsonDocument document = JsonDocument.Parse(stream);
        return Deserialize(document);
    }

    /// <summary>
    /// Deserializes a <see cref="JsonDocument"/> into a <see cref="LorebookV3"/> model.
    /// </summary>
    /// <param name="document">
    /// The <see cref="JsonDocument"/> to deserialize.
    /// </param>
    /// <returns>
    /// The deserialized <see cref="LorebookV3"/> model.
    /// </returns>
    public static LorebookV3 Deserialize(JsonDocument document) => Deserialize(document.RootElement);

    /// <summary>
    /// Deserializes a <see cref="JsonElement"/> into a <see cref="LorebookV3"/> model.
    /// </summary>
    /// <param name="element">
    /// The <see cref="JsonElement"/> to deserialize.
    /// </param>
    /// <returns>
    /// The deserialized <see cref="LorebookV3"/> model.
    /// </returns>
    public static LorebookV3 Deserialize(JsonElement element)
    {
        bool TryGetString(string propertyName) => element.TryGetProperty(propertyName, out JsonElement propertyElement) && propertyElement.ValueKind == JsonValueKind.String;

        if (element.ValueKind != JsonValueKind.Object) throw new JsonException($"Invalid lorebook JSON: {nameof(element)} must be an object.");

        // Version 3.x+ format.
        if (TryGetString("spec") &&
            element.TryGetProperty("data", out JsonElement dataElement) &&
            dataElement.ValueKind == JsonValueKind.Object)
        {
            return element.Deserialize<LorebookV3>(Constants.IndentableJsonSerializerOptions[false]) ?? throw new JsonException($"Failed to deserialize {nameof(LorebookV3)} from JSON.");
        }

        // Version 2.x format.
        if (TryGetString("extensions") &&
            TryGetString("entries"))
        {
            return element.Deserialize<LorebookV2>(Constants.IndentableJsonSerializerOptions[false])?.ToV3() ?? throw new JsonException($"Failed to deserialize {nameof(LorebookV2)} from JSON.");
        }

        throw new JsonException("Invalid lorebook JSON: unrecognized format.");
    }

    /// <summary>
    /// Asynchronously deserializes a JSON stream into a <see cref="LorebookV3"/> model.
    /// </summary>
    /// <param name="stream">
    /// The stream containing JSON to parse.
    /// </param>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation requests.
    /// </param>
    public static async Task<LorebookV3> DeserializeAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        using JsonDocument document = await JsonDocument.ParseAsync(stream, default, cancellationToken);
        return Deserialize(document);
    }
}
