using System;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CharacterForge.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace CharacterForge.Desktop.ViewModels;

[UsedImplicitly]
public sealed partial class GenerateDescriptionModalViewModel : ViewModel
{
    [ObservableProperty]
    public partial string Personality { get; set; }

    [ObservableProperty]
    public partial bool IncludePersonality { get; set; }

    [ObservableProperty]
    public partial string Scenario { get; set; }

    [ObservableProperty]
    public partial bool IncludeScenario { get; set; }

    [ObservableProperty]
    public partial string ExampleMessages { get; set; }

    [ObservableProperty]
    public partial bool IncludeExampleMessages { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGenerate))]
    public partial string Prompt { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGenerate))]
    public partial bool IsGenerating { get; set; }

    public bool CanGenerate { get => !IsGenerating && !Prompt.IsWhiteSpace(); }

    public Func<string, bool, bool, bool, Task>? StartGeneration;

    private readonly DialogService _dialogService;

    public GenerateDescriptionModalViewModel(DialogService dialogService, CharacterCardEditorViewModel characterCardEditorViewModel)
    {
        _dialogService = dialogService;

        Personality = characterCardEditorViewModel.Personality.Trim();
        IncludePersonality = Personality.Length != 0;
        Scenario = characterCardEditorViewModel.Scenario.Trim();
        IncludeScenario = Scenario.Length != 0;
        ExampleMessages = characterCardEditorViewModel.ExampleMessages.Trim();
        IncludeExampleMessages = ExampleMessages.Length != 0;
    }

    [RelayCommand]
    private async Task Generate()
    {
        if (StartGeneration is not null && CanGenerate)
        {
            try
            {
                IsGenerating = true;
                await StartGeneration.Invoke(Prompt, IncludePersonality, IncludeScenario, IncludeExampleMessages);
                _dialogService.HideModal();
            }
            catch (Exception exception)
            {
                _dialogService.ShowNotification(exception.Message, NotificationType.Error, title: "Generation Failed");
            }
            finally
            {
                IsGenerating = false;
            }
        }
    }

    [RelayCommand]
    private void Cancel() => _dialogService.HideModal();
}
