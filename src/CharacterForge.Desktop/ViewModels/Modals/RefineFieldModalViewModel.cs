using System;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CharacterForge.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace CharacterForge.Desktop.ViewModels;

[UsedImplicitly]
public sealed partial class RefineFieldModalViewModel : ViewModel
{
    [ObservableProperty]
    public partial string Title { get; set; } = "Refine Field";

    [ObservableProperty]
    public partial bool ShowDescriptionCheckBox { get; set; } = true;

    [ObservableProperty]
    public partial bool CanIncludeDescription { get; set; }

    [ObservableProperty]
    public partial bool IncludeDescription { get; set; }

    [ObservableProperty]
    public partial bool ShowPersonalityCheckBox { get; set; } = true;

    [ObservableProperty]
    public partial bool CanIncludePersonality { get; set; }

    [ObservableProperty]
    public partial bool IncludePersonality { get; set; }

    [ObservableProperty]
    public partial bool ShowScenarioCheckBox { get; set; } = true;

    [ObservableProperty]
    public partial bool CanIncludeScenario { get; set; }

    [ObservableProperty]
    public partial bool IncludeScenario { get; set; }

    [ObservableProperty]
    public partial bool ShowExampleMessagesCheckBox { get; set; } = true;

    [ObservableProperty]
    public partial bool CanIncludeExampleMessages { get; set; }

    [ObservableProperty]
    public partial bool IncludeExampleMessages { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRefine))]
    public partial string Prompt { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRefine))]
    public partial bool IsRefining { get; set; }

    public bool CanRefine { get => !IsRefining && !Prompt.IsWhiteSpace(); }

    public Func<string, bool, bool, bool, bool, Task>? StartRefinement;

    private readonly WindowService _windowService;

    public RefineFieldModalViewModel(WindowService windowService, CharacterCardEditorViewModel characterCardEditorViewModel)
    {
        _windowService = windowService;

        CanIncludeDescription = !characterCardEditorViewModel.Description.IsWhiteSpace();
        IncludeDescription = CanIncludeDescription;

        CanIncludePersonality = !characterCardEditorViewModel.Personality.IsWhiteSpace();
        IncludePersonality = CanIncludePersonality;

        CanIncludeScenario = !characterCardEditorViewModel.Scenario.IsWhiteSpace();
        IncludeScenario = CanIncludeScenario;

        CanIncludeExampleMessages = !characterCardEditorViewModel.ExampleMessages.IsWhiteSpace();
        IncludeExampleMessages = CanIncludeExampleMessages;
    }

    [RelayCommand]
    private async Task Refine()
    {
        if (StartRefinement is not null && CanRefine)
        {
            try
            {
                IsRefining = true;
                await StartRefinement.Invoke(Prompt, IncludeDescription, IncludePersonality, IncludeScenario, IncludeExampleMessages);
                _windowService.HideModal();
            }
            catch (Exception exception)
            {
                _windowService.ShowNotification(exception.Message, NotificationType.Error, title: "Refinement Failed");
            }
            finally
            {
                IsRefining = false;
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

    partial void OnShowExampleMessagesCheckBoxChanged(bool value)
    {
        if (!value) IncludeExampleMessages = false;
    }
}
