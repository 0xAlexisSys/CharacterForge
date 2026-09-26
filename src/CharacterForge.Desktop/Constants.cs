using System.Text.RegularExpressions;

namespace CharacterForge.Desktop;

public static partial class Constants
{
    public const string CharacterCardMarkdownHeader = "# Character Card\n\n";

    [GeneratedRegex(@"```plaintext\n(?<Value>.+)\n```", RegexOptions.CultureInvariant | RegexOptions.Singleline)]
    public static partial Regex GeneratedMultiLineTextPattern { get; }
}
