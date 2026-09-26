using System;
using System.Net.Http;
using Avalonia;
using CharacterForge.Desktop.Extensions;
using CharacterForge.Desktop.Services;
using CharacterForge.Desktop.ViewModels;
using CharacterForge.Desktop.Views;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ML.Tokenizers;

namespace CharacterForge.Desktop;

public static class Program
{
    private static ServiceProvider _serviceProvider = null!;

    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, static lifetime =>
    {
        ServiceCollection services = new();
        services.AddSingleton(lifetime);
        BuildServiceProviderFrom(services);
    });

    public static void BuildServiceProviderFrom(ServiceCollection services)
    {
        services.AddSingleton(WeakReferenceMessenger.Default)
                .AddSingleton(StrongReferenceMessenger.Default)
                .AddSingleton(new HttpClient {Timeout = TimeSpan.FromMinutes(3L)})
                .AddSingleton<Tokenizer>(TiktokenTokenizer.CreateForModel("gpt-5"))
                .AddSingleton<FieldGeneratorService>()
                .AddSingleton<WindowService>()
                .AddSingletonViewAndViewModel<MainWindowView, MainWindowViewModel>()
                .AddSingletonViewAndViewModel<TopbarView, TopbarViewModel>()
                .AddSingletonViewAndViewModel<CharacterCardEditorView, CharacterCardEditorViewModel>()
                .AddTransientViewAndViewModel<AcceptModalView, AcceptModalViewModel>()
                .AddTransientViewAndViewModel<ConfirmModalView, ConfirmModalViewModel>()
                .AddTransientViewAndViewModel<InspectModalView, InspectModalViewModel>()
                .AddTransientViewAndViewModel<GenerateIconPromptModalView, GenerateIconPromptModalViewModel>()
                .AddTransientViewAndViewModel<GenerateDescriptionModalView, GenerateDescriptionModalViewModel>()
                .AddTransientViewAndViewModel<GeneratePersonalityModalView, GeneratePersonalityModalViewModel>()
                .AddTransientViewAndViewModel<GenerateGreetingModalView, GenerateGreetingModalViewModel>();
        _serviceProvider = services.BuildServiceProvider();
    }

    public static T GetService<T>() where T : class => _serviceProvider.GetRequiredService<T>();

    public static T GetService<T>(Type type) where T : class => (T)_serviceProvider.GetRequiredService(type);

    private static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
                                                              .UsePlatformDetect()
                                                              .WithInterFont()
                                                              .LogToTrace();
}
