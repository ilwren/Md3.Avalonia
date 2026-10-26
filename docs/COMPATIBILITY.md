# Compatibility and support policy

## Supported baseline

| Area | Baseline | Status |
|---|---|---|
| .NET NuGet assets | `net8.0;net10.0` for the five core-family packages and optional LibVLC adapter; `net10.0` for the twelve opt-in third-party theme packages | The theme packages follow the TFMs their upstream libraries publish. The LibVLC adapter is desktop-only and not AOT-compatible. The desktop Gallery and tests target .NET 10 |
| Avalonia | 12.1.2 | Build and headless-test baseline for the Avalonia control packages. The floor is set by the ecosystem packages, not by the core library; see `Directory.Packages.props` |
| Windows | x64/ARM64 hosts supported by Avalonia 12 | Library-compatible; device and assistive-technology sign-off remains external |
| macOS | x64/Apple Silicon hosts supported by Avalonia 12 | Library-compatible; VoiceOver sign-off remains external |
| Linux | x64/ARM64 hosts supported by Avalonia 12 | Headless Linux tested; Orca sign-off remains external |
| Android | `Md3.Avalonia.Notifications` packs a `net10.0-android` asset (minimum API 23) when the pack scripts pass `-p:IncludeAndroidTarget=true`; Gallery host uses `net10.0-android` | Only `net10.0-android` is offered: `net8.0-android` is out of support and raises NETSDK1202. Emulator/device sign-off remains external |
| iOS/browser | Not a preview-release target | No support commitment |

The core runtime package references only `Avalonia` and `MaterialColorUtilities`. It does not reference Avalonia Desktop, Win32, X11, macOS, or a desktop-only theme package. Platform-specific notification targets are documented separately below.

## Versioning

- Current source package baseline: `0.4.6-preview.1` (publication pending); the `0.4.5-preview.1` notes and validation evidence describe the previous baseline and are historical records.
- Preview packages may adjust API after a changelog entry and migration note.
- Stable releases follow Semantic Versioning.
- Avalonia minor-version upgrades are validated by Release builds for both supported .NET package TFMs, headless tests, package validation, and the platform matrix before the supported baseline changes.
- The public `Md*` control types, styled properties, enums, theme JSON fields, and resource-key names form the compatibility surface.

## Native notification package

`Md3.Avalonia.Notifications` is a standalone package targeting `net8.0` and `net10.0` for macOS/Linux; `net8.0-windows10.0.19041.0` and `net10.0-windows10.0.19041.0` for Windows; and, when the pack scripts pass `-p:IncludeAndroidTarget=true`, `net10.0-android` for Android. The Android target has a minimum API level of 23; `net8.0-android` is not offered because it is out of support. The package provides title-and-body notifications without referencing Avalonia. Windows calls WinRT toast APIs directly, macOS uses AppKit's legacy/deprecated `NSUserNotificationCenter`, Linux calls the native `libnotify` library, and Android uses `NotificationManager`; no external helper processes are launched. Linux still requires libnotify and a D-Bus notification service. See the [package README](../src/Md3.Avalonia.Notifications/README.md) for registration, native-library, and permission requirements. The implementation still requires manual display/permission verification on each operating system; no device or desktop sign-off is claimed.

## Optional desktop video backend

The core `MdVideoPlayer` is a renderer-neutral Avalonia control and introduces no native dependency. `Md3.Avalonia.LibVLC` separately targets `net8.0;net10.0`, depends on LibVLCSharp, composites decoded frames into an Avalonia bitmap, and requires the host to deploy native LibVLC for Windows, macOS, or Linux. It is deliberately marked non-trimmable/non-AOT-compatible and must not be referenced by the Android Gallery or core NativeAOT graph. See [VIDEO_PLAYER_RESEARCH.md](VIDEO_PLAYER_RESEARCH.md) and the [adapter README](../src/Md3.Avalonia.LibVLC/README.md).

## Android boundary

`gallery/Md3.Avalonia.Gallery.Android` is deliberately kept outside the desktop solution so developers without the Android workload can build the library and desktop Gallery. The Gallery host targets .NET 10 only; the independent NuGet libraries retain their .NET 8 assets. Validate the host with Java 17 and an installed .NET Android workload:

```bash
dotnet workload install android
dotnet build gallery/Md3.Avalonia.Gallery.Android -c Release
```

The host uses `ISingleViewApplicationLifetime` and `AndroidGalleryView`. Popup templates do not force `OverlayLayer`, preserving Avalonia's Android fallback behavior.

## External acceptance still required

The repository cannot substitute automation for ARM64 device behavior or assistive-technology review. Before a stable release, sign the checklist in `docs/RELEASE_VALIDATION.md` on physical or representative systems for Android touch/IME/rotation/lifecycle, Windows Narrator, macOS VoiceOver, and Linux Orca.
