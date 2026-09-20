using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CharacterForge.Desktop.ViewModels;

namespace CharacterForge.Desktop;

public sealed class ViewLocator : IDataTemplate
{
    private static readonly Dictionary<Type, Type> RegisteredViews = [];

    public static void RegisterView<TView, TViewModel>() where TView : Control where TViewModel : ViewModel => RegisteredViews.Add(typeof(TViewModel), typeof(TView));

    public Control Build(object? data)
    {
        const string UnknownViewModelName = "UnknownViewModel";

        Type? viewModelType = data?.GetType();
        if (viewModelType is null) return new TextBlock {Text = $"No view for {UnknownViewModelName}"};
        Type? viewType = RegisteredViews.GetValueOrDefault(viewModelType);
        return viewType is not null ? Program.GetService<Control>(viewType) : new TextBlock {Text = $"No view for {viewModelType.FullName ?? UnknownViewModelName}"};
    }

    public bool Match(object? data) => data is ViewModel;
}
