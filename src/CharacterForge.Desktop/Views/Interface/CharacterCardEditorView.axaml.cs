using System;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using CharacterForge.Desktop.Services;
using JetBrains.Annotations;

namespace CharacterForge.Desktop.Views;

[UsedImplicitly]
public sealed partial class CharacterCardEditorView : UserControl
{
    private readonly WindowService _windowService = null!;

    public CharacterCardEditorView() => InitializeComponent();

    public CharacterCardEditorView(WindowService windowService) : this() => _windowService = windowService;

    protected override void OnLoaded(RoutedEventArgs args) => IconBox.ImageSetFailed += OnIconBoxImageSetFailed;

    protected override void OnUnloaded(RoutedEventArgs args) => IconBox.ImageSetFailed -= OnIconBoxImageSetFailed;

    private void OnIconBoxImageSetFailed(object? sender, Exception exception) => _windowService.ShowNotification("Icon file is invalid.", NotificationType.Error, title: "Open Failed");
}
