using System;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CharacterForge.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace CharacterForge.Desktop.ViewModels;

[UsedImplicitly]
public sealed partial class GeneratePersonalityModalViewModel : ViewModel
{
    [ObservableProperty]
    public partial string Description { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGenerate))]
    public partial bool IncludeDescription { get; set; }

    [ObservableProperty]
    public partial string Scenario { get; set; }

    [ObservableProperty]
    public partial bool IncludeScenario { get; set; }

    [ObservableProperty]
    public partial string ExampleMessages { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGenerate))]
    public partial bool IncludeExampleMessages { get; set; }

    [ObservableProperty]
    public partial string Prompt { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGenerate))]
    public partial bool IsGenerating { get; set; } = false;

    public bool CanGenerate { get => !IsGenerating && (IncludeDescription || IncludeExampleMessages); }

    public Func<string, bool, bool, bool, Task>? StartGeneration;

    private readonly WindowService _windowService;

    public GeneratePersonalityModalViewModel(WindowService windowService, CharacterCardEditorViewModel characterCardEditorViewModel)
    {
        _windowService = windowService;

        Description = characterCardEditorViewModel.Description.Trim();
        IncludeDescription = Description.Length != 0;
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
                await StartGeneration.Invoke(Prompt, IncludeDescription, IncludeScenario, IncludeExampleMessages);
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
}
