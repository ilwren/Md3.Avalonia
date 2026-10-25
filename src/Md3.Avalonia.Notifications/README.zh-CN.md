# Md3.Avalonia11.Notifications

[English](README.md) | [简体中文](README.zh-CN.md)

一个轻量、独立的 .NET 库，可在 Windows、macOS、Linux 和 Android 上显示原生系统通知；不依赖 Avalonia。

## 安装

当前预览版本为 [`0.5.0-preview.2`](https://www.nuget.org/packages/Md3.Avalonia11.Notifications/0.5.0-preview.2)。

```bash
dotnet add package Md3.Avalonia11.Notifications --version 0.5.0-preview.2
```

## 显示通知

```csharp
using Md3.Avalonia.Notifications;

var notifications = MdSystemNotificationService.CreateDefault();
await notifications.ShowAsync("Build complete", "The package is ready to download.");
```

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

当前 `ShowAsync` 仅支持标题和正文，不提供通知点击回调、操作按钮、替换或关闭能力。操作系统和用户设置可能会抑制通知或改变其呈现方式。

## 平台要求

| 平台 | 实现与设置 |
|---|---|
| Windows 10+ | 通过 C# 直接调用 WinRT toast API。将宿主项目目标框架设为 `net8.0-windows10.0.19041.0` 或 `net10.0-windows10.0.19041.0` 以选择原生 Windows 资产。传统桌面应用必须将 `WindowsAppUserModelId` 设为开始菜单快捷方式上的 ID；仅设置该值并不会注册应用。此库不会创建快捷方式。Gallery 的 Windows 宿主演示了通过 Windows Shell COM API 为当前用户注册（不启动外部辅助进程）。 |
| macOS | 通过 C# 互操作使用 AppKit 旧版/已弃用的 `NSUserNotificationCenter` API。通知投递和应用归属仍受 macOS 通知设置及应用身份影响；请在受支持的 macOS 版本上使用打包后的宿主应用验证行为。 |
| Linux | 直接调用原生 `libnotify` 库，不会启动 `notify-send`。系统必须提供 `libnotify` 共享库以及兼容 freedesktop 的 D-Bus 通知服务（例如 Debian/Ubuntu 使用 `sudo apt install libnotify4`，Fedora 使用 `sudo dnf install libnotify`）。 |
| Android 6.0+ (API 23) | 调用 `NotificationManager`，在 Android 8.0+ 创建配置的通知渠道；若提供 `AndroidSmallIconResourceId` 则使用它，否则依次回退到宿主应用图标和通用信息图标。在应用清单中添加 `android.permission.POST_NOTIFICATIONS`，并在 Android 13+ 调用 `ShowAsync` 前从前台 `Activity` 请求权限。缺少运行时授权时会抛出 `MdSystemNotificationPermissionException`；此库刻意不显示权限提示框。 |

目标框架为普通 `net8.0` 或 `net10.0` 的 Windows 应用会选择通用桌面资产，而非 WinRT 后端；`CreateDefault()` 会指出需要 Windows 专用目标框架。将 Windows 宿主改为 `net8.0-windows10.0.19041.0` 或 `net10.0-windows10.0.19041.0`（或新建 Windows 专用宿主项目）以选择原生资产。Gallery Desktop 在 Windows 上构建时会选择此 Windows 目标框架；显示窗口前，它会为当前用户注册带匹配 AppUserModelID 的开始菜单快捷方式。

此包为 macOS/Linux 提供 `net8.0` 和 `net10.0` 资产，为 Windows 后端提供 `net8.0-windows10.0.19041.0` 和 `net10.0-windows10.0.19041.0` 资产。Android 资产为可选项：打包脚本传入 `-p:IncludeAndroidTarget=true` 后会加入 `net10.0-android`（最低 API 23）。仅提供 `net10.0-android`，因为 `net8.0-android` 已停止支持，SDK 会对此目标框架报 NETSDK1202。Gallery 宿主仍仅支持 .NET 10。构建或打包 Android 资产需要兼容的 .NET Android workload 和 JDK（Java 17）：

```bash
dotnet workload install android
```

取消令牌会在提交给原生 API 前检查。操作系统接受通知后，其生命周期由对应平台管理。
