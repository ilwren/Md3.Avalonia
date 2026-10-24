# Md3.Avalonia

[English](README.md) | [简体中文](README.zh-CN.md)

面向 Avalonia 12 的 Material Design 3 控件库与组件 Gallery。

- 353 个公共 API 类型，覆盖按钮、输入、导航、对话框、图表与动效，全部收录在 API 参考中并由脚本门禁校验。

- HCT 动态配色：任意 seed color，6 种 scheme、3 档对比度、49 个色彩角色。

- 内嵌官方 Material Symbols Rounded 字体，构建期校验 checksum 与 glyph 数量。

- 18 个可独立安装的 NuGet 包，第三方集成主题包均按需引用。

- 每个控件使用独立 CLR 类型与 scoped `ControlTheme`，不覆盖 Avalonia 原生控件。

- 核心包不依赖 `Avalonia.Themes.Fluent` 或 `Avalonia.Themes.Simple`。

- 桌面与 Android 双端，全部包声明 `IsTrimmable` 与 `IsAotCompatible`。

> **当前预览版：[`0.5.0-preview.2`](https://www.nuget.org/packages/Md3.Avalonia/0.5.0-preview.2)。** 以下安装示例均使用此版本。预览期 API 仍可能做兼容性调整。屏幕阅读器与 Android 真机等人工验收状态见 [已知边界](#已知边界)。

## 目录

- [环境要求](#环境要求)
- [安装](#安装)
- [快速开始](#快速开始)
- [NuGet 包](#nuget-包)
- [主题](#主题)
- [控件总览](#控件总览)
- [目录结构](#目录结构)
- [构建与测试](#构建与测试)
- [文档](#文档)
- [已知边界](#已知边界)
- [许可证](#许可证)

## 环境要求

| 项 | 要求 |
|---|---|
| Avalonia | `12.1.2`（最低版本由第三方集成包的依赖下限决定，见 [`docs/COMPATIBILITY.zh-CN.md`](docs/COMPATIBILITY.zh-CN.md)） |
| 库目标框架 | 核心族 `net8.0;net10.0`；第三方主题包 `net10.0`（见 [NuGet 包](#nuget-包)） |
| Gallery 与测试 | .NET 10 |
| Android | 最低 API 23，宿主 `net10.0-android` |
| 打包脚本 | .NET 10 SDK、Java 17、`dotnet workload install android` |

## 安装

安装核心包：

```bash
dotnet add package Md3.Avalonia --version 0.5.0-preview.2
```

按需添加可选包，全部包版本一致：

```bash
dotnet add package Md3.Avalonia.Icons --version 0.5.0-preview.2
```

## 快速开始

注册主题：

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:themes="using:Md3.Avalonia.Themes"
             xmlns:extra="using:Md3.Avalonia.Extra.Themes">
  <Application.Styles>
    <themes:MaterialTheme />
    <!-- 引用 Md3.Avalonia.Extra 时加入： -->
    <extra:ExtraTheme />
  </Application.Styles>
</Application>
```

使用控件：

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

全部可绑定 API 都是 Avalonia property，可直接配合 CommunityToolkit.MVVM 使用；控件也可以在代码中直接构造和操作。

| Material 3 操作 | Material 3 组件 |
|---|---|
| ![Action-button preview](docs/MdActionButtonsPreview.png) | ![Component preview](docs/MdNewComponentsPreview.png) |

## NuGet 包

十八个包版本统一，均可独立安装；不使用就不会被引入。

| 包 | 能力 | 主题注册 | 目标框架 |
|---|---|---|---|
| `Md3.Avalonia` | 核心 Material 3 控件与设计令牌 | `MaterialTheme` | `net8.0;net10.0` |
| `Md3.Avalonia.Icons` | 完整 Material Symbols Rounded 字体与图标目录 | 自动发现 | `net8.0;net10.0` |
| `Md3.Avalonia.Icons.Lite` | 精简图标子集，与完整 Icons 二选一 | 自动发现 | `net8.0;net10.0` |
| `Md3.Avalonia.Extra` | Flutter-inspired 扩展控件 | `ExtraTheme` | `net8.0;net10.0` |
| `Md3.Avalonia.DataGrid` | Avalonia 原生 `DataGrid` 主题 | `MaterialDataGridTheme` | `net8.0;net10.0` |
| `Md3.Avalonia.Notifications` | Windows / macOS / Linux / Android 系统通知 | 无（独立于 UI 主题） | `net8.0;net10.0` + Windows 专用 |
| `Md3.Avalonia.AvaloniaEdit` | AvaloniaEdit 编辑器主题 | `MaterialAvaloniaEditTheme` | `net10.0` |
| `Md3.Avalonia.AvaloniaHex` | AvaloniaHex 十六进制编辑器主题 | `MaterialAvaloniaHexTheme` | `net10.0` |
| `Md3.Avalonia.Dock` | Dock 停靠布局主题 | `MaterialDockTheme` | `net10.0` |
| `Md3.Avalonia.Tabalonia` | Tabalonia 可拖拽标签页主题 | `MaterialTabaloniaTheme` | `net10.0` |
| `Md3.Avalonia.Markdown` | Markdown.Avalonia 文档样式 | 赋给 `MarkdownScrollViewer.MarkdownStyle` | `net10.0` |
| `Md3.Avalonia.LiveCharts2` | LiveCharts2 图表主题 | `LiveCharts.Configure` 配置一次 | `net10.0` |
| `Md3.Avalonia.ScottPlot` | ScottPlot 图表主题 | 绘图对象级 `MaterialScottPlotTheme` | `net10.0` |
| `Md3.Avalonia.PropertyGrid` | Avalonia.PropertyGrid 属性网格主题 | `MaterialPropertyGridTheme` | `net10.0` |
| `Md3.Avalonia.NodifyM` | NodifyM 节点编辑器主题 | `MaterialNodifyMTheme` | `net10.0` |
| `Md3.Avalonia.Mapsui` | Mapsui 地图主题 | 地图级 `MaterialMapsuiTheme` | `net10.0` |
| `Md3.Avalonia.InkCanvas` | 手写画布主题与 `MdSignaturePad` | 控件级 `MaterialInkCanvasTheme` | `net10.0` |
| `Md3.Avalonia.RichEditor` | AvaloniaRichEditor 维护版主题 | `MaterialRichEditorTheme` | `net10.0` |

每个第三方主题包是**唯一**引入对应上游依赖的包，因此核心包不会被这些依赖污染。`Markdown`、`LiveCharts2`、`ScottPlot`、`Mapsui`、`InkCanvas` 五个包不注册到 `Application.Styles`：前者的样式赋给文档控件，其余是对象级入口。

设计原则：

- 每个交互控件使用独立的 `Md*` CLR 类型与 scoped `ControlTheme`，尽量复用 Avalonia 原生行为、绑定、键盘与选择模型。

- 颜色、字体、形状、状态层、阴影与 motion 一律通过 token 与 `DynamicResource` 消费。

- 字体或图标 provider 缺失、校验失败时图标保持隐藏，不使用形似的替代字形。

## 主题

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

`MaterialColorUtilities` HCT 生成器支持 TonalSpot / Neutral / Vibrant / Expressive / Monochrome / Fidelity 六种 scheme、Standard / Medium / High 三档对比度、49 个色彩角色，并提供 System 与 Component 两层令牌、对比度诊断和主题 JSON round-trip。

### 内嵌官方 Material Symbols 字体

`Md3.Avalonia.Icons` 直接分发完整的 Google **Material Symbols Rounded** variable TTF，`Md3.Avalonia.Icons.Lite` 分发由同一文件生成的真实轮廓子集，使用者无需手动下载或注册字体。

来源固定为 `google/material-design-icons` commit `737e3324305806514d7909874fa1818ae1808232`，完整 TTF 的 SHA-256 为 `95b24392bb49efd1bc3e92cff4e2452ad094461bab7c97e7d8723fab97e330ca`。构建与 CI 均执行：

```bash
python3 scripts/verify-fonts.py
```

校验器核对完整字体与 Lite 子集的 checksum、variable-font tables 与 glyph 数量，拒绝缺失、占位矩形或被替换的二进制。同时引用 Core 与 Icons 后不需要手动调用 `ConfigureFonts`：provider 会被自动发现并注册，校验失败时 symbol glyph 保持隐藏。

## 控件总览

在线 [DocFX API 参考](https://ilwren.github.io/Md3.Avalonia/)从 18 个包的 C# XML 注释生成页面；[`docs/API.zh-CN.md`](docs/API.zh-CN.md)说明覆盖的包与文档来源。353 个公共类型由 `scripts/check-api-doc-coverage.py` 扫描，下表只列出各分组的主要类型。

| 分组 | 主要类型 |
|---|---|
| 按钮 | `MdButton`、`MdToggleButton`、`MdIconButton`、`MdToggleIconButton`、`MdSplitButton`、`MdStandardButtonGroup`、`MdConnectedButtonGroup`、`MdFloatingActionButton`、`MdExtendedFloatingActionButton`、`MdFabMenu` |
| 输入 | `MdTextBox`、`MdCheckBox`、`MdRadioButton`、`MdComboBox`、`MdAutoCompleteBox`、`MdSlider`、`MdSwitch`、`MdDatePicker`、`MdTimePicker`、`MdPinInput`、`MdTagInput`、`MdForm` |
| 导航 | `MdNavigationDrawer`、`MdNavigationBar`、`MdTabs`、`MdToolbar`、`MdTopAppBar`、`MdBottomAppBar`、`MdSearchBar`、`MdSearchView`、`MdCommandPalette`、`MdBreadcrumb` |
| 容器与浮层 | `MdCard`、`MdDialog`、`MdDialogHost`、`MdMenu`、`MdPopover`、`MdTooltip`、`MdSnackbar`、`MdBanner`、`MdSheetHost`、`MdExpansionPanelList`、`MdFloatingWindow`、`MdBorderlessWindow` |
| 数据展示 | `MdList`、`MdDataTable`、`MdDataGrid`、`MdCarousel`、`MdCalendar`、`MdTreeView`、`MdAvatar`、`MdBadge`、`MdChip`、`MdTimeline`、`MdWaveform`、`MdChart` |
| 反馈与进度 | `MdCircularProgressIndicator`、`MdLinearProgressIndicator`、`MdLoadingIndicator`、`MdSkeleton`、`MdRefreshIndicator`、`MdSpinKit`、`MdStateLayer`、`MdFocusRing` |
| 布局与动效 | `MdSurface`、`MdAdaptiveLayout`、`MdScrollViewer`、`MdWaterfall`、`MdMasonryPanel`、`MdStaggeredPanel`、`MdAnimationSequence`、`MdAnimatedText`、`MdRevealHost`、`MdMorphPanel`、`MdHero` |

全部按钮、Icon Button 与 FAB 模板都接入 `MdRipplePresenter`：按下位置产生涟漪，裁切到完整 container shape，并支持 Expressive / Standard / Reduced / None 四种 motion scheme。`MdButton` 的 Elevated 配置为 M3 已标记 deprecated 的兼容项，新界面应优先选用其他配置。

`Md3.Avalonia.Extra` 是可选的 clean-room 扩展包，不声称复刻对应 Flutter package 的全部 API；需要严格 parity 时查阅 [`docs/FLUTTER_PARITY_STATUS.zh-CN.md`](docs/FLUTTER_PARITY_STATUS.zh-CN.md)，不要仅凭相似的 `Md*` 类型名推断完全兼容。

## 目录结构

```text
src/Md3.Avalonia/                  # 核心控件与设计令牌，不依赖具体图标字体
├─ Controls/                       # Md* 控件、枚举与属性 API
├─ Motion/                         # motion scheme、spring 令牌与转场
└─ Themes/
   ├─ MaterialTheme.axaml
   ├─ Tokens/                      # 11 个令牌字典
   └─ Controls/                    # 每类控件的 scoped ControlTheme
src/Md3.Avalonia.Icons/            # 图标 provider，内嵌完整官方 TTF
src/Md3.Avalonia.Extra/            # Flutter-inspired 扩展控件
src/Md3.Avalonia.<Vendor>/         # 12 个 opt-in 第三方主题包，各自唯一引入上游依赖
src/Md3.Avalonia.Notifications/    # 原生系统通知，独立于 Avalonia UI

gallery/Md3.Avalonia.Gallery/          # 桌面 Gallery：108 个组件页 + Theme Lab
gallery/Md3.Avalonia.Gallery.Android/  # net10.0-android single-view 宿主
tests/Md3.Avalonia.HeadlessTests/      # API、输入、主题、回归与渲染测试
tests/.../Spec/                        # 规范一致性分层校验（L1 令牌 / L2 几何 / L3 金图 / L5 动效）
docs/                                  # API、兼容性、发布与验证文档
spec-snapshot/                         # 外部来源的期望值 oracle 与棘轮策略
```

规范一致性校验的分层职责、棘轮流程与已知差距见 [`docs/SPEC_VERIFICATION.md`](docs/SPEC_VERIFICATION.md)。

## 构建与测试

```bash
python3 scripts/verify-fonts.py     # 校验内嵌字体
bash scripts/build-nuget.sh         # 构建 18 个 .nupkg / .snupkg 包

dotnet build gallery/Md3.Avalonia.Gallery.Desktop/Md3.Avalonia.Gallery.Desktop.csproj -c Release
dotnet test tests/Md3.Avalonia.HeadlessTests/Md3.Avalonia.HeadlessTests.csproj -c Release
```

构建完整 solution 需要先安装 Android workload：

```bash
dotnet workload install android
dotnet build Md3.Avalonia.sln -c Release
```

以下静态检查不需要 .NET SDK：

```bash
python3 scripts/lint-design-tokens.py       # 设计令牌 lint
python3 scripts/check-api-doc-coverage.py   # DocFX 项目、XML 输出及双语接口文档
python3 scripts/check-gallery-localization.py --strict
```

原生 AOT 发布桌面 Gallery（Linux 需要 clang/zlib，`--smoke` 另需 xvfb）：

```bash
bash scripts/publish-gallery-aot.sh --rid linux-x64 --smoke   # 也可指定 win-x64 / osx-x64 / osx-arm64
```

脚本会断言产物是原生可执行文件且旁边没有托管 dll，`--smoke` 在 xvfb 下实际启动该二进制，确认裁剪未丢失运行时所需的反射元数据。`.github/workflows/aot-probe.yml` 在 main 与 arena 分支上运行同一脚本。

发布门禁会构建十八个包，并严格检查十八个 `.nupkg`、十八个 `.snupkg`、统一版本与已内嵌的官方字体。发布使用 NuGet Trusted Publishing，配置步骤见 [`docs/PUBLISHING.md`](docs/PUBLISHING.md)。

每次 build、test 或 pack 结束后必须立即清理编译产物。仓库不交付 `bin/`、`obj/`、`TestResults/`、DLL、PDB、NuGet 包、APK 或 AAB；`docs/*.png` 是保留的文档参考图。

## 文档

| 文档 | 内容 |
|---|---|
| [`CHANGELOG.md`](CHANGELOG.md) | 版本变更记录 |
| [在线 DocFX API 参考](https://ilwren.github.io/Md3.Avalonia/) · [`docs/API.zh-CN.md`](docs/API.zh-CN.md) | 由 XML 注释生成的公共 API 页面与包入口 |
| [`docs/AI_DISCLOSURE.zh-CN.md`](docs/AI_DISCLOSURE.zh-CN.md) | AI 生成与辅助内容说明及免责声明 |
| [`docs/GETTING_STARTED.zh-CN.md`](docs/GETTING_STARTED.zh-CN.md) | 快速上手、主题注册与首个页面 |
| [`docs/USAGE_EXAMPLES.zh-CN.md`](docs/USAGE_EXAMPLES.zh-CN.md) | 表单、图标、动态配色、瀑布流与对话框示例（含配图） |
| [`docs/NUGET_PACKAGES.zh-CN.md`](docs/NUGET_PACKAGES.zh-CN.md) | 18 个 NuGet 包、目标框架与主题入口 |
| [`docs/PROJECT_STRUCTURE.zh-CN.md`](docs/PROJECT_STRUCTURE.zh-CN.md) | 仓库目录、项目职责与 API 文档构建流程 |
| [`docs/COMPATIBILITY.zh-CN.md`](docs/COMPATIBILITY.zh-CN.md) | 目标框架、平台与依赖兼容策略 |
| [`docs/RELEASE_NOTES_0.5.0-preview.1.md`](docs/RELEASE_NOTES_0.5.0-preview.1.md) | 已发布版本的变更说明 |
| [`docs/RELEASE_NOTES_0.5.0-preview.2.md`](docs/RELEASE_NOTES_0.5.0-preview.2.md) | 0.5.0-preview.2 变更说明 |
| [`docs/RELEASE_VALIDATION.md`](docs/RELEASE_VALIDATION.md) | 人工验收清单与签署状态 |
| [`docs/PUBLISHING.md`](docs/PUBLISHING.md) | 发布流程与 NuGet Trusted Publishing 配置 |
| [`docs/SPEC_VERIFICATION.md`](docs/SPEC_VERIFICATION.md) | 规范一致性分层校验与已知差距 |
| [`docs/FLUTTER_PARITY_STATUS.zh-CN.md`](docs/FLUTTER_PARITY_STATUS.zh-CN.md) | Extra 包与 Flutter 对应控件的 parity 状态 |
| [`src/Md3.Avalonia.Notifications/README.zh-CN.md`](src/Md3.Avalonia.Notifications/README.zh-CN.md) | 系统通知的用法与平台前置条件 |

## 已知边界

- **人工验收尚未签署。** Narrator、VoiceOver、Orca 与 TalkBack 必须由具备对应操作系统和辅助技术的人员按发布清单验收。Android APK 构建成功不等于真机验收；ARM64、旋转、生命周期与 TalkBack 仍需在设备上签署，结果记录于 [`docs/RELEASE_VALIDATION.md`](docs/RELEASE_VALIDATION.md)。
- **动效。** Ripple、spring 令牌与继承式 motion scheme 已建立。shared-axis、fade-through、container transform 与 animated visibility 四种 M3 转场已实化（`MdRevealHost` 让出布局空间并裁剪，`MdMorphPanel` 插值两端尺寸），但对 `Control` 类型内容不做退场叠化。
- **AOT。** 十八个包均声明 `IsTrimmable` + `IsAotCompatible` 并启用相应分析器，主题 JSON 使用源生成序列化；Linux x64 原生 Gallery 已发布并在 xvfb 下 smoke-run。AOT workflow 会报告诊断，但诊断数为零不代表 Windows、macOS、Android 或使用方应用的 AOT 验收已通过。
- **Android 返回手势与安全区。** `MdBackNavigation` 覆盖 dialog、sheet、drawer、search、menu、FAB menu 与两个 picker；`MdSafeArea` 提供按边内缩。headless 行为有回归测试，真机手势动画与挖孔几何仍需人工验收。
- **RTL 与多语种。** 相关适配代码持续保留，但完整 RTL/多语种布局与屏幕阅读器人工验收当前不作为预览版阻塞项。
- **字体来源。** 完整 Material Symbols Rounded variable TTF 与真实 Lite 子集已提交并自动打包；CI 以固定 upstream commit、SHA-256、variable tables 与 glyph 数量阻止占位或替代字体混入发布。

官网页面、冻结的 AndroidX commit `e2171b56112f43bf962f5d608413f74033b580b6` 与 token 版本见 [`spec-snapshot/manifest.json`](spec-snapshot/manifest.json)。

## 许可证

Apache-2.0，见 [`LICENSE`](LICENSE)。第三方组件的许可声明见 [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md)；内嵌的 Material Symbols 字体同为 Apache-2.0，许可证随两个 Icons 包一起分发。
