using JetBrains.Annotations;

namespace CharacterForge.Infrastructure.Lorebooks;

/// <summary>
/// Specifies the container format of a character card.
/// </summary>
[PublicAPI]
public enum LorebookFormat : byte
{
    Json,
}

/// <summary>
/// Encapsulates loaded lorebook data.
/// </summary>
/// <param name="Model">
/// The deserialized <see cref="LorebookV3"/> model.
/// </param>
/// <param name="Format">
/// The container format from which the lorebook was loaded.
/// </param>
[PublicAPI]
public sealed record class LorebookInfo(LorebookV3 Model, LorebookFormat Format)
{
    public readonly SemanticVersion Version = new("3");
}
