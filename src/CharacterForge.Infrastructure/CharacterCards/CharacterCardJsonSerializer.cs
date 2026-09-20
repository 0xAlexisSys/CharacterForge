using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CharacterForge.Infrastructure.CharacterCards;

/// <summary>
/// Provides static methods for serializing and deserializing character card JSON.
/// </summary>
[PublicAPI]
public static class CharacterCardJsonSerializer
{
    /// <summary>
    /// Serializes a <see cref="CharacterCardV3"/> model to a JSON string.
    /// </summary>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV3"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON string with indentation.
    /// </param>
    /// <returns>
    /// A JSON string representation of the <see cref="CharacterCardV3"/> model.
    /// </returns>
    public static string Serialize(CharacterCardV3 characterCard, bool writeIndented = true) => JsonSerializer.Serialize(characterCard, Constants.IndentableJsonSerializerOptions[writeIndented]);

    /// <summary>
    /// Serializes a <see cref="CharacterCardV3"/> model to a stream as JSON.
    /// </summary>
    /// <param name="stream">
    /// The stream to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV3"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    public static void Serialize(Stream stream, CharacterCardV3 characterCard, bool writeIndented = true) => JsonSerializer.Serialize(stream, characterCard, Constants.IndentableJsonSerializerOptions[writeIndented]);

    /// <summary>
    /// Serializes a <see cref="CharacterCardV3"/> model to a <see cref="Utf8JsonWriter"/>.
    /// </summary>
    /// <param name="jsonWriter">
    /// The <see cref="Utf8JsonWriter"/> to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV3"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    public static void Serialize(Utf8JsonWriter jsonWriter, CharacterCardV3 characterCard, bool writeIndented = true) => JsonSerializer.Serialize(jsonWriter, characterCard, Constants.IndentableJsonSerializerOptions[writeIndented]);

    /// <summary>
    /// Asynchronously serializes a <see cref="CharacterCardV3"/> model to a stream as JSON.
    /// </summary>
    /// <param name="stream">
    /// The stream to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV3"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation requests.
    /// </param>
    public static Task SerializeAsync(Stream stream, CharacterCardV3 characterCard, bool writeIndented = true, CancellationToken cancellationToken = default) => JsonSerializer.SerializeAsync(stream, characterCard, Constants.IndentableJsonSerializerOptions[writeIndented], cancellationToken);

    /// <summary>
    /// Asynchronously serializes a <see cref="CharacterCardV3"/> model to a <see cref="PipeWriter"/>.
    /// </summary>
    /// <param name="jsonWriter">
    /// The <see cref="PipeWriter"/> to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV3"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation requests.
    /// </param>
    public static Task SerializeAsync(PipeWriter jsonWriter, CharacterCardV3 characterCard, bool writeIndented = true, CancellationToken cancellationToken = default) => JsonSerializer.SerializeAsync(jsonWriter, characterCard, Constants.IndentableJsonSerializerOptions[writeIndented], cancellationToken);

    /// <summary>
    /// Serializes a <see cref="CharacterCardV3"/> model to a UTF-8 encoded <see cref="byte"/>[].
    /// </summary>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV3"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    /// <returns>
    /// A UTF-8 encoded <see cref="byte"/>[] representing the characterCard JSON.
    /// </returns>
    public static byte[] SerializeToUtf8Bytes(CharacterCardV3 characterCard, bool writeIndented = false) => JsonSerializer.SerializeToUtf8Bytes(characterCard, Constants.IndentableJsonSerializerOptions[writeIndented]);

    /// <summary>
    /// Serializes a <see cref="CharacterCardV2"/> model to a JSON string.
    /// </summary>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV2"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON string with indentation.
    /// </param>
    /// <returns>
    /// A JSON string representation of the <see cref="CharacterCardV2"/> model.
    /// </returns>
    public static string Serialize(CharacterCardV2 characterCard, bool writeIndented = true) => JsonSerializer.Serialize(characterCard, Constants.IndentableJsonSerializerOptions[writeIndented]);

    /// <summary>
    /// Serializes a <see cref="CharacterCardV2"/> model to a stream as JSON.
    /// </summary>
    /// <param name="stream">
    /// The stream to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV2"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    public static void Serialize(Stream stream, CharacterCardV2 characterCard, bool writeIndented = true) => JsonSerializer.Serialize(stream, characterCard, Constants.IndentableJsonSerializerOptions[writeIndented]);

    /// <summary>
    /// Serializes a <see cref="CharacterCardV2"/> model to a <see cref="Utf8JsonWriter"/>.
    /// </summary>
    /// <param name="jsonWriter">
    /// The <see cref="Utf8JsonWriter"/> to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV2"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    public static void Serialize(Utf8JsonWriter jsonWriter, CharacterCardV2 characterCard, bool writeIndented = true) => JsonSerializer.Serialize(jsonWriter, characterCard, Constants.IndentableJsonSerializerOptions[writeIndented]);

    /// <summary>
    /// Asynchronously serializes a <see cref="CharacterCardV2"/> model to a stream as JSON.
    /// </summary>
    /// <param name="stream">
    /// The stream to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV2"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation requests.
    /// </param>
    public static Task SerializeAsync(Stream stream, CharacterCardV2 characterCard, bool writeIndented = true, CancellationToken cancellationToken = default) => JsonSerializer.SerializeAsync(stream, characterCard, Constants.IndentableJsonSerializerOptions[writeIndented], cancellationToken);

    /// <summary>
    /// Asynchronously serializes a <see cref="CharacterCardV2"/> model to a <see cref="PipeWriter"/>.
    /// </summary>
    /// <param name="jsonWriter">
    /// The <see cref="PipeWriter"/> to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV2"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation requests.
    /// </param>
    public static Task SerializeAsync(PipeWriter jsonWriter, CharacterCardV2 characterCard, bool writeIndented = true, CancellationToken cancellationToken = default) => JsonSerializer.SerializeAsync(jsonWriter, characterCard, Constants.IndentableJsonSerializerOptions[writeIndented], cancellationToken);

    /// <summary>
    /// Serializes a <see cref="CharacterCardV2"/> model to a UTF-8 encoded <see cref="byte"/>[].
    /// </summary>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV2"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    /// <returns>
    /// A UTF-8 encoded <see cref="byte"/>[] representing the characterCard JSON.
    /// </returns>
    public static byte[] SerializeToUtf8Bytes(CharacterCardV2 characterCard, bool writeIndented = false) => JsonSerializer.SerializeToUtf8Bytes(characterCard, Constants.IndentableJsonSerializerOptions[writeIndented]);

    /// <summary>
    /// Serializes a <see cref="CharacterCardV1"/> model to a JSON string.
    /// </summary>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV1"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON string with indentation.
    /// </param>
    /// <returns>
    /// A JSON string representation of the <see cref="CharacterCardV1"/> model.
    /// </returns>
    public static string Serialize(CharacterCardV1 characterCard, bool writeIndented = true) => JsonSerializer.Serialize(characterCard, Constants.IndentableJsonSerializerOptions[writeIndented]);

    /// <summary>
    /// Serializes a <see cref="CharacterCardV1"/> model to a stream as JSON.
    /// </summary>
    /// <param name="stream">
    /// The stream to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV1"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    public static void Serialize(Stream stream, CharacterCardV1 characterCard, bool writeIndented = true) => JsonSerializer.Serialize(stream, characterCard, Constants.IndentableJsonSerializerOptions[writeIndented]);

    /// <summary>
    /// Serializes a <see cref="CharacterCardV1"/> model to a <see cref="Utf8JsonWriter"/>.
    /// </summary>
    /// <param name="jsonWriter">
    /// The <see cref="Utf8JsonWriter"/> to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV1"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    public static void Serialize(Utf8JsonWriter jsonWriter, CharacterCardV1 characterCard, bool writeIndented = true) => JsonSerializer.Serialize(jsonWriter, characterCard, Constants.IndentableJsonSerializerOptions[writeIndented]);

    /// <summary>
    /// Asynchronously serializes a <see cref="CharacterCardV1"/> model to a stream as JSON.
    /// </summary>
    /// <param name="stream">
    /// The stream to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV1"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation requests.
    /// </param>
    public static Task SerializeAsync(Stream stream, CharacterCardV1 characterCard, bool writeIndented = true, CancellationToken cancellationToken = default) => JsonSerializer.SerializeAsync(stream, characterCard, Constants.IndentableJsonSerializerOptions[writeIndented], cancellationToken);

    /// <summary>
    /// Asynchronously serializes a <see cref="CharacterCardV1"/> model to a <see cref="PipeWriter"/>.
    /// </summary>
    /// <param name="jsonWriter">
    /// The <see cref="PipeWriter"/> to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV1"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation requests.
    /// </param>
    public static Task SerializeAsync(PipeWriter jsonWriter, CharacterCardV1 characterCard, bool writeIndented = true, CancellationToken cancellationToken = default) => JsonSerializer.SerializeAsync(jsonWriter, characterCard, Constants.IndentableJsonSerializerOptions[writeIndented], cancellationToken);

    /// <summary>
    /// Serializes a <see cref="CharacterCardV1"/> model to a UTF-8 encoded <see cref="byte"/>[].
    /// </summary>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV1"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    /// <returns>
    /// A UTF-8 encoded <see cref="byte"/>[] representing the characterCard JSON.
    /// </returns>
    public static byte[] SerializeToUtf8Bytes(CharacterCardV1 characterCard, bool writeIndented = false) => JsonSerializer.SerializeToUtf8Bytes(characterCard, Constants.IndentableJsonSerializerOptions[writeIndented]);

    /// <summary>
    /// Deserializes a JSON string into a <see cref="CharacterCardV3"/> model.
    /// </summary>
    /// <param name="jsonString">
    /// The JSON string to parse.
    /// </param>
    /// <returns>
    /// The deserialized <see cref="CharacterCardV3"/> model.
    /// </returns>
    public static CharacterCardV3 Deserialize(string jsonString)
    {
        using JsonDocument document = JsonDocument.Parse(jsonString);
        return Deserialize(document);
    }

    /// <summary>
    /// Deserializes a <see cref="byte"/> sequence into a <see cref="CharacterCardV3"/> model.
    /// </summary>
    /// <param name="sequence">
    /// The <see cref="byte"/> sequence to parse.
    /// </param>
    /// <returns>
    /// The deserialized <see cref="CharacterCardV3"/> model.
    /// </returns>
    public static CharacterCardV3 Deserialize(ReadOnlySequence<byte> sequence)
    {
        using JsonDocument document = JsonDocument.Parse(sequence);
        return Deserialize(document);
    }

    /// <summary>
    /// Deserializes <see cref="byte"/>s from memory into a <see cref="CharacterCardV3"/> model.
    /// </summary>
    /// <param name="memory">
    /// The <see cref="byte"/> memory buffer to parse.
    /// </param>
    /// <returns>
    /// The deserialized <see cref="CharacterCardV3"/> model.
    /// </returns>
    public static CharacterCardV3 Deserialize(ReadOnlyMemory<byte> memory)
    {
        using JsonDocument document = JsonDocument.Parse(memory);
        return Deserialize(document);
    }

    /// <summary>
    /// Deserializes <see cref="char"/>s from memory into a <see cref="CharacterCardV3"/> model.
    /// </summary>
    /// <param name="memory">
    /// The <see cref="char"/> memory buffer to parse.
    /// </param>
    /// <returns>
    /// The deserialized <see cref="CharacterCardV3"/> model.
    /// </returns>
    public static CharacterCardV3 Deserialize(ReadOnlyMemory<char> memory)
    {
        using JsonDocument document = JsonDocument.Parse(memory);
        return Deserialize(document);
    }

    /// <summary>
    /// Deserializes a JSON stream into a <see cref="CharacterCardV3"/> model.
    /// </summary>
    /// <param name="stream">
    /// The stream containing JSON to parse.
    /// </param>
    /// <returns>
    /// The deserialized <see cref="CharacterCardV3"/> model.
    /// </returns>
    public static CharacterCardV3 Deserialize(Stream stream)
    {
        using JsonDocument document = JsonDocument.Parse(stream);
        return Deserialize(document);
    }

    /// <summary>
    /// Deserializes a <see cref="JsonDocument"/> into a <see cref="CharacterCardV3"/> model.
    /// </summary>
    /// <param name="document">
    /// The <see cref="JsonDocument"/> to deserialize.
    /// </param>
    /// <returns>
    /// The deserialized <see cref="CharacterCardV3"/> model.
    /// </returns>
    public static CharacterCardV3 Deserialize(JsonDocument document) => Deserialize(document.RootElement);

    /// <summary>
    /// Deserializes a <see cref="JsonElement"/> into a <see cref="CharacterCardV3"/> model.
    /// </summary>
    /// <param name="element">
    /// The <see cref="JsonElement"/> to deserialize.
    /// </param>
    /// <returns>
    /// The deserialized <see cref="CharacterCardV3"/> model.
    /// </returns>
    public static CharacterCardV3 Deserialize(JsonElement element)
    {
        bool TryGetString(string propertyName, out string propertyValue)
        {
            if (element.TryGetProperty(propertyName, out JsonElement propertyElement) && propertyElement.ValueKind == JsonValueKind.String)
            {
                propertyValue = propertyElement.GetString()!;
                return true;
            }
            propertyValue = string.Empty;
            return false;
        }

        if (element.ValueKind != JsonValueKind.Object) throw new JsonException($"Invalid character card JSON: {nameof(element)} must be an object.");

        // Version 2.x+ format.
        if (TryGetString("spec", out string specification) &&
            TryGetString("spec_version", out string specificationVersionString) &&
            element.TryGetProperty("data", out JsonElement dataElement) &&
            dataElement.ValueKind == JsonValueKind.Object)
        {
            SemanticVersion specificationVersion = new(specificationVersionString);
            if (specificationVersion.Major >= 2U)
            {
                return specification == CharacterCardV2.TypicalSpecification || specificationVersion.Major == 2U
                           ? element.Deserialize<CharacterCardV2>(Constants.IndentableJsonSerializerOptions[false])?.ToV3() ?? throw new JsonException($"Failed to deserialize {nameof(CharacterCardV2)} from JSON.")
                           : element.Deserialize<CharacterCardV3>(Constants.IndentableJsonSerializerOptions[false]) ?? throw new JsonException($"Failed to deserialize {nameof(CharacterCardV3)} from JSON.");
            }
        }

        // Version 1.x format.
        if (TryGetString("name", out _) &&
            TryGetString("description", out _) &&
            TryGetString("personality", out _) &&
            TryGetString("scenario", out _) &&
            TryGetString("first_mes", out _) &&
            TryGetString("mes_example", out _))
        {
            return element.Deserialize<CharacterCardV1>(Constants.IndentableJsonSerializerOptions[false])?.ToV3() ?? throw new JsonException($"Failed to deserialize {nameof(CharacterCardV1)} from JSON.");
        }

        throw new JsonException("Invalid character card JSON: unrecognized format.");
    }

    /// <summary>
    /// Asynchronously deserializes a JSON stream into a <see cref="CharacterCardV3"/> model.
    /// </summary>
    /// <param name="stream">
    /// The stream containing JSON to parse.
    /// </param>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation requests.
    /// </param>
    public static async Task<CharacterCardV3> DeserializeAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        using JsonDocument document = await JsonDocument.ParseAsync(stream, default, cancellationToken);
        return Deserialize(document);
    }
}
