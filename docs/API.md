# API Reference / API 参考

The DocFX site generates API pages from all 18 packable projects under `src/`. Browse by package and namespace, or use site search to find public types and members. Each project's compiled public API and C# XML documentation comments are the source for the generated reference.

DocFX 文档站为 `src/` 下全部 18 个可打包项目生成 API 页面。可按包和命名空间浏览，或使用站内搜索查找公共类型与成员。生成内容以各项目编译后的公共 API 和 C# XML 文档注释为来源。

[Open the published API reference / 打开在线 API 参考](https://ilwren.github.io/Md3.Avalonia/)

## NuGet packages / NuGet 包

| Package / 包 | API area / 内容 |
|---|---|
| `Md3.Avalonia` | Core controls, themes, motion, and platform services. / 核心控件、主题、动效与平台服务。 |
| `Md3.Avalonia.Extra` | Optional controls, layout, and data-provider contracts. / 可选控件、布局与数据提供接口。 |
| `Md3.Avalonia.Icons` | Full Material Symbols Rounded catalog. / 完整 Material Symbols Rounded 图标目录。 |
| `Md3.Avalonia.Icons.Lite` | Reduced Material Symbols font and catalog. / 精简 Material Symbols 字体与图标目录。 |
| `Md3.Avalonia.DataGrid` | Material theme for Avalonia `DataGrid`. / Avalonia `DataGrid` 的 Material 主题。 |
| `Md3.Avalonia.RichEditor` | Material theme for AvaloniaRichEditor. / AvaloniaRichEditor 的 Material 主题。 |
| `Md3.Avalonia.AvaloniaEdit` | Material theme for AvaloniaEdit. / AvaloniaEdit 的 Material 主题。 |
| `Md3.Avalonia.AvaloniaHex` | Material theme for AvaloniaHex. / AvaloniaHex 的 Material 主题。 |
| `Md3.Avalonia.Dock` | Material theme for Dock. / Dock 的 Material 主题。 |
| `Md3.Avalonia.Tabalonia` | Material theme for Tabalonia. / Tabalonia 的 Material 主题。 |
| `Md3.Avalonia.Markdown` | Material styles for Markdown.Avalonia. / Markdown.Avalonia 的 Material 样式。 |
| `Md3.Avalonia.LiveCharts2` | Material theme for LiveCharts2. / LiveCharts2 的 Material 主题。 |
| `Md3.Avalonia.ScottPlot` | Material theme for ScottPlot. / ScottPlot 的 Material 主题。 |
| `Md3.Avalonia.PropertyGrid` | Material theme for Avalonia.PropertyGrid. / Avalonia.PropertyGrid 的 Material 主题。 |
| `Md3.Avalonia.NodifyM` | Material theme for NodifyM. / NodifyM 的 Material 主题。 |
| `Md3.Avalonia.Mapsui` | Material theme for Mapsui. / Mapsui 的 Material 主题。 |
| `Md3.Avalonia.InkCanvas` | Handwriting canvas theme and signature pad. / 手写画布主题与签名板。 |
| `Md3.Avalonia.Notifications` | Cross-platform system notifications. / 跨平台系统通知。 |

## Source of truth / 单一文档来源

Public API summaries, contracts, parameter descriptions, and examples are maintained in the C# XML comments beside their declarations. The projects emit XML documentation files, and DocFX uses those files to build the reference. The generated `MdSymbols` and `MdSymbolsLite` glyph catalogs are intentionally excluded so thousands of constants do not overwhelm API navigation and search.

公共 API 摘要、契约、参数说明与示例统一维护在对应 C# 声明旁的 XML 注释中。各项目生成 XML 文档文件，DocFX 以这些文件构建 API 参考。生成的 `MdSymbols` 与 `MdSymbolsLite` 图标目录已排除，以免数千个常量淹没 API 导航与搜索。

For task-oriented usage, see the [dialog service guide](DIALOG_SERVICE.md). Run `python3 scripts/check-api-doc-coverage.py` from the repository root to verify the DocFX project set, XML output, XML comment validity, and bilingual public-interface summaries.

任务型用法请参阅[对话框服务指南](DIALOG_SERVICE.md)。在仓库根目录运行 `python3 scripts/check-api-doc-coverage.py`，可检查 DocFX 项目集合、XML 输出、注释格式及公共接口的双语摘要。
