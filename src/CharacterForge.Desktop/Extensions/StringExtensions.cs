using System;
using System.Text.RegularExpressions;

namespace CharacterForge.Desktop.Extensions;

public static partial class StringExtensions
{
    [GeneratedRegex(@"[\r\n]+", RegexOptions.CultureInvariant)]
    private static partial Regex NewLinePattern { get; }

    extension(string @this)
    {
        public string ToReverse()
        {
            Span<char> reversedSpan = stackalloc char[@this.Length];
            for (int i = 0; i < @this.Length; i++) reversedSpan[@this.Length - 1 - i] = @this[i];
            return new(reversedSpan);
        }

        public string ReplaceLineEndingsWithOneWhiteSpace() => NewLinePattern.Replace(@this, " ");
    }
}
