# Md3.Avalonia — Material Design 3 controls for Avalonia 12

一个面向 Avalonia 12 的跨平台 Material 3 / M3 Expressive 控件库与组件 Gallery。项目同时提供核心控件、动态 HCT 主题、Material Symbols provider、Flutter-inspired Extra 控件、桌面窗口适配和 Android single-view Gallery。

> 当前版本：`0.4.1-preview.1`。项目适合预览、内部应用和组件验证；物理 Android/TalkBack、Windows Narrator、macOS VoiceOver、Linux Orca 等外部验收仍需单独签署。

设计原则：

- 每个交互控件使用独立的 `Md*` CLR 类型和 scoped `ControlTheme`；不全局覆盖 Avalonia 原生控件；
- 核心包不依赖 Fluent/Simple theme，尽量复用 Avalonia 原生行为、绑定、键盘和选择模型；
- Material 颜色、字体、形状、状态层、阴影和 motion 通过 token 与 `DynamicResource` 消费；
- Core、Icons、Icons.Lite、Extra、DataGrid、Dock、Tabalonia、Markdown、LiveCharts2、ScottPlot、PropertyGrid、AvaloniaEdit、AvaloniaHex 十四个包可以独立发布；Extra 和图表能力不强制绑定第三方 vendor；
- RTL、现有 accessibility 和多平台适配代码会持续保留；当前开发优先级暂不把完整 RTL/多语种布局和屏幕阅读器人工验收作为预览版阻塞项。

| Dark components and app bar | Dark outlined fields |
|---|---|
| ![Dark component preview](docs/MdFoundationComponentsDarkPreview.png) | ![Dark outlined-field preview](docs/MdOutlinedDarkPreview.png) |

![Action-button preview](docs/MdActionButtonsPreview.png)

![Carousel, card, chips and picker preview](docs/MdNewComponentsPreview.png)

| Text field | Checkbox | Combo box |
|---|---|---|
| ![MdTextBox preview](docs/MdTextBoxPreview.png) | ![MdCheckBox preview](docs/MdCheckBoxPreview.png) | ![MdComboBox preview](docs/MdComboBoxPreview.png) |

## 已实现控件

### Buttons

- `MdButton`：Filled、Tonal、Outlined、Text、Elevated；五种 Expressive 尺寸、Round/Square、前后图标；
- `MdToggleButton`：Filled、Tonal、Outlined；selected 颜色和 Round/Square shape morph；
- `MdIconButton`、`MdToggleIconButton`：Standard、Filled、Tonal、Outlined，五种尺寸、Narrow/Default/Wide、Round/Square，支持 selected icon；
- `MdSplitButton`：主操作与尾部操作拥有独立 command，精确 2 DIP visual gap，token-sized trailing segment，并通过独立 `MdDropdownMenu` 展开；selected trailing segment 保持连接侧 4 DIP 内角；
- `MdStandardButtonGroup`：按尺寸使用 18/12/8/8/8 DIP 间距；
- `MdConnectedButtonGroup`：2 DIP 间距，自动计算首、中、尾按钮的外圆内方轮廓；
- `MdFloatingActionButton`：Small、Regular、Medium、Large，Primary/Secondary/Tertiary 配色与 Level 3/4 elevation；
- `MdExtendedFloatingActionButton`：当前 Expressive Small、Medium、Large；
- `MdFabMenu` + `MdFabMenuItem`：可展开 2–6 个相关动作，56 DIP full-pill item、独立 `ExpansionDirection="Up|Down"`（默认向上）、首次挂载 `IsInitiallyOpen`、双向 `IsOpen`、`Show`/`Dismiss`、Escape 和可逆展开/收缩 motion。

所有按钮、Icon Button 和 FAB 模板都接入 `MdRipplePresenter`：按下位置产生涟漪，裁切到完整 container shape，并支持 Expressive、Standard、Reduced、None motion scheme。

`MdButton` 的 Elevated 配置为 M3 已标记 deprecated 的兼容项。新界面应优先选用其他配置。

### Radio、App bars 与 Badge

- `MdRadioButton : RadioButton`：20 DIP icon、40 DIP state layer、48 DIP target，支持 selected、error、disabled、`GroupName`、键盘和自动化；
- `MdTopAppBar`：Small、Medium Flexible、Large Flexible、centered title 和 scrolled container；subtitle 自动采用完整双行高度，默认保持 edge-to-edge surface，不绘制 card 式 outline/圆角；
- `MdBottomAppBar`：仅用于 Baseline 兼容，新设计应优先使用后续 docked toolbar；
- `MdBadge`：6 DIP dot 与 16 DIP labeled badge；
- `MdBadgedBox`：将 badge 放置到任意 icon/control 的 top-trailing corner。

### `MdTextBox`

- Filled、Outlined，56 DIP 容器、label、placeholder、icons、prefix/suffix；
- supporting/error、Avalonia validation、字符计数和 `MaxLength`；
- `ShowClearButton` 提供内嵌清空动作；`IsPassword` 提供原生密码遮罩和显示/隐藏按钮；
- label 使用不参与测量的 150 ms `RenderTransform` 动画；outline focus overlay 不改变布局尺寸；
- Outlined light/dark 均保持透明 field surface，浮动 label 准确位于 outline 中线；
- 保留原生 `PART_TextPresenter`、`PART_ScrollViewer`、选择、IME、软键盘和 AutomationPeer。

### `MdCheckBox`

- Unselected、selected、indeterminate、error、disabled；
- 18 DIP container、2 DIP 圆角、40 DIP state layer、48 DIP interactive target；
- 方框区域本身响应 pointer hover/pressed/click；
- check/uncheck 使用 150 ms stroke-draw、opacity 和 scale transition；
- 保留 Avalonia 原生三态、command、键盘和自动化行为。

### `MdComboBox`

- Filled/Outlined text-field anchor，与 `MdTextBox` 使用一致的浮动 label/notch 几何；
- attached popup：顶部方角、底部 4 DIP 圆角、零垂直间距、强制圆角裁切、`SurfaceContainerLow`、56 DIP item、selected `TertiaryContainer` 和 12 DIP selected shape；
- 保留 `PART_Popup`、`PART_ItemsPresenter`、`PART_EditableTextBox`；
- 支持 `ItemsSource`、selection、editable text、键盘导航、light dismiss 和 AutomationPeer；
- 弱引用 popup coordinator 保证打开另一个 `MdComboBox` 时自动关闭前一个；
- 原生 `ComboBoxItem` 只获得 popup 内的局部主题，不存在全局覆盖。

### 桌面适配与响应式基础

- `MdScrollViewer` + `MdScrollBar`：独立 Material 滚动模板，保留 wheel、touch、chaining、extent、viewport 与双向 offset，不覆盖原生 ScrollViewer；桌面鼠标拖页默认关闭并可用 `AllowMouseDrag` 显式启用，子控件直接操作和 `SuppressMouseDragScrolling` 子树优先；
- `MdAutoCompleteBox : AutoCompleteBox`：原生同步/异步过滤、text completion、selection 与键盘 API，采用 Material exposed-field 与弱引用 popup coordinator；
- `MdNumericBox : NumericUpDown`：原生 Value/Minimum/Maximum/Increment、解析、键盘、滚轮和 validation，复用 Material text-field 外观；
- `MdAdaptiveLayout`：按可配置 600/840/1200/1600 DIP breakpoint 选择 Compact/Medium/Expanded/Large/ExtraLarge 内容，并公开 portrait/landscape 与 Touch/Pointer/Keyboard input mode；
- `MdSurface`、`MdText`、`MdFocusRing`、`MdStateLayer`、`MdWindow`：可选的语义表面、完整 M3 type scale 与桌面基础适配类型；
- `MdBorderlessWindow`、`MdWindowTitleBar`、`MdCaptionButton`、`MdWindowResizeGrip` 与可替换平台 adapter：扩展客户区的 Material caption，同时保留平台边框、四角、阴影和 resize frame；Android 使用 safe no-op。

### Carousel、Card、Chips 与 Pickers

- `MdCarousel : ListBox` + `MdCarouselItem`：MultiBrowse、Hero、CenterAligned、Uncontained；按 Material/Flutter keyline 跟随滚动和 controller 导航调整 large/medium/40–56 DIP small 尺寸，点击选择不会令卡片突变；保留 ItemsSource/DataTemplate、键盘、autoplay/hover pause 与 finite/infinite loop；
- `MdCard : Button`：Elevated、Filled、Outlined；支持 `Command`/`Click`，`IsInteractive=False` 可作为纯展示容器；
- `MdChip : ToggleButton` 及 `MdAssistChip`、`MdFilterChip`、`MdInputChip`、`MdSuggestionChip`：Filter 使用双向 `IsChecked`，Input 支持 `RemoveCommand`/`RemoveRequested`；
- `MdDatePicker` / `MdDatePickerDialog`：Docked/Modal、固定 42 格完整日期网格、按当前 CultureInfo 本地化星期顺序和文本、最小/最大日期、双向 `SelectedDate`/`IsOpen`；
- `MdTimePicker` / `MdTimePickerDialog`：真实可拖动的 M3 时钟表盘（12/24 小时双环）、Dial/Input、默认任意一分钟精度、滚轮、键盘输入、双向 `SelectedTime`/`IsOpen`；
- ComboBox、Dropdown、DatePicker、TimePicker 共用弱引用 popup coordinator，任一临时 surface 打开时自动关闭前一个。

### Dialog、Divider、Lists 与进度反馈

- `MdDialog` + `MdDialogHost`：Basic/FullScreen、声明式 `IsOpen` 双向绑定、按模型类型匹配多个预声明 `DataTemplates`，以及返回结果的 `ShowAsync(model)`/`Close(result)`；支持 Escape 与可选 scrim dismiss；
- `MdDivider : Control`：水平/垂直、任意 inset/thickness/brush；
- `MdList : ListBox` + `MdListItem`：Standard/Segmented、单选/多选、leading/headline/supporting/trailing slots，保留 `ItemsSource`、selection、command 与键盘行为；
- `MdLoadingIndicator`：遵循继承式 motion scheme 的 Expressive morphing indicator，支持官网当前 contained/uncontained 两种形式；
- `MdLinearProgressIndicator` / `MdCircularProgressIndicator`：determinate/indeterminate；线性 indicator 另支持 Flat/Wavy、可配置 thickness/amplitude/wavelength。

### Menus、Navigation、Search 与反馈控件

- `MdMenu` + `MdMenuItem`：M3 临时 surface、leading/trailing slots、enabled/disabled state、`Command` 与 click API；
- `MdNavigationBar` + `MdNavigationBarItem`：当前 M3 Expressive Flexible（默认）与 Baseline 兼容模式、stacked/horizontal destination、active indicator、label 与 badge slots，保留 `ItemsSource` 和 selection；
- `MdNavigationDrawer`：Standard/Modal、left/right placement、scrim dismiss、Escape、双向 `IsOpen`，使用单一 drawer presenter 避免内容双父级；
- `MdSearchBar` + `MdSearchView`：搜索输入、leading/trailing actions、展开结果 surface，支持绑定与直接操作；
- `MdSheetHost`：Bottom/Left/Right 的 Standard/Modal sheet、drag handle、scrim dismiss、Escape 与双向 `IsOpen`；
- `MdSlider : Slider`：continuous/discrete step、16 DIP track、44×4 DIP handle、stop indicator 和随 thumb 移动的 value indicator；
- `MdSnackbar`：single/two-line、inverse color roles、action/dismiss slots、timeout 与 `Show`/`Dismiss`；`MdSnackbarHost` + `IMdSnackbarService` 为 ViewModel 提供单实例排队显示入口；
- `MdSwitch : ToggleButton`：selected/unselected、可选状态图标、双向 `IsChecked`、`Command`、键盘与 pointer 行为。

### Flutter 生态补全与桌面计划

- Phase 1：`MdBanner`、`MdExpansionPanelList`/`MdExpansionPanel`、`MdDataTable`/`MdDataTableRow`、`MdStepper`/`MdStep`、`MdRefreshIndicator`；
- Phase 2：`MdPaginatedDataTable`、`MdReorderableList`、`MdGridTile`/`MdGridTileBar`、`MdDismissible` 与交互式 `MdScrollBar`；
- Phase 3：`MdForm`/`MdFormField`、`MdDropdownFormField`、`MdSimpleDialog`、`MdAboutDialog`、`MdLicensePage` 与 `MdPickerRestorationStore`；
- Phase 4：`MdDraggableScrollableSheet`、`MdAdaptiveSwitch`、`MdAdaptiveProgressIndicator`、`MdHero`、`MdFocusTraversalGroup` 与 `MdShortcutScope`；
- 富文本有两种选择：`MdRichEditor`（Extra）只提供 Material 命令面，文档交给适配器，不往库里塞编辑器引擎；需要真实编辑能力（行内格式、表格、图片、分页、HTML/JSON/RTF 往返）时，请用 opt-in 的 `Md3.Avalonia.RichEditor` 主题包搭配 AvaloniaRichEditor——该控件自绘文档、不暴露 chrome（没有 `Background`/`CornerRadius`/`Padding`），所以主题只负责墨水（选区、光标、默认字号），容器由宿主提供；另注意它的 `DefaultFontSize` 单位是**磅**而非 DIP；
- 表格有两种选择：`MdDataGrid`（Extra）是 Material 数据表；需要完整电子表格能力（列宽调整、列重排、冻结列、分组、自动生成列）时，请用 Avalonia 原生 `DataGrid` 搭配 opt-in 的 `Md3.Avalonia.DataGrid` 主题包——控件是原生的，只有外观是 Material 的；
- 代码/文本编辑器请用 [AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit) 搭配 opt-in 的 `Md3.Avalonia.AvaloniaEdit` 主题包：它把 `TextEditor`、`TextArea`、查找替换面板、补全列表和补全气泡都换成 M3 外观，编辑器内部需要的紧凑输入框、查找按钮与选项开关全部是只在该编辑器模板里引用的 `Md.Comp.AvaloniaEdit.*` 主题，因此**不会覆盖应用其余部分的原生控件样式**，也不需要 FluentTheme。Gallery 的 `CodeExample` 已改用该包，不再为 AvaloniaEdit 局部加载 Fluent 资源；
- 需要看二进制/十六进制时用 [AvaloniaHex](https://github.com/Washi1337/AvaloniaHex) 搭配 opt-in 的 `Md3.Avalonia.AvaloniaHex` 主题包：它填的是 AvaloniaHex 自己发布的那些资源键（`HexForegroundBase`、`OffsetForeground`、表头、当前行、光标与选区画刷和 Pen），并给编辑器自带一个 Material 的 `ControlTheme`（`SurfaceContainerLow` 表面、`OutlineVariant` 描边、ExtraSmall 圆角、`MdScrollViewer` 滚动条），因此**不会覆盖应用其余部分的原生控件样式**，也不需要 FluentTheme；
- 停靠/多文档布局请用 [Dock](https://github.com/wieslawsoltes/Dock) 搭配 opt-in 的 `Md3.Avalonia.Dock` 主题包：它把 Dock 自己的强调色、表面、标签、分隔条、标题栏按钮与拖放目标全部换成 `Md.Sys` token（悬停/按下是 `onSurface` 状态层，选中标签是 `Primary` 容器配 `OnPrimary` 文字），并且**不复刻**上游模板——控件字典直接按 `avares://` 引用上游包、只自带一份 accent 字典，所以模板与部件名仍由 Dock 维护，升级不会漂移；它也是唯一引入 `Dock.Avalonia`/`Dock.Avalonia.Themes.Fluent` 依赖的包，不要与 `DockFluentTheme` 同时引入；由于 Dock 的 `ButtonCreateDocument`/`ChromeButton`/`DockDrawnCaptionButton` 都基于 `{x:Type Button}`，而核心主题不碰 Avalonia 原生控件、这里也不引入 `FluentTheme`，所以该包自带一份无法回避的 Material 文本按钮主题（`Themes/Controls/MaterialButton.axaml`）——按推荐的 `Application.Styles` 用法，应用自己的普通 `Button` 也会拿到它，这是 Dock 无需 `FluentTheme` 的代价；其余原生控件仍然不受影响；
- 需要**可拖拽、可撕出为独立窗口、可跨窗口吸附**的标签页时用 [Tabalonia](https://github.com/egorozh/Tabalonia) 搭配 opt-in 的 `Md3.Avalonia.Tabalonia` 主题包：它把上游 `Themes/Custom` 的 19 个画刷全部重写成 `Md.Sys.Color` token（标签条用 `Surface`，未选中标签是 `OnSurfaceVariant` 文字、悬停为 `OnSurface` 状态层，选中标签是 `Primary` 容器配 `OnPrimary` 文字），并补上上游其实**依赖 Avalonia `FluentTheme`** 才有的六个 `TabItemHeader*` 键，因此不需要任何 Fluent 资源；与 Dock 包一样**不复刻模板**——控件字典与 `Buttons`/`DragTabItem` 样式都按 `avares://Tabalonia/...` 直接引用上游包，所以模板与部件名仍由 Tabalonia 维护；它也是唯一引入 `Tabalonia` 依赖的包，不要与 Tabalonia 自带的 `CustomTheme`/`FluentTheme` 同时引入；
- 需要渲染 Markdown 文档时用 [Markdown.Avalonia](https://github.com/whistyun/Markdown.Avalonia) 搭配 opt-in 的 `Md3.Avalonia.Markdown` 主题包：它的形态与其他主题包不同——**不注册到 `Application.Styles`**，因为 `MarkdownScrollViewer` 自己挑选样式集合并把它插进自己的 `Styles`（最近者胜），五个内置集合由反射在 `MarkdownStyle` 的静态属性上按名选取、其 `INamedStyle` 标记是 internal，第三方无法命名也无法注册，所以唯一可用的接缝是给查看器赋值：`<md:MarkdownScrollViewer.MarkdownStyle><themes:MaterialMarkdownTheme /></md:MarkdownScrollViewer.MarkdownStyle>`（每个查看器一个实例，与内置集合的静态属性一致）。它按上游 `MarkdownStyleFluentTheme` 的选择器清单把颜色全部换成 `Md.Sys.Color`（正文 `OnSurfaceVariant`、标题与代码 `OnSurface`、链接 `Primary`（悬停 `Tertiary`）、引用条 `Primary`、表格线与分隔线 `OutlineVariant`、代码块 `SurfaceContainerLow`、提示与表头 `SurfaceContainerHighest`、上游标红的未处理 `:::` 容器用 `Error`）、字号换成 `Md.Sys.TypeScale`（h1 `HeadlineLarge` 到 h6 `TitleSmall`，其中 h5/h6 上游完全没有样式、由本包补齐；正文 `BodyLarge`、代码与表格单元 `BodyMedium`、表头 `LabelLarge`；标题按 M3 各角色的字重（到 TitleLarge 为止 Regular，TitleMedium/TitleSmall 与表头为 Medium）而非 Fluent 的 Light），并把查看器与每个文本块的 `SelectionBrush` 指向 `Md.Sys.Color.PrimarySelection.Brush`（与 AvaloniaEdit、AvaloniaHex、输入框主题同一层选区色，否则 `ComputedSelectionBrush` 会退回上游的青色），并补上 `mdc|Rule` 的前景色；文档不继承宿主字体而固定为 `Md.Sys.Typeface.Plain`；布局基本照搬上游、只在 Material 自有形状处改动（代码块是 `SurfaceContainerLow` 加 8 圆角与 12/10 内边距的卡片，取代上游上下两条细线；表格单元格内边距 10/6；未处理 `:::` 容器的 `Error` 标记为 3 宽边框），等宽字体栈没有 token、原样保留；它也是唯一引入 `Markdown.Avalonia.Tight` 依赖的包，若自行再加 SyntaxHigh 插件还需该插件自己的 appendix 样式，见 [API 参考](docs/API.md#markdown)；
- 需要给 [LiveCharts2](https://github.com/Live-Charts/LiveCharts2) 图表换 Material 外观时用 opt-in 的 `Md3.Avalonia.LiveCharts2`：它是唯一**完全不是 Avalonia 样式集合**的 Material 主题包（包内没有 `Styles`、也没有 `ControlTheme`），因为 LiveCharts 没有任何针对单个控件的样式接缝——每张图的外观都来自 `LiveCharts.DefaultSettings` 里那唯一一个主题，所以接缝是配置入口：启动时、在第一个图表控件出现之前调用 `LiveCharts.Configure(settings => settings.AddMaterialTheme());`。它叠在 LiveCharts 自带默认主题之上（`AddDefaultTheme` + 本包的规则），因此工具提示与图例工厂、动画和上游已经定好的几何都原样保留（平面与系列的规则追加到上游的规则列表之后；标签、指针、刻度这三类可视化元素，因为 LiveCharts 每种类型只有一个槽位、重复注册会抛异常，改为接管上游已占的槽位），只改 Material 有主张的部分：系列调色板取八个角色（`Primary`、`Tertiary`、`Secondary`、`Error`、`PrimaryContainer`、`TertiaryContainer`、`InversePrimary`、`Outline`，前四个与 LiveCharts 自带调色板一样凑出四种色相，后四个是同色相的更多色调），坐标轴用 `LabelMedium` 字号与 `OnSurfaceVariant` 墨色、网格线 `OutlineVariant` 且只画水平线（上游如此）、坐标轴内边距 12；折线是 3 DIP 描边、清空上游的面积填充、8 DIP 数据点（外圈系列色、内里表面色）；柱状以 4 DIP 圆角收尾、间距 6 DIP，堆叠柱保持直角；饼图/环图靠表面色（2 DIP）分隔切片；仪表是 `SurfaceContainerHighest` 轨道配 `Primary` 指示弧、指针沿用上游 20 DIP 宽度但墨色改用 `OnSurfaceVariant`（与 `MdProgress` 一致）；K 线用 `Tertiary` 涨、`Error` 跌，取代上游硬编码的绿与红；工具提示与图例是反相表面卡片（`InverseSurface` 底 + `InverseOnSurface` 字 + 投影，字号 `LabelLarge`），画布清屏到 `Surface`；所有颜色都按每张图绘制时所在的变体读取 `Md.Sys.Color.*`（未注册 `MaterialTheme` 时回退到包内自带的 M3 基线取值），明暗跟随应用；控件自身的覆盖（`ChartTheme`、工具提示与图例画刷）与系列自己设置的画刷依旧优先。核心包的 `MdChart` 保持与绘图引擎无关。上游固定 **2.1.0-dev-798**：Avalonia 12 线目前只有 2.1.0-dev 预发布（`net8.0`/`net10.0`，声明 `Avalonia >= 12.0.0`，低于本仓库 12.1.2 基线），稳定线 2.0.x 是 Avalonia 11 构建（声明 `Avalonia >= 11.0.0`），在 Avalonia 12 下无法加载，故不能选，见 [API 参考](docs/API.md#livecharts2)；
- 需要给 [ScottPlot](https://github.com/ScottPlot/ScottPlot) 图表换 Material 外观时用 opt-in 的 `Md3.Avalonia.ScottPlot`：它和 LiveCharts2 包一样**不是 Avalonia 样式集合**（包内没有 `Styles`、也没有 `ControlTheme`），因为 ScottPlot 的绘图对象自己保存颜色与字体，接缝是绘图对象级的入口：`MaterialScottPlotTheme.Follow(avaPlot)` 按控件当前变体应用规则并订阅控件自身的 `ActualThemeVariantChanged`，在应用切换明暗时重放（ScottPlot 只在设置时把颜色写进绘图对象），离屏绘图对象则用 `Apply(plot, dark)` 或实例 `Apply`。它只写 Material 有主张的部分：图形背景与数据区背景同为 `Surface`；`Plot.Axes.Color` 一次把边框、刻度线、刻度标签、轴名与标题刷成 `OnSurfaceVariant`；网格主刻度 `OutlineVariant` 且次网格关闭；图例是 `SurfaceContainerHigh` 面板配 `OutlineVariant` 描边与 `OnSurfaceVariant` 文字；字阶为标题 `TitleMedium`、轴名与图例 `LabelLarge`、刻度标签 `LabelMedium`，字族取自 `Md.Sys.Typeface.Plain`；系列走与 LiveCharts2 相同的八个角色调色板（经 `Plot.Add.Palette`，之后添加的 plottable 都沿用）。ScottPlot 的边框、刻度生成、布局与交互保持原样，未被处理的绘图对象保持默认外观，进程级状态（如 `Fonts.Default`）不被改写；未注册 `MaterialTheme` 时回退到包内自带的 M3 基线取值。上游固定 **5.1.59**（`net8.0`/`net10.0`，声明 `Avalonia >= 12.0.0` 与 `ScottPlot >= 5.1.59`，低于本仓库 12.1.2 基线），见 [API 参考](docs/API.md#scottplot)；
- 需要给 [Avalonia.PropertyGrid](https://github.com/bodong1987/Avalonia.PropertyGrid) 属性网格换 Material 外观时用 opt-in 的 `Md3.Avalonia.PropertyGrid`：网格自身不设置任何颜色（它的 604 行内嵌样式只负责模板布局，单元格编辑器全部是运行期创建的原生 Avalonia 控件），因此在没有原生控件主题的应用里它只画得出外壳、画不出单元格。`MaterialPropertyGridTheme` 用 32 个 `Md.Comp.PropertyGrid.*` 键控控件主题补齐这些缺口：每个单元格外的透传容器、Material 填充字段、按钮、筛选标签块、复选框、单选按钮、组合框及其条目、数值字段与微调器、滑块与滑块柄、进度条、日期/时间字段及其滚轮面板、分类展开器、名称/值分隔条、滚动器、上下文菜单与其条目和分隔线、工具提示；颜色全部走 `Md.Sys.Color.*`、字号走 `Md.Sys.TypeScale.*`、圆角走 `Md.Sys.Shape.Corner.*`，属性列表滚动在核心的 `MdScrollBar` 上。它保持局部的方式与 AvaloniaEdit 包相同：这些主题都不按原生类型注册，而是由「选择器要求存在 `PropertyGrid` 祖先」的样式逐个下发，因此注册它不会替换应用其他位置的任何原生控件主题，也不需要 `FluentTheme`。三处仍属上游：其模板与可展开对象工厂作为本地值设置的 0.5 DIP 灰色细线（样式覆盖不到，本包也不复刻上游 11 个内嵌样式以免与 part 名漂移）、颜色单元格沿用的 Avalonia `ColorPicker`（网格本就为自己的子树加载其样式表），以及日期/时间浮层保留控件自带的滚轮机制。上游固定 **12.1.1.1**（`net8.0`/`net9.0`/`net10.0`，声明 `Avalonia >= 12.1.1`、`Avalonia.Controls.ColorPicker >= 12.1.1` 与 `bodong.PropertyModels >= 12.0.0`），见 [API 参考](docs/API.md#property-grid)；
- 独立 `Md3.Avalonia.Extra` 包包含 `MdAvatar`/`MdAvatarGroup`、fractional `MdRating`、`MdBreadcrumb`、`MdBeforeAfter`、`MdAnimatedText`、`MdSpinKit` 与 `MdStaggeredPanel`，并通过 opt-in `ExtraTheme` 复用核心 Material tokens；其中 `MdChart` 保持 provider-neutral，不内置第三方图表引擎；
- Ecosystem Waves A–C：density/overlay/async/shortcut contracts、`MdPopover`、`MdHoverCard`、`MdCommandPalette`、`MdSlidableItem`、`MdPagedItemsView`、`MdMasonryPanel`、`MdDataGrid`、`MdAsyncSelect`、`MdCalendar`、`MdTimeline`、`MdResultView`、`MdCascader` 与 `MdTransfer`；
- Ecosystem Waves D–E：provider-neutral `MdChart`、`MdRichEditor`、`MdChatView`、`MdSkeleton` 与 `MdAnimationSequence`；不捆绑 chart vendor、editor engine、network/AI provider 或数据库；
- Ecosystem Wave F：`MdPinInput` 分格输入/粘贴/遮罩/完成状态，`MdTreeView` 无限层级/展开选择/键盘/RTL，以及由正式 Material input chips 构成的 `MdTagInput` 标签输入、建议、验证和换行布局；
- Flutter 官方对齐登记于 [`docs/FLUTTER_COMPONENT_PARITY.md`](docs/FLUTTER_COMPONENT_PARITY.md)；Ecosystem A–F 与 clean-room gate 见 [`docs/FLUTTER_ECOSYSTEM_RESEARCH_PLAN.md`](docs/FLUTTER_ECOSYSTEM_RESEARCH_PLAN.md)；无边框窗口 W0–W3 见 [`docs/BORDERLESS_WINDOW_PLAN.md`](docs/BORDERLESS_WINDOW_PLAN.md)。

### Tabs、Toolbars 与 Tooltips

- `MdTabs : ListBox` + `MdTabItem : ListBoxItem`：Primary/Secondary、fixed/scrollable、stacked/inline icon、badge slot，保留 `ItemsSource`、selection 与键盘导航；
- `MdToolbar : ItemsControl`：Docked/Floating、Standard/Vibrant、Horizontal/Vertical，以及 leading/trailing slots；默认 floating container 为 64 DIP 高的 full shape；
- `MdTooltip` + `MdTooltipHost`：Plain/Rich surface、title/action slots、hover、focus、long-press、可选 click、Escape、outside dismiss，以及双向 `IsOpen` 和 `Show()`/`Dismiss()`；
- Tooltip popup 与其他临时 surface 一样不强制 OverlayLayer，以兼容 Android 和无可用 overlay 的 Avalonia host。

## 共用基础

- Avalonia **12.1.2**；当前仓库和预览包以 `net10.0` / `net10.0-android` 为 CI 验证基线；
- Light、Dark、System 主题和 `DynamicResource` tokens；
- `MaterialColorUtilities` HCT 任意 seed color 生成器，TonalSpot/Neutral/Vibrant/Expressive/Monochrome/Fidelity 六种 scheme、Standard/Medium/High contrast、49 个标准/固定/surface-container 色彩角色；
- `MdThemeManager`、`MdThemeJson` 与对比度诊断，支持 motion、font、shape 和主题 JSON round-trip；System/Component token 分层；
- 核心包提供通用 `MdIcon`/`MdSymbolPresenter` 与可替换的 `Md.Sys.Typeface.Symbols.Rounded`、`Md.Icon.*` contract；完整 Google codepoint catalog、强类型 `MdSymbols` 和自动嵌入字体 provider 位于可选 `Md3.Avalonia.Icons` 包；字体/包缺失或验证失败时图标视觉为空，不使用 look-alike fallback；
- M3 motion physics 参数、继承式 motion scheme、pointer-origin ripple、popup/FAB-menu transition；
- 核心 `Md3.Avalonia` 控件库不依赖 `Avalonia.Themes.Fluent` 或 `Avalonia.Themes.Simple`；Gallery 也不再加载 Fluent——`CodeExample` 的 AvaloniaEdit 现在由 opt-in 的 `Md3.Avalonia.AvaloniaEdit` 主题包接管，该包自带编辑器所需的全部部件外观，因此整个仓库不再需要 Fluent/Simple；
- 核心程序集不引用 Desktop、Win32、X11 或 macOS 专属程序集，可由 Android 宿主引用；当前 Android Gallery 的 CI target 为 `net10.0-android`；
- Gallery 使用官网式顶部导航、真实 `MdNavigationDrawer` 左侧组件栏、中央文档与右侧动态目录；按 Compact `<600`、Medium `600–839`、Expanded `840–1199`、Large `1200–1599`、Extra-large `>=1600` 五档切换一至三栏，compact/medium 使用 modal drawer 并自动收缩过宽示例；Android bottom destinations 会切换真实页面；
- shell 由单一 `MdScrollViewer` 持有有限 viewport，导航时解包页面预览用根 ScrollViewer，避免嵌套无限测量，并已用真实 wheel input 验证 Offset 变化；Gallery 现为 101 个页面、每个组件独立成页（见 `docs/GALLERY_DEFECTS.md` 的 #12），Components 概览页由 gallery index 生成并有测试钉死不会出现死链；
- 每个组件页使用 AvaloniaEdit 提供具备 Light/Dark 语法高亮、选择、滚动和一键复制能力的 AXAML/C# 示例；示例语言使用单选 Material segmented button group 切换；Symbols 页面虚拟化浏览并点击复制官方 catalog 中的全部图标；
- Avalonia Headless + Skia 行为、输入、主题隔离、布局和渲染测试。

### 已内嵌官方 Material Symbols 字体

仓库直接包含并随 `Md3.Avalonia.Icons` 分发完整 Google **Material Symbols Rounded** variable TTF；`Md3.Avalonia.Icons.Lite` 也包含由同一官方文件生成的真实轮廓子集。使用者无需下载、复制或手动注册字体。

来源固定为 `google/material-design-icons` commit `737e3324305806514d7909874fa1818ae1808232`，完整 TTF SHA-256 为 `95b24392bb49efd1bc3e92cff4e2452ad094461bab7c97e7d8723fab97e330ca`。构建和 CI 会执行：

```bash
python3 scripts/verify-fonts.py
```

验证器会核对完整字体与 Lite 子集 checksum、真实 variable-font tables 和 glyph 数量，拒绝缺失、占位矩形或被替换的二进制。Apache-2.0 字体许可证同时打入两个 Icons 包。

打包脚本要求 .NET 10 SDK，依次打包 Core、Icons、Icons.Lite 与 Extra，并在 `artifacts/nuget` 生成四个 `.nupkg` 和四个 `.snupkg`。Bash 使用 `--output`，PowerShell 使用 `-Output` 修改输出目录。

同时引用 Core 与 Icons 后不需要手动调用 `ConfigureFonts`：`MdSymbols` 和核心 `MdSymbolPresenter` 会自动发现 Icons provider、注册程序集内嵌字体，验证 internal family、typeface 和官方 `search` glyph，再注入核心 `Md.Icon.*` resources。验证失败时 symbol glyph 保持隐藏，不使用 Unicode 仿制图标。

## 目录

```text
src/Md3.Avalonia/                 # 官方 Flutter Material 对齐核心包；不依赖具体图标字体
├─ Controls/                       # 核心 Md* CLR 控件、枚举和属性 API
└─ Themes/
   ├─ MaterialTheme.axaml
   ├─ Tokens/                      # 11 个 token 字典，全部登记在 MaterialTheme.axaml
   │  ├─ ColorTokens.axaml          # M3 基线配色 + Light/Dark ThemeDictionaries
   │  ├─ ExtendedColorTokens.axaml
   │  ├─ SystemTokens.axaml
   │  ├─ FoundationTokens.axaml
   │  ├─ ContainmentTokens.axaml
   │  ├─ ButtonTokens.axaml
   │  ├─ ActionButtonTokens.axaml
   │  ├─ TextFieldTokens.axaml
   │  ├─ CheckBoxTokens.axaml
   │  ├─ ComboBoxTokens.axaml
   │  └─ ToolbarTokens.axaml
   └─ Controls/                    # 每类控件的 scoped ControlTheme
src/Md3.Avalonia.Icons/           # 可选 Symbols catalog/loader；内嵌完整官方 TTF
src/Md3.Avalonia.Extra/           # 第三方 Flutter clean-room 控件；依赖核心，不依赖 Icons
src/Md3.Avalonia.AvaloniaEdit/    # opt-in：AvaloniaEdit 的 Material 3 主题（唯一引入该依赖的包）
src/Md3.Avalonia.AvaloniaHex/     # opt-in：AvaloniaHex 十六进制编辑器的 Material 3 主题（唯一引入该依赖的包）
src/Md3.Avalonia.Tabalonia/       # opt-in：Tabalonia 可拖拽标签页的 Material 3 主题（唯一引入该依赖的包）
src/Md3.Avalonia.Markdown/        # opt-in：Markdown.Avalonia 文档渲染的 Material 3 样式集合（唯一引入该依赖的包）
src/Md3.Avalonia.LiveCharts2/     # opt-in：LiveCharts2 图表的 Material 3 主题（唯一引入该依赖的包）
src/Md3.Avalonia.ScottPlot/       # opt-in：ScottPlot 图表的 Material 3 主题（唯一引入该依赖的包）
src/Md3.Avalonia.PropertyGrid/    # opt-in：Avalonia.PropertyGrid 属性网格的 Material 3 主题（唯一引入该依赖的包）

gallery/Md3.Avalonia.Gallery/     # 组件、Theme Lab、资源与字体图标页面
gallery/Md3.Avalonia.Gallery.Android/ # net10.0-android single-view host（solution 外）
tests/Md3.Avalonia.HeadlessTests/ # API、输入、主题、回归及渲染测试
tests/.../Spec/                   # 规范一致性分层校验（L1 令牌 / L2 几何 / L3 金图 / L5 动效）
docs/                              # API、兼容性、发布验证与参考渲染图
spec-snapshot/manifest.json        # 官网、AndroidX commit、token 版本与决策
spec-snapshot/tokens.json          # 外部来源的期望值 oracle（禁止由本仓库生成）
spec-snapshot/conformance-policy.json # 棘轮开关 bootstrap/enforce 与豁免清单
```

规范一致性校验的分层职责、棘轮流程与**已知差距清单**见
[`docs/SPEC_VERIFICATION.md`](docs/SPEC_VERIFICATION.md)。以下静态检查不需要 SDK：

```bash
python3 scripts/lint-design-tokens.py        # 设计令牌 lint
python3 scripts/check-api-doc-coverage.py    # docs/API.md 是否覆盖全部公共类型
```

## 引入主题

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

十四个可独立 pack 的 NuGet 包版本均为 `0.4.1-preview.1`：`Md3.Avalonia`（核心）、`Md3.Avalonia.Icons`、`Md3.Avalonia.Icons.Lite`（两种可选图标 provider）、`Md3.Avalonia.Extra`（依赖核心），以及九个 opt-in 的第三方控件主题包——`Md3.Avalonia.DataGrid`（Avalonia 原生 `DataGrid` 的 Material 主题，是唯一引入 `Avalonia.Controls.DataGrid` 依赖的包）、`Md3.Avalonia.RichEditor`（[AvaloniaRichEditor](https://github.com/centwon/AvaloniaRichEditor) 的 Material 主题，唯一引入该依赖的包）、`Md3.Avalonia.AvaloniaEdit`（[AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit) 的 Material 主题，唯一引入 `Avalonia.AvaloniaEdit` 依赖的包；它同时替代了 Gallery 过去为 `CodeExample` 局部加载的 Fluent 资源）和 `Md3.Avalonia.AvaloniaHex`（[AvaloniaHex](https://github.com/Washi1337/AvaloniaHex) 十六进制编辑器的 Material 主题，唯一引入 `AvaloniaHex` 依赖的包）和 `Md3.Avalonia.Dock`（[Dock](https://github.com/wieslawsoltes/Dock) 停靠布局系统的 Material 主题，唯一引入 `Dock.Avalonia`/`Dock.Avalonia.Themes.Fluent` 依赖的包）和 `Md3.Avalonia.Tabalonia`（[Tabalonia](https://github.com/egorozh/Tabalonia) 可拖拽/可撕出标签页的 Material 主题，唯一引入 `Tabalonia` 依赖的包）和 `Md3.Avalonia.Markdown`（[Markdown.Avalonia](https://github.com/whistyun/Markdown.Avalonia) 文档渲染的 Material 3 样式集合，唯一引入 `Markdown.Avalonia.Tight` 依赖的包，且是唯一不注册到 `Application.Styles`、而是赋给 `MarkdownScrollViewer.MarkdownStyle` 的主题包）和 `Md3.Avalonia.LiveCharts2`（[LiveCharts2](https://github.com/Live-Charts/LiveCharts2) 图表的 Material 3 主题，唯一引入 `LiveChartsCore.SkiaSharpView.Avalonia` 依赖的包；它不是 Avalonia 样式集合，而是通过 `LiveCharts.Configure` 配置一次的全局主题入口）和 `Md3.Avalonia.ScottPlot`（[ScottPlot](https://github.com/ScottPlot/ScottPlot) 图表的 Material 3 主题，唯一引入 `ScottPlot.Avalonia` 依赖的包；同样不是 Avalonia 样式集合，而是绘图对象级入口 `MaterialScottPlotTheme`）和 `Md3.Avalonia.PropertyGrid`（[Avalonia.PropertyGrid](https://github.com/bodong1987/Avalonia.PropertyGrid) 属性网格的 Material 主题，唯一引入 `bodong.Avalonia.PropertyGrid` 依赖的包；它为网格运行期创建的原生控件提供 32 个 `Md.Comp.PropertyGrid.*` 键控控件主题，全部通过「要求 `PropertyGrid` 祖先」的选择器下发，因此不替换应用其他位置的原生控件主题）。不用就不会被拖进来。核心与 Extra 都不强制引用 Icons；十四个包均包含 XML API 文档、README 和第三方声明。重复缺陷复核见 [`docs/COMPONENT_QUALITY_CHECKLIST.md`](docs/COMPONENT_QUALITY_CHECKLIST.md)。

任意 seed 主题可在启动时或运行时应用：

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

完整 API 入口见 [`docs/API.md`](docs/API.md)，兼容策略见 [`docs/COMPATIBILITY.md`](docs/COMPATIBILITY.md)，本版说明见 [`docs/RELEASE_NOTES_0.4.1-preview.1.md`](docs/RELEASE_NOTES_0.4.1-preview.1.md)。

## XAML 与 MVVM

```xml
<Window xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:md="using:Md3.Avalonia.Controls">
  <StackPanel Spacing="16">
    <md:MdToggleButton Content="Favorite"
                       IsChecked="{Binding IsFavorite}"
                       Command="{Binding ToggleFavoriteCommand}" />

    <md:MdSplitButton Content="Create"
                      Command="{Binding CreateCommand}"
                      TrailingCommand="{Binding ShowCreateMenuCommand}"
                      IsDropDownOpen="{Binding IsCreateMenuOpen}" />

    <md:MdTextBox Text="{Binding Name}"
                  Label="Project name"
                  MaxLength="40"
                  ShowCharacterCounter="True" />

    <md:MdCheckBox Content="Include diagnostics"
                   IsChecked="{Binding IncludeDiagnostics}"
                   IsError="{Binding HasDiagnosticsError}" />

    <md:MdComboBox Label="Platform"
                   Variant="Outlined"
                   ItemsSource="{Binding Platforms}"
                   SelectedItem="{Binding Platform}" />

    <md:MdRadioButton Content="Automatic updates"
                      GroupName="updateMode"
                      IsChecked="{Binding UseAutomaticUpdates}" />

    <md:MdExtendedFloatingActionButton Content="Compose"
                                       Icon="{x:Static md:MdSymbols.Edit}"
                                       Command="{Binding ComposeCommand}" />

    <md:MdChip Content="Open now" Variant="Filter"
               IsChecked="{Binding OpenNow}" />

    <md:MdDatePicker SelectedDate="{Binding DueDate}" Mode="Modal" />
    <md:MdTimePicker SelectedTime="{Binding StartTime}" Mode="Input" />

    <md:MdLinearProgressIndicator Value="{Binding Progress}" Shape="Wavy" />

    <md:MdList ItemsSource="{Binding Files}"
               SelectedItem="{Binding SelectedFile}" />
  </StackPanel>
</Window>
```

全部 bindable API 都是 Avalonia properties，可直接用于 CommunityToolkit.MVVM；同时保留直接控件操作：

```csharp
var menu = new MdFabMenu { IsOpen = true };
var comboBox = new MdComboBox
{
    Label = "Platform",
    ItemsSource = new[] { "Android", "Windows", "Linux" },
    SelectedIndex = 0
};
comboBox.IsDropDownOpen = true;

// DialogHost.DataTemplates can predeclare one MdDialog view for each model type.
var result = await dialogHost.ShowAsync(new DiscardDraftDialogModel(message));
dialogHost.Close(result: true);

// Inject the same service instance into the ViewModel and an attached MdSnackbarHost.
await snackbarService.ShowAsync(new MdSnackbarMessage("Draft archived")
{
    ActionContent = "Undo",
    ActionCommand = undoCommand
});
```

## Extra 与第三方 Flutter UI 能力

`Md3.Avalonia.Extra` 是可选的 clean-room 扩展包，不声称复刻 Flutter package 的全部 API。当前提供：

- `MdBeforeAfter`：水平/垂直 Before/After 内容对比和可拖动分隔线；
- `MdAnimatedText`：Typewriter、Fade、Pop-friendly、None 等文字显示模式，支持 `Start`、`Stop`、循环和完成事件；
- `MdSpinKit`：RotatingPlain、ThreeBounce、Wave、FadingCircle、ChasingDots 等非核心 Material 加载动画；
- `MdStaggeredPanel` / `MdAnimationSequence`：可取消的 staggered entrance 和序列 motion；
- `MdSlidableItem`：Start/End action、拖动、键盘操作、RTL 基础、`IsOpen`、dismiss threshold 和 action command；
- `MdMasonryPanel`：Masonry、Quilted、Woven 三种布局策略；
- `MdChart`：provider-neutral 基础图表展示，不内置图表数据引擎，也不试图替代第三方 chart library；
- Avatar、Rating、Breadcrumb、DataGrid、Calendar、TreeView、TagInput、RichEditor、ChatView 等生态控件。

这些控件的成熟度不同。需要严格 Flutter API parity 时，请查看 [`docs/FLUTTER_PARITY_STATUS.md`](docs/FLUTTER_PARITY_STATUS.md)，不要仅根据相似的 `Md*` 类型名推断完全兼容。

## Borderless window / Chromeless window

桌面应用可以使用 `MdBorderlessWindow` 和 `MdWindowTitleBar` 构建 Material 标题栏。默认行为是：

- Windows 保留原生 Full caption style bits，以保留 DWM 的最小化、最大化和还原行为；
- Material 自己绘制客户区标题栏，不再叠加 Fluent/Simple 的第二套标题栏；
- `ShowIcon` 默认为 `true`，标题栏显示 `Window.Icon`；设为 `False` 可隐藏。标题栏使用窗口主体 surface 颜色且不绘制额外分隔线；
- `ShowMinimizeButton`、`ShowMaximizeButton`、`ShowCloseButton` 控制按钮是否显示；
- `IsMinimizeButtonEnabled`、`IsMaximizeButtonEnabled`、`IsCloseButtonEnabled` 控制按钮是否可操作；
- `PreserveNativeBorder`、`CanResize`、`ExtendIntoTitleBar` 控制 native frame 和客户区扩展；
- Android adapter 是 safe no-op，Android Gallery 不展示 Borderless windows 页面。

```xml
<md:MdBorderlessWindow Title="My app"
                       IsMinimizeButtonEnabled="False"
                       IsMaximizeButtonEnabled="True"
                       ShowCloseButton="True">
  <views:Shell />
</md:MdBorderlessWindow>
```

更换平台行为时注入 `IMdWindowPlatformAdapter`。完整 API 和限制见 [`docs/BORDERLESS_WINDOW_PLAN.md`](docs/BORDERLESS_WINDOW_PLAN.md)。

## Android 约束

- Android 为 Tier 1；`gallery/Md3.Avalonia.Gallery.Android` 提供 `net10.0-android` single-view 宿主（最低 API 23，正式发布验证目标 API 26+）；
- Gallery 的 `App` 同时处理 desktop classic lifetime 与 Android `ISingleViewApplicationLifetime`；
- 核心控件不引用桌面专属 API；
- TextBox/可编辑 ComboBox 保留原生 IME 和软键盘链路；
- Popup 继续由 Avalonia 原生 popup/fallback 宿主处理可用空间、light-dismiss 与返回键；模板不强制 OverlayLayer，避免无可用 overlay 的 Android/headless host 卡死或崩溃；
- 小尺寸按钮仍保留至少 48 DIP 的 interaction target；
- Android Gallery 源码宿主已加入且独立于 desktop solution；Android 导航隐藏不适用移动端的 Borderless windows 页面；ARM64、旋转、生命周期和真机/模拟器人工矩阵仍须在具备 Android workload/设备的环境按 `docs/RELEASE_VALIDATION.md` 签署。

## 构建与测试

```bash
python3 scripts/verify-fonts.py
scripts/build-nuget.sh

dotnet build Md3.Avalonia.sln -c Release
dotnet test tests/Md3.Avalonia.HeadlessTests/Md3.Avalonia.HeadlessTests.csproj -c Release --no-build
```

当前发布门禁会构建十四个包并严格检查十四个 `.nupkg`、十四个 `.snupkg`、统一版本和已内嵌官方字体。仓库不保留普通构建产物。测试覆盖 Light/Dark Gallery screenshots、任意 HCT seed golden vectors、49 roles、三档 contrast、主题 JSON round-trip、五档 breakpoint、搜索索引、LTR/RTL 渲染、CommunityToolkit.Mvvm、Automation/live-region、虚拟化和主题生命周期、AvaloniaEdit 双语言编辑器、真实 ScrollViewer extent/viewport/wheel offset、Autocomplete/Numeric input、adaptive breakpoints、Flexible NavigationBar、official chip/item 类型、popup 非强制 OverlayLayer、Tabs/Toolbars/Tooltips、Menus/Drawer/Rail/Search/Sheets/Slider/Snackbar/Switch 的渲染与直接 API，以及 Dialog、Lists、contained Loading/Progress、popup 互斥、文化日期网格、一分钟 TimePicker、state layer、buttons、fields、Carousel/Card/Chips、AppBar、Symbols、Radio、Badge、ripple/motion。

每一次 build、test 或 pack 命令结束后必须立即清理编译产物，再继续后续实现或验证。Workspace 不交付 `bin/`、`obj/`、`TestResults/`、DLL、PDB、NuGet、APK 或 AAB；`docs/*.png` 是保留的文档参考图。

## 当前边界

- Ripple、spring token 和 inherited motion scheme 已建立；shared-axis、fade-through、container transform 与 animated visibility 四种 M3 转场已实化（`MdRevealHost` 让出布局空间并裁剪，`MdMorphPanel` 插值两端尺寸），但对 `Control` 类型的内容不做退场叠化，详见 [Flutter parity status](docs/FLUTTER_PARITY_STATUS.md)；
- HCT dynamic theme、49 个当前 Material role、Theme Lab 与 JSON round-trip 已完成；`MdTextBox.IsPassword` 已覆盖密码输入与 reveal 行为；
- Android 系统返回键／预测性返回已接入（`MdBackNavigation`，覆盖 dialog、sheet、drawer、search、menu、FAB menu 与两个 picker），安全区内缩已提供按边控制（`MdSafeArea`）；两者的 headless 行为有回归测试，真机手势动画与挖孔几何仍需人工验收；
- Android Gallery 源码宿主已提供，但当前环境未安装 Android workload，也没有 ARM64 设备，因此 APK、旋转、生命周期、真机/模拟器矩阵不得视为已签署；
- 十四个包均已声明 `IsTrimmable` + `IsAotCompatible` 并开启 IL2xxx/IL3xxx 分析器；主题 JSON 走源生成序列化，`MdDataGrid`/`MdAsyncSelect`/`MdSearchView` 的字符串属性路径均提供了免反射的 selector 委托（见 [API 参考的 Trimming 一节](docs/API.md#trimming)）。**原生 AOT 已验证可用**：`.github/workflows/aot-probe.yml` 用 `PublishAot=true` 发布桌面 Gallery，断言产物是原生 ELF（无托管 dll 残留）、IL 诊断为零，并在 xvfb 下实际启动该二进制确认运行时未因缺反射元数据而崩溃；
- Narrator、VoiceOver 和 Orca 必须由具备对应 OS/辅助技术的人员按发布清单人工验收；
- 完整 Material Symbols Rounded variable TTF 与真实 Lite 子集已提交并自动打包；CI 以固定 upstream commit、SHA-256、variable tables 和 glyph 数量阻止占位或替代字体混入发布。

官网页面、冻结 AndroidX commit `e2171b56112f43bf962f5d608413f74033b580b6` 和 token 版本见 [`spec-snapshot/manifest.json`](spec-snapshot/manifest.json)。
