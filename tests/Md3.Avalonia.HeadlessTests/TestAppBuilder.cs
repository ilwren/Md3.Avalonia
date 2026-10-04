using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
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
