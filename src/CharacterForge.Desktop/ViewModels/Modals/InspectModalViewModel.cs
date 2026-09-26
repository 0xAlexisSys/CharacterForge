using CharacterForge.Desktop.Services;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;
namespace CharacterForge.Desktop.ViewModels;

[UsedImplicitly]
public sealed partial class InspectModalViewModel(WindowService windowService) : ViewModel
{
    public string PreviewText { get; set; } = string.Empty;
    public int TotalTokenCount { get; set; } = 0;
    public int StandardGreetingCount { get; set; } = 0;
    public int GroupOnlyGreetingCount { get; set; } = 0;

    [RelayCommand]
    private void Close() => windowService.HideModal();
}
