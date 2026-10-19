# Project structure / 项目结构

This page explains where the library, optional packages, examples, tests, and generated documentation live. Consumer apps normally reference NuGet packages; the repository layout is useful when browsing source or contributing.

本页介绍控件库、可选包、示例、测试与生成文档所在的位置。应用通常通过 NuGet 引用；了解仓库结构有助于阅读源码或参与开发。

![Diagram of the repository's NuGet package groups. / 仓库 NuGet 包分组示意图。](images/package-map.svg)

## Repository map / 仓库目录

```text
Md3.Avalonia/
├─ src/
│  ├─ Md3.Avalonia/                  # Core controls, tokens, motion, localization, themes
│  ├─ Md3.Avalonia.Extra/            # Optional extra controls, layouts, and templates
│  ├─ Md3.Avalonia.Icons/            # Full Material Symbols font and glyph catalog
│  ├─ Md3.Avalonia.Icons.Lite/       # Reduced Material Symbols font and catalog
│  ├─ Md3.Avalonia.DataGrid/         # Avalonia DataGrid theme
│  ├─ Md3.Avalonia.RichEditor/       # AvaloniaRichEditor theme
│  ├─ Md3.Avalonia.AvaloniaEdit/     # AvaloniaEdit theme
│  ├─ Md3.Avalonia.AvaloniaHex/      # AvaloniaHex theme
│  ├─ Md3.Avalonia.Dock/             # Dock theme
│  ├─ Md3.Avalonia.Tabalonia/        # Tabalonia theme
│  ├─ Md3.Avalonia.Markdown/         # Markdown.Avalonia styles
│  ├─ Md3.Avalonia.LiveCharts2/      # LiveCharts2 theme/configuration
│  ├─ Md3.Avalonia.ScottPlot/        # ScottPlot theme
│  ├─ Md3.Avalonia.PropertyGrid/     # Avalonia.PropertyGrid theme
│  ├─ Md3.Avalonia.NodifyM/          # NodifyM theme
│  ├─ Md3.Avalonia.Mapsui/           # Mapsui theme
│  ├─ Md3.Avalonia.InkCanvas/        # InkCanvas theme and MdSignaturePad
│  └─ Md3.Avalonia.Notifications/    # Native notifications (standalone)
├─ gallery/
│  ├─ Md3.Avalonia.Gallery/          # Desktop component gallery and examples
│  ├─ Md3.Avalonia.Gallery.Desktop/  # Desktop application host
│  └─ Md3.Avalonia.Gallery.Android/  # Android single-view host
├─ tests/Md3.Avalonia.HeadlessTests/ # Headless API, behavior, theme, and rendering tests
├─ docs/                             # DocFX guides and selected reference images
├─ templates/material/               # Local Material 3 overrides for the DocFX Modern theme
├─ .docfx/                           # DocFX filters and metadata configuration
├─ scripts/                          # Build, packaging, validation, and coverage scripts
└─ spec-snapshot/                    # Pinned conformance baselines and verification policy
```

## What belongs where / 各目录职责

| Location / 目录 | Responsibility / 职责 |
|---|---|
| `src/Md3.Avalonia/Controls` | Public controls and contracts. / 公共控件与契约。 |
| `src/Md3.Avalonia/Themes` | `MaterialTheme`, component styles, color/shape/type tokens, and dynamic-theme code. / `MaterialTheme`、控件样式、颜色/形状/字体令牌与动态主题代码。 |
| `src/Md3.Avalonia/Motion` | Motion schemes, transitions, and animation helpers. / 动效方案、转场与动画辅助工具。 |
| `src/Md3.Avalonia.<Package>/` | Independent opt-in package projects; vendor packages keep ecosystem dependencies out of the core package. / 独立的可选包项目；第三方主题包将生态依赖隔离在核心包之外。 |
| `gallery/` | Runnable desktop and Android demonstrations; many code samples in this guide are taken from its pages. / 可运行的桌面与 Android 示例；本指南中的部分代码示例取自 Gallery 页面。 |
| `tests/` | Headless regression tests for API behavior, rendering, themes, and interactions. / API 行为、渲染、主题与交互的 headless 回归测试。 |
| `docs/` and `index.md` | Bilingual DocFX guides, navigation, and preview assets. / DocFX 双语指南、导航与预览图片。 |
| `templates/material/` | Project-local CSS customization over DocFX Modern; no third-party Material theme is layered on top. / 基于 DocFX Modern 的本地 CSS 定制，不叠加第三方 Material 主题。 |
| `scripts/` and `.github/workflows/` | Reproducible package builds, static checks, CI, and Pages deployment. / 可复现的包构建、静态检查、CI 与 Pages 部署。 |

## How API documentation is generated / API 文档如何生成

Each packable project emits an XML documentation file. `docfx.json` lists the 18 package projects, DocFX reads their compiled public API and XML comments, then builds the searchable API reference. This keeps member descriptions next to their C# declarations; this site adds task-oriented guides without copying the full API catalog into Markdown.

每个可打包项目都会生成 XML 文档文件。`docfx.json` 列出 18 个包项目，DocFX 从编译后的公共 API 与 XML 注释生成可搜索的 API 参考。成员说明与 C# 声明放在一起维护；文档站另外提供任务型指南，不在 Markdown 中重复维护整份 API 清单。

The local DocFX Modern customization is in `templates/material/public/main.css`. The documentation workflow builds the same configuration used for the [published preview](https://ilwren.github.io/Md3.Avalonia/).

DocFX Modern 的本地样式定制位于 `templates/material/public/main.css`。文档 workflow 使用与[在线预览站](https://ilwren.github.io/Md3.Avalonia/)相同的配置进行构建。

## Useful commands / 常用命令

```bash
# Validate package and bilingual API-documentation coverage
python3 scripts/check-api-doc-coverage.py

# Build the desktop gallery
dotnet build gallery/Md3.Avalonia.Gallery.Desktop/Md3.Avalonia.Gallery.Desktop.csproj -c Release

# Run headless tests
dotnet test tests/Md3.Avalonia.HeadlessTests/Md3.Avalonia.HeadlessTests.csproj -c Release
```

The complete solution and release packaging may require the Android workload; see the repository [README](https://github.com/ilwren/Md3.Avalonia/blob/main/README.md) and [compatibility guide](COMPATIBILITY.md).

构建完整 solution 与发布包时可能还需安装 Android workload；详见仓库 [README](../README.md#构建与测试) 和[兼容性指南](COMPATIBILITY.md)。
