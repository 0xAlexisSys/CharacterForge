using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace CharacterForge.Infrastructure;

/// <summary>
/// Represents a semantic version.
/// </summary>
[PublicAPI]
public readonly partial record struct SemanticVersion
{
    [GeneratedRegex(@"v?(?<Major>[0-9]+)(?:\.(?<Minor>[0-9]+))?", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex VersionStringPattern { get; }

    public readonly uint Major = 0U;
    public readonly uint Minor = 0U;

    /// <summary>
    /// Initializes a new <see cref="SemanticVersion"/> <see langword="struct"/> from the
    /// specified <paramref name="versionString"/>.
    /// </summary>
    /// <param name="versionString">
    /// The string representation of the version.
    /// </param>
    public SemanticVersion(string versionString)
    {
        Match match = VersionStringPattern.Match(versionString);
        if (match.Groups["Major"].Success) Major = Convert.ToUInt32(match.Groups["Major"].Value);
        if (match.Groups["Minor"].Success) Minor = Convert.ToUInt32(match.Groups["Minor"].Value);
    }

    public override string ToString() => $"{Major}.{Minor}";
}
