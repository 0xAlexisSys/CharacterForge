using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CharacterForge.Infrastructure.Lorebooks;

/// <summary>
/// Provides static methods for saving and loading lorebooks.
/// </summary>
[PublicAPI]
public static class LorebookFile
{
    #region JSON Save Methods
    /// <summary>
    /// Saves a <see cref="LorebookV3"/> model to a JSON file.
    /// </summary>
    /// <param name="path">
    /// The path of the file to write to.
    /// </param>
    /// <param name="lorebook">
    /// The <see cref="LorebookV3"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    public static void SaveToJson(string path, LorebookV3 lorebook, bool writeIndented = true)
    {
        using FileStream fileStream = File.Create(path);
        SaveToJson(fileStream, lorebook, writeIndented);
    }

    /// <summary>
    /// Saves a <see cref="LorebookV3"/> model to a JSON stream.
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
    public static void SaveToJson(Stream stream, LorebookV3 lorebook, bool writeIndented = true) => LorebookJsonSerializer.Serialize(stream, lorebook, writeIndented);

    /// <summary>
    /// Asynchronously saves a <see cref="LorebookV3"/> model to a JSON file.
    /// </summary>
    /// <param name="path">
    /// The path of the file to write to.
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
    public static async Task SaveToJsonAsync(string path, LorebookV3 lorebook, bool writeIndented = true, CancellationToken cancellationToken = default)
    {
        await using FileStream fileStream = File.Create(path);
        await SaveToJsonAsync(fileStream, lorebook, writeIndented, cancellationToken);
    }

    /// <summary>
    /// Saves a <see cref="LorebookV3"/> model to a JSON stream.
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
    public static async Task SaveToJsonAsync(Stream stream, LorebookV3 lorebook, bool writeIndented = true, CancellationToken cancellationToken = default) => await LorebookJsonSerializer.SerializeAsync(stream, lorebook, writeIndented, cancellationToken);
    #endregion JSON Save Methods

    #region Load Methods
    /// <summary>
    /// Loads a lorebook from a file path.
    /// </summary>
    /// <param name="path">
    /// The path of the file to load.
    /// </param>
    /// <returns>
    /// The deserialized <see cref="LorebookV3"/> model.
    /// </returns>
    /// <remarks>
    /// JSON is supported.
    /// </remarks>
    public static LorebookInfo Load(string path)
    {
        using FileStream fileStream = File.OpenRead(path);
        return Load(fileStream);
    }

    /// <summary>
    /// Loads a lorebook from a stream.
    /// </summary>
    /// <param name="stream">
    /// The stream containing file bytes to load.
    /// </param>
    /// <returns>
    /// The deserialized <see cref="LorebookV3"/> model.
    /// </returns>
    /// <remarks>
    /// JSON is supported.
    /// </remarks>
    public static LorebookInfo Load(Stream stream) => new(LorebookJsonSerializer.Deserialize(stream), LorebookFormat.Json);

    /// <summary>
    /// Asynchronously loads a lorebook from a file path.
    /// </summary>
    /// <param name="path">
    /// The path of the file to load.
    /// </param>
    /// <returns>
    /// The deserialized <see cref="LorebookV3"/> model.
    /// </returns>
    /// <remarks>
    /// JSON is supported.
    /// </remarks>
    public static async Task<LorebookInfo> LoadAsync(string path)
    {
        await using FileStream fileStream = File.OpenRead(path);
        return await LoadAsync(fileStream);
    }

    /// <summary>
    /// Asynchronously loads a lorebook from a stream.
    /// </summary>
    /// <param name="stream">
    /// The stream containing file bytes to load.
    /// </param>
    /// <returns>
    /// The deserialized <see cref="LorebookV3"/> model.
    /// </returns>
    public static async Task<LorebookInfo> LoadAsync(Stream stream) => new(await LorebookJsonSerializer.DeserializeAsync(stream), LorebookFormat.Json);
    #endregion Load Methods

}
