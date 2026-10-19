# Md3.Avalonia

A Material Design 3 control library and component gallery for Avalonia 12.

面向 Avalonia 12 的 Material Design 3 控件库与组件 Gallery。



https://github.com/user-attachments/assets/53c05bc6-f1b5-4076-8b62-c4a7a2b819bf



[![CI](https://github.com/ilwren/Md3.Avalonia/actions/workflows/ci.yml/badge.svg)](https://github.com/ilwren/Md3.Avalonia/actions/workflows/ci.yml)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg)](LICENSE)

- 353 public API types for buttons, inputs, navigation, dialogs, charts, and motion; indexed in the API reference and checked by a script gate.
  353 个公共 API 类型，覆盖按钮、输入、导航、对话框、图表与动效，全部收录在 API 参考中并由脚本门禁校验。
- HCT dynamic color: any seed color, 6 schemes, 3 contrast levels, and 49 color roles.
  HCT 动态配色：任意 seed color，6 种 scheme、3 档对比度、49 个色彩角色。
- Bundles the official Material Symbols Rounded font; checksum and glyph count are verified at build time.
  内嵌官方 Material Symbols Rounded 字体，构建期校验 checksum 与 glyph 数量。
- 18 independently installable NuGet packages; third-party integration themes are opt-in.
  18 个可独立安装的 NuGet 包，第三方集成主题包均按需引用。
- Each control has its own CLR type and scoped `ControlTheme`, without replacing native Avalonia controls.
  每个控件使用独立 CLR 类型与 scoped `ControlTheme`，不覆盖 Avalonia 原生控件。
- The core package does not depend on `Avalonia.Themes.Fluent` or `Avalonia.Themes.Simple`.
  核心包不依赖 `Avalonia.Themes.Fluent` 或 `Avalonia.Themes.Simple`。
- Desktop and Android targets; every package declares `IsTrimmable` and `IsAotCompatible`.
  桌面与 Android 双端，全部包声明 `IsTrimmable` 与 `IsAotCompatible`。

> **Current version: `0.5.0-preview.1`.** Preview APIs may still receive compatibility changes. NuGet install examples become usable after this version is published. See [Known limitations / 已知边界](#已知边界) for manual screen-reader and Android-device validation status.
>
> **当前版本 `0.5.0-preview.1`。** 预览版 API 仍可能做兼容性调整，NuGet 安装示例需在该版本发布后才可用。屏幕阅读器与 Android 真机等人工验收状态见 [已知边界](#已知边界)。

## Contents / 目录

- [Requirements / 环境要求](#环境要求)
- [Install / 安装](#安装)
- [Quick start / 快速开始](#快速开始)
- [NuGet packages / NuGet 包](#nuget-包)
- [Theming / 主题](#主题)
- [Controls overview / 控件总览](#控件总览)
- [Repository layout / 目录结构](#目录结构)
- [Build and test / 构建与测试](#构建与测试)
- [Documentation / 文档](#文档)
- [Known limitations / 已知边界](#已知边界)
- [License / 许可证](#许可证)

## 环境要求

| Item / 项 | Requirement / 要求 |
|---|---|
| Avalonia | `12.1.2` (minimum version is determined by the dependency floor of third-party integrations; see [`docs/COMPATIBILITY.md`](docs/COMPATIBILITY.md)) / `12.1.2`（最低版本由第三方集成包的依赖下限决定，见 [`docs/COMPATIBILITY.md`](docs/COMPATIBILITY.md)） |
| Library target frameworks / 库目标框架 | Core family: `net8.0;net10.0`; third-party theme packages: `net10.0` (see [NuGet packages / NuGet 包](#nuget-包)) / 核心族 `net8.0;net10.0`；第三方主题包 `net10.0`（见 [NuGet 包](#nuget-包)） |
| Gallery and tests / Gallery 与测试 | .NET 10 |
| Android | Minimum API 23; host target `net10.0-android` / 最低 API 23，宿主 `net10.0-android` |
| Packaging scripts / 打包脚本 | .NET 10 SDK, Java 17, and `dotnet workload install android` / .NET 10 SDK、Java 17、`dotnet workload install android` |

## 安装

Install the core package / 安装核心包：

```bash
dotnet add package Md3.Avalonia --version 0.5.0-preview.1
```

Add optional packages as needed; all packages use the same version / 按需添加可选包，全部包版本一致：

```bash
dotnet add package Md3.Avalonia.Icons --version 0.5.0-preview.1
```

## 快速开始

Register the theme / 注册主题：

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:themes="using:Md3.Avalonia.Themes"
             xmlns:extra="using:Md3.Avalonia.Extra.Themes">
  <Application.Styles>
    <themes:MaterialTheme />
    <!-- Add when using Md3.Avalonia.Extra / 引用 Md3.Avalonia.Extra 时加入： -->
    <extra:ExtraTheme />
  </Application.Styles>
</Application>
```

Use the controls / 使用控件：

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:md="using:Md3.Avalonia.Controls">
  <StackPanel Spacing="16" Margin="24">
    <md:MdButton Content="继续" Variant="Filled" Command="{Binding ContinueCommand}" />
    <md:MdTextBox Label="项目名称" Text="{Binding ProjectName}" MaxLength="40" />
    <md:MdComboBox Label="平台" Variant="Outlined"
                   ItemsSource="{Binding Platforms}" SelectedItem="{Binding Platform}" />
  </StackPanel>
</Window>
```

All bindable APIs are Avalonia properties and work directly with CommunityToolkit.MVVM; controls can also be constructed and used from code.

全部可绑定 API 都是 Avalonia property，可直接配合 CommunityToolkit.MVVM 使用；控件也可以在代码中直接构造和操作。

| Material 3 actions | Material 3 components |
|---|---|
| ![Action-button preview](docs/MdActionButtonsPreview.png) | ![Component preview](docs/MdNewComponentsPreview.png) |

## NuGet 包

All 18 packages share one version and can be installed independently; unused packages are not pulled in.

十八个包版本统一，均可独立安装；不使用就不会被引入。

| Package / 包 | Capability / 能力 | Theme registration / 主题注册 | Target frameworks / 目标框架 |
|---|---|---|---|
| `Md3.Avalonia` | Core Material 3 controls and design tokens<br>核心 Material 3 控件与设计令牌 | `MaterialTheme` | `net8.0;net10.0` |
| `Md3.Avalonia.Icons` | Complete Material Symbols Rounded font and icon catalog<br>完整 Material Symbols Rounded 字体与图标目录 | Auto-discovered / 自动发现 | `net8.0;net10.0` |
| `Md3.Avalonia.Icons.Lite` | Reduced icon subset; choose this or the full Icons package<br>精简图标子集，与完整 Icons 二选一 | Auto-discovered / 自动发现 | `net8.0;net10.0` |
| `Md3.Avalonia.Extra` | Flutter-inspired extension controls<br>Flutter-inspired 扩展控件 | `ExtraTheme` | `net8.0;net10.0` |
| `Md3.Avalonia.DataGrid` | Theme for Avalonia's native `DataGrid`<br>Avalonia 原生 `DataGrid` 主题 | `MaterialDataGridTheme` | `net8.0;net10.0` |
| `Md3.Avalonia.Notifications` | Native system notifications for Windows, macOS, Linux, and Android<br>Windows / macOS / Linux / Android 系统通知 | None (independent of UI themes) / 无（独立于 UI 主题） | `net8.0;net10.0` + Windows-specific / Windows 专用 |
| `Md3.Avalonia.AvaloniaEdit` | AvaloniaEdit editor theme<br>AvaloniaEdit 编辑器主题 | `MaterialAvaloniaEditTheme` | `net10.0` |
| `Md3.Avalonia.AvaloniaHex` | AvaloniaHex hex-editor theme<br>AvaloniaHex 十六进制编辑器主题 | `MaterialAvaloniaHexTheme` | `net10.0` |
| `Md3.Avalonia.Dock` | Dock layout theme<br>Dock 停靠布局主题 | `MaterialDockTheme` | `net10.0` |
| `Md3.Avalonia.Tabalonia` | Draggable Tabalonia tab theme<br>Tabalonia 可拖拽标签页主题 | `MaterialTabaloniaTheme` | `net10.0` |
| `Md3.Avalonia.Markdown` | Markdown.Avalonia document styling<br>Markdown.Avalonia 文档样式 | Assign to `MarkdownScrollViewer.MarkdownStyle` / 赋给 `MarkdownScrollViewer.MarkdownStyle` | `net10.0` |
| `Md3.Avalonia.LiveCharts2` | LiveCharts2 chart theme<br>LiveCharts2 图表主题 | Configure once with `LiveCharts.Configure` / `LiveCharts.Configure` 配置一次 | `net10.0` |
| `Md3.Avalonia.ScottPlot` | ScottPlot chart theme<br>ScottPlot 图表主题 | Plot-level `MaterialScottPlotTheme` / 绘图对象级 `MaterialScottPlotTheme` | `net10.0` |
| `Md3.Avalonia.PropertyGrid` | Avalonia.PropertyGrid property-grid theme<br>Avalonia.PropertyGrid 属性网格主题 | `MaterialPropertyGridTheme` | `net10.0` |
| `Md3.Avalonia.NodifyM` | NodifyM node-editor theme<br>NodifyM 节点编辑器主题 | `MaterialNodifyMTheme` | `net10.0` |
| `Md3.Avalonia.Mapsui` | Mapsui map theme<br>Mapsui 地图主题 | Map-level `MaterialMapsuiTheme` / 地图级 `MaterialMapsuiTheme` | `net10.0` |
| `Md3.Avalonia.InkCanvas` | Handwriting-canvas theme and `MdSignaturePad`<br>手写画布主题与 `MdSignaturePad` | Control-level `MaterialInkCanvasTheme` / 控件级 `MaterialInkCanvasTheme` | `net10.0` |
| `Md3.Avalonia.RichEditor` | Theme for the maintained AvaloniaRichEditor fork<br>AvaloniaRichEditor 维护版主题 | `MaterialRichEditorTheme` | `net10.0` |

Each third-party theme package is the **only** package that brings in its upstream dependency, keeping those dependencies out of the core package. The `Markdown`, `LiveCharts2`, `ScottPlot`, `Mapsui`, and `InkCanvas` packages do not register in `Application.Styles`: Markdown styles are assigned to the document control, while the others expose object-level entry points.

每个第三方主题包是**唯一**引入对应上游依赖的包，因此核心包不会被这些依赖污染。`Markdown`、`LiveCharts2`、`ScottPlot`、`Mapsui`、`InkCanvas` 五个包不注册到 `Application.Styles`：前者的样式赋给文档控件，其余是对象级入口。

Design principles / 设计原则：

- Each interactive control has its own `Md*` CLR type and scoped `ControlTheme`, while reusing Avalonia's native behavior, binding, keyboard, and selection models where possible.
  每个交互控件使用独立的 `Md*` CLR 类型与 scoped `ControlTheme`，尽量复用 Avalonia 原生行为、绑定、键盘与选择模型。
- Consume color, typography, shape, state layers, elevation, and motion through tokens and `DynamicResource`.
  颜色、字体、形状、状态层、阴影与 motion 一律通过 token 与 `DynamicResource` 消费。
- If a font or icon provider is missing or fails validation, keep the icon hidden instead of substituting a similar-looking glyph.
  字体或图标 provider 缺失、校验失败时图标保持隐藏，不使用形似的替代字形。

## 主题

Light, Dark, and System modes all consume theme tokens through `DynamicResource`. Apply any seed color at startup or at runtime:

Light、Dark、System 三种模式，全部通过 `DynamicResource` 令牌消费。任意 seed 配色可在启动或运行时应用：

```csharp
var options = new MdThemeOptions
{
    SeedColor = "#006A6A",
    SchemeVariant = MdThemeSchemeVariant.Expressive,
    ContrastLevel = MdThemeContrastLevel.High,
    ThemeMode = MdThemeMode.System
};
var dark = Application.Current!.ActualThemeVariant == ThemeVariant.Dark;
MdThemeManager.Apply(Application.Current, options, dark);
var json = MdThemeJson.Serialize(options);
```

The `MaterialColorUtilities` HCT generator supports six schemes (TonalSpot, Neutral, Vibrant, Expressive, Monochrome, Fidelity), three contrast levels (Standard, Medium, High), and 49 color roles. It also provides System and Component token layers, contrast diagnostics, and theme JSON round-tripping.

`MaterialColorUtilities` HCT 生成器支持 TonalSpot / Neutral / Vibrant / Expressive / Monochrome / Fidelity 六种 scheme、Standard / Medium / High 三档对比度、49 个色彩角色，并提供 System 与 Component 两层令牌、对比度诊断和主题 JSON round-trip。

### Embedded official Material Symbols font / 内嵌官方 Material Symbols 字体

`Md3.Avalonia.Icons` ships the complete Google **Material Symbols Rounded** variable TTF. `Md3.Avalonia.Icons.Lite` ships a genuine outline subset generated from the same file; consumers do not need to download or register a font manually.

`Md3.Avalonia.Icons` 直接分发完整的 Google **Material Symbols Rounded** variable TTF，`Md3.Avalonia.Icons.Lite` 分发由同一文件生成的真实轮廓子集，使用者无需手动下载或注册字体。

The source is pinned to commit `737e3324305806514d7909874fa1818ae1808232` of `google/material-design-icons`. The complete TTF has SHA-256 `95b24392bb49efd1bc3e92cff4e2452ad094461bab7c97e7d8723fab97e330ca`. Both local builds and CI run:

来源固定为 `google/material-design-icons` commit `737e3324305806514d7909874fa1818ae1808232`，完整 TTF 的 SHA-256 为 `95b24392bb49efd1bc3e92cff4e2452ad094461bab7c97e7d8723fab97e330ca`。构建与 CI 均执行：

```bash
python3 scripts/verify-fonts.py
```

The verifier checks checksums, variable-font tables, and glyph counts for the full font and Lite subset. It rejects missing, placeholder, or substituted binaries. After referencing Core and Icons, there is no need to call `ConfigureFonts` manually: the provider is discovered and registered automatically, and symbol glyphs remain hidden if validation fails.

校验器核对完整字体与 Lite 子集的 checksum、variable-font tables 与 glyph 数量，拒绝缺失、占位矩形或被替换的二进制。同时引用 Core 与 Icons 后不需要手动调用 `ConfigureFonts`：provider 会被自动发现并注册，校验失败时 symbol glyph 保持隐藏。

## 控件总览

The online [DocFX API reference](https://ilwren.github.io/Md3.Avalonia/) is generated from the C# XML comments in all 18 packages. [`docs/API.md`](docs/API.md) describes package coverage and documentation sources. `scripts/check-api-doc-coverage.py` scans all 353 public types; the table lists representative types in each group.

在线 [DocFX API 参考](https://ilwren.github.io/Md3.Avalonia/)从 18 个包的 C# XML 注释生成页面；[`docs/API.md`](docs/API.md)说明覆盖的包与文档来源。353 个公共类型由 `scripts/check-api-doc-coverage.py` 扫描，下表只列出各分组的主要类型。

| Group / 分组 | Representative types / 主要类型 |
|---|---|
| Buttons / 按钮 | `MdButton`、`MdToggleButton`、`MdIconButton`、`MdToggleIconButton`、`MdSplitButton`、`MdStandardButtonGroup`、`MdConnectedButtonGroup`、`MdFloatingActionButton`、`MdExtendedFloatingActionButton`、`MdFabMenu` |
| Inputs / 输入 | `MdTextBox`、`MdCheckBox`、`MdRadioButton`、`MdComboBox`、`MdAutoCompleteBox`、`MdSlider`、`MdSwitch`、`MdDatePicker`、`MdTimePicker`、`MdPinInput`、`MdTagInput`、`MdForm` |
| Navigation / 导航 | `MdNavigationDrawer`、`MdNavigationBar`、`MdTabs`、`MdToolbar`、`MdTopAppBar`、`MdBottomAppBar`、`MdSearchBar`、`MdSearchView`、`MdCommandPalette`、`MdBreadcrumb` |
| Containers and overlays / 容器与浮层 | `MdCard`、`MdDialog`、`MdDialogHost`、`MdMenu`、`MdPopover`、`MdTooltip`、`MdSnackbar`、`MdBanner`、`MdSheetHost`、`MdExpansionPanelList`、`MdFloatingWindow`、`MdBorderlessWindow` |
| Data display / 数据展示 | `MdList`、`MdDataTable`、`MdDataGrid`、`MdCarousel`、`MdCalendar`、`MdTreeView`、`MdAvatar`、`MdBadge`、`MdChip`、`MdTimeline`、`MdWaveform`、`MdChart` |
| Feedback and progress / 反馈与进度 | `MdCircularProgressIndicator`、`MdLinearProgressIndicator`、`MdLoadingIndicator`、`MdSkeleton`、`MdRefreshIndicator`、`MdSpinKit`、`MdStateLayer`、`MdFocusRing` |
| Layout and motion / 布局与动效 | `MdSurface`、`MdAdaptiveLayout`、`MdScrollViewer`、`MdWaterfall`、`MdMasonryPanel`、`MdStaggeredPanel`、`MdAnimationSequence`、`MdAnimatedText`、`MdRevealHost`、`MdMorphPanel`、`MdHero` |

All Button, Icon Button, and FAB templates use `MdRipplePresenter`: ripples originate at the press point, clip to the full container shape, and support Expressive, Standard, Reduced, and None motion schemes. The Elevated `MdButton` variant is retained for compatibility but is deprecated in M3; prefer another variant for new interfaces.

全部按钮、Icon Button 与 FAB 模板都接入 `MdRipplePresenter`：按下位置产生涟漪，裁切到完整 container shape，并支持 Expressive / Standard / Reduced / None 四种 motion scheme。`MdButton` 的 Elevated 配置为 M3 已标记 deprecated 的兼容项，新界面应优先选用其他配置。

`Md3.Avalonia.Extra` is an optional clean-room extension package and does not claim to reproduce the full API of corresponding Flutter packages. For precise parity details, see [`docs/FLUTTER_PARITY_STATUS.md`](docs/FLUTTER_PARITY_STATUS.md); similar `Md*` type names alone do not imply full compatibility.

`Md3.Avalonia.Extra` 是可选的 clean-room 扩展包，不声称复刻对应 Flutter package 的全部 API；需要严格 parity 时查阅 [`docs/FLUTTER_PARITY_STATUS.md`](docs/FLUTTER_PARITY_STATUS.md)，不要仅凭相似的 `Md*` 类型名推断完全兼容。

## 目录结构

```text
src/Md3.Avalonia/                  # Core controls and design tokens; no specific icon-font dependency / 核心控件与设计令牌，不依赖具体图标字体
├─ Controls/                       # Md* controls, enums, and property APIs / Md* 控件、枚举与属性 API
├─ Motion/                         # Motion schemes, spring tokens, and transitions / motion scheme、spring 令牌与转场
└─ Themes/
   ├─ MaterialTheme.axaml
   ├─ Tokens/                      # 11 token dictionaries / 11 个令牌字典
   └─ Controls/                    # Scoped ControlTheme for each control / 每类控件的 scoped ControlTheme
src/Md3.Avalonia.Icons/            # Icon provider with the complete official TTF embedded / 图标 provider，内嵌完整官方 TTF
src/Md3.Avalonia.Extra/            # Flutter-inspired extension controls / Flutter-inspired 扩展控件
src/Md3.Avalonia.<Vendor>/         # 12 opt-in third-party theme packages, each the sole upstream dependency entry point / 12 个 opt-in 第三方主题包，各自唯一引入上游依赖
src/Md3.Avalonia.Notifications/    # Native system notifications, independent of Avalonia UI / 原生系统通知，独立于 Avalonia UI

gallery/Md3.Avalonia.Gallery/          # Desktop Gallery: 108 component pages + Theme Lab / 桌面 Gallery：108 个组件页 + Theme Lab
gallery/Md3.Avalonia.Gallery.Android/  # net10.0-android single-view host / net10.0-android single-view 宿主
tests/Md3.Avalonia.HeadlessTests/      # API, input, theme, regression, and rendering tests / API、输入、主题、回归与渲染测试
tests/.../Spec/                        # Layered spec checks (L1 tokens / L2 geometry / L3 golden images / L5 motion) / 规范一致性分层校验（L1 令牌 / L2 几何 / L3 金图 / L5 动效）
docs/                                  # API, compatibility, publishing, and validation docs / API、兼容性、发布与验证文档
spec-snapshot/                         # External-source expected-value oracle and ratchet policy / 外部来源的期望值 oracle 与棘轮策略
```

See [`docs/SPEC_VERIFICATION.md`](docs/SPEC_VERIFICATION.md) for the responsibilities of each spec-verification layer, the ratchet process, and known gaps.

规范一致性校验的分层职责、棘轮流程与已知差距见 [`docs/SPEC_VERIFICATION.md`](docs/SPEC_VERIFICATION.md)。

## 构建与测试

```bash
python3 scripts/verify-fonts.py     # Verify embedded fonts / 校验内嵌字体
bash scripts/build-nuget.sh         # Build 18 .nupkg / .snupkg files / 构建十八个 .nupkg / .snupkg

dotnet build gallery/Md3.Avalonia.Gallery.Desktop/Md3.Avalonia.Gallery.Desktop.csproj -c Release
dotnet test tests/Md3.Avalonia.HeadlessTests/Md3.Avalonia.HeadlessTests.csproj -c Release
```

Install the Android workload before building the full solution:

构建完整 solution 需要先安装 Android workload：

```bash
dotnet workload install android
dotnet build Md3.Avalonia.sln -c Release
```

The following static checks do not require the .NET SDK:

以下静态检查不需要 .NET SDK：

```bash
python3 scripts/lint-design-tokens.py       # Lint design tokens / 设计令牌 lint
python3 scripts/check-api-doc-coverage.py   # Check DocFX projects, XML output, and bilingual interface docs / DocFX 项目、XML 输出及双语接口文档
python3 scripts/check-gallery-localization.py --strict
```

Publish the desktop Gallery with native AOT (Linux requires clang/zlib; `--smoke` also requires xvfb):

原生 AOT 发布桌面 Gallery（Linux 需要 clang/zlib，`--smoke` 另需 xvfb）：

```bash
bash scripts/publish-gallery-aot.sh --rid linux-x64 --smoke   # or win-x64 / osx-x64 / osx-arm64 / 或 win-x64 / osx-x64 / osx-arm64
```

The script verifies that the output is a native executable with no managed DLL beside it. `--smoke` runs the binary under xvfb to check that trimming has not removed runtime reflection metadata. `.github/workflows/aot-probe.yml` runs the same script on the main and arena branches.

脚本会断言产物是原生可执行文件且旁边没有托管 dll，`--smoke` 在 xvfb 下实际启动该二进制，确认裁剪未丢失运行时所需的反射元数据。`.github/workflows/aot-probe.yml` 在 main 与 arena 分支上运行同一脚本。

The release gate builds all 18 packages and verifies 18 `.nupkg` files, 18 `.snupkg` files, a unified version, and the embedded official font. Releases use NuGet Trusted Publishing; see [`docs/PUBLISHING.md`](docs/PUBLISHING.md) for setup.

发布门禁会构建十八个包，并严格检查十八个 `.nupkg`、十八个 `.snupkg`、统一版本与已内嵌的官方字体。发布使用 NuGet Trusted Publishing，配置步骤见 [`docs/PUBLISHING.md`](docs/PUBLISHING.md)。

Clean build artifacts immediately after every build, test, or pack operation. Do not ship `bin/`, `obj/`, `TestResults/`, DLLs, PDBs, NuGet packages, APKs, or AABs in the repository; `docs/*.png` are retained as documentation reference images.

每次 build、test 或 pack 结束后必须立即清理编译产物。仓库不交付 `bin/`、`obj/`、`TestResults/`、DLL、PDB、NuGet 包、APK 或 AAB；`docs/*.png` 是保留的文档参考图。

## 文档

| Document / 文档 | Description / 内容 |
|---|---|
| [`CHANGELOG.md`](CHANGELOG.md) | Release history / 版本变更记录 |
| [Online DocFX API reference](https://ilwren.github.io/Md3.Avalonia/) · [`docs/API.md`](docs/API.md) | Public API pages generated from XML comments and package entry points / 由 XML 注释生成的公共 API 页面与包入口 |
| [`docs/AI_DISCLOSURE.md`](docs/AI_DISCLOSURE.md) | AI-generated and AI-assisted content disclosure and disclaimer / AI 生成与辅助内容说明及免责声明 |
| [`docs/GETTING_STARTED.md`](docs/GETTING_STARTED.md) | Quick start, theme registration, and first page / 快速上手、主题注册与首个页面 |
| [`docs/USAGE_EXAMPLES.md`](docs/USAGE_EXAMPLES.md) | Form, icon, dynamic-color, waterfall, and dialog examples with images / 表单、图标、动态配色、瀑布流与对话框示例（含配图） |
| [`docs/NUGET_PACKAGES.md`](docs/NUGET_PACKAGES.md) | 18 NuGet packages, target frameworks, and theme entry points / 18 个 NuGet 包、目标框架与主题入口 |
| [`docs/PROJECT_STRUCTURE.md`](docs/PROJECT_STRUCTURE.md) | Repository layout, project responsibilities, and API-doc build flow / 仓库目录、项目职责与 API 文档构建流程 |
| [`docs/COMPATIBILITY.md`](docs/COMPATIBILITY.md) | Target frameworks, platforms, and dependency compatibility policy / 目标框架、平台与依赖兼容策略 |
| [`docs/RELEASE_NOTES_0.5.0-preview.1.md`](docs/RELEASE_NOTES_0.5.0-preview.1.md) | Changes in this release / 本版变更说明 |
| [`docs/RELEASE_VALIDATION.md`](docs/RELEASE_VALIDATION.md) | Manual validation checklist and sign-off status / 人工验收清单与签署状态 |
| [`docs/PUBLISHING.md`](docs/PUBLISHING.md) | Release process and NuGet Trusted Publishing setup / 发布流程与 NuGet Trusted Publishing 配置 |
| [`docs/SPEC_VERIFICATION.md`](docs/SPEC_VERIFICATION.md) | Layered spec verification and known gaps / 规范一致性分层校验与已知差距 |
| [`docs/FLUTTER_PARITY_STATUS.md`](docs/FLUTTER_PARITY_STATUS.md) | Flutter parity status for corresponding Extra controls / Extra 包与 Flutter 对应控件的 parity 状态 |
| [`src/Md3.Avalonia.Notifications/README.md`](src/Md3.Avalonia.Notifications/README.md) | System-notification usage and platform prerequisites / 系统通知的用法与平台前置条件 |

## 已知边界

- **Manual validation is not signed off / 人工验收未签署。** Narrator, VoiceOver, Orca, and TalkBack must be tested by people with the corresponding OS and assistive technology, following the release checklist. A successful Android APK build is not device validation; ARM64, rotation, lifecycle, and TalkBack still require on-device sign-off. Results are tracked in [`docs/RELEASE_VALIDATION.md`](docs/RELEASE_VALIDATION.md).
  Narrator、VoiceOver、Orca 与 TalkBack 必须由具备对应 OS 与辅助技术的人员按发布清单验收；Android APK 构建成功不等于真机验收，ARM64、旋转、生命周期与 TalkBack 仍需在设备上签署，结果记录于 [`docs/RELEASE_VALIDATION.md`](docs/RELEASE_VALIDATION.md)。
- **Motion / 动效。** Ripple, spring tokens, and inherited motion schemes are in place. The four M3 transitions—shared-axis, fade-through, container transform, and animated visibility—are implemented (`MdRevealHost` yields layout space and clips; `MdMorphPanel` interpolates endpoint sizes), but `Control` content does not cross-fade on exit.
  Ripple、spring 令牌与继承式 motion scheme 已建立，shared-axis、fade-through、container transform 与 animated visibility 四种 M3 转场已实化（`MdRevealHost` 让出布局空间并裁剪，`MdMorphPanel` 插值两端尺寸），但对 `Control` 类型的内容不做退场叠化。
- **AOT.** All 18 packages declare `IsTrimmable` and `IsAotCompatible` and enable the corresponding analyzers; theme JSON uses source-generated serialization. The Linux x64 native Gallery has been published and smoke-run under xvfb. The AOT workflow reports diagnostics, but zero diagnostics do not mean AOT validation has passed for Windows, macOS, Android, or a consuming application.
  十八个包均声明 `IsTrimmable` + `IsAotCompatible` 并启用相应分析器，主题 JSON 使用源生成序列化；Linux x64 原生 Gallery 已发布并在 xvfb 下 smoke-run。AOT workflow 会报告诊断，但诊断数为零不代表 Windows、macOS、Android 或使用方应用的 AOT 验收已通过。
- **Android back gestures and safe areas / Android 返回手势与安全区。** `MdBackNavigation` covers dialogs, sheets, drawers, search, menus, FAB menus, and both pickers. `MdSafeArea` provides per-edge insets. Headless behavior has regression tests; on-device gesture animation and cutout geometry still need manual validation.
  `MdBackNavigation` 覆盖 dialog、sheet、drawer、search、menu、FAB menu 与两个 picker，`MdSafeArea` 提供按边内缩；headless 行为有回归测试，真机手势动画与挖孔几何仍需人工验收。
- **RTL and localization / RTL 与多语种。** Related adaptation code remains in place, but full RTL/multilingual layout and manual screen-reader validation are not currently preview release blockers.
  相关适配代码持续保留，但完整 RTL/多语种布局与屏幕阅读器人工验收当前不作为预览版阻塞项。
- **Font provenance / 字体来源。** The complete Material Symbols Rounded variable TTF and genuine Lite subset are checked in and packaged automatically. CI pins the upstream commit and verifies SHA-256, variable tables, and glyph counts to prevent placeholder or substituted fonts from entering a release.
  完整 Material Symbols Rounded variable TTF 与真实 Lite 子集已提交并自动打包；CI 以固定 upstream commit、SHA-256、variable tables 与 glyph 数量阻止占位或替代字体混入发布。

See [`spec-snapshot/manifest.json`](spec-snapshot/manifest.json) for the official pages, pinned AndroidX commit `e2171b56112f43bf962f5d608413f74033b580b6`, and token version.

官网页面、冻结的 AndroidX commit `e2171b56112f43bf962f5d608413f74033b580b6` 与 token 版本见 [`spec-snapshot/manifest.json`](spec-snapshot/manifest.json)。

## 许可证

Apache-2.0. See [`LICENSE`](LICENSE) and [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md) for the license terms of third-party components. The embedded Material Symbols font is also Apache-2.0; its license is distributed with both Icons packages.

Apache-2.0，见 [`LICENSE`](LICENSE)。第三方组件的许可声明见 [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md)；内嵌的 Material Symbols 字体同为 Apache-2.0，许可证随两个 Icons 包一起分发。
