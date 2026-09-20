using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CharacterForge.Desktop.Views;

#if DEBUG
using Avalonia.Diagnostics;
#endif

namespace CharacterForge.Desktop;

public sealed class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            lifetime.MainWindow = Program.GetService<MainWindowView>();
            #if DEBUG
            lifetime.MainWindow.AttachDevTools(new DevToolsOptions {LaunchView = DevToolsViewKind.VisualTree});
            #endif
        }
    }
}
