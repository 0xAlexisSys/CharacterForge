using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace CharacterForge.Desktop.ViewModels;

[UsedImplicitly]
public sealed partial class ConfirmModalViewModel : ViewModel
{
    [ObservableProperty]
    public partial string Title { get; set; } = "Please Confirm...";

    [ObservableProperty]
    public partial string Message { get; set; } = "Are you sure you want to proceed?";

    [ObservableProperty]
    public partial string ConfirmButtonText { get; set; } = "Confirm";

    [ObservableProperty]
    public partial string CancelButtonText { get; set; } = "Cancel";

    public Action? OnConfirmed { get; set; }
    public Action? OnCancelled { get; set; }

    [RelayCommand]
    private void Confirm() => OnConfirmed?.Invoke();

    [RelayCommand]
    private void Cancel() => OnCancelled?.Invoke();
}
