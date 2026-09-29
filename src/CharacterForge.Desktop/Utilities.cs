using System;
using System.Text;
using System.Text.RegularExpressions;
using CharacterForge.Desktop.Extensions;
using NameData = (string Name, string? Nickname);

namespace CharacterForge.Desktop;

public static partial class Utilities
{
    [GeneratedRegex(@"{{user}}|<user>", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex MacroUserPattern { get; }

    // {{bot}} is not part of any specifications and appears to be exclusive to Risuai.
    [GeneratedRegex(@"{{char}}|{{bot}}|<char>|<bot>", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex MacroCharPattern { get; }

    // While CCv3 defines comment macros as {{comment: A}}, the value and separators
    // are ignored to match SillyTavern's parser behavior. The hidden_key macro is
    // treated like the comment macro for consistency here.
    [GeneratedRegex(@"{{//.*}}|{{hidden_key.*}}|{{comment.*}}", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex MacroCommentPattern { get; }

    [GeneratedRegex(@"{{reverse(?:\s+(?<Value>.+)|:{1,2}(?<Value>.+))}}", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex MacroReversePattern { get; }

    public static string ReplaceGeneralMacros(string text, NameData nameData)
    {
        text = MacroUserPattern.Replace(text, "User");
        text = MacroCharPattern.Replace(text, string.IsNullOrEmpty(nameData.Nickname) ? nameData.Name : nameData.Nickname);
        text = MacroCommentPattern.Replace(text, string.Empty);
        text = MacroReversePattern.Replace(text, static match => match.Groups["Value"].Value.ToReverse());
        return text;
    }

    /// <remarks>
    /// <c>***</c> is the default example message separator in <i>SillyTavern</i>.
    /// </remarks>
    public static string ReplaceMacroExampleMessageStart(string value) => value.Replace("<START>", "***", StringComparison.Ordinal);

    public static string BuildUserPromptForGeneration(string name, string? description, string? personality, string? scenario, string? exampleMessages, string? prompt)
    {
        name = name.Trim();
        description = description?.Trim();
        personality = personality?.Trim();
        scenario = scenario?.Trim();
        exampleMessages = exampleMessages?.Trim();
        prompt = prompt?.Trim();

        NameData nameData = (name, null);
        StringBuilder inputBuilder = new(Constants.CharacterCardMarkdownHeader);

        inputBuilder.AppendTextBlockWithHeader("Name", name.Length != 0 ? name : "Unspecified");
        if (!string.IsNullOrEmpty(description)) inputBuilder.AppendTextBlockWithHeader("Description", ReplaceGeneralMacros(description, nameData));
        if (!string.IsNullOrEmpty(personality)) inputBuilder.AppendTextBlockWithHeader("Personality", ReplaceGeneralMacros(personality, nameData));
        if (!string.IsNullOrEmpty(scenario)) inputBuilder.AppendTextBlockWithHeader("Scenario", ReplaceGeneralMacros(scenario, nameData));
        if (!string.IsNullOrEmpty(exampleMessages)) inputBuilder.AppendTextBlockWithHeader("Example Messages", ReplaceMacroExampleMessageStart(ReplaceGeneralMacros(exampleMessages, nameData)));
        if (!string.IsNullOrEmpty(prompt)) inputBuilder.AppendLine($"---\n\n{prompt}");
        return inputBuilder.ToString().TrimEnd();
    }
}
