# 第三方 Avalonia 控件 —— Material 3 主题接入调研（2026-10-11）

本文件回答一个问题：**当前有哪些热度较高的「单个、第三方」Avalonia 控件，适合以 opt-in 主题包的形式接入
Md3.Avalonia 做 Material 3 美化？** 结论先行，后面是数据与接入设计。

基线：本仓库 Avalonia `12.1.2`、`net10.0`，六个包全部 `IsTrimmable` + `IsAotCompatible`，核心包不依赖
Fluent/Simple 主题，且**不允许全局覆盖原生控件样式**。已有先例：`Md3.Avalonia.DataGrid`（原生 DataGrid 的 M3
主题）与 `Md3.Avalonia.RichEditor`（AvaloniaRichEditor 的 M3 主题），都是「唯一引入该依赖」的独立包。

数据日期 **2026-10-11**；星标来自 GitHub API，Avalonia 版本来自各仓库 `Directory.Packages.props` /
`*.csproj`（**源码现状，不等于 NuGet 已发布版的依赖**，接线前必须核对，见 §6）。

---

## 1. 结论速览

**最值得先做的三个**（成本低、收益直接、有现成主题缝）：

| 排名 | 组件 | 为什么值得 |
|---|---|---|
| 1 | **AvaloniaEdit**（`Avalonia.AvaloniaEdit` 12.0.0，1.14k★，MIT） | 上游主题就是一个 38 行的资源映射文件；做了它，还能顺手消掉 Gallery 目前「为了 AvaloniaEdit 局部加载 Fluent resources」的妥协，并同时惠及 Dock、Markdown.Avalonia.SyntaxHigh 等**自己也用 AvaloniaEdit** 的控件 |
| 2 | **Dock**（1.49k★，MIT，Avalonia 12.1.3） | 5,311 行主题看起来吓人，但 48 个 ControlTheme 里只有 **4 处硬编码颜色**、引用 **146 个 Dock token** —— M3 化 ≈ 换掉 token 字典 + 少量几何 |
| 3 | **AvaloniaHex**（136★，MIT，12.0.0） | 与 AvaloniaEdit 同构：53 行 Base 样式 + 46 行 Fluent 资源变体，只需约 50 行资源字典 |

**热度高、适合做「颜色/字体桥接」型主题（不做控件重排）**：
LiveCharts2（5.5k★）、ScottPlot（6.8k★）、Mapsui（1.6k★，有专门的 `Mapsui.Avalonia12` 包）、
Markdown.Avalonia（417★）+ LiveMarkdown.Avalonia（176★）。

**明确不要接**：整套设计系统主题库（Semi/Ursa/AtomUI/SukiUI/ShadUI/Huskui/FluentAvalonia/Material.Avalonia）
——它们本身就是「另一套设计语言」，与 M3 令牌体系正面冲突；以及所有仍停留在 Avalonia 11 的库。

---

## 2. 评估门槛

一个第三方控件要能进入本仓库，至少要过五关：

1. **Avalonia 12 可用**。仓库基线 12.1.2；Avalonia 12 是 major 版本，11.x 编译的控件库不能假定可用。
   最硬的判据是上游源码/NuGet 依赖指向 12.x，且能通过 `dotnet build` 实测。
2. **许可证可登记**。MIT / Apache-2.0 优先，需要写进 `THIRD-PARTY-NOTICES.md`。
3. **存在「主题缝」**。控件自身把外观放在可替换的 `Styles` / `ResourceDictionary` / `ControlTheme` 里；
   纯自绘控件只能改颜色，不能改圆角、高度、状态层，例如 DataGrid 那种情况必须整份模板重写。
4. **可以做成 opt-in 独立包**。不能污染全局：必须是 scoped `ControlTheme` 或用户显式引入的 `Styles`。
5. **AOT/裁剪友好**。新包要跟随现有约定声明 `IsTrimmable` / `IsAotCompatible` / `EnableTrimAnalyzer`。

---

## 3. 候选清单（含主题缝实测）

「主题缝」一列是本仓库实际拉取各仓库源码后统计的：主题文件位置与规模、是否 token 驱动。

| 组件 | 星标 | 最近提交 | Avalonia 12 | 许可 | 主题缝（源码实测） | 接入成本 |
|---|---|---|---|---|---|---|
| AvaloniaEdit | 1,137 | 2026-08-28 | 12.0.0 ✔ | MIT | `Themes/Base.xaml`(11 行) + `Themes/Fluent/AvaloniaEdit.xaml`(38 行，纯资源映射) + `Simple` 变体；控件模板内嵌 | **极低** |
| Dock | 1,490 | 2026-10-11 | 12.1.3 ✔（最新 tag 要求；本仓库固定 12.1.0.6，见 §4.2） | MIT | `Dock.Avalonia.Themes.Fluent` = 56 个 axaml / 5,311 行 / 52 个 ControlTheme（48 个控件主题文件）；`Accents/Fluent.axaml` 提供 token，控制主题里仅 4 处硬编码色 | 低-中 |
| AvaloniaHex | 136 | 2026-04-18 | 12.0.0 ✔ | MIT | `Themes/Base.axaml`(53 行 styles) + `Fluent`/`Simple` 资源变体(46 行) | **极低** |
| Tabalonia（可拖拽/撕出标签页） | 264 | 2026-07-05 | 12.0.5 ✔ | MIT | `Themes/Custom/{Brushes,CustomTheme,Controls,Buttons,DragTabItem}.axaml` 共 476 行，Brush 驱动；另有 Fluent 变体；官方声明「fully restylable via brush resources」 | 低 |
| Avalonia.Labs（Lottie/Gif/Qr/AnimatedImage/InfoBadge/Panels/SwipeView/FlipView/Notifications…） | 329 | 2026-09-09 | 12.0.1 ✔ | MIT | `Themes/ControlThemes.xaml` + `Themes/Controls/*.axaml`（每控件一个 ControlTheme） | 低 |
| Avalonia.PropertyGrid | 344 | 2026-08-24 | 12.1.1 ✔ | MIT | 样式内嵌在 `Sources/Avalonia.PropertyGrid/Controls/*.axaml`（11 个控件） | 中 |
| Markdown.Avalonia | 417 | 2026-07-13 | 12.0.0 ✔ | MIT | `Markdown.Avalonia.Tight/StyleCollections/MarkdownStyle{DefaultTheme,FluentTheme,Standard,GithubLike}.axaml` + SyntaxHigh 附录样式 | 低-中 |
| LiveMarkdown.Avalonia | 176 | 2026-09-19 | 12.0.0 ✔ | Apache-2.0 | `src/LiveMarkdown.Avalonia/Styles.axaml`（346 行 Style）+ Mermaid 变体 | 中 |
| NodifyM.Avalonia（节点编辑器） | 222 | 2026-05-12 | 12.0.2 ✔ | MIT | `Styles/ControlStyles.axaml`（单文件集中样式） | 中 |
| LiveCharts2 | 5,483 | 2026-07-20 | 12.0.0 ✔ | MIT | `LiveChartsCore/Themes/`（`Theme`、`ColorPalletes`、`LvcThemeKind`、`LiveChartsStylerExtensions`）+ `SkiaSharp/ThemesExtensions.cs` | 低（调色板/字体） |
| ScottPlot | 6,773 | 2026-08-15 | 12.0.0 ✔ | MIT | `ScottPlot.Avalonia` 5.1.60；`Plot.Palette`、背景/字体/网格属性 | 低（调色板/字体） |
| Mapsui | 1,577 | 2026-10-02 | **有 `Mapsui.Avalonia12` 包** ✔ | MIT | 地图渲染自绘；可主题化的是控件外壳与图层配色 | 低 |
| PanAndZoom | 479 | 2026-10-03 | 12.1.1 ✔ | MIT | 无 chrome，本身不需要主题 | 极低（仅登记） |
| Svg.Skia（`Svg.Controls.Skia.Avalonia`） | 738 | 2026-08-27 | 12.0.0 ✔ | MIT | 叶子控件，无主题需求 | 极低（仅登记） |
| UVtools.AvaloniaControls（AdvancedImageBox 等） | — | 活跃 | 12.1.3 ✔ | MIT | `UVtools.AvaloniaControls` 5.1.1 | 低 |
| AvaloniaGraphControl（MSAGL 图布局） | 357 | 2026-04-19 | 12.0.1 ✔ | NOASSERTION | 面板类控件，样式量小 | 低 |
| MessageBox.Avalonia | 596 | 2026-04-21 | 12.0.1 ✔ | MIT | 自带模板 | 中（与 `MdDialog` 重叠） |
| DialogHost.Avalonia | 368 | 2026-07-15 | 12.0.0 ✔ | MIT | 自带模板 | 中（与 `MdDialogHost` 重叠） |
| Jc.PopupView.Avalonia（toast/popup） | 14 | 2026-10-09 | 12.0.5 ✔ | MIT | 自带模板 | 低-中（与 `MdSnackbar` 重叠） |
| Avalonia.RangeSlider | 53 | 2026-06-11 | 12.0.4 ✔ | MIT | 单个模板 | 低（M3 有 range slider 规范） |

> 星标为热度近似指标；此处未采用 NuGet 下载量（本沙箱只能访问 NuGet 搜索 API，见 §6）。

### 3.1 排除：仍停留在 Avalonia 11

| 组件 | 实测 Avalonia 引用 | 说明 |
|---|---|---|
| Projektanker/Icons.Avalonia | 11.3.0（最后提交 2025-05-14） | 本仓库自带官方 Material Symbols，不需要 |
| AvaloniaCommunity/Notification.Avalonia | 11.0.9 | 与 `MdSnackbar` 重叠 |
| BAndysc/nodify-avalonia | 11.1.0 | 节点编辑器请改用 NodifyM.Avalonia（12.0.2） |
| satial-interfaces/CalendarControl | 11.2.5 | 与 `MdCalendar` 重叠；周视图可作为未来自研项 |
| Egorozh.ColorPicker | 2024 停更 | 官方 `Avalonia.Controls.ColorPicker` 已是 12.x |

### 3.2 排除：设计语言冲突（不建议做主题）

`Semi.Avalonia`(1.96k★)、`Ursa.Avalonia`(1.58k★)、`SukiUI`(2.69k★)、`AtomUI`(842★, LGPL-3.0)、
`ShadUI`(564★)、`Huskui.Avalonia`、`FluentAvalonia`(1.61k★)、`Material.Avalonia`(1.16k★)、
`Citrus.Avalonia`、`Classic.Avalonia`。

这些是「整套主题/设计系统」而不是单个组件。引入它们等于在同一个应用里跑两套设计令牌：颜色、圆角、
类型比例、状态层都会打架，而且多数会全局重写原生控件样式 —— 与本仓库「不全局覆盖原生控件」的原则直接矛盾。
其中 `Material.Avalonia` 还占据 Material Design 命名空间，容易被使用者误认成同一体系。

### 3.3 官方但已归档：TreeDataGrid

`AvaloniaUI/Avalonia.Controls.TreeDataGrid`（347★）**已于 2025-10-13 归档**。社区 fork
`wieslawsoltes/TreeDataGrid`（58★，2026-10-03 仍活跃）在继续。若要做 Material 版的树形表格主题，
应对接该 fork 或自研，不要依赖归档仓库。

---

## 4. 推荐接入设计

命名与结构完全沿用现有惯例：包名 `Md3.Avalonia.<控件名>`、`RootNamespace=Md3.Avalonia.Themes`
（避免命名空间与被主题化控件自身的命名空间互相遮蔽，DataGrid 包里对此有专门注释）、主题文件放
`Themes/Material<X>Theme.axaml`、`x:Class` 形式、只引用 Avalonia + 上游控件 + 核心项目。

### 4.1 Wave 1（低成本、高收益，建议立刻做）

**A. `Md3.Avalonia.AvaloniaEdit`**

- 上游 `Themes/Fluent/AvaloniaEdit.xaml` 只是把 `ToolTipBackground/Foreground/BorderBrush`、
  `SystemChromeMediumColor`、`SystemBaseLowColor`、`SystemAccentColor` 等 Fluent 资源键映射到
  AvaloniaEdit 自己的键（`SearchPanelBackgroundBrush`、`SearchPanelBorderBrush`、`TextAreaSelectionBrush`、
  `CompletionToolTip*`、`OverloadViewer*`、`SearchPanelFontSize/FontFamily`）。
- M3 版本就是**同结构的另一个资源文件**：Selection → `Md.Sys.Color.Primary`（配 alpha 状态层）、
  搜索面板 → `SurfaceContainerHigh`、补全弹窗 → `SurfaceContainer` + `OutlineVariant` 描边、字体走
  `Md.Sys.Typeface` + M3 type scale，dark 变体走 ThemeDictionaries。
- 额外收益：Gallery 里 `CodeExample` 现在**为了 AvaloniaEdit 的模板资源依赖而局部加载 Fluent**；
  有了这个包就能彻底去掉。Dock（内部依赖 `Avalonia.AvaloniaEdit 12.0.0`）与
  `Markdown.Avalonia.SyntaxHigh` 的代码块也会跟着受益。
- 预期规模：一个 axaml 文件 ≈ 80~120 行 + 一个 `Styles` 子类。

**B. `Md3.Avalonia.AvaloniaHex`**

- `Themes/Base.axaml` 用 `HexForegroundBase`、`OffsetForeground`、`HeaderForeground/Background`、
  `CurrentLineBackground`、`Primary/SecondarySelectionBackground`、`Primary/SecondaryCaretBackground`
  以及同名 `Pen`；Fluent 变体只是给这些键填色。
- M3 版本 ≈ 50 行资源字典：选择色用 Primary 状态层、当前行用 `SurfaceContainerHighest`、
  caret 用 `OnSurface`、header 用 `OnSurfaceVariant`。

**C. `Md3.Avalonia.Labs`（可选，只挑需要的控件）**

- Avalonia.Labs 每个控件一个 ControlTheme（`AsyncImage`、`ContentDialog`、`FlipView`、`InfoBadge`、
  `NavigationControl`、`StepBar`、`TabControl/TabHeader…`），入口是 `Themes/ControlThemes.xaml`。
- 做法：只对需要的控件写 M3 覆盖层（`ControlTheme` 派生 + `BasedOn`），而不是全量复制。

### 4.2 Wave 2（中等成本，业务驱动）

**D. `Md3.Avalonia.Dock`** —— 价值最高的一个，但工作量取决于 Dock 的接口稳定性。

- Dock 的主题层已经很干净：`Accents/Fluent.axaml` 定义 `DockApplicationAccentBrush{Low,Med,High}`、
  `DockTheme{Background,Foreground,BorderLow,Accent}Brush`、`DockSurface{Workbench,Sidebar,Editor,Panel,
  Header,HeaderActive}Brush` 以及尺寸 token；52 个 ControlTheme 只有 4 处硬编码颜色。
- **已落地（2026-10-11）**：`Md3.Avalonia.Themes.MaterialDockTheme : Styles`（类名跟另外五个主题一致，
  不叫 `DockMaterialTheme`），在 `src/Md3.Avalonia.Dock/`。做法比这里设想的更省：控件字典不复制，而是照
  `DockFluentTheme` 的同名同序清单用绝对 `avares://Dock.Avalonia.Themes.Fluent/Controls/*.axaml` 直接引用
  上游，只自带一份 `Themes/Accents/Material.axaml`（197 行 / 159 个键，44 个画刷重写成 `Md.Sys.Color.*`），
  于是模板、部件名连 `/Assets/*.png` 指示图都留在上游，Dock 升级不会漂移。两处例外：上游的 compact 密度样式
  与 `CacheDocumentTabContent` 文档模板覆盖没有搬（密度由核心 `MaterialTheme` 管）；上游那 4 处硬编码颜色其实
  都是 BoxShadow 阴影色（`#33000000`/`#80000000`），M3 的阴影同样是黑色，故原样保留。
- **一个无法回避的隐式主题**：`ButtonCreateDocument`/`ChromeButton`/`DockDrawnCaptionButton` 都
  `BasedOn="{StaticResource {x:Type Button}}"`，而核心 `MaterialTheme` 不碰 Avalonia 原生控件、这里也不引入
  `FluentTheme`，所以包里必须自带 `Themes/Controls/MaterialButton.axaml`（Material 文本按钮），这是该包唯一
  会影响的原生类型，`MdDockThemeTests` 把这条边界钉住；按文档推荐的 `Application.Styles` 用法，应用自己的普通 Button
  也会拿到这个文本按钮外观，这是 Dock 在没有 `FluentTheme` 时能工作的代价。（也因此
  AvaloniaEdit/AvaloniaHex 的「不替换原生主题」测试改为只断言各自相关的类型，Button 归 Dock 的测试管。）
- 版本：仓库基线 Avalonia 12.1.2，而 Dock 12.1.0.7 要求 Avalonia ≥ 12.1.3，因此固定 **12.1.0.6**（要求
  12.1.1；accent 键集与 12.1.0.7 完全一致），`Directory.Packages.props` 里注明两者要一起升。
- 风险：Dock 迭代很快（当天仍有提交），主题包需要跟版本对齐，建议固定上游版本号并在 README 标注适配范围。

**E. `Md3.Avalonia.Tabalonia`** —— 与已有 `MdTabs` 互补：Dock 场景之外，用户要「可拖拽 / 可撕出窗口 / 可
跨窗口停靠」的标签页时用得上。以 `Themes/Custom` 为基线换成 M3 token（Brushes 55 行 + 少量模板）。

**F. Markdown 主题集** —— 二选一或都做：

- `Markdown.Avalonia` 有现成的 `StyleCollections` 机制（Default/Fluent/Standard/GithubLike），
  加一个 `MarkdownStyleM3.axaml` 即可，还能顺带接管 `SyntaxHigh` 的代码块配色（依赖 4.1.A）。
- `LiveMarkdown.Avalonia` 只有一个 `Styles.axaml`（346 行），是「实时渲染 + LLM 场景」的常见选择，
  覆盖它的 `MarkdownTextBlock` / `CodeBlock` 等 selector 即可。

**G. 图表 provider 主题** —— 与「`MdChart` provider-neutral、不捆绑图表引擎」的定位一致：

- `Md3.Avalonia.LiveCharts2`：实现一个 `LiveCharts` 主题（LvcThemeKind + ColorPalletes + 字体），
  把 M3 tonal palette 映射成系列色。
- `Md3.Avalonia.ScottPlot`：设置 `Plot.Palette`、背景、网格与字体。
- 二者都**只做外观**，不引入新的数据管线。

### 4.3 Wave 3（按需）

- `Md3.Avalonia.PropertyGrid`（11 个内嵌控件样式，需要逐个替换）、`Md3.Avalonia.NodifyM`、
  `Md3.Avalonia.Mapsui`（外壳 + 图层配色）、Avalonia.Labs 剩余控件。
- 与已有 `Md*` 控件重叠的（MessageBox.Avalonia、DialogHost.Avalonia、Notification.Avalonia、
  Jc.PopupView、RangeSlider）：**优先用仓库自身的 `MdDialogHost` / `MdSnackbar` / `MdSlider`**；
  只有当使用者的既有代码必须保留第三方 API 形态时，才值得单独出主题包。
- 无需主题、只需登记的：`Svg.Controls.Skia.Avalonia`、`PanAndZoom`、`UVtools.AvaloniaControls`。

---

## 5. 每个新包的最小骨架（沿用 DataGrid / RichEditor 惯例）

1. `src/Md3.Avalonia.<X>/Md3.Avalonia.<X>.csproj`
   - `TargetFramework=net10.0`、`RootNamespace=Md3.Avalonia.Themes`、`AssemblyName=PackageId=Md3.Avalonia.<X>`；
   - `IsPackable/GenerateDocumentationFile/IncludeSymbols/IsTrimmable/IsAotCompatible/EnableTrimAnalyzer`；
   - `PackageReference`：`Avalonia` + 上游控件包 + 核心 `ProjectReference`；**不引用 Fluent/Simple**；
   - 打包 `README.md` 与 `THIRD-PARTY-NOTICES.md`。
2. `Themes/Material<X>Theme.axaml` + `.axaml.cs`（`Styles` 子类），颜色全部 `DynamicResource Md.Sys.*`。
3. `THIRD-PARTY-NOTICES.md` 增加一行：包名、版本、用途、许可证、上游地址。
4. `README.md`、`docs/API.md`、`Directory.Packages.props`（新增 `PackageVersion`）登记。
5. Gallery 增加一个页面，并且**只在页面内局部加载该主题**，不做全局 `StyleInclude`。
6. 测试：Headless + Skia 渲染快照、主题隔离（引入前后原生控件不受影响）、令牌消费检查。
7. 每次 build/test/pack 后立即清理产物（仓库约定：不留 `bin/obj/`）。

---

## 6. 核查纪律（重要）

- **「Avalonia 12」这一列是源码现状**。各仓库 `Directory.Packages.props` / `csproj` 指向 12.x，不代表
  NuGet 上已发布版本的依赖也是 12.x。接线前务必：
  `dotnet add package <X>` 后 `dotnet list package --include-transitive`，或在 NuGet 页面查看 Dependencies；
  能编译 + 能跑 Gallery 才算通过。
- **本沙箱没有 .NET SDK**，也无法从命令行访问 NuGet（只有 NuGet 搜索 API 可读）。因此本文所有结论都是
  静态源码核对（codeload tarball + GitHub API），**必须**在有 SDK 的环境执行 build/test 后再落库。
- Avalonia 12 是 major 版本；同进程只能有一个 Avalonia 主版本。若同时出现 11.x 依赖，NuGet 会以
  版本冲突或运行期失败的方式暴露，不要试图用 binding redirect 掩盖。
- 每个主题包都应声明**适配的上游版本范围**（Dock / Tabalonia / PropertyGrid 迭代都很快）。

---

## 7. 数据来源

- GitHub REST API（星标、最近提交、许可证、归档状态、仓库树、文件内容），2026-10-11。
- 各仓库源码：通过 `codeload.github.com` tarball 拉取后统计主题文件行数、ControlTheme 数量、
  硬编码颜色数与 token 引用数。
- 列表来源：[`AvaloniaCommunity/awesome-avalonia`](https://github.com/AvaloniaCommunity/awesome-avalonia)
  的 Controls / Theme & Icons 章节，以及 GitHub 上 `topic:avalonia` 按星标排序的仓库检索。
- NuGet 最新版本信息来自 NuGet 搜索 API（例：Avalonia 最新稳定版 12.1.4）。
