using System;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CharacterForge.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace CharacterForge.Desktop.ViewModels;

[UsedImplicitly]
public sealed partial class GenerateFieldModalViewModel : ViewModel
{
    [ObservableProperty]
    public partial bool ShowDescriptionCheckBox { get; set; } = true;

    [ObservableProperty]
    public partial bool ShowPersonalityCheckBox { get; set; } = true;

    [ObservableProperty]
    public partial bool ShowScenarioCheckBox { get; set; } = true;

    [ObservableProperty]
    public partial bool ShowExampleDialogueCheckBox { get; set; } = true;

    [ObservableProperty]
    public partial bool IncludeDescription { get; set; }

    [ObservableProperty]
    public partial bool IncludePersonality { get; set; }

    [ObservableProperty]
    public partial bool IncludeScenario { get; set; }

    [ObservableProperty]
    public partial bool IncludeExampleDialogue { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGenerate))]
    public partial string Prompt { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGenerate))]
    public partial bool IsGenerating { get; set; } = false;

    public string Title { get; set; } = "Generate Field";
    public bool CanIncludeDescription { get; set; }
    public bool CanIncludePersonality { get; set; }
    public bool CanIncludeScenario { get; set; }
    public bool CanIncludeExampleDialogue { get; set; }
    public bool IsPromptRequired { get; set; } = true;

    public bool CanGenerate { get => !IsGenerating && (!IsPromptRequired || !Prompt.IsWhiteSpace()); }

    public Func<string, bool, bool, bool, bool, Task>? StartGeneration;

    private readonly WindowService _windowService;

    public GenerateFieldModalViewModel(WindowService windowService, CharacterCardEditorViewModel characterCardEditorViewModel)
    {
        _windowService = windowService;

        CanIncludeDescription = !characterCardEditorViewModel.Description.IsWhiteSpace();
        IncludeDescription = CanIncludeDescription;

        CanIncludePersonality = !characterCardEditorViewModel.Personality.IsWhiteSpace();
        IncludePersonality = CanIncludePersonality;

        CanIncludeScenario = !characterCardEditorViewModel.Scenario.IsWhiteSpace();
        IncludeScenario = CanIncludeScenario;

        CanIncludeExampleDialogue = !characterCardEditorViewModel.ExampleDialogue.IsWhiteSpace();
        IncludeExampleDialogue = CanIncludeExampleDialogue;
    }

    [RelayCommand]
    private async Task Generate()
    {
        if (StartGeneration is not null && CanGenerate)
        {
            try
            {
                IsGenerating = true;
                await StartGeneration.Invoke(Prompt, IncludeDescription, IncludePersonality, IncludeScenario, IncludeExampleDialogue);
                _windowService.HideModal();
            }
            catch (Exception exception)
            {
                _windowService.ShowNotification(exception.Message, NotificationType.Error, title: "Generation Failed");
            }
            finally
            {
                IsGenerating = false;
            }
        }
    }

    [RelayCommand]
    private void Cancel() => _windowService.HideModal();

    partial void OnShowDescriptionCheckBoxChanged(bool value)
    {
        if (!value) IncludeDescription = false;
    }

    partial void OnShowPersonalityCheckBoxChanged(bool value)
    {
        if (!value) IncludePersonality = false;
    }

    partial void OnShowScenarioCheckBoxChanged(bool value)
    {
        if (!value) IncludeScenario = false;
    }

    partial void OnShowExampleDialogueCheckBoxChanged(bool value)
    {
        if (!value) IncludeExampleDialogue = false;
    }
}
