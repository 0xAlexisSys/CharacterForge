using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CharacterForge.Desktop.Extensions;
using CharacterForge.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace CharacterForge.Desktop.ViewModels;

[UsedImplicitly]
public sealed partial class GenerateIconPromptModalViewModel(WindowService windowService, FieldGeneratorService fieldGeneratorService, CharacterCardEditorViewModel characterCardEditorViewModel) : ViewModel
{
    [ObservableProperty]
    public partial string Guidance { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasOutput))]
    public partial string Output { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGenerate))]
    public partial bool IsGenerating { get; set; }

    public bool CanGenerate { get => !IsGenerating; }
    public bool HasOutput { get => !Output.IsWhiteSpace(); }

    [RelayCommand]
    private async Task CopyOutputToClipboard()
    {
        if (HasOutput) await windowService.CopyToClipboardAsync(Output);
    }

    [RelayCommand]
    private async Task Generate()
    {
        if (!CanGenerate) return;

        try
        {
            IsGenerating = true;

            StringBuilder inputBuilder = new(Constants.CharacterCardMarkdownHeader);
            inputBuilder.AppendTextBlockWithHeader(nameof(CharacterCardEditorViewModel.Name), characterCardEditorViewModel.Name.Trim());
            inputBuilder.AppendTextBlockWithHeader(nameof(CharacterCardEditorViewModel.Description), characterCardEditorViewModel.Description.Trim());
            if (!Guidance.IsWhiteSpace()) inputBuilder.Append($"---\n\n{Guidance.Trim()}");

            Group output = Constants.GeneratedMultiLineTextPattern.Match(await fieldGeneratorService.GenerateAsync("IconPromptWriter", "MultiLineText", inputBuilder.ToString().TrimEnd())).Groups["Value"];
            if (!output.Success || output.ValueSpan.IsWhiteSpace()) throw new InvalidOperationException("Response did not contain text.");
            Output = output.Value.Trim();
        }
        catch (Exception exception)
        {
            windowService.ShowNotification(exception.Message, NotificationType.Error, title: "Generation Failed");
        }
        finally
        {
            IsGenerating = false;
        }
    }

    [RelayCommand]
    private void Close() => windowService.HideModal();
}
