using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace CharacterForge.Desktop.ViewModels;

[UsedImplicitly]
public sealed partial class TopbarViewModel(CharacterCardEditorViewModel characterCardEditorViewModel) : ViewModel
{
    [RelayCommand]
    private void OpenCharacterCard() => characterCardEditorViewModel.OpenCharacterCardCommand.Execute(null);

    [RelayCommand]
    private void ExportCharacterCard() => characterCardEditorViewModel.ExportCharacterCardCommand.Execute(null);
}
