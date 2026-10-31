---
_lang: zh-CN
---

# API 参考

[English](API.md) | [简体中文](API.zh-CN.md)

DocFX 文档站为 `src/` 下全部 18 个可打包项目生成 API 页面。可按包和命名空间浏览，或使用站内搜索查找公共类型与成员。生成内容以各项目编译后的公共 API 和 C# XML 文档注释为来源。

[打开在线 API 参考](https://ilwren.github.io/Md3.Avalonia/)

## NuGet 包

| 包 | 内容 |
|---|---|
| `Md3.Avalonia` | 核心控件、主题、动效与平台服务。 |
| `Md3.Avalonia.Extra` | 可选控件、布局与数据提供接口。 |
| `Md3.Avalonia.Icons` | 完整 Material Symbols Rounded 图标目录。 |
| `Md3.Avalonia.Icons.Lite` | 精简 Material Symbols 字体与图标目录。 |
| `Md3.Avalonia.DataGrid` | Avalonia `DataGrid` 的 Material 主题。 |
| `Md3.Avalonia.RichEditor` | AvaloniaRichEditor 的 Material 主题。 |
| `Md3.Avalonia.AvaloniaEdit` | AvaloniaEdit 的 Material 主题。 |
| `Md3.Avalonia.AvaloniaHex` | AvaloniaHex 的 Material 主题。 |
| `Md3.Avalonia.Dock` | Dock 的 Material 主题。 |
| `Md3.Avalonia.Tabalonia` | Tabalonia 的 Material 主题。 |
| `Md3.Avalonia.Markdown` | Markdown.Avalonia 的 Material 样式。 |
| `Md3.Avalonia.LiveCharts2` | LiveCharts2 的 Material 主题。 |
| `Md3.Avalonia.ScottPlot` | ScottPlot 的 Material 主题。 |
| `Md3.Avalonia.PropertyGrid` | Avalonia.PropertyGrid 的 Material 主题。 |
| `Md3.Avalonia.NodifyM` | NodifyM 的 Material 主题。 |
| `Md3.Avalonia.Mapsui` | Mapsui 的 Material 主题。 |
| `Md3.Avalonia.InkCanvas` | 手写画布主题与签名板。 |
| `Md3.Avalonia.Notifications` | 跨平台系统通知。 |

## 单一文档来源

公共 API 摘要、契约、参数说明与符号级示例统一维护在对应 C# 声明旁的 XML 注释中。DocFX 根据 18 个项目的公共 API 与 XML 文档文件生成参考页面。任务型说明和完整页面示例放在下方指南中；生成的 `MdSymbols` 与 `MdSymbolsLite` 图标目录已排除，以免数千个常量淹没 API 导航与搜索。

## 使用指南

- [快速上手](GETTING_STARTED.zh-CN.md) — 安装核心包、注册 `MaterialTheme` 并创建首个页面。
- [使用示例](USAGE_EXAMPLES.zh-CN.md) — 表单绑定、图标、动态配色、瀑布流与对话框示例，并配有图片。
- [NuGet 包](NUGET_PACKAGES.zh-CN.md) — 全部 18 个包、目标框架与主题入口。
- [项目结构](PROJECT_STRUCTURE.zh-CN.md) — 仓库目录与 API 文档生成流程。
- [兼容性](COMPATIBILITY.zh-CN.md) 与 [对话框服务指南](DIALOG_SERVICE.zh-CN.md)。

在仓库根目录运行 `python3 scripts/check-api-doc-coverage.py`，可检查 DocFX 项目集合、XML 输出、注释格式及公共接口的双语摘要。
