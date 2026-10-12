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
- **原生 AOT 已验证可用。** 十一个包全部声明 `IsAotCompatible`，在原有 IL2xxx 裁剪分析器之外打开了
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
