# Compatibility and support policy / 兼容性与支持策略

## Supported baseline / 当前支持基线

| Area / 范围 | Baseline / 基线 | Status / 状态 |
|---|---|---|
| .NET NuGet assets / .NET NuGet 资产 | `net8.0;net10.0` for six core/general-purpose packages; `net10.0` for eleven opt-in third-party theme packages. / 六个核心与通用包支持 `net8.0;net10.0`；11 个可选第三方主题包支持 `net10.0`。 | Platform-specific notification assets are listed below. The desktop Gallery and tests target .NET 10. / 通知包的平台专用资产见下文；桌面 Gallery 与测试以 .NET 10 为目标框架。 |
| Avalonia | `12.1.2` | Build and headless-test baseline for Avalonia control packages. The floor is set by ecosystem packages, not by the core library. / Avalonia 控件包的构建与 headless 测试基线。最低版本由生态包而非核心库决定。 |
| Windows | Avalonia 12-supported x64/ARM64 hosts / Avalonia 12 支持的 x64/ARM64 主机 | Library-compatible; device and assistive-technology sign-off remains external. / 库级兼容；设备与辅助技术验收仍需人工完成。 |
| macOS | Avalonia 12-supported x64/Apple Silicon hosts / Avalonia 12 支持的 x64/Apple Silicon 主机 | Library-compatible; VoiceOver sign-off remains external. / 库级兼容；VoiceOver 验收仍需人工完成。 |
| Linux | Avalonia 12-supported x64/ARM64 hosts / Avalonia 12 支持的 x64/ARM64 主机 | Headless Linux tested; Orca sign-off remains external. / 已进行 Linux headless 测试；Orca 验收仍需人工完成。 |
| Android | Notifications package: `net10.0-android`, minimum API 23, when the pack script opts in; Gallery host: `net10.0-android`. / 通知包由打包脚本显式加入 `net10.0-android`（最低 API 23）；Gallery 宿主使用 `net10.0-android`。 | `net8.0-android` is not offered because it is out of support and raises `NETSDK1202`. Emulator/device sign-off remains external. / 不提供已停止支持并会触发 `NETSDK1202` 的 `net8.0-android`；模拟器与真机验收仍需人工完成。 |
| iOS / browser / iOS / 浏览器 | Not a preview-release target / 不属于预览版目标平台 | No support commitment. / 暂无支持承诺。 |

The core runtime package references only `Avalonia` and `MaterialColorUtilities`. It does not reference Avalonia Desktop, Win32, X11, macOS, or a desktop-only theme package. See the [NuGet package guide](NUGET_PACKAGES.md) for all package IDs and target frameworks.

核心运行时包仅依赖 `Avalonia` 与 `MaterialColorUtilities`，不依赖 Avalonia Desktop、Win32、X11、macOS 或仅面向桌面的主题包。所有包 ID 与目标框架请见 [NuGet 包指南](NUGET_PACKAGES.md)。

## Versioning / 版本策略

- Current source package baseline: `0.5.0-preview.1` (publication pending). Earlier preview notes describe historical package baselines. / 当前源码包基线为 `0.5.0-preview.1`（尚待发布）；更早的预览说明仅记录历史基线。
- Preview packages may adjust API after a changelog entry and migration note. / 预览包可能调整 API；调整时应同时更新变更记录与迁移说明。
- Stable releases follow Semantic Versioning. / 稳定版遵循 Semantic Versioning。
- Avalonia minor-version upgrades are validated by Release builds for the supported .NET package TFMs, headless tests, package validation, and the platform matrix before the baseline changes. / 更新 Avalonia 次版本前，会对支持的 .NET 目标框架进行 Release 构建、headless 测试、包校验与平台矩阵验证。
- Public `Md*` control types, styled properties, enums, theme JSON fields, and resource-key names form the compatibility surface. / 公共 `Md*` 控件类型、styled property、枚举、主题 JSON 字段与资源键名称均属于兼容性接口。

## Native notification package / 原生通知包

`Md3.Avalonia.Notifications` is standalone and does not reference Avalonia. It targets `net8.0` and `net10.0` for macOS/Linux; `net8.0-windows10.0.19041.0` and `net10.0-windows10.0.19041.0` for Windows; and, when the pack scripts pass `-p:IncludeAndroidTarget=true`, `net10.0-android` for Android. The Android target has a minimum API level of 23; `net8.0-android` is not offered because it is out of support. Windows calls WinRT toast APIs, macOS uses AppKit's legacy/deprecated `NSUserNotificationCenter`, Linux calls the native `libnotify` library, and Android uses `NotificationManager`; no external helper processes are launched. Linux requires `libnotify` and a D-Bus notification service. See the [package README](https://github.com/ilwren/Md3.Avalonia/blob/main/src/Md3.Avalonia.Notifications/README.md) for registration and permission requirements. Display and permission behavior still require manual checks on each operating system.

`Md3.Avalonia.Notifications` 是独立包，不依赖 Avalonia。macOS/Linux 使用 `net8.0` 与 `net10.0`；Windows 使用 `net8.0-windows10.0.19041.0` 与 `net10.0-windows10.0.19041.0`；打包脚本传入 `-p:IncludeAndroidTarget=true` 时，为 Android 加入 `net10.0-android`。Android 最低 API 级别为 23；由于 `net8.0-android` 已停止支持，因此不提供该目标。Windows 使用 WinRT toast API，macOS 使用 AppKit 中已弃用的 `NSUserNotificationCenter`，Linux 调用原生 `libnotify`，Android 使用 `NotificationManager`；实现不会启动外部辅助进程。Linux 需要 `libnotify` 与 D-Bus 通知服务。注册与权限要求见[包 README](../src/Md3.Avalonia.Notifications/README.md)。各操作系统上的显示与权限行为仍需人工验证。

## Android boundary / Android 范围

`gallery/Md3.Avalonia.Gallery.Android` is deliberately kept outside the desktop solution so contributors without the Android workload can build the library and desktop Gallery. Validate the host with Java 17 and an installed .NET Android workload:

`gallery/Md3.Avalonia.Gallery.Android` 刻意置于桌面 solution 之外，使未安装 Android workload 的贡献者也能构建库与桌面 Gallery。验证该宿主需要 Java 17 和 .NET Android workload：

```bash
dotnet workload install android
dotnet build gallery/Md3.Avalonia.Gallery.Android -c Release
```

The host uses `ISingleViewApplicationLifetime` and `AndroidGalleryView`. Popup templates do not force `OverlayLayer`, preserving Avalonia's Android fallback behavior.

该宿主使用 `ISingleViewApplicationLifetime` 与 `AndroidGalleryView`。弹层模板不会强制使用 `OverlayLayer`，以保留 Avalonia 在 Android 上的回退行为。

## External acceptance still required / 仍需外部人工验收

Automation cannot replace ARM64 device testing or assistive-technology review. Before a stable release, complete the checklist in `docs/RELEASE_VALIDATION.md` for Android touch/IME/rotation/lifecycle, Windows Narrator, macOS VoiceOver, and Linux Orca.

自动化无法替代 ARM64 设备测试或辅助技术审查。稳定版发布前，应按 `docs/RELEASE_VALIDATION.md` 完成 Android 触控/输入法/旋转/生命周期、Windows Narrator、macOS VoiceOver 与 Linux Orca 的验收清单。
