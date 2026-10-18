using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using LiveChartsCore;
using Md3.Avalonia.Extra.Themes;
using Md3.Avalonia.Themes;

[assembly: AvaloniaTestApplication(typeof(Md3.Avalonia.HeadlessTests.TestAppBuilder))]

namespace Md3.Avalonia.HeadlessTests;

public sealed class TestApplication : Application
{
    public override void Initialize()
    {
        Styles.Add(new MaterialTheme());
        Styles.Add(new EcosystemTheme());
        Styles.Add(new MaterialDataGridTheme());
        Styles.Add(new MaterialRichEditorTheme());
        Styles.Add(new MaterialAvaloniaEditTheme());
        Styles.Add(new MaterialAvaloniaHexTheme());
        Styles.Add(new MaterialDockTheme());
        Styles.Add(new MaterialTabaloniaTheme());
        Styles.Add(new MaterialInkCanvasTheme());

        // LiveCharts is the one dependency whose theme is not an Avalonia style collection: every
        // chart resolves its look from one theme held by its global settings, so the test app
        // configures the Material one exactly as the gallery's App.axaml.cs does - a chart view
        // created without a theme configured throws instead of drawing unthemed.
        LiveCharts.Configure(settings => settings.AddMaterialTheme());
    }
}

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<TestApplication>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions
            {
                UseHeadlessDrawing = false,
                ShouldRenderOnUIThread = true
            });
}
