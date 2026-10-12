using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LiveChartsCore;
using Md3.Avalonia.Themes;

namespace Md3.Avalonia.Gallery;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        // LiveCharts resolves every chart's look from one theme held by its global settings - there
        // is no per-control style seam - so the gallery configures it once here, before the first
        // chart view exists. The chart page itself sets no paints at all.
        LiveCharts.Configure(settings => settings.AddMaterialTheme());

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            singleView.MainView = new AndroidGalleryView();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
