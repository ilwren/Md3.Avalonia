using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace Md3.Avalonia.Gallery.Android;

[Activity(
    Label = "Material 3 Gallery",
    Theme = "@style/MyTheme.NoActionBar",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public sealed class MainActivity : AvaloniaMainActivity<AndroidApp>
{
}
