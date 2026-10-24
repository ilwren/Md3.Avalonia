# Compatibility and support policy

[English](COMPATIBILITY.md) | [简体中文](COMPATIBILITY.zh-CN.md)

## Supported baseline

| Area | Baseline | Status |
|---|---|---|
| .NET NuGet assets | `net8.0;net10.0` for six core/general-purpose packages; `net10.0` for eleven opt-in third-party theme packages. | Platform-specific notification assets are listed below. The desktop Gallery and tests target .NET 10. |
| Avalonia | `12.1.2` | Build and headless-test baseline for Avalonia control packages. The minimum is set by third-party integration packages, not by the core library. |
| Windows | Avalonia 12-supported x64/ARM64 hosts | Library-compatible; device and assistive-technology sign-off remains external. |
| macOS | Avalonia 12-supported x64/Apple Silicon hosts | Library-compatible; VoiceOver sign-off remains external. |
| Linux | Avalonia 12-supported x64/ARM64 hosts | Headless Linux tested; Orca sign-off remains external. |
| Android | Notifications package: `net10.0-android`, minimum API 23, when the pack script opts in; Gallery host: `net10.0-android`. | `net8.0-android` is not offered because it is out of support and raises `NETSDK1202`. Emulator/device sign-off remains external. |
| iOS / browser | Not a preview-release target | No support commitment. |

The core runtime package references only `Avalonia` and `MaterialColorUtilities`. It does not reference Avalonia Desktop, Win32, X11, macOS, or a desktop-only theme package. See the [NuGet package guide](NUGET_PACKAGES.md) for all package IDs and target frameworks.

## Versioning

- Current package version: [`0.5.0-preview.2`](https://www.nuget.org/packages/Md3.Avalonia/0.5.0-preview.2) (18 packages). Earlier preview notes describe historical baselines.
- Preview packages may adjust API after a changelog entry and migration note.
- Stable releases follow Semantic Versioning.
- Avalonia minor-version upgrades are validated by Release builds for the supported .NET package TFMs, headless tests, package validation, and the platform matrix before the baseline changes.
- Public `Md*` control types, styled properties, enums, theme JSON fields, and resource-key names form the compatibility surface.

## Native notification package

`Md3.Avalonia.Notifications` is standalone and does not reference Avalonia. It targets `net8.0` and `net10.0` for macOS/Linux; `net8.0-windows10.0.19041.0` and `net10.0-windows10.0.19041.0` for Windows; and, when the pack scripts pass `-p:IncludeAndroidTarget=true`, `net10.0-android` for Android. The Android target has a minimum API level of 23; `net8.0-android` is not offered because it is out of support. Windows calls WinRT toast APIs, macOS uses AppKit's legacy/deprecated `NSUserNotificationCenter`, Linux calls the native `libnotify` library, and Android uses `NotificationManager`; no external helper processes are launched. Linux requires `libnotify` and a D-Bus notification service. See the [package README](https://github.com/ilwren/Md3.Avalonia/blob/main/src/Md3.Avalonia.Notifications/README.md) for registration and permission requirements. Display and permission behavior still require manual checks on each operating system.

## Android boundary

`gallery/Md3.Avalonia.Gallery.Android` is deliberately kept outside the desktop solution so contributors without the Android workload can build the library and desktop Gallery. Validate the host with Java 17 and an installed .NET Android workload:

```bash
dotnet workload install android
dotnet build gallery/Md3.Avalonia.Gallery.Android -c Release
```

The host uses `ISingleViewApplicationLifetime` and `AndroidGalleryView`. Popup templates do not force `OverlayLayer`, preserving Avalonia's Android fallback behavior.

## External acceptance still required

Automation cannot replace ARM64 device testing or assistive-technology review. Before a stable release, complete the checklist in `docs/RELEASE_VALIDATION.md` for Android touch/IME/rotation/lifecycle, Windows Narrator, macOS VoiceOver, and Linux Orca.
