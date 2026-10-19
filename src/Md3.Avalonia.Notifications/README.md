# Md3.Avalonia.Notifications

A small, standalone .NET library for showing native system notifications on Windows, macOS, Linux, and Android. It has no Avalonia dependency.

一个轻量、独立的 .NET 库，可在 Windows、macOS、Linux 和 Android 上显示原生系统通知；不依赖 Avalonia。

## Install / 安装

The project source baseline is `0.5.0-preview.1`; the package is not installable from NuGet until that preview is published.

项目源代码基线为 `0.5.0-preview.1`；该预览版发布前无法从 NuGet 安装此包。

```bash
dotnet add package Md3.Avalonia.Notifications --version 0.5.0-preview.1
```

## Show a notification / 显示通知

```csharp
using Md3.Avalonia.Notifications;

var notifications = MdSystemNotificationService.CreateDefault();
await notifications.ShowAsync("Build complete", "The package is ready to download.");
```

Or pass a notification record and platform options explicitly:

也可以显式传入通知记录和平台选项：

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

当前 `ShowAsync` 仅支持标题和正文，不提供通知点击回调、操作按钮、替换或关闭能力。操作系统和用户设置可能会抑制通知或改变其呈现方式。

## Platform requirements / 平台要求

| Platform / 平台 | Implementation and setup / 实现与设置 |
|---|---|
| Windows 10+ | Calls the WinRT toast API directly from C#. Target the host project at `net8.0-windows10.0.19041.0` or `net10.0-windows10.0.19041.0` to select the native Windows asset. Classic desktop apps must set `WindowsAppUserModelId` to the ID on a Start-menu shortcut; setting the value alone does not register the app. The library does not create the shortcut. The Gallery's Windows host demonstrates current-user registration through the Windows Shell COM API (no external helper process).<br><br>通过 C# 直接调用 WinRT toast API。将宿主项目目标框架设为 `net8.0-windows10.0.19041.0` 或 `net10.0-windows10.0.19041.0` 以选择原生 Windows 资产。传统桌面应用必须将 `WindowsAppUserModelId` 设为开始菜单快捷方式上的 ID；仅设置该值并不会注册应用。此库不会创建快捷方式。Gallery 的 Windows 宿主演示了通过 Windows Shell COM API 为当前用户注册（不启动外部辅助进程）。 |
| macOS | Uses AppKit's legacy/deprecated `NSUserNotificationCenter` API through C# interop. Delivery and attribution remain subject to macOS notification settings and app identity; verify the behavior with the packaged host app on supported macOS versions.<br><br>通过 C# 互操作使用 AppKit 旧版/已弃用的 `NSUserNotificationCenter` API。通知投递和应用归属仍受 macOS 通知设置及应用身份影响；请在受支持的 macOS 版本上使用打包后的宿主应用验证行为。 |
| Linux | Calls the native `libnotify` library directly; it does not launch `notify-send`. The `libnotify` shared library and a freedesktop-compatible D-Bus notification service must be available (for example, `sudo apt install libnotify4` on Debian/Ubuntu or `sudo dnf install libnotify` on Fedora).<br><br>直接调用原生 `libnotify` 库，不会启动 `notify-send`。系统必须提供 `libnotify` 共享库以及兼容 freedesktop 的 D-Bus 通知服务（例如 Debian/Ubuntu 使用 `sudo apt install libnotify4`，Fedora 使用 `sudo dnf install libnotify`）。 |
| Android 6.0+ (API 23) | Calls `NotificationManager`, creates the configured notification channel on Android 8.0+, and uses `AndroidSmallIconResourceId` when supplied, falling back to the host app icon and then a generic information icon. Add `android.permission.POST_NOTIFICATIONS` to the app manifest and request it from a foreground `Activity` on Android 13+ before calling `ShowAsync`. A missing runtime grant raises `MdSystemNotificationPermissionException`; this library deliberately does not show a permission prompt.<br><br>调用 `NotificationManager`，在 Android 8.0+ 创建配置的通知渠道；若提供 `AndroidSmallIconResourceId` 则使用它，否则依次回退到宿主应用图标和通用信息图标。在应用清单中添加 `android.permission.POST_NOTIFICATIONS`，并在 Android 13+ 调用 `ShowAsync` 前从前台 `Activity` 请求权限。缺少运行时授权时会抛出 `MdSystemNotificationPermissionException`；此库刻意不显示权限提示框。 |

A Windows app built against plain `net8.0` or `net10.0` selects the generic desktop asset, not the WinRT backend; `CreateDefault()` reports that the Windows-specific target is required. Retarget the Windows host to `net8.0-windows10.0.19041.0` or `net10.0-windows10.0.19041.0` (or create a Windows-specific host project) to select the native asset. The Gallery Desktop selects this Windows target when built on Windows; it registers a current-user Start-menu shortcut with the matching AppUserModelID before showing its window.

目标框架为普通 `net8.0` 或 `net10.0` 的 Windows 应用会选择通用桌面资产，而非 WinRT 后端；`CreateDefault()` 会指出需要 Windows 专用目标框架。将 Windows 宿主改为 `net8.0-windows10.0.19041.0` 或 `net10.0-windows10.0.19041.0`（或新建 Windows 专用宿主项目）以选择原生资产。Gallery Desktop 在 Windows 上构建时会选择此 Windows 目标框架；显示窗口前，它会为当前用户注册带匹配 AppUserModelID 的开始菜单快捷方式。

The package provides `net8.0` and `net10.0` assets for macOS/Linux and `net8.0-windows10.0.19041.0` and `net10.0-windows10.0.19041.0` for the Windows backend. The Android asset is opt-in: the pack scripts pass `-p:IncludeAndroidTarget=true`, which adds `net10.0-android` (minimum API 23). Only `net10.0-android` is offered, because `net8.0-android` is out of support and the SDK raises NETSDK1202 for it. The Gallery host remains .NET 10-only. Building or packing Android assets requires a compatible .NET Android workload and JDK (Java 17):

此包为 macOS/Linux 提供 `net8.0` 和 `net10.0` 资产，为 Windows 后端提供 `net8.0-windows10.0.19041.0` 和 `net10.0-windows10.0.19041.0` 资产。Android 资产为可选项：打包脚本传入 `-p:IncludeAndroidTarget=true` 后会加入 `net10.0-android`（最低 API 23）。仅提供 `net10.0-android`，因为 `net8.0-android` 已停止支持，SDK 会对此目标框架报 NETSDK1202。Gallery 宿主仍仅支持 .NET 10。构建或打包 Android 资产需要兼容的 .NET Android workload 和 JDK（Java 17）：

```bash
dotnet workload install android
```

The cancellation token is checked before submission to the native API. Once the operating system accepts a notification, its lifetime is owned by that platform.

取消令牌会在提交给原生 API 前检查。操作系统接受通知后，其生命周期由对应平台管理。
