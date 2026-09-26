using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using CharacterForge.Desktop.ViewModels;
using CharacterForge.Desktop.Views;

namespace CharacterForge.Desktop.Services;

public sealed class WindowService(IClassicDesktopStyleApplicationLifetime lifetime)
{
    private static readonly ImmutableArray<string> DefaultNotificationTitles =
    [
        "Information",
        "Success",
        "Warning",
        "Error",
    ];

    private readonly Lazy<MainWindowView> _mainWindowView = new(() => (MainWindowView)lifetime.MainWindow!);
    private readonly Lazy<MainWindowViewModel> _mainWindowViewModel = new(() => (MainWindowViewModel)lifetime.MainWindow!.DataContext!);
    private readonly Lazy<IStorageProvider> _storageProvider = new(() => TopLevel.GetTopLevel(lifetime.MainWindow)!.StorageProvider);
    private readonly Lazy<IClipboard> _clipboard = new(() => TopLevel.GetTopLevel(lifetime.MainWindow)!.Clipboard!);

    public void ShowModal<TViewModel>(Action<TViewModel>? postGetAction = null) where TViewModel : ViewModel
    {
        TViewModel viewModel = Program.GetService<TViewModel>();
        postGetAction?.Invoke(viewModel);
        _mainWindowViewModel.Value.ModalContent = viewModel;
    }

    public void HideModal() => _mainWindowViewModel.Value.ModalContent = null;

    public void HideModal<TViewModel>() where TViewModel : ViewModel
    {
        if (_mainWindowViewModel.Value.ModalContent is TViewModel) HideModal();
    }

    public async Task<string[]> ShowOpenFileDialogAsync(FilePickerOpenOptions options) => [..(await _storageProvider.Value.OpenFilePickerAsync(options)).Select(static file => file.Path.LocalPath)];

    public async Task<string?> ShowSaveFileDialogAsync(FilePickerSaveOptions options) => (await _storageProvider.Value.SaveFilePickerAsync(options))?.Path.LocalPath;

    public void ShowNotification(string message, NotificationType type, string? title = null, TimeSpan? duration = null) => _mainWindowView.Value.NotificationManager.Show(new Notification(title ?? DefaultNotificationTitles[(int)type], message, type, duration ?? TimeSpan.FromSeconds(5L)));

    public async Task CopyToClipboardAsync(string text, bool showNotification = true)
    {
        await _clipboard.Value.SetTextAsync(text);
        if (showNotification) ShowNotification("Copied to clipboard!", NotificationType.Information, title: "Text Copied");
    }
}
