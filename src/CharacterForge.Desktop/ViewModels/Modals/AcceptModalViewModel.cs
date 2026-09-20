using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace CharacterForge.Desktop.ViewModels;

[UsedImplicitly]
public sealed partial class AcceptModalViewModel : ViewModel
{
    [ObservableProperty]
    public partial string Title { get; set; } = "Alert!";

    [ObservableProperty]
    public partial string Message { get; set; } = "Are you sure you want to proceed?";

    [ObservableProperty]
    public partial string AcceptButtonText { get; set; } = "OK";

    public Action? OnAccepted { get; set; }

    [RelayCommand]
    private void Accept() => OnAccepted?.Invoke();
}
