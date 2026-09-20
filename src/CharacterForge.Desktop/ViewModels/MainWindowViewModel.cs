using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;

namespace CharacterForge.Desktop.ViewModels;

[UsedImplicitly]
public sealed partial class MainWindowViewModel : ViewModel
{
    [ObservableProperty]
    public partial ViewModel CurrentTab { get; set; }

    [ObservableProperty]
    public partial ViewModel? ModalContent { get; set; }

    public TopbarViewModel TopbarViewModel { get; }

    public MainWindowViewModel(TopbarViewModel topbarViewModel, CharacterCardEditorViewModel characterCardEditorViewModel)
    {
        TopbarViewModel = topbarViewModel;
        CurrentTab = characterCardEditorViewModel;
    }
}
