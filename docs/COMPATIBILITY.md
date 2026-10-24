# Compatibility and support policy

## Supported baseline

| Area | Baseline | Status |
|---|---|---|
| .NET NuGet assets | `net8.0;net10.0` for the five core-family packages; `net10.0` for the eleven opt-in third-party theme packages | The theme packages retain the TFMs needed by their selected Avalonia 11 upstream dependencies. The desktop Gallery and tests target .NET 10 |
| Avalonia | `11.3.22` | Build and headless-test baseline for the Avalonia control packages; see `Directory.Packages.props` |
| Windows | x64/ARM64 hosts supported by Avalonia 11 | Library-compatible; device and assistive-technology sign-off remains external |
| macOS | x64/Apple Silicon hosts supported by Avalonia 11 | Library-compatible; VoiceOver sign-off remains external |
| Linux | x64/ARM64 hosts supported by Avalonia 11 | Headless Linux tested; Orca sign-off remains external |
| Android | `Md3.Avalonia11.Notifications` packs a `net10.0-android` asset (minimum API 23) when the pack scripts pass `-p:IncludeAndroidTarget=true`; Gallery host uses `net10.0-android` | Only `net10.0-android` is offered: `net8.0-android` is out of support and raises NETSDK1202. Emulator/device sign-off remains external |
| iOS/browser | Not a preview-release target | No support commitment |

The core runtime package references only `Avalonia` and `MaterialColorUtilities`. It does not reference Avalonia Desktop, Win32, X11, macOS, or a desktop-only theme package. Platform-specific notification targets are documented separately below.

## Package identities and versioning

- All seventeen generated packages use the `Md3.Avalonia11` / `Md3.Avalonia11.*` identity family. Assembly names, C# namespaces, project paths, and Avalonia resource URIs intentionally retain their existing `Md3.Avalonia*` names.
- Current source package baseline: `0.5.0-preview.1` (publication pending). Older release notes and validation evidence are historical records, not statements that those old packages support Avalonia 11.
- Preview packages may adjust API after a changelog entry and migration note. Stable releases follow Semantic Versioning.
- Avalonia minor-version upgrades are validated by Release builds for both supported .NET package TFMs, headless tests, package validation, and the platform matrix before the supported baseline changes.
- The public `Md*` control types, styled properties, enums, theme JSON fields, and resource-key names form the compatibility surface.

## Avalonia 11 third-party dependency baseline

Each integration is pinned to a release whose dependency chain supports Avalonia 11. A newer Avalonia 12-only release is not selected merely because it has a higher version number.

| Integration | Selected package and version |
|---|---|
| DataGrid | `Avalonia.Controls.DataGrid 11.3.13` |
| AvaloniaEdit | `Avalonia.AvaloniaEdit 11.4.1` |
| AvaloniaHex | `AvaloniaHex 0.1.12` |
| Dock | `Dock.Avalonia`, `Dock.Avalonia.Themes.Fluent`, and `Dock.Model.Avalonia 11.3.12.1` |
| Tabalonia | `Tabalonia 0.10.7` |
| Markdown | `Markdown.Avalonia.Tight 11.0.3` |
| LiveCharts2 | `LiveChartsCore.SkiaSharpView.Avalonia 2.0.5` |
| ScottPlot | `ScottPlot.Avalonia 5.0.56` |
| PropertyGrid | `bodong.Avalonia.PropertyGrid 11.3.11.1` |
| NodifyM | `NodifyM.Avalonia 1.1.9` |
| Mapsui | `Mapsui.Avalonia 5.0.2` |
| InkCanvas | `DotNetCampus.AvaloniaInkCanvas 1.0.1` |

`OlivierJC.AvaloniaRichEditor` has no Avalonia 11-compatible release, so the third-party `Md3.Avalonia.RichEditor` theme package, its Gallery integration, tests, and packaging entries are removed. The independent `MdRichEditor` / `MdRichTextEditor` controls and text-box adapter in `Md3.Avalonia11.Extra` do not depend on that package and remain available.

ScottPlot 5.0.56 brings `SkiaSharp.NativeAssets.Linux.NoDependencies` into its dependency chain. Runnable desktop and test heads exclude that native asset so Avalonia.Skia's fontconfig-enabled `libSkiaSharp` remains the only copy in their output.

## Native notification package

`Md3.Avalonia11.Notifications` is a standalone package targeting `net8.0` and `net10.0` for macOS/Linux; `net8.0-windows10.0.19041.0` and `net10.0-windows10.0.19041.0` for Windows; and, when the pack scripts pass `-p:IncludeAndroidTarget=true`, `net10.0-android` for Android. The Android target has a minimum API level of 23; `net8.0-android` is not offered because it is out of support. The package provides title-and-body notifications without referencing Avalonia. Windows calls WinRT toast APIs directly, macOS uses AppKit's legacy/deprecated `NSUserNotificationCenter`, Linux calls the native `libnotify` library, and Android uses `NotificationManager`; no external helper processes are launched. Linux still requires libnotify and a D-Bus notification service. See the [package README](../src/Md3.Avalonia.Notifications/README.md) for registration, native-library, and permission requirements. The implementation still requires manual display/permission verification on each operating system; no device or desktop sign-off is claimed.

## Android boundary

`gallery/Md3.Avalonia.Gallery.Android` is deliberately kept outside the desktop solution so developers without the Android workload can build the library and desktop Gallery. The Gallery host targets .NET 10 only; the independent NuGet libraries retain their .NET 8 assets. Validate the host with Java 17 and an installed .NET Android workload:

```bash
dotnet workload install android
dotnet build gallery/Md3.Avalonia.Gallery.Android -c Release
```

The Avalonia 11 host derives from `AvaloniaMainActivity<GalleryApp>`, overrides `CustomizeAppBuilder` to add `.WithInterFont()`, and uses `ISingleViewApplicationLifetime` with `AndroidGalleryView`. Popup templates do not force `OverlayLayer`, preserving Avalonia's Android fallback behavior.

## External acceptance still required

The repository cannot substitute automation for ARM64 device behavior or assistive-technology review. Before a stable release, sign the checklist in `docs/RELEASE_VALIDATION.md` on physical or representative systems for Android touch/IME/rotation/lifecycle, Windows Narrator, macOS VoiceOver, and Linux Orca.
