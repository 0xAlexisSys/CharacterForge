using System.Collections.Generic;
using JetBrains.Annotations;

namespace CharacterForge.Infrastructure.CharacterCards;

/// <summary>
/// Specifies the container format of a character card.
/// </summary>
[PublicAPI]
public enum CharacterCardFormat : byte
{
    Png,
    Json,
    Charx,
}

/// <summary>
/// Encapsulates loaded character card data.
/// </summary>
/// <param name="Model">
/// The deserialized <see cref="CharacterCardV3"/> model.
/// </param>
/// <param name="Format">
/// The container format from which the character card was loaded.
/// </param>
[PublicAPI]
public sealed record class CharacterCardInfo(CharacterCardV3 Model, CharacterCardFormat Format)
{
    /// <summary>
    /// The character icon extracted from an image or CHARX archive.
    /// <see langword="null"/> if not applicable or not present.
    /// </summary>
    public byte[]? IconBytes { get; internal set; }

    /// <summary>
    /// All files extracted from a CHARX archive (excluding <c>card.json</c>) as
    /// raw bytes, keyed by their archive entry path. <see langword="null"/> if the
    /// character card was not loaded from a CHARX archive.
    /// </summary>
    public IReadOnlyDictionary<string, byte[]>? CharxFiles { get; internal set; }

    public readonly SemanticVersion SpecificationVersion = new(Model.SpecificationVersionString);
}
