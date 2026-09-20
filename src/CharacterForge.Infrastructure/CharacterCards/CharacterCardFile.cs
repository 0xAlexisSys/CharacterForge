using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageMagick;
using JetBrains.Annotations;

namespace CharacterForge.Infrastructure.CharacterCards;

/// <summary>
/// Provides static methods for saving and loading character cards.
/// </summary>
[PublicAPI]
public static class CharacterCardFile
{
    private const string PngAttributeName = "ccv3";
    private const string PngLegacyAttributeName = "chara";
    private const string CharxCardEntryName = "card.json";
    private const string CharxMainIconEntryBaseName = "assets/icon/images/main";
    private const string CharxMainIconAssetType = "icon";
    private const string CharxMainIconAssetName = "main";

    // \u0089 silently adds an extra element so 0x89 is used instead.
    private static readonly byte[] PngHeader = [0x89, .."PNG\r\n\u001A\n"u8];

    private static readonly byte[] CharxHeaderPart1 = [.."PK"u8];

    private static readonly byte[][] CharxHeaderPart2 =
    [
        [.."\x03\x04"u8],
        [.."\x05\x06"u8],
        [.."\a\b"u8],
    ];

    private static readonly string[] CharxAssetUriPrefixes =
    [
        "embeded://",
        "embedded://", // From SillyTavern/src/charx.js
        "__asset:", // From SillyTavern/src/charx.js
    ];

    #region PNG Save Methods
    /// <summary>
    /// Saves a <see cref="CharacterCardV3"/> model to a PNG file.
    /// </summary>
    /// <param name="path">
    /// The path of the file to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV3"/> model to serialize and embed as a
    /// <c>ccv3</c> text chunk.
    /// </param>
    /// <param name="iconBytes">
    /// Optional icon bytes to use. If <see langword="null"/>, a transparent 1x1 image
    /// is used.
    /// </param>
    public static void SaveToPng(string path, CharacterCardV3 characterCard, byte[]? iconBytes = null)
    {
        using FileStream fileStream = File.Create(path);
        using MemoryStream? iconDataStream = iconBytes is not null ? new(iconBytes) : null;
        SaveToPng(fileStream, characterCard, iconDataStream);
    }

    /// <summary>
    /// Saves a <see cref="CharacterCardV3"/> model to a PNG file.
    /// </summary>
    /// <param name="path">
    /// The path of the file to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV3"/> model to serialize and embed as a
    /// <c>ccv3</c> text chunk.
    /// </param>
    /// <param name="iconStream">
    /// Optional stream containing icon bytes to use. If <see langword="null"/>,
    /// a transparent 1x1 image is used.
    /// </param>
    public static void SaveToPng(string path, CharacterCardV3 characterCard, Stream? iconStream = null)
    {
        using FileStream fileStream = File.Create(path);
        SaveToPng(fileStream, characterCard, iconStream);
    }

    /// <summary>
    /// Saves a <see cref="CharacterCardV3"/> model to a PNG stream.
    /// </summary>
    /// <param name="stream">
    /// The stream to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV3"/> model to serialize and embed as a
    /// <c>ccv3</c> text chunk.
    /// </param>
    /// <param name="iconBytes">
    /// Optional icon bytes to use. If <see langword="null"/>, a transparent 1x1 image
    /// is used.
    /// </param>
    public static void SaveToPng(Stream stream, CharacterCardV3 characterCard, byte[]? iconBytes = null)
    {
        using MemoryStream? iconDataStream = iconBytes is not null ? new(iconBytes) : null;
        SaveToPng(stream, characterCard, iconDataStream);
    }

    /// <summary>
    /// Saves a <see cref="CharacterCardV3"/> model to a PNG stream.
    /// </summary>
    /// <param name="stream">
    /// The stream to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV3"/> model to serialize and embed as a
    /// <c>ccv3</c> text chunk.
    /// </param>
    /// <param name="iconStream">
    /// Optional stream containing icon bytes to use. If <see langword="null"/>,
    /// a transparent 1x1 image is used.
    /// </param>
    public static void SaveToPng(Stream stream, CharacterCardV3 characterCard, Stream? iconStream = null)
    {
        using MagickImage magickImage = GetMagickImageOrBlankFromStream(iconStream);
        TryConvertMagickImageToPng(magickImage);
        magickImage.SetAttribute(PngAttributeName, Convert.ToBase64String(CharacterCardJsonSerializer.SerializeToUtf8Bytes(characterCard)));
        magickImage.Write(stream);
    }

    /// <summary>
    /// Asynchronously saves a <see cref="CharacterCardV3"/> model to a PNG file.
    /// </summary>
    /// <param name="path">
    /// The path of the file to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV3"/> model to serialize and embed as a
    /// <c>ccv3</c> text chunk.
    /// </param>
    /// <param name="iconBytes">
    /// Optional icon bytes to use. If <see langword="null"/>, a transparent 1x1 image
    /// is used.
    /// </param>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation requests.
    /// </param>
    public static async Task SaveToPngAsync(string path, CharacterCardV3 characterCard, byte[]? iconBytes = null, CancellationToken cancellationToken = default)
    {
        await using FileStream fileStream = File.Create(path);
        using MemoryStream? iconDataStream = iconBytes is not null ? new(iconBytes) : null;
        await SaveToPngAsync(fileStream, characterCard, iconDataStream, cancellationToken);
    }

    /// <summary>
    /// Asynchronously saves a <see cref="CharacterCardV3"/> model to a PNG file.
    /// </summary>
    /// <param name="path">
    /// The path of the file to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV3"/> model to serialize and embed as a
    /// <c>ccv3</c> text chunk.
    /// </param>
    /// <param name="iconStream">
    /// Optional stream containing icon bytes to use. If <see langword="null"/>,
    /// a transparent 1x1 image is used.
    /// </param>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation requests.
    /// </param>
    public static async Task SaveToPngAsync(string path, CharacterCardV3 characterCard, Stream? iconStream = null, CancellationToken cancellationToken = default)
    {
        await using FileStream fileStream = File.Create(path);
        await SaveToPngAsync(fileStream, characterCard, iconStream, cancellationToken);
    }

    /// <summary>
    /// Asynchronously saves a <see cref="CharacterCardV3"/> model to a PNG stream.
    /// </summary>
    /// <param name="stream">
    /// The stream to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV3"/> model to serialize and embed as a
    /// <c>ccv3</c> text chunk.
    /// </param>
    /// <param name="iconBytes">
    /// Optional icon bytes to use. If <see langword="null"/>, a transparent 1x1 image
    /// is used.
    /// </param>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation requests.
    /// </param>
    public static async Task SaveToPngAsync(Stream stream, CharacterCardV3 characterCard, byte[]? iconBytes = null, CancellationToken cancellationToken = default)
    {
        using MemoryStream? iconDataStream = iconBytes is not null ? new(iconBytes) : null;
        await SaveToPngAsync(stream, characterCard, iconDataStream, cancellationToken);
    }

    /// <summary>
    /// Asynchronously saves a <see cref="CharacterCardV3"/> model to a PNG stream.
    /// </summary>
    /// <param name="stream">
    /// The stream to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV3"/> model to serialize and embed as a
    /// <c>ccv3</c> text chunk.
    /// </param>
    /// <param name="iconStream">
    /// Optional stream containing icon bytes to use. If <see langword="null"/>,
    /// a transparent 1x1 image is used.
    /// </param>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation requests.
    /// </param>
    public static async Task SaveToPngAsync(Stream stream, CharacterCardV3 characterCard, Stream? iconStream = null, CancellationToken cancellationToken = default)
    {
        using MagickImage magickImage = GetMagickImageOrBlankFromStream(iconStream);
        TryConvertMagickImageToPng(magickImage);
        magickImage.SetAttribute(PngAttributeName, Convert.ToBase64String(CharacterCardJsonSerializer.SerializeToUtf8Bytes(characterCard)));
        await magickImage.WriteAsync(stream, cancellationToken);
    }
    #endregion PNG Save Methods

    #region JSON Save Methods
    /// <summary>
    /// Saves a <see cref="CharacterCardV3"/> model to a JSON file.
    /// </summary>
    /// <param name="path">
    /// The path of the file to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV3"/> model to serialize.
    /// </param>
    /// <param name="writeIndented">
    /// Whether to format the JSON output with indentation.
    /// </param>
    public static void SaveToJson(string path, CharacterCardV3 characterCard, bool writeIndented = true)
    {
        using FileStream fileStream = File.Create(path);
        SaveToJson(fileStream, characterCard, writeIndented);
    }

    /// <summary>
    /// Saves a <see cref="CharacterCardV3"/> model to a JSON stream.
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
    public static void SaveToJson(Stream stream, CharacterCardV3 characterCard, bool writeIndented = true) => CharacterCardJsonSerializer.Serialize(stream, characterCard, writeIndented);

    /// <summary>
    /// Asynchronously saves a <see cref="CharacterCardV3"/> model to a JSON file.
    /// </summary>
    /// <param name="path">
    /// The path of the file to write to.
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
    public static async Task SaveToJsonAsync(string path, CharacterCardV3 characterCard, bool writeIndented = true, CancellationToken cancellationToken = default)
    {
        await using FileStream fileStream = File.Create(path);
        await SaveToJsonAsync(fileStream, characterCard, writeIndented, cancellationToken);
    }

    /// <summary>
    /// Asynchronously saves a <see cref="CharacterCardV3"/> model to a JSON stream.
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
    public static async Task SaveToJsonAsync(Stream stream, CharacterCardV3 characterCard, bool writeIndented = true, CancellationToken cancellationToken = default) => await CharacterCardJsonSerializer.SerializeAsync(stream, characterCard, writeIndented, cancellationToken);
    #endregion JSON Save Methods

    #region CHARX Save Methods
    /// <summary>
    /// Saves a <see cref="CharacterCardV3"/> model to a CHARX file.
    /// </summary>
    /// <param name="path">
    /// The path of the file to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV3"/> model to serialize and embed as a
    /// <c>card.json</c> file.
    /// </param>
    /// <param name="iconBytes">
    /// Optional icon bytes to use. If <see langword="null"/>, a transparent 1x1 image
    /// is used.
    /// </param>
    /// <param name="files">
    /// Optional dictionary of file bytes to embed into the file, keyed by
    /// ZIP archive entry path.
    /// </param>
    public static void SaveToCharx(string path, CharacterCardV3 characterCard, byte[]? iconBytes = null, IReadOnlyDictionary<string, byte[]>? files = null)
    {
        using FileStream fileStream = File.Create(path);
        using MemoryStream? imageStream = iconBytes is not null ? new(iconBytes) : null;
        SaveToCharx(fileStream, characterCard, imageStream, files);
    }

    /// <summary>
    /// Saves a <see cref="CharacterCardV3"/> model to a CHARX file.
    /// </summary>
    /// <param name="path">
    /// The path of the file to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV3"/> model to serialize and embed as a
    /// <c>card.json</c> file.
    /// </param>
    /// <param name="iconStream">
    /// Optional stream containing icon bytes to use. If <see langword="null"/>,
    /// a transparent 1x1 image is used.
    /// </param>
    /// <param name="files">
    /// Optional dictionary of file bytes to embed into the file, keyed by
    /// ZIP archive entry path.
    /// </param>
    public static void SaveToCharx(string path, CharacterCardV3 characterCard, Stream? iconStream = null, IReadOnlyDictionary<string, byte[]>? files = null)
    {
        using FileStream fileStream = File.Create(path);
        SaveToCharx(fileStream, characterCard, iconStream, files);
    }

    /// <summary>
    /// Saves a <see cref="CharacterCardV3"/> model to a CHARX stream.
    /// </summary>
    /// <param name="stream">
    /// The stream to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV3"/> model to serialize and embed as a <c>card.json</c>
    /// file.
    /// </param>
    /// <param name="iconBytes">
    /// Optional icon bytes to use. If <see langword="null"/>, a transparent 1x1 image
    /// is used.
    /// </param>
    /// <param name="files">
    /// Optional dictionary of file bytes to embed into the file, keyed by
    /// ZIP archive entry path.
    /// </param>
    public static void SaveToCharx(Stream stream, CharacterCardV3 characterCard, byte[]? iconBytes = null, IReadOnlyDictionary<string, byte[]>? files = null)
    {
        using MemoryStream? imageStream = iconBytes is not null ? new(iconBytes) : null;
        SaveToCharx(stream, characterCard, imageStream, files);
    }

    /// <summary>
    /// Saves a <see cref="CharacterCardV3"/> model to a CHARX stream.
    /// </summary>
    /// <param name="stream">
    /// The stream to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV3"/> model to serialize and embed as a <c>card.json</c>
    /// file.
    /// </param>
    /// <param name="iconStream">
    /// Optional stream containing icon bytes to use. If <see langword="null"/>,
    /// a transparent 1x1 image is used.
    /// </param>
    /// <param name="files">
    /// Optional dictionary of file bytes to embed into the file, keyed by
    /// ZIP archive entry path.
    /// </param>
    public static void SaveToCharx(Stream stream, CharacterCardV3 characterCard, Stream? iconStream = null, IReadOnlyDictionary<string, byte[]>? files = null)
    {
        using ZipArchive zipArchive = new(stream, ZipArchiveMode.Create, leaveOpen: true);
        string? writtenMainIconEntryName = null;

        if (iconStream is not null)
        {
            using MagickImage? magickImage = GetMagickImageOrNullFromStream(iconStream);
            if (magickImage is not null)
            {
                TryConvertMagickImageToPng(magickImage);

                string mainIconExtension = magickImage.Format != MagickFormat.APng ? "png" : "apng";
                string mainIconEntryName = $"{CharxMainIconEntryBaseName}.{mainIconExtension}";
                writtenMainIconEntryName = mainIconEntryName;

                AddMainIconAsset(characterCard, mainIconEntryName, mainIconExtension);

                ZipArchiveEntry mainIconEntry = zipArchive.CreateEntry(mainIconEntryName);
                using Stream mainIconEntryStream = mainIconEntry.Open();
                magickImage.Write(mainIconEntryStream);
            }
        }

        if (files is not null)
        {
            foreach (var pair in files)
            {
                if (string.Equals(pair.Key, CharxCardEntryName, StringComparison.OrdinalIgnoreCase)) continue;

                if (writtenMainIconEntryName is not null && string.Equals(pair.Key, writtenMainIconEntryName, StringComparison.OrdinalIgnoreCase)) continue;

                ZipArchiveEntry entry = zipArchive.CreateEntry(pair.Key);
                using Stream entryStream = entry.Open();
                entryStream.Write(pair.Value);
            }
        }

        ZipArchiveEntry cardEntry = zipArchive.CreateEntry(CharxCardEntryName);
        using Stream cardStream = cardEntry.Open();
        CharacterCardJsonSerializer.Serialize(cardStream, characterCard);
    }

    /// <summary>
    /// Asynchronously saves a <see cref="CharacterCardV3"/> model to a CHARX file.
    /// </summary>
    /// <param name="path">
    /// The path of the file to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV3"/> model to serialize and embed as a <c>card.json</c>
    /// file.
    /// </param>
    /// <param name="iconBytes">
    /// Optional icon bytes to use. If <see langword="null"/>, a transparent 1x1 image
    /// is used.
    /// </param>
    /// <param name="files">
    /// Optional dictionary of file bytes to embed into the file, keyed by
    /// ZIP archive entry path.
    /// </param>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation requests.
    /// </param>
    public static async Task SaveToCharxAsync(string path, CharacterCardV3 characterCard, byte[]? iconBytes = null, IReadOnlyDictionary<string, byte[]>? files = null, CancellationToken cancellationToken = default)
    {
        await using FileStream fileStream = File.Create(path);
        using MemoryStream? iconDataStream = iconBytes is not null ? new(iconBytes) : null;
        await SaveToCharxAsync(fileStream, characterCard, iconDataStream, files, cancellationToken);
    }

    /// <summary>
    /// Asynchronously saves a <see cref="CharacterCardV3"/> model to a CHARX file.
    /// </summary>
    /// <param name="path">
    /// The path of the file to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV3"/> model to serialize and embed as a <c>card.json</c>
    /// file.
    /// </param>
    /// <param name="iconStream">
    /// Optional stream containing icon bytes to use. If <see langword="null"/>,
    /// a transparent 1x1 image is used.
    /// </param>
    /// <param name="files">
    /// Optional dictionary of file bytes to embed into the file, keyed by
    /// ZIP archive entry path.
    /// </param>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation requests.
    /// </param>
    public static async Task SaveToCharxAsync(string path, CharacterCardV3 characterCard, Stream? iconStream = null, IReadOnlyDictionary<string, byte[]>? files = null, CancellationToken cancellationToken = default)
    {
        await using FileStream fileStream = File.Create(path);
        await SaveToCharxAsync(fileStream, characterCard, iconStream, files, cancellationToken);
    }

    /// <summary>
    /// Asynchronously saves a <see cref="CharacterCardV3"/> model to a CHARX stream.
    /// </summary>
    /// <param name="stream">
    /// The stream to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV3"/> model to serialize and embed as a <c>card.json</c>
    /// file.
    /// </param>
    /// <param name="iconBytes">
    /// Optional icon bytes to use. If <see langword="null"/>, a transparent 1x1 image
    /// is used.
    /// </param>
    /// <param name="files">
    /// Optional dictionary of file bytes to embed into the file, keyed by
    /// ZIP archive entry path.
    /// </param>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation requests.
    /// </param>
    public static async Task SaveToCharxAsync(Stream stream, CharacterCardV3 characterCard, byte[]? iconBytes = null, IReadOnlyDictionary<string, byte[]>? files = null, CancellationToken cancellationToken = default)
    {
        using MemoryStream? imageStream = iconBytes is not null ? new(iconBytes) : null;
        await SaveToCharxAsync(stream, characterCard, imageStream, files, cancellationToken);
    }

    /// <summary>
    /// Asynchronously saves a <see cref="CharacterCardV3"/> model to a CHARX stream.
    /// </summary>
    /// <param name="stream">
    /// The stream to write to.
    /// </param>
    /// <param name="characterCard">
    /// The <see cref="CharacterCardV3"/> model to serialize and embed as a <c>card.json</c>
    /// file.
    /// </param>
    /// <param name="iconStream">
    /// Optional stream containing icon bytes to use. If <see langword="null"/>,
    /// a transparent 1x1 image is used.
    /// </param>
    /// <param name="files">
    /// Optional dictionary of file bytes to embed into the file, keyed by
    /// ZIP archive entry path.
    /// </param>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation requests.
    /// </param>
    public static async Task SaveToCharxAsync(Stream stream, CharacterCardV3 characterCard, Stream? iconStream = null, IReadOnlyDictionary<string, byte[]>? files = null, CancellationToken cancellationToken = default)
    {
        await using ZipArchive zipArchive = new(stream, ZipArchiveMode.Create, leaveOpen: true);
        string? writtenMainIconEntryName = null;

        if (iconStream is not null)
        {
            using MagickImage? magickImage = GetMagickImageOrNullFromStream(iconStream);
            if (magickImage is not null)
            {
                TryConvertMagickImageToPng(magickImage);

                string mainIconExtension = magickImage.Format != MagickFormat.APng ? "png" : "apng";
                string mainIconEntryName = $"{CharxMainIconEntryBaseName}.{mainIconExtension}";
                writtenMainIconEntryName = mainIconEntryName;

                AddMainIconAsset(characterCard, mainIconEntryName, mainIconExtension);

                ZipArchiveEntry mainIconEntry = zipArchive.CreateEntry(mainIconEntryName);
                await using Stream mainIconEntryStream = await mainIconEntry.OpenAsync(cancellationToken);
                await magickImage.WriteAsync(mainIconEntryStream, cancellationToken);
            }
        }

        if (files is not null)
        {
            foreach (var pair in files)
            {
                if (string.Equals(pair.Key, CharxCardEntryName, StringComparison.OrdinalIgnoreCase)) continue;

                if (writtenMainIconEntryName is not null && string.Equals(pair.Key, writtenMainIconEntryName, StringComparison.OrdinalIgnoreCase)) continue;

                ZipArchiveEntry entry = zipArchive.CreateEntry(pair.Key);
                await using Stream entryStream = await entry.OpenAsync(cancellationToken);
                await entryStream.WriteAsync(pair.Value, cancellationToken);
            }
        }

        ZipArchiveEntry cardEntry = zipArchive.CreateEntry(CharxCardEntryName);
        await using Stream cardStream = await cardEntry.OpenAsync(cancellationToken);
        await CharacterCardJsonSerializer.SerializeAsync(cardStream, characterCard, cancellationToken: cancellationToken);
    }
    #endregion CHARX Save Methods

    #region Load Methods
    /// <summary>
    /// Loads a character card from a file path.
    /// </summary>
    /// <param name="path">
    /// The path of the file to load.
    /// </param>
    /// <returns>
    /// A <see cref="CharacterCardInfo"/> containing the deserialized
    /// <see cref="CharacterCardV3"/> model and detected format.
    /// </returns>
    /// <remarks>
    /// PNG, APNG, JSON, and CHARX are supported.
    /// </remarks>
    public static CharacterCardInfo Load(string path)
    {
        using FileStream fileStream = File.OpenRead(path);
        return Load(fileStream);
    }

    /// <summary>
    /// Loads a character card from a stream.
    /// </summary>
    /// <param name="stream">
    /// The stream containing file bytes to load.
    /// </param>
    /// <returns>
    /// A <see cref="CharacterCardInfo"/> containing the deserialized
    /// <see cref="CharacterCardV3"/> model and detected format.
    /// </returns>
    /// <remarks>
    /// PNG, APNG, JSON, and CHARX are supported.
    /// </remarks>
    public static CharacterCardInfo Load(Stream stream)
    {
        // ReSharper disable ConvertIfStatementToReturnStatement
        if (IsPng(stream)) return LoadFromPng(stream);
        if (IsCharx(stream)) return LoadFromCharx(stream);
        return LoadFromJson(stream);
        // ReSharper restore ConvertIfStatementToReturnStatement
    }

    /// <summary>
    /// Asynchronously loads a character card from a file path.
    /// </summary>
    /// <param name="path">
    /// The path of the file to load.
    /// </param>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A <see cref="CharacterCardInfo"/> containing the deserialized
    /// <see cref="CharacterCardV3"/> model and detected format.
    /// </returns>
    /// <remarks>
    /// PNG, APNG, JSON, and CHARX are supported.
    /// </remarks>
    public static async Task<CharacterCardInfo> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        await using FileStream fileStream = File.OpenRead(path);
        return await LoadAsync(fileStream, cancellationToken);
    }

    /// <summary>
    /// Asynchronously loads a character card from a stream.
    /// </summary>
    /// <param name="stream">
    /// The stream containing file bytes to load.
    /// </param>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A <see cref="CharacterCardInfo"/> containing the deserialized
    /// <see cref="CharacterCardV3"/> model and detected format.
    /// </returns>
    /// <remarks>
    /// PNG, APNG, JSON, and CHARX are supported.
    /// </remarks>
    public static async Task<CharacterCardInfo> LoadAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        if (IsPng(stream)) return await LoadFromPngAsync(stream, cancellationToken);
        if (IsCharx(stream)) return await LoadFromCharxAsync(stream, cancellationToken);
        return await LoadFromJsonAsync(stream, cancellationToken);
    }
    #endregion Load Methods

    internal static CharacterCardInfo LoadFromPng(Stream stream)
    {
        using MagickImage magickImage = new(stream);
        string base64String = magickImage.GetAttribute(PngAttributeName) ??
                              magickImage.GetAttribute(PngLegacyAttributeName) ??
                              throw new InvalidDataException("The PNG file does not contain a valid character card.");

        using MemoryStream imageStream = new();
        magickImage.Write(imageStream);

        return new(CharacterCardJsonSerializer.Deserialize(Convert.FromBase64String(base64String)), CharacterCardFormat.Png) {IconBytes = imageStream.ToArray()};
    }

    internal static async Task<CharacterCardInfo> LoadFromPngAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        using MagickImage magickImage = new(stream);

        string base64String = magickImage.GetAttribute(PngAttributeName) ??
                              magickImage.GetAttribute(PngLegacyAttributeName) ??
                              throw new InvalidDataException("The PNG file does not contain a valid character card.");
        using MemoryStream jsonStream = new(Convert.FromBase64String(base64String));

        using MemoryStream imageStream = new();
        await magickImage.WriteAsync(imageStream, cancellationToken);

        return new(await CharacterCardJsonSerializer.DeserializeAsync(jsonStream, cancellationToken), CharacterCardFormat.Png) {IconBytes = imageStream.ToArray()};
    }

    internal static CharacterCardInfo LoadFromJson(Stream stream) => new(CharacterCardJsonSerializer.Deserialize(stream), CharacterCardFormat.Json);

    internal static async Task<CharacterCardInfo> LoadFromJsonAsync(Stream stream, CancellationToken cancellationToken = default) => new(await CharacterCardJsonSerializer.DeserializeAsync(stream, cancellationToken), CharacterCardFormat.Json);

    internal static CharacterCardInfo LoadFromCharx(Stream stream)
    {
        using ZipArchive zipArchive = new(stream, ZipArchiveMode.Read);

        ZipArchiveEntry cardEntry = zipArchive.GetEntry(CharxCardEntryName) ?? throw new InvalidDataException("The CHARX file does not contain a valid character card.");
        using Stream cardStream = cardEntry.Open();

        CharacterCardInfo characterCardInfo = new(CharacterCardJsonSerializer.Deserialize(cardStream), CharacterCardFormat.Charx);

        Dictionary<string, byte[]> files = new(StringComparer.Ordinal);
        foreach (ZipArchiveEntry entry in zipArchive.Entries.Where(static entry => !string.Equals(entry.FullName, CharxCardEntryName, StringComparison.OrdinalIgnoreCase) && !entry.FullName.EndsWith('/') && !entry.FullName.EndsWith('\\')))
        {
            using Stream entryStream = entry.Open();
            using MemoryStream entryMemoryStream = new();
            entryStream.CopyTo(entryMemoryStream);
            files.TryAdd(entry.FullName, entryMemoryStream.ToArray());
        }
        characterCardInfo.CharxFiles = files;

        if (characterCardInfo.Model.Properties.Assets is not null)
        {
            foreach (CharacterCardV3Asset iconAsset in characterCardInfo.Model.Properties.Assets
                                                                            .Where(IsCharxIconAsset)
                                                                            .OrderBy(static asset => asset.Name.Equals("main", StringComparison.OrdinalIgnoreCase)))
            {
                int entryNameStartIndex = iconAsset.Uri.IndexOf("://", StringComparison.OrdinalIgnoreCase);
                int entryNameStartIndexIncrement = 3;

                if (entryNameStartIndex == -1)
                {
                    entryNameStartIndex = iconAsset.Uri.IndexOf(':', StringComparison.OrdinalIgnoreCase);
                    entryNameStartIndexIncrement = 1;
                }

                string relativeEntryName = iconAsset.Uri[(entryNameStartIndex + entryNameStartIndexIncrement)..];
                if (files.TryGetValue(relativeEntryName, out byte[]? iconBytes))
                {
                    characterCardInfo.IconBytes = iconBytes;
                }
                else
                {
                    ZipArchiveEntry? iconEntry = zipArchive.GetEntry(relativeEntryName);
                    if (iconEntry is not null)
                    {
                        using Stream iconStream = iconEntry.Open();
                        using MemoryStream memoryStream = new();
                        iconStream.CopyTo(memoryStream);
                        characterCardInfo.IconBytes = memoryStream.ToArray();
                    }
                }
            }
        }
        return characterCardInfo;
    }

    internal static async Task<CharacterCardInfo> LoadFromCharxAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        await using ZipArchive archive = new(stream, ZipArchiveMode.Read);

        ZipArchiveEntry cardEntry = archive.GetEntry(CharxCardEntryName) ?? throw new InvalidDataException("The CHARX file does not contain a valid character card.");
        await using Stream cardStream = await cardEntry.OpenAsync(cancellationToken);

        CharacterCardInfo characterCardInfo = new(await CharacterCardJsonSerializer.DeserializeAsync(cardStream, cancellationToken), CharacterCardFormat.Charx);

        static async Task<byte[]> GetEntryBytes(ZipArchiveEntry entry, CancellationToken cancellationToken = default)
        {
            await using Stream entryStream = await entry.OpenAsync(cancellationToken);
            using MemoryStream entryCopyStream = new();
            await entryStream.CopyToAsync(entryCopyStream, cancellationToken);
            return entryCopyStream.ToArray();
        }

        Dictionary<string, byte[]> files = new(StringComparer.OrdinalIgnoreCase);
        foreach (ZipArchiveEntry entry in archive.Entries.Where(static entry => !string.Equals(entry.FullName, CharxCardEntryName, StringComparison.OrdinalIgnoreCase) && !Path.EndsInDirectorySeparator(entry.FullName) && !entry.FullName.EndsWith('/') && !entry.FullName.EndsWith('\\')))
        {
            if (!files.ContainsKey(entry.FullName)) files.Add(entry.FullName, await GetEntryBytes(entry, cancellationToken));
        }
        characterCardInfo.CharxFiles = files;

        if (characterCardInfo.Model.Properties.Assets is not null)
        {
            foreach (string iconEntryName in characterCardInfo.Model.Properties.Assets
                                                              .Where(IsCharxIconAsset)
                                                              .OrderBy(static asset => asset.Name.Equals("main", StringComparison.OrdinalIgnoreCase))
                                                              .Select(static asset => asset.Uri[GetCharxAssetUriStartIndex(asset)..]))
            {
                if (files.TryGetValue(iconEntryName, out byte[]? iconBytes))
                {
                    characterCardInfo.IconBytes = iconBytes;
                }
                else
                {
                    ZipArchiveEntry? iconEntry = archive.GetEntry(iconEntryName);
                    if (iconEntry is not null) characterCardInfo.IconBytes = await GetEntryBytes(iconEntry, cancellationToken);
                }
            }
        }
        return characterCardInfo;
    }

    private static MagickImage GetMagickImageOrBlankFromStream(Stream? stream)
    {
        if (stream is not null)
        {
            if (!IsPng(stream)) return new(MagickColors.White, 1U, 1U);

            try
            {
                return new(stream);
            }
            catch
            {
                return new(MagickColors.White, 1U, 1U);
            }
        }
        return new(MagickColors.White, 1U, 1U);
    }

    private static MagickImage? GetMagickImageOrNullFromStream(Stream? stream)
    {
        if (stream is not null)
        {
            if (!IsPng(stream)) return null;

            try
            {
                return new(stream);
            }
            catch
            {
                return null;
            }
        }
        return null;
    }

    private static void TryConvertMagickImageToPng(MagickImage magickImage)
    {
        if (magickImage.Format is not (MagickFormat.Png or
                                       MagickFormat.Png00 or
                                       MagickFormat.Png24 or
                                       MagickFormat.Png32 or
                                       MagickFormat.Png48 or
                                       MagickFormat.Png64 or
                                       MagickFormat.Png8 or
                                       MagickFormat.APng))
        {
            magickImage.Format = MagickFormat.Png;
        }
    }

    private static int GetCharxAssetUriStartIndex(CharacterCardV3Asset asset)
    {
        int uriStartIndex = asset.Uri.IndexOf("://", StringComparison.OrdinalIgnoreCase);
        if (uriStartIndex != -1) uriStartIndex += 3;

        // Fall back to __asset:
        if (uriStartIndex == -1)
        {
            uriStartIndex = asset.Uri.IndexOf(':', StringComparison.OrdinalIgnoreCase);
            if (uriStartIndex != -1) ++uriStartIndex;
        }

        return uriStartIndex;
    }

    public static bool IsPng(Stream stream)
    {
        Span<byte> headerSpan = stackalloc byte[8];
        int bytesRead = stream.Read(headerSpan);

        stream.Position = 0L;
        return bytesRead >= 8 && headerSpan.SequenceEqual(PngHeader);
    }

    public static bool IsCharx(string path)
    {
        using FileStream fileStream = File.OpenRead(path);
        return IsCharx(fileStream);
    }

    public static bool IsCharx(Stream stream)
    {
        Span<byte> headerSpan = stackalloc byte[4];
        int bytesRead = stream.Read(headerSpan);

        if (bytesRead < 4 || !headerSpan[..2].SequenceEqual(CharxHeaderPart1))
        {
            stream.Position = 0L;
            return false;
        }

        bool isHeaderPart2Valid = false;
        foreach (byte[] sequence in CharxHeaderPart2)
        {
            if (headerSpan[2..4].SequenceEqual(sequence))
            {
                isHeaderPart2Valid = true;
                break;
            }
        }
        stream.Position = 0L;
        return isHeaderPart2Valid;
    }

    private static bool IsCharxIconAsset(CharacterCardV3Asset asset) => asset.Type == "icon" && !asset.Uri.IsWhiteSpace() && CharxAssetUriPrefixes.Any(prefix => asset.Uri.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

    private static void AddMainIconAsset(CharacterCardV3 characterCard, string mainIconEntryName, string mainIconExtension)
    {
        characterCard.Properties.Assets ??= [with(1)];
        CharacterCardV3Asset? mainIconAsset = characterCard.Properties.Assets.OrderBy(static asset => asset.Name == "main").FirstOrDefault(IsCharxIconAsset);
        string mainIconAssetUri = $"{CharxAssetUriPrefixes[0]}{mainIconEntryName}";
        if (mainIconAsset is null)
        {
            characterCard.Properties.Assets.Add(new()
            {
                Type = CharxMainIconAssetType,
                Uri = mainIconAssetUri,
                Name = CharxMainIconAssetName,
                Extension = mainIconExtension,
            });
        }
        else
        {
            if (mainIconAsset.Name == CharxMainIconAssetName && mainIconAsset.Uri != mainIconAssetUri) mainIconAsset.Uri = mainIconAssetUri;
        }
    }
}
