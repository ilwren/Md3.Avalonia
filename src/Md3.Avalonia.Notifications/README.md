# Md3.Avalonia11.Notifications

[English](README.md) | [简体中文](README.zh-CN.md)

A small, standalone .NET library for showing native system notifications on Windows, macOS, Linux, and Android. It has no Avalonia dependency.

## Install

The current preview is [`0.5.0-preview.2`](https://www.nuget.org/packages/Md3.Avalonia11.Notifications/0.5.0-preview.2).

```bash
dotnet add package Md3.Avalonia11.Notifications --version 0.5.0-preview.2
```

## Show a notification

```csharp
using Md3.Avalonia.Notifications;

var notifications = MdSystemNotificationService.CreateDefault();
await notifications.ShowAsync("Build complete", "The package is ready to download.");
```

Or pass a notification record and platform options explicitly:

```csharp
var notifications = MdSystemNotificationService.CreateDefault(new MdSystemNotificationOptions
{
    ApplicationName = "Contoso Builder",
    WindowsAppUserModelId = "Contoso.Builder",
    AndroidChannelId = "build-status",
    AndroidChannelName = "Build status",
});

await notifications.ShowAsync(new MdSystemNotification("Build complete", "The package is ready."));
```

`ShowAsync` currently covers title and body only. It does not expose notification click callbacks, actions, replacement, or dismissal. Operating systems and user settings may suppress or change the presentation of a notification.

## Platform requirements

| Platform | Implementation and setup |
|---|---|
| Windows 10+ | Calls the WinRT toast API directly from C#. Target the host project at `net8.0-windows10.0.19041.0` or `net10.0-windows10.0.19041.0` to select the native Windows asset. Classic desktop apps must set `WindowsAppUserModelId` to the ID on a Start-menu shortcut; setting the value alone does not register the app. The library does not create the shortcut. The Gallery's Windows host demonstrates current-user registration through the Windows Shell COM API (no external helper process). |
| macOS | Uses AppKit's legacy/deprecated `NSUserNotificationCenter` API through C# interop. Delivery and attribution remain subject to macOS notification settings and app identity; verify the behavior with the packaged host app on supported macOS versions. |
| Linux | Calls the native `libnotify` library directly; it does not launch `notify-send`. The `libnotify` shared library and a freedesktop-compatible D-Bus notification service must be available (for example, `sudo apt install libnotify4` on Debian/Ubuntu or `sudo dnf install libnotify` on Fedora). |
| Android 6.0+ (API 23) | Calls `NotificationManager`, creates the configured notification channel on Android 8.0+, and uses `AndroidSmallIconResourceId` when supplied, falling back to the host app icon and then a generic information icon. Add `android.permission.POST_NOTIFICATIONS` to the app manifest and request it from a foreground `Activity` on Android 13+ before calling `ShowAsync`. A missing runtime grant raises `MdSystemNotificationPermissionException`; this library deliberately does not show a permission prompt. |

A Windows app built against plain `net8.0` or `net10.0` selects the generic desktop asset, not the WinRT backend; `CreateDefault()` reports that the Windows-specific target is required. Retarget the Windows host to `net8.0-windows10.0.19041.0` or `net10.0-windows10.0.19041.0` (or create a Windows-specific host project) to select the native asset. The Gallery Desktop selects this Windows target when built on Windows; it registers a current-user Start-menu shortcut with the matching AppUserModelID before showing its window.

The package provides `net8.0` and `net10.0` assets for macOS/Linux and `net8.0-windows10.0.19041.0` and `net10.0-windows10.0.19041.0` for the Windows backend. The Android asset is opt-in: the pack scripts pass `-p:IncludeAndroidTarget=true`, which adds `net10.0-android` (minimum API 23). Only `net10.0-android` is offered, because `net8.0-android` is out of support and the SDK raises NETSDK1202 for it. The Gallery host remains .NET 10-only. Building or packing Android assets requires a compatible .NET Android workload and JDK (Java 17):

```bash
dotnet workload install android
```

The cancellation token is checked before submission to the native API. Once the operating system accepts a notification, its lifetime is owned by that platform.
