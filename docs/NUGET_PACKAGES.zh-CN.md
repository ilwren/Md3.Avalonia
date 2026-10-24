---
_lang: zh-CN
---

# NuGet 包

[English](NUGET_PACKAGES.md) | [简体中文](NUGET_PACKAGES.zh-CN.md)

Md3.Avalonia 拆分为 18 个 NuGet 包，应用可按需引用核心控件，并独立添加图标字体、扩展控件或第三方主题。每次发布的所有包使用相同版本号。

> **当前预览版本：** `0.5.0-preview.2`。本预览版的 18 个包均使用此版本号。可查看 [NuGet.org 上的 `Md3.Avalonia` 核心包](https://www.nuget.org/packages/Md3.Avalonia/0.5.0-preview.2)。

## 安装

先安装核心包。只有在应用需要随包提供的 Material Symbols 图标目录时，才额外安装一个图标包。

```bash
dotnet add package Md3.Avalonia --version 0.5.0-preview.2
dotnet add package Md3.Avalonia.Icons --version 0.5.0-preview.2
```

仅在应用实际使用对应功能时添加扩展控件或第三方主题包。所有 Md3.Avalonia 包应保持相同版本。如需使用仓库源码进行本地测试，可从仓库构建本地 NuGet 源：

```bash
# 在库仓库中构建全部包并输出到本地 NuGet 源。
bash scripts/build-nuget.sh --output "$HOME/.cache/md3-nuget"

# 在应用项目中从该源还原包。
dotnet add package Md3.Avalonia --source "$HOME/.cache/md3-nuget"
```

发布脚本会构建全部 18 个包，并校验 `.nupkg` 与 `.snupkg` 产物；需要 .NET 10 SDK、Java 17 和 .NET Android workload。详情见[仓库构建指南](https://github.com/ilwren/Md3.Avalonia/blob/arena/7fa0e0b4-md3-avalonia/README.zh-CN.md#构建与测试)。

## 包关系

核心包提供 Avalonia 控件基础，也是多数可选主题包的基础。图标包是可选项；Notifications 包独立于 Avalonia UI。

![NuGet 包关系图。](images/package-map.svg)

## 核心与通用扩展

| 包 | 目标框架 | 用途 |
|---|---|---|
| `Md3.Avalonia` | `net8.0`, `net10.0` | 核心控件、Material 主题、动态配色、动效与平台适配；注册 `MaterialTheme`。 |
| `Md3.Avalonia.Extra` | `net8.0`, `net10.0` | 可选控件与布局，如 `MdWaterfall`、`MdFloatingWindow`、`MdWaveform`；在 `MaterialTheme` 后注册 `ExtraTheme`。 |
| `Md3.Avalonia.Icons` | `net8.0`, `net10.0` | 完整 Material Symbols Rounded 字体与图标目录。 |
| `Md3.Avalonia.Icons.Lite` | `net8.0`, `net10.0` | 精简版 Material Symbols 字体与图标目录；不需要完整目录时替代 `Icons` 使用。 |
| `Md3.Avalonia.DataGrid` | `net8.0`, `net10.0` | Avalonia `DataGrid` 的 Material 主题；注册 `MaterialDataGridTheme`。 |
| `Md3.Avalonia.RichEditor` | `net8.0`, `net10.0` | 维护版 AvaloniaRichEditor 的 Material 主题；注册 `MaterialRichEditorTheme`。 |

## 第三方主题包

这些包在当前源码中以 `net10.0` 为目标框架。每个包均需显式添加；对应上游组件的依赖只由该主题适配包引入，核心包不会因此增加依赖。

| 包 | 主题入口 | 适配组件 |
|---|---|---|
| `Md3.Avalonia.AvaloniaEdit` | `MaterialAvaloniaEditTheme` | AvaloniaEdit |
| `Md3.Avalonia.AvaloniaHex` | `MaterialAvaloniaHexTheme` | AvaloniaHex |
| `Md3.Avalonia.Dock` | `MaterialDockTheme` | Dock |
| `Md3.Avalonia.Tabalonia` | `MaterialTabaloniaTheme` | Tabalonia |
| `Md3.Avalonia.Markdown` | `MaterialMarkdownTheme` | Markdown.Avalonia；将样式集合赋给 viewer。 |
| `Md3.Avalonia.LiveCharts2` | `LiveCharts.Configure(settings => settings.AddMaterialTheme())` | LiveCharts2 |
| `Md3.Avalonia.ScottPlot` | `MaterialScottPlotTheme.Follow(plot)` or `.Apply(...)` | ScottPlot |
| `Md3.Avalonia.PropertyGrid` | `MaterialPropertyGridTheme` | Avalonia.PropertyGrid |
| `Md3.Avalonia.NodifyM` | `MaterialNodifyMTheme` | NodifyM |
| `Md3.Avalonia.Mapsui` | `MaterialMapsuiTheme.Follow(mapView)` or `.Apply(...)` | Mapsui |
| `Md3.Avalonia.InkCanvas` | `MaterialInkCanvasTheme` and `MdSignaturePad` | DotNetCampus.AvaloniaInkCanvas |

多数 `*Theme` 类可在 `MaterialTheme` 后加入 `Application.Styles`。Markdown、LiveCharts2、ScottPlot、Mapsui 与 InkCanvas 使用对象级或组件专属入口；请按 API 参考和对应包 README 配置，不要一概当作全局样式注册。

## 平台服务

| 包 | 目标框架 | 用途 |
|---|---|---|
| `Md3.Avalonia.Notifications` | `net8.0`、`net10.0`、Windows 专用 .NET 8/10 目标；Android 目标 `net10.0-android` 由发布打包脚本加入。 | Windows、macOS、Linux 与 Android 原生系统通知；不依赖 Material UI 主题。 |

## 注册可选样式

只注册应用实际使用的样式包。以下 XML namespace 仅为示例；请先添加对应 NuGet 包。

```xml
<Application.Styles>
  <themes:MaterialTheme />
  <extra:ExtraTheme />
  <themes:MaterialDataGridTheme />
  <themes:MaterialRichEditorTheme />
  <themes:MaterialAvaloniaEditTheme />
</Application.Styles>
```

独立应用的最小接入流程请见[快速上手](GETTING_STARTED.zh-CN.md)；带截图和代码片段的页面示例请见[使用示例](USAGE_EXAMPLES.zh-CN.md)。
