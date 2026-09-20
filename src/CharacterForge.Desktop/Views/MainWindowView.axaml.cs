using Avalonia.Controls;
using Avalonia.Input;
using CharacterForge.Desktop.ViewModels;
using JetBrains.Annotations;

namespace CharacterForge.Desktop.Views;

[UsedImplicitly]
public sealed partial class MainWindowView : Window
{
    public MainWindowView() => InitializeComponent();

    public MainWindowView(MainWindowViewModel mainWindowViewModel) : this() => DataContext = mainWindowViewModel;

    private void OnModalOverlayPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        args.Handled = true;
        ModalOverlay.IsVisible = false;
    }

    private void OnModalContainerPointerPressed(object? sender, PointerPressedEventArgs args) => args.Handled = true;
}
