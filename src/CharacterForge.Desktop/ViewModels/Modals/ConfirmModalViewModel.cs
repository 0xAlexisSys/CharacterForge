using System;
using CharacterForge.Desktop.Services;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace CharacterForge.Desktop.ViewModels;

[UsedImplicitly]
public sealed partial class ConfirmModalViewModel : ViewModel
{
    public string Title { get; set; } = "Please Confirm...";
    public string Message { get; set; } = "Are you sure you want to proceed?";
    public string ConfirmButtonText { get; set; } = "Confirm";
    public string CancelButtonText { get; set; } = "Cancel";
    public Action? OnConfirmed { get; set; }
    public Action? OnCanceled { get; set; }

    public ConfirmModalViewModel(WindowService windowService) => OnCanceled ??= windowService.HideModal;

    [RelayCommand]
    private void Confirm() => OnConfirmed?.Invoke();

    [RelayCommand]
    private void Cancel() => OnCanceled?.Invoke();
}
