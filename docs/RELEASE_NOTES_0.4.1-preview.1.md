# Md3.Avalonia 0.4.1-preview.1 release notes

发布日期：2026-10-06

这是一个**缺陷修复版本**。重点是上一轮 Gallery 实跑评审中反馈的四类桌面端显示问题，以及新增一个
opt-in 的富文本主题包和已验证的原生 AOT 支持。

> 版本号从 `0.4.0-preview.1` 进到 `0.4.1-preview.1`：没有破坏性改动，公共 API 面只做了加法，
> 所以走 patch 位。**仍为预览版**——API 在首个稳定版前仍可能做保持兼容的打磨。

## 修复

### Carousel 在宽窗口下只占半条轨道

排布规则在**任何窗口宽度下都只产出「一个大项 + 一个中项」**，第三项起一律压到 56 DIP 的小 keyline。
实测 1188 DIP 的视口里，四个 item 只占 584 DIP，剩下 605 DIP 全空：

```
修复前   280 | 8 | 168 | 8 | 56 | 8 | 56 | ──── 空 605 ────
```

这套规则本身没错，只是**只为手机窄屏写的**——所以 Android 上看着正常，桌面上就是半空加一排细条。

`MdCarousel` 现在会求解视口放得下几个大项，并且从焦点项**向尾部**递减而不是对称递减。对称窗口会发出
`2 × largeCount − 1` 个大项，正好撑爆刚解出来的那个数。大项数为 1 时新规则与旧规则逐字等价，**窄屏
行为分毫未动**。

### 无边框窗口四角是方的

主题特意把窗口自身设成透明，好让合成器把圆角从客户区里切出来，圆角表面交给 `PART_WindowFrame` 画。
但这个 Border 把自己的 `Background` **绑回了那个刚被设成透明的窗口背景**，于是它什么都没画。平台一旦
拒绝透明，不透明的 `TransparencyBackgroundFallback` 就会把整个方形窗口刷满——这就是截图里的方角。

现在 frame 画 `TransparencyBackgroundFallback`：语义上正是「本窗口的不透明底色」，已主题化为 Material
surface，调用方仍可覆盖。

### 图像对比滑块与指针脱节

手柄骑在分隔线上。当分隔线位于最左/最上时，**手柄有一半被裁到控件之外**，指针够得着的每一个像素都落在
同一侧，于是按下时拾取的偏移是单向的，最大可达半个手柄宽。拖拽是有意设计成按位移量跟踪的（抓偏了不让
分隔线跳到光标下），所以这个偏移会被完整保留到整个拖拽结束。

现在保留的偏移被**裁剪到分隔线到较近一侧边缘的距离**：中段几乎等于半个控件，手感不变；两端恰好为 0，
自动退化为吸附。一条规则同时满足两种情形。

### 桌面端 picker 弹窗收不到滚轮和按键

桌面上弹窗是独立的 `PopupRoot`，在它内部抛出的事件止步于该 root，不会冒泡回控件；Android 把弹窗渲染进
`TopLevel` overlay，所以同样的手势在那边一直是好的，在 Windows 上却是死的。弹窗表面现在转发进同一套
处理器，`MdTimePicker` 也会按指针位置决定调整的是小时列还是分钟列。

### 其它

- 页面滚动时桌面弹窗会被关闭，而不是停在原地、锚点跑掉。
- Cascader 下拉改为与锚点左边缘对齐，不再居中。

## 新增

- **`Md3.Avalonia.RichEditor`** —— [AvaloniaRichEditor](https://github.com/centwon/AvaloniaRichEditor)
  的 Material Design 3 主题，opt-in。它是唯一引入该依赖的包，包总数到六个。
- **`Md3.Avalonia.AvaloniaEdit`** —— [AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit)
  的 Material Design 3 主题，opt-in。它是唯一引入 `Avalonia.AvaloniaEdit` 依赖的包。
  主题保留了上游模板的部件名与绑定；编辑器自己创建、但没有公开外观入口的原生控件（紧凑查找输入框、
  查找按钮、match case/whole word/regex 开关、展开箭头、补全行）全部改成只在该编辑器模板里引用的
  `Md.Comp.AvaloniaEdit.*` 键，所以注册它**不会**替换应用其余部分的原生控件样式。Gallery 的
  `CodeExample` 已改用它，仓库不再需要 Fluent 资源。
- **`Md3.Avalonia.AvaloniaHex`** —— [AvaloniaHex](https://github.com/Washi1337/AvaloniaHex)
  十六进制编辑器的 Material Design 3 主题，opt-in，唯一引入 `AvaloniaHex` 依赖的包，包总数到八个。
  它沿用上游的资源键与样式结构（十六进制文本、偏移量、表头、当前行、光标与选区），只是把颜色换成
  `Md.Sys` token，并给 `HexEditor` 一个 Material 的 `ControlTheme`：`SurfaceContainerLow` 表面、
  `OutlineVariant` 描边、ExtraSmall 圆角、`MdScrollViewer` 滚动条，因此同样不需要 FluentTheme；
- **`Md3.Avalonia.Dock`** —— [Dock](https://github.com/wieslawsoltes/Dock) 停靠布局系统的
  Material Design 3 主题，opt-in，唯一引入 `Dock.Avalonia` 与 `Dock.Avalonia.Themes.Fluent`
  依赖的包，包总数到九个。它把 Dock 自己的五个 `DockApplicationAccent*` 画刷以及全部
  `DockTab*`/`DockSurface*`/`DockBorder*`/`DockSplitter*`/`DockChrome*`/`DockTarget*` 键重写成
  `Md.Sys.Color` token（悬停/按下用 `onSurface` 状态层，选中标签用 `Primary` 容器配
  `OnPrimary` 文字，窗口关闭悬停用 `ErrorContainer`），并且在结构上**不复刻**上游模板——它通过
  `avares://` 直接引用 Dock 控件的字典，只自带一份 `Accents/Material.axaml`，因此模板与部件名
  仍然归 Dock 维护，升级不会漂移；另外，Dock 的 `ButtonCreateDocument`/`ChromeButton`/
  `DockDrawnCaptionButton` 都是 `BasedOn="{StaticResource {x:Type Button}}"`，而既不引入
  `FluentTheme`、核心 `MaterialTheme` 又不碰 Avalonia 原生控件，所以包里还带了一份无法回避的
  隐式原生主题 `Themes/Controls/MaterialButton.axaml`（Material 文本按钮）：按文档推荐的
  `Application.Styles` 用法，这也是应用自己的普通 `Button` 会拿到的外观——这是 Dock 无需
  `FluentTheme` 也能工作的代价；其余原生控件仍交由宿主应用自行决定（所以其他 opt-in 主题包的隔离测试
  改为只断言各自的控件类型）。若想让应用自己的按钮保持宿主主题，可把该主题注册在 Dock 所在的范围内
  （Gallery 的停靠页就放在 `UserControl.Styles`）里；
- **`Md3.Avalonia.Tabalonia`** —— [Tabalonia](https://github.com/egorozh/Tabalonia) 可拖拽/
  可撕出标签页的 Material Design 3 主题，opt-in，唯一引入 `Tabalonia` 依赖的包，包总数到十个。
  它把上游 `Themes/Custom/Brushes.axaml` 的 19 个画刷全部重写成 `Md.Sys.Color` token（标签条与
  标签容器用 `Surface`，未选中标签是 `OnSurfaceVariant` 文字配透明容器，悬停是 0.08 的 `OnSurface`
  状态层、非活动窗口 0.04，选中标签是 `Primary` 容器配 `OnPrimary` 文字，分隔线用 `OutlineVariant`），
  并补上上游模板真正读取、却由 Avalonia `FluentTheme` 提供的六个 `TabItemHeader*` 键
  （`TabItemHeaderForeground{Unselected,UnselectedPointerOver,Selected,SelectedPointerOver}`、
  `TabItemHeaderMargin`、`TabItemHeaderThemeFontWeight`）——因此它不需要任何 Fluent 资源；
  另外补了两个上游留给宿主的空白：添加按钮的常态前景色，以及选中（填充）标签上关闭按钮的
  `OnPrimary` 文字。结构上与 Dock 包一致：**不复刻**模板，而是按 `avares://` 直接引用上游的
  `TabsControl.axaml`（字典）与 `Buttons.axaml`/`DragTabItem.axaml`（样式），只自带一份
  `Themes/Accents/Material.axaml`，所以模板与部件名仍由 Tabalonia 维护，升级不会漂移；
  由于 accent 字典本身不带主题字典，明暗完全跟随核心 `MaterialTheme` 变体。原包自带
  `CustomTheme`/`FluentTheme` 都不要同时引入；
- **`Md3.Avalonia.Markdown`** —— [Markdown.Avalonia](https://github.com/whistyun/Markdown.Avalonia)
  文档渲染的 Material Design 3 样式集合，opt-in，唯一引入 `Markdown.Avalonia.Tight` 依赖的包
  （核心包，程序集名 `Markdown.Avalonia`），包总数到十一个。它是唯一**不注册到 `Application.Styles`**
  的主题包，这不是取舍而是上游的接缝：`MarkdownScrollViewer` 自己挑选样式集合并把它插进自己的 `Styles`
  （最近的集合胜出，应用级 `StyleInclude` 永远赢不了），五个内置集合是通过 `MarkdownStyleName` 反射
  `MarkdownStyle` 的静态属性按名选取的，而它们的 `INamedStyle` 标记是 internal——第三方既无法命名也
  无法注册。唯一可用的接缝就是 `MarkdownStyle` 属性本身：
  `<md:MarkdownScrollViewer.MarkdownStyle><themes:MaterialMarkdownTheme /></md:MarkdownScrollViewer.MarkdownStyle>`，
  每个查看器一个实例（内置集合的静态属性同样每次返回新对象）。它按上游 `MarkdownStyleFluentTheme`
  （Avalonia 12 那一线，正是固定版本所带的集合）的选择器清单逐条重写：形状全部保留，颜色换成
  `Md.Sys.Color`（正文 `OnSurfaceVariant`、标题与代码 `OnSurface`、链接 `Primary` 加下划线、悬停为
  上游要求的互补强调色即 `Tertiary`、引用条 `Primary`、表格线与分隔线 `OutlineVariant`、代码块
  `SurfaceContainerLow`、提示/表头/行内代码 `SurfaceContainerHighest`、上游用红色标记的未处理 `:::`
  容器换成 `Error` 角色），字号换成 `Md.Sys.TypeScale`（h1 `HeadlineLarge` 一路到 h6 `TitleSmall`、
  正文 `BodyLarge`、代码与表格单元 `BodyMedium`、表头 `LabelLarge`；标题按 M3 给这些角色的字重——到 `TitleLarge`
  为止 Regular，`TitleMedium`/`TitleSmall` 与表头为 Medium，与 MdText 自己的 ControlTheme 同一套；
  文档固定使用 `Md.Sys.Typeface.Plain` 而不继承宿主字体）。
  另外补了三个内置集合留下的空白：查看器与每个文本块的 `SelectionBrush` 指向
  `Md.Sys.Color.PrimarySelection.Brush`（与 AvaloniaEdit、AvaloniaHex、输入框主题同一层选区色，
  否则 `ComputedSelectionBrush` 会退回青色）、h5/h6 的字号（解析器会发类名但没有任何集合为它们写过
  样式）、以及 `mdc|Rule` 的前景色（该控件用自己的 `Foreground` 画线，否则直接继承窗口的墨色）；
  三者都有 headless 断言。布局基本照搬上游，只在 Material 自有形状处改动：代码块是 SurfaceContainerLow
  卡片、8 圆角与 12/10 内边距（取代上游上下两条细线），表格单元格内边距 10/6（上游为 2），未处理
  `:::` 容器的 `Error` 标记为 3 宽边框。它不引用 `SyntaxHigh` 插件（代码块是 Tight 的普通 `TextBlock`）；应用若自行再加该插件，需要自己补它的 appendix 样式
  （`avedit|TextEditor`/`Label.LangInfo`/`Button.CopyButton`），因为插件只为上游具名集合注入这些样式。
- **`Md3.Avalonia.LiveCharts2`** —— [LiveCharts2](https://github.com/Live-Charts/LiveCharts2) 图表的
  Material Design 3 主题，opt-in，唯一引入 `LiveChartsCore.SkiaSharpView.Avalonia` 依赖的包，包总数到十二个。
  它也是仓库里唯一**完全不是 Avalonia 样式集合**的 Material 主题：包内没有任何 `Styles` 或 `ControlTheme`，
  因此不可能泄漏进没配置过它的应用。这不是取舍而是上游的接缝——LiveCharts 没有针对单个控件的样式，
  每张图的外观都来自 `LiveCharts.DefaultSettings` 里那唯一一个 `Theme`，所以包的公开面就是一个配置入口：
  启动时、在第一个图表控件出现之前 `LiveCharts.Configure(settings => settings.AddMaterialTheme());`
  （`AddMaterialTheme` = 上游 `AddDefaultTheme` + `ApplyMaterialRules`，规则追加在其后，因此上游的工具提示与
  图例工厂、动画与既定的几何全部原样保留；应用已有自己的主题时改调 `theme.ApplyMaterialRules()`）。
  映射：系列色是八个角色（`Primary`、`Tertiary`、`Secondary`、`Error`、`PrimaryContainer`、
  `TertiaryContainer`、`InversePrimary`、`Outline`——前四个与上游自带调色板一样凑出四种色相，后四个是同色相
  的更多色调）；坐标轴用 `LabelMedium` 刻度 / `LabelLarge` 轴名 + `OnSurfaceVariant` 墨色、`OutlineVariant`
  网格（只画水平线，与上游一致）与 12 内边距，极坐标轴拿到完整网格与 `Surface` 标签底色；折线与阶梯线是
  3 DIP 描边、清空上游的面积填充、8 DIP 数据点（外圈系列色、内里 `Surface`），堆叠面积保留填充但去掉数据点；
  柱/条以 4 DIP 圆角收尾、间距 6，堆叠柱保持直角；饼/环图用 2 DIP 的 `Surface` 描边（即表面色透出，
  而非边框）；仪表是 `SurfaceContainerHighest` 轨道配 `Primary` 指示弧（与 `MdProgress` 同一映射）；K 线用
  `Tertiary` 涨、`Error` 跌，取代上游硬编码的绿与红；工具提示与图例是 `InverseSurface` 卡片配
  `InverseOnSurface` 文字与投影、字号 `LabelLarge`（图例无底色），画布清屏到 `Surface`。所有颜色都按每张图
  绘制时所在的变体读取 `Md.Sys.Color.*`（Avalonia 视图取应用的主题变体），明暗跟随应用；未注册
  `MaterialTheme` 时回退到包内自带的 M3 基线取值。控件自身的覆盖（`ChartTheme`、工具提示与图例画刷）与系列
  自己设置的画刷依旧优先。核心包的 `MdChart` 保持与绘图引擎无关。
  版本固定 **`2.1.0-dev-798`**：Avalonia 12 线目前只有 2.1.0-dev 预发布（`net8.0`/`net10.0`，声明
  `Avalonia >= 12.0.0`，低于本仓库 12.1.2 基线与 Tabalonia/Markdown 同一档），稳定线 2.0.x 是 Avalonia 11
  构建（声明 `Avalonia >= 11.0.0`、`netstandard2.0`/`net6.0`/`net8.0`），在 Avalonia 12 下无法加载，故不选。
  headless 测试钉住：八个角色的顺序与两套变体的取值、`AddMaterialTheme` 装上的主题（调色板、工具提示/图例
  画刷与投影、画布表面色、字号）、折线/柱/堆叠/饼/仪表/散点/K 线与坐标轴的逐项规则、暗色变体只读暗色 token，
  以及「包内没有 `Styles`/`Style`/`ControlTheme`、公开类型只有 `MaterialLiveChartsTheme`」。
- **`Md3.Avalonia.ScottPlot`** —— [ScottPlot](https://github.com/ScottPlot/ScottPlot) 图表的
  Material Design 3 主题，opt-in，唯一引入 `ScottPlot.Avalonia` 依赖的包，包总数到十三个。
  与 LiveCharts2 包同属「不是 Avalonia 样式集合」的形态：ScottPlot 的 `Plot` 自己保存颜色与字体，
  包内没有 `Styles` 或 `ControlTheme`，公开面是绘图对象级入口 `MaterialScottPlotTheme`——
  `Follow(avaPlot)` 按控件当前变体应用规则，并订阅控件自身的 `ActualThemeVariantChanged` 在明暗切换时重放；
  离屏绘图对象用 `Apply(plot, dark)` 或实例 `Apply`。
  映射：图形背景与数据区背景同为 `Surface`；`Plot.Axes.Color` 一次把边框、刻度线、刻度标签、轴名与标题
  刷成 `OnSurfaceVariant`；网格主刻度 `OutlineVariant`、次网格关闭；图例是 `SurfaceContainerHigh` 面板配
  `OutlineVariant` 描边与 `OnSurfaceVariant` 文字；字阶为标题 `TitleMedium`、轴名与图例 `LabelLarge`、
  刻度标签 `LabelMedium`，字族取自 `Md.Sys.Typeface.Plain`；系列走与 LiveCharts2 相同的八个角色调色板
  （经 `Plot.Add.Palette`，之后添加的 plottable 都沿用）。ScottPlot 的边框、刻度生成、布局与交互保持原样，
  未被处理的绘图对象保持默认外观，进程级状态（`Fonts.Default` 等）不被改写；未注册 `MaterialTheme` 时
  回退到包内自带的 M3 基线取值。核心包的 `MdChart` 保持与绘图引擎无关。
  版本固定 **`5.1.59`**（`net8.0`/`net10.0`，声明 `Avalonia >= 12.0.0` 与 `ScottPlot >= 5.1.59`，
  低于本仓库 12.1.2 基线）。headless 测试钉住：八个角色的顺序与两套变体取值、表面/墨色/网格/图例/字阶规则、
  暗色变体、`Follow` 跟随应用明暗、未被处理的绘图对象保持默认值且不改写 `Fonts.Default`、每个角色都有与核心
  token 一致的 M3 基线取值，以及「包内没有 `Styles`/`ControlTheme`、公开类型只有 `MaterialScottPlotTheme`」。
- **`Md3.Avalonia.PropertyGrid`** —— [Avalonia.PropertyGrid](https://github.com/bodong1987/Avalonia.PropertyGrid)
  属性网格的 Material Design 3 主题，opt-in，唯一引入 `bodong.Avalonia.PropertyGrid` 依赖的包，
  包总数到十四个，也是 Wave 3 的第一个。网格自身不设置任何颜色：它的 604 行内嵌样式只负责模板布局，
  单元格编辑器全部是运行期创建的原生 Avalonia 控件，因此在没有原生控件主题的应用里它只画得出外壳、画不出单元格。
  `MaterialPropertyGridTheme`（普通 `Styles` 集合，注册在 `MaterialTheme` 之后）用 32 个
  `Md.Comp.PropertyGrid.*` 键控控件主题补齐：每个单元格外的透传容器、40 DIP 的 Material 填充字段
  （`SurfaceContainerHighest` 填充、`Outline` 细线、聚焦时 2 DIP `Primary`、校验失败 `Error`）、按钮、
  筛选标签块、复选框、单选按钮、组合框及其条目、数值字段与微调器、滑块与滑块柄、进度条、日期/时间字段及其
  滚轮面板、分类展开器、名称/值分隔条、滚动器、上下文菜单与其条目和分隔线、工具提示。颜色全部走
  `Md.Sys.Color.*`、字号走 `Md.Sys.TypeScale.*`、圆角走 `Md.Sys.Shape.Corner.*`，属性列表滚动在核心的
  `MdScrollBar` 上。局部性与 AvaloniaEdit 包同构：主题都不按原生类型注册，而是由「选择器要求存在
  `PropertyGrid` 祖先」的样式逐个下发，所以注册它不会替换应用其他位置的任何原生控件主题。
  三处仍属上游：其模板与可展开对象工厂作为本地值设置的 0.5 DIP 灰色细线（样式覆盖不到，本包也不复刻上游
  11 个内嵌样式以免与 part 名漂移）、颜色单元格沿用的 Avalonia `ColorPicker`，以及日期/时间浮层保留的
  滚轮机制。版本固定 **`12.1.1.1`**（`net8.0`/`net9.0`/`net10.0`，声明 `Avalonia >= 12.1.1`、
  `Avalonia.Controls.ColorPicker >= 12.1.1` 与 `bodong.PropertyModels >= 12.0.0`）。
  headless 测试钉住：单元格与外壳拿到的主题、包内所有控件主题都是键控而非隐式、网格外的控件仍然没有主题、
  日期浮层的必需 part 与确认回写、选项菜单，以及网格的编辑、筛选与写回行为未变。
- **`Md3.Avalonia.NodifyM`** —— [NodifyM.Avalonia](https://github.com/Maklith/NodifyM.Avalonia)
  节点编辑器的 Material Design 3 主题，opt-in，唯一引入 `NodifyM.Avalonia` 依赖的包，包总数到十五个。
  上游把「模板读取的画刷」和「给控件下发样式的十一个样式表」放在同一个 `Styles/ControlStyles.axaml` 里，
  而 `MaterialNodifyMTheme` 把这两半拆开：十一个单控件样式表原样 `StyleInclude`，本主题自己的资源里
  先合并上游的数据模板与画刷字典、再合并我们的 `Themes/Accents/Material.axaml`（把 44 个画刷中的 43 个
  重新指向 `Md.Sys.Color.*`）。字典按「后合并者优先」查找，所以配色字典替上游应答这些键，而模板仍然
  读取它们原本写的那些键；明暗两套变体都经由 `Md.Sys.Color.*` 解析，因此字典自身不带
  `ThemeDictionaries`。画布是 `Surface` 并以 `OutlineVariant` 画网格线；节点是 `SurfaceContainerHigh`
  卡片，标题栏 `SecondaryContainer`、页脚 `SurfaceContainer`；输入引脚是 `SecondaryContainer`、
  输出引脚是 `PrimaryContainer`，一根连线的两端因此无需标签即可区分；连线走 `OnSurfaceVariant`，
  circuit 变体与拖拽中的 pending 连线走 `Primary`；框选、对齐参考线与选中边框共用同一个 `Primary`。
  上游以字面量设置、配色字典够不到的取值由附加样式重设：节点的 `White` 前景、悬停时的 `LightBlue`
  与选中时的 `Orange` 边框、条目背后的 `Azure`、分组节点内联的四种颜色，以及网格线属性自带的
  `Gainsboro`。节点模板里写死的 3 DIP 圆角留给上游。`MdNodifyMThemeTests` 从真正实例化的控件上把画刷
  读回来核对（画布、节点、两个方向的引脚、pending 连线、网格线、小地图六个画刷、框选矩形），再钉住
  选中边框、变体切换，以及同一窗口内的普通控件仍然没有主题。
- **原生 AOT 已验证可用。** 十五个包全部声明 `IsAotCompatible`，在原有 IL2xxx 裁剪分析器之外打开了
  IL3xxx 分析器。`.github/workflows/aot-probe.yml` 用 `PublishAot=true` 发布桌面 Gallery，并且
  **不把「步骤成功」当作答案**：它断言产物是原生 ELF 可执行文件、输出目录里没有托管 dll 残留（防止
  apphost 冒充），然后在 xvfb 下把那个二进制真正跑起来。原生 AOT 是在运行期因反射元数据被裁而失败的，
  不是在编译期，所以最后这一步才是结论的承重点。
  未覆盖：Windows / macOS 发布，以及 Android（运行时模型不同）。
- `MdBreadcrumb` 新增 `Powerline` 变体。
- [`docs/DIALOG_SERVICE.md`](DIALOG_SERVICE.md)：如何通过 `IMdDialogService` 在 ViewModel 中弹出
  对话框，含组合根接线。

## Gallery

- Usage 区不再显示占位代码。普查发现 32 个页面只有 XAML、7 个只有 C#，缺失的那一页会渲染成一个空框；
  现在缺失的标签直接隐藏。
- 480 DIP 窗口下会横向溢出的 34 个页面中，29 个已能自适应（约 50 处固定 `Width` 改为 `MaxWidth`）。
  剩余 5 个在测试中用**双向断言**钉住——既不会被悄悄遗忘，回流后也不会被悄悄留在清单里。
- 本地化表新增 366 条中文词条。

## 测试

渲染后的像素现在真的被读了。`UseHeadlessDrawing=false` 意味着这些测试跑的是真实 Skia，但此前三十多处
`CaptureRenderedFrame()` 调用**无一例外只断言了「帧不为 null」**。圆角类缺陷曾据此被判为「headless 不可
观测」——这个判断是错的，上面那个无边框窗口的缺陷正是这样找到的。

## 升级

无破坏性改动，直接改版本号即可：

```xml
<PackageReference Include="Md3.Avalonia" Version="0.4.1-preview.1" />
```

如果你显式依赖 carousel 的 item 宽度（例如写死了「第二项是中号」这类假设），注意宽窗口下现在会铺出更多
大项；窄窗口下的结果与 0.4.0-preview.1 完全一致。
