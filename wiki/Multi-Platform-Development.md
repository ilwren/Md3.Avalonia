# Multi-Platform Development

Md3.Avalonia targets **.NET 10.0** and runs seamlessly across Desktop (Windows, macOS, Linux) and Mobile (Android, iOS).

---

## 1. Project Organization Pattern

```
gallery/
├── Md3.Avalonia.Gallery/           # Shared views, pages, viewmodels, App.axaml (net10.0 library)
├── Md3.Avalonia.Gallery.Desktop/   # Desktop entry point (net10.0 WinExe)
└── Md3.Avalonia.Gallery.Android/   # Android entry point (net10.0-android Exe)
```

---

## 2. Desktop Launcher (`Md3.Avalonia.Gallery.Desktop`)

`Program.cs`:
```csharp
using Avalonia;
using Md3.Avalonia.Gallery;

namespace Md3.Avalonia.Gallery.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
```

---

## 3. Android Launcher (`Md3.Avalonia.Gallery.Android`)

`MainActivity.cs`:
```csharp
using Android.App;
using Android.Content.PM;
using Avalonia.Android;
using Md3.Avalonia.Gallery;

namespace Md3.Avalonia.Gallery.Android;

[Activity(
    Label = "Material 3 Gallery",
    Theme = "@style/MyTheme.NoActionBar",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public sealed class MainActivity : AvaloniaMainActivity<App>
{
}
```

---

## 4. Adaptive Responsive Breakpoints (`MdAdaptiveLayout`)

For applications adapting between phones, foldables, tablets, and desktops:

```xml
<md:MdAdaptiveLayout>
    <md:MdAdaptiveLayout.CompactContent>
        <!-- Phone layout (Bottom Navigation Bar) -->
        <md:MdNavigationBar ... />
    </md:MdAdaptiveLayout.CompactContent>
    <md:MdAdaptiveLayout.MediumContent>
        <!-- Tablet layout (Navigation Rail) -->
        <md:MdNavigationRail ... />
    </md:MdAdaptiveLayout.MediumContent>
    <md:MdAdaptiveLayout.ExpandedContent>
        <!-- Desktop layout (Full Navigation Drawer) -->
        <md:MdNavigationDrawer ... />
    </md:MdAdaptiveLayout.ExpandedContent>
</md:MdAdaptiveLayout>
```
