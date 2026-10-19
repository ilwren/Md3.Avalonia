# NuGet packages / NuGet 包

Md3.Avalonia is split into 18 packages so an app can take the core controls it needs and add optional fonts, widgets, or third-party themes independently. All packages from a release use the same version.

Md3.Avalonia 拆分为 18 个 NuGet 包，应用可按需引用核心控件，并独立添加图标字体、扩展控件或第三方主题。每次发布的所有包使用相同版本号。

> **Preview status / 预览版状态:** The repository currently tracks `0.5.0-preview.1`; the README marks it as not yet published. The commands below assume a matching version is available on your NuGet source. / 当前仓库源码版本为 `0.5.0-preview.1`，README 标注该版本尚未发布。以下命令假设当前 NuGet 源已提供对应版本。

## Install / 安装

Start with the core package. Add exactly one icon package only if the app uses the bundled Material Symbols catalog.

先安装核心包。只有在应用需要随包提供的 Material Symbols 图标目录时，才额外安装一个图标包。

```bash
dotnet add package Md3.Avalonia --version 0.5.0-preview.1
dotnet add package Md3.Avalonia.Icons --version 0.5.0-preview.1
```

Add optional controls or a vendor theme only when the app uses them. Keep every Md3.Avalonia package on the same version. If the preview is not available on NuGet yet, build a local feed from a repository checkout:

仅在应用实际使用对应功能时添加扩展控件或第三方主题包。所有 Md3.Avalonia 包应保持相同版本。如果预览包尚未上架，可从仓库构建本地 NuGet 源：

```bash
# In the library checkout: build all packages into a local feed.
bash scripts/build-nuget.sh --output "$HOME/.cache/md3-nuget"

# In your app project: restore from that feed.
dotnet add package Md3.Avalonia --source "$HOME/.cache/md3-nuget"
```

The release script builds all 18 packages and validates both `.nupkg` and `.snupkg` outputs. It requires the .NET 10 SDK, Java 17, and the .NET Android workload; see the [repository build guide](https://github.com/ilwren/Md3.Avalonia/blob/main/README.md).

发布脚本会构建全部 18 个包，并校验 `.nupkg` 与 `.snupkg` 产物；需要 .NET 10 SDK、Java 17 和 .NET Android workload。详情见[仓库构建指南](https://github.com/ilwren/Md3.Avalonia/blob/main/README.md)。

## Package map / 包关系

The core package is the base for Avalonia controls and most opt-in theme packages. Icons are an optional choice, and the Notifications package is independent of Avalonia UI.

核心包提供 Avalonia 控件基础，也是多数可选主题包的基础。图标包是可选项；Notifications 包独立于 Avalonia UI。

![NuGet package map. / NuGet 包关系图。](images/package-map.svg)

## Core and general-purpose add-ons / 核心与通用扩展

| Package / 包 | Target frameworks / 目标框架 | Use / 用途 |
|---|---|---|
| `Md3.Avalonia` | `net8.0`, `net10.0` | Core controls, Material theme, dynamic color, motion, and platform adapters. Register `MaterialTheme`. / 核心控件、Material 主题、动态配色、动效与平台适配；注册 `MaterialTheme`。 |
| `Md3.Avalonia.Extra` | `net8.0`, `net10.0` | Optional controls and layouts such as `MdWaterfall`, `MdFloatingWindow`, and `MdWaveform`. Register `ExtraTheme` after `MaterialTheme`. / 可选控件与布局，如 `MdWaterfall`、`MdFloatingWindow`、`MdWaveform`；在 `MaterialTheme` 后注册 `ExtraTheme`。 |
| `Md3.Avalonia.Icons` | `net8.0`, `net10.0` | Full Material Symbols Rounded font and catalog. / 完整 Material Symbols Rounded 字体与图标目录。 |
| `Md3.Avalonia.Icons.Lite` | `net8.0`, `net10.0` | Smaller Material Symbols font subset and catalog. Use instead of `Icons` when the full set is unnecessary. / 精简版 Material Symbols 字体与图标目录；不需要完整目录时替代 `Icons` 使用。 |
| `Md3.Avalonia.DataGrid` | `net8.0`, `net10.0` | Material theme for Avalonia `DataGrid`; register `MaterialDataGridTheme`. / Avalonia `DataGrid` 的 Material 主题；注册 `MaterialDataGridTheme`。 |
| `Md3.Avalonia.RichEditor` | `net8.0`, `net10.0` | Material theme for the maintained AvaloniaRichEditor fork; register `MaterialRichEditorTheme`. / 维护版 AvaloniaRichEditor 的 Material 主题；注册 `MaterialRichEditorTheme`。 |

## Third-party theme packages / 第三方主题包

These packages target `net10.0` in the current source. Each is opt-in and is the only Md3.Avalonia package that brings its corresponding upstream component dependency; the core package does not acquire those dependencies.

这些包在当前源码中以 `net10.0` 为目标框架。每个包均需显式添加；对应上游组件的依赖只由该主题适配包引入，核心包不会因此增加依赖。

| Package / 包 | Theme entry point / 主题入口 | Integrates with / 适配组件 |
|---|---|---|
| `Md3.Avalonia.AvaloniaEdit` | `MaterialAvaloniaEditTheme` | AvaloniaEdit |
| `Md3.Avalonia.AvaloniaHex` | `MaterialAvaloniaHexTheme` | AvaloniaHex |
| `Md3.Avalonia.Dock` | `MaterialDockTheme` | Dock |
| `Md3.Avalonia.Tabalonia` | `MaterialTabaloniaTheme` | Tabalonia |
| `Md3.Avalonia.Markdown` | `MaterialMarkdownTheme` | Markdown.Avalonia; assign the style collection to the viewer. / Markdown.Avalonia；将样式集合赋给 viewer。 |
| `Md3.Avalonia.LiveCharts2` | `LiveCharts.Configure(settings => settings.AddMaterialTheme())` | LiveCharts2 |
| `Md3.Avalonia.ScottPlot` | `MaterialScottPlotTheme.Follow(plot)` or `.Apply(...)` | ScottPlot |
| `Md3.Avalonia.PropertyGrid` | `MaterialPropertyGridTheme` | Avalonia.PropertyGrid |
| `Md3.Avalonia.NodifyM` | `MaterialNodifyMTheme` | NodifyM |
| `Md3.Avalonia.Mapsui` | `MaterialMapsuiTheme.Follow(mapView)` or `.Apply(...)` | Mapsui |
| `Md3.Avalonia.InkCanvas` | `MaterialInkCanvasTheme` and `MdSignaturePad` | DotNetCampus.AvaloniaInkCanvas |

Most `*Theme` classes are added to `Application.Styles` after `MaterialTheme`. Markdown, LiveCharts2, ScottPlot, Mapsui, and InkCanvas have object-level or library-specific entry points; follow the linked API reference and the package README rather than registering them as ordinary global styles.

多数 `*Theme` 类可在 `MaterialTheme` 后加入 `Application.Styles`。Markdown、LiveCharts2、ScottPlot、Mapsui 与 InkCanvas 使用对象级或组件专属入口；请按 API 参考和对应包 README 配置，不要一概当作全局样式注册。

## Platform service / 平台服务

| Package / 包 | Target frameworks / 目标框架 | Use / 用途 |
|---|---|---|
| `Md3.Avalonia.Notifications` | `net8.0`, `net10.0`, Windows-specific .NET 8/10 targets; `net10.0-android` is included by the release pack script. / `net8.0`、`net10.0`、Windows 专用 .NET 8/10 目标；Android 目标 `net10.0-android` 由发布打包脚本加入。 | Native system notifications on Windows, macOS, Linux, and Android. It does not require the Material UI theme. / Windows、macOS、Linux 与 Android 原生系统通知；不依赖 Material UI 主题。 |

## Register optional styles / 注册可选样式

Add only the style packages your app uses. The XML namespaces below are examples; add the corresponding package references first.

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

For the standalone control-first path, see the [quick start](GETTING_STARTED.md). For working screens with screenshots and source snippets, see [usage examples](USAGE_EXAMPLES.md).

独立应用的最小接入流程请见[快速上手](GETTING_STARTED.md)；带截图和代码片段的页面示例请见[使用示例](USAGE_EXAMPLES.md)。
