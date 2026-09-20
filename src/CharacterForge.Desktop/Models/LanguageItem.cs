using System.Collections.Immutable;

namespace CharacterForge.Desktop.Models;

public sealed record class LanguageItem(string Code, string DisplayName)
{
    public static readonly ImmutableArray<LanguageItem> Languages =
    [
        new("en", "English"),
        new("es", "Spanish"),
        new("fr", "French"),
        new("nl", "Dutch"),
        new("it", "Italian"),
        new("de", "German"),
        new("sv", "Swedish"),
        new("fi", "Finnish"),
        new("da", "Danish"),
        new("pl", "Polish"),
        new("cs", "Czech"),
        new("el", "Greek"),
        new("ru", "Russian"),
        new("uk", "Ukrainian"),
        new("ja", "Japanese"),
        new("zh", "Chinese"),
        new("ko", "Korean"),
        new("pt", "Portuguese"),
        new("vi", "Vietnamese"),
        new("no", "Norwegian"),
        new("hu", "Hungarian"),
        new("ro", "Romanian"),
        new("id", "Indonesian"),
        new("tr", "Turkish"),
        new("ar", "Arabic"),
        new("hi", "Hindi"),
        new("th", "Thai"),
    ];
}
