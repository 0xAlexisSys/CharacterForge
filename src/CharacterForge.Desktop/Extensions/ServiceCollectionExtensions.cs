using Avalonia.Controls;
using CharacterForge.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CharacterForge.Desktop.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection self)
    {
        public IServiceCollection AddSingletonViewAndViewModel<TView, TViewModel>() where TView : Control where TViewModel : ViewModel
        {
            ViewLocator.RegisterView<TView, TViewModel>();
            return self.AddSingleton<TView>()
                       .AddSingleton<TViewModel>();
        }

        public IServiceCollection AddTransientViewAndViewModel<TView, TViewModel>() where TView : Control where TViewModel : ViewModel
        {
            ViewLocator.RegisterView<TView, TViewModel>();
            return self.AddTransient<TView>()
                       .AddTransient<TViewModel>();
        }
    }
}
