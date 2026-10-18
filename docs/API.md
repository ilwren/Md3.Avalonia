# API Reference / API 参考

The generated API reference covers the six packable projects under `src/`. Browse the published
[DocFX site](https://ilwren.github.io/Md3.Avalonia/) by package and namespace, or use its search to
find public types and members. The reference pages are generated from each project's compiled API
and C# XML documentation comments.

生成的 API 参考覆盖 `src/` 下六个可打包项目。请访问[已发布的 DocFX 文档站点](https://ilwren.github.io/Md3.Avalonia/)，按包和命名空间浏览，或使用站内搜索查找公共类型与成员。API 页面由各项目编译后的 API 与 C# XML 文档注释生成。

## NuGet packages / NuGet 包

| Package / 包 | Scope / 内容 |
|---|---|
| `Md3.Avalonia` | Core Material 3 controls, themes, motion, and platform services. / Material 3 核心控件、主题、动效与平台服务。 |
| `Md3.Avalonia.Extra` | Optional ecosystem controls and data-provider contracts. / 可选生态控件与数据提供接口。 |
| `Md3.Avalonia.Icons` | Full Material Symbols Rounded catalog. / 完整 Material Symbols Rounded 图标目录。 |
| `Md3.Avalonia.Icons.Lite` | Reduced Material Symbols font and catalog. / 精简 Material Symbols 字体与图标目录。 |
| `Md3.Avalonia.DataGrid` | Material theme for Avalonia `DataGrid`. / Avalonia `DataGrid` 的 Material 主题。 |
| `Md3.Avalonia.RichEditor` | Material theme for AvaloniaRichEditor. / AvaloniaRichEditor 的 Material 主题。 |

## Source of truth / 单一文档来源

Public API summaries, contracts, and examples are maintained in the C# XML comments next to their
declarations. The projects emit XML documentation files, and DocFX uses those files to build the
reference. The `MdSymbols` and `MdSymbolsLite` glyph catalogs are intentionally excluded from
DocFX API pages so thousands of constants do not overwhelm the API navigation and search.

公共 API 摘要、契约与示例统一维护在对应 C# 声明旁的 XML 注释中。各项目生成 XML 文档文件，DocFX 以此构建 API 参考。`MdSymbols` 与 `MdSymbolsLite` 图标目录已从 DocFX API 页面中排除，以免数千个常量淹没 API 导航与搜索。

For task-oriented guidance, see the [dialog service guide](DIALOG_SERVICE.md). Run
`python3 scripts/check-api-doc-coverage.py` from the repository root to verify that all six projects
are configured and every public interface has bilingual XML documentation.

任务型用法请参阅[对话框服务指南](DIALOG_SERVICE.md)。在仓库根目录运行
`python3 scripts/check-api-doc-coverage.py`，可检查六个项目的 DocFX 配置以及所有公共接口的双语 XML 文档。
