---
_lang: zh-CN
---

# 兼容性与支持策略

[English](COMPATIBILITY.md) | [简体中文](COMPATIBILITY.zh-CN.md)

## 当前支持基线

| 范围 | 基线 | 状态 |
|---|---|---|
| .NET NuGet 资产 | 五个核心与通用包支持 `net8.0;net10.0`；12 个可选集成与主题包支持 `net10.0`。 | 通知包的平台专用资产见下文；桌面 Gallery 与测试以 .NET 10 为目标框架。 |
| Avalonia | `12.1.2` | Avalonia 控件包的构建与 headless 测试基线。最低版本由第三方集成包而非核心库决定。 |
| Windows | Avalonia 12 支持的 x64/ARM64 主机 | 库级兼容；设备与辅助技术验收仍需人工完成。 |
| macOS | Avalonia 12 支持的 x64/Apple Silicon 主机 | 库级兼容；VoiceOver 验收仍需人工完成。 |
| Linux | Avalonia 12 支持的 x64/ARM64 主机 | 已进行 Linux headless 测试；Orca 验收仍需人工完成。 |
| Android | 通知包由打包脚本显式加入 `net10.0-android`（最低 API 23）；Gallery 宿主使用 `net10.0-android`。 | 不提供已停止支持并会触发 `NETSDK1202` 的 `net8.0-android`；模拟器与真机验收仍需人工完成。 |
| iOS / 浏览器 | 不属于预览版目标平台 | 暂无支持承诺。 |

核心运行时包仅依赖 `Avalonia` 与 `MaterialColorUtilities`，不依赖 Avalonia Desktop、Win32、X11、macOS 或仅面向桌面的主题包。所有包 ID 与目标框架请见 [NuGet 包指南](NUGET_PACKAGES.zh-CN.md)。

## 版本策略

当前源码基线版本为 `0.5.6-preview.1`（尚未发布，共 18 个计划发布包）。更早的预览说明仅记录历史基线。
预览包可能调整 API；调整时应同时更新变更记录与迁移说明。
稳定版遵循 Semantic Versioning。
更新 Avalonia 次版本前，会对支持的 .NET 目标框架进行 Release 构建、headless 测试、包校验与平台矩阵验证。
公共 `Md*` 控件类型、styled property、枚举、主题 JSON 字段与资源键名称均属于兼容性接口。

## 原生通知包

`Md3.Avalonia.Notifications` 是独立包，不依赖 Avalonia。macOS/Linux 使用 `net8.0` 与 `net10.0`；Windows 使用 `net8.0-windows10.0.19041.0` 与 `net10.0-windows10.0.19041.0`；打包脚本传入 `-p:IncludeAndroidTarget=true` 时，为 Android 加入 `net10.0-android`。Android 最低 API 级别为 23；由于 `net8.0-android` 已停止支持，因此不提供该目标。Windows 使用 WinRT toast API，macOS 使用 AppKit 中已弃用的 `NSUserNotificationCenter`，Linux 调用原生 `libnotify`，Android 使用 `NotificationManager`；实现不会启动外部辅助进程。Linux 需要 `libnotify` 与 D-Bus 通知服务。注册与权限要求见[中文包 README](https://github.com/ilwren/Md3.Avalonia/blob/main/src/Md3.Avalonia.Notifications/README.zh-CN.md)。各操作系统上的显示与权限行为仍需人工验证。

## Android 范围

`gallery/Md3.Avalonia.Gallery.Android` 刻意置于桌面 solution 之外，使未安装 Android workload 的贡献者也能构建库与桌面 Gallery。验证该宿主需要 Java 17 和 .NET Android workload：

```bash
dotnet workload install android
dotnet build gallery/Md3.Avalonia.Gallery.Android -c Release
```

该宿主使用 `ISingleViewApplicationLifetime` 与 `AndroidGalleryView`。弹层模板不会强制使用 `OverlayLayer`，以保留 Avalonia 在 Android 上的回退行为。

## 仍需外部人工验收

自动化无法替代 ARM64 设备测试或辅助技术审查。稳定版发布前，应按 `docs/RELEASE_VALIDATION.md` 完成 Android 触控/输入法/旋转/生命周期、Windows Narrator、macOS VoiceOver 与 Linux Orca 的验收清单。
