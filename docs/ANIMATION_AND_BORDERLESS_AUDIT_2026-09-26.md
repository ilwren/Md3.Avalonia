# 动画缺口与无边框窗口差距审计

日期：2026-09-26  
范围：`Md3.Avalonia`、`Md3.Avalonia.Ecosystem`、Gallery、现有 Headless tests；对照当前 Material 3 Motion 指南、Flutter `master` 中相关 Material/Widgets 源码，以及 FluentAvalonia `master` 的 AppWindow/Win32 windowing 源码。

> 本文是只读分析，不把尚未在桌面或 Android 真机验证的行为标为完成。

## 1. 结论摘要

### 1.1 目前不是“少数控件漏了动画”，而是缺少统一的组件动画运行层

项目已经有：

- `MdMotionScheme`（Expressive、Standard、Reduced、None）；
- 一组 spring 参数；
- pointer-origin ripple；
- indeterminate progress/loading 的逐帧绘制；
- Checkbox、Radio、Switch、TextBox label、FAB menu、Sheet、Tooltip、TabView 等少量局部 Transition；
- Ecosystem skeleton pulse 和简单 opacity sequence。

但它们没有形成统一的组件 motion system：

1. 静态主题清点中，Core 的 134 个 `ControlTheme` 只有 22 个声明了至少一个 Avalonia Transition；Ecosystem 的 38 个主题只有 1 个声明了 Transition。这个数字包含无需动画的内部主题，不能直接等同于 149 个缺陷，但足以说明大多数状态切换是 jump cut。
2. `MdMotionTokens` 在运行时组件中实际只被 `MdRipplePresenter`消费；Motion Gallery 自己也直接采样 spring。导航、弹层、选中指示器、尺寸、颜色和布局变化都没有走这些 token。
3. `MdMotion.Scheme` 只有 Ripple、Loading/Progress 的少量代码会读取；XAML 中硬编码的 80–250 ms Transition 不会根据 Expressive、Standard、Reduced、None 切换。
4. `MdThemeOptions.MotionScheme` 被 `MdThemeManager`保存，却没有自动应用到应用根或窗口根；Gallery 另行调用 `MdMotion.SetScheme`，普通宿主容易以为 ThemeOptions 已经生效。
5. 大多数 Transition 未显式指定 Material easing/spring；它们只具有“有时长”，不等于 Material choreography。
6. 多个弹层用 `IsVisible=False` 或直接关闭 `Popup`结束生命周期；这样即使子元素有 Transition，退出动画也会被立即从视觉树移除。
7. 现有动画测试主要断言 `Transitions` 集合里存在某种类型，或只验证起止状态；几乎没有确定时钟下的中间帧、反向、打断、re-target、Reduced Motion 和 detach/cancel 测试。

### 1.2 Material 官网与 Flutter 源码不能被当成同一套 motion 标准

当前 Material 3 官网（2025 年后）以 physics-based spring 为主：

- Expressive 与 Standard 两个 scheme；
- spatial 与 effects 两类 spring；
- fast/default/slow 三档；
- spatial 用于位置、尺寸、旋转、圆角；effects 用于颜色和 opacity；
- spring 的重要价值包括手势打断和重新定向。

但官网同时明确：当前 Flutter 尚未提供新的 M3 physics system；Flutter Material 组件源码仍大量使用 `AnimationController + Duration + Curve`。因此正确策略应是：

- 用 Material 官网定义视觉意图、motion 类型和无障碍规则；
- 用 Flutter 源码确定每个组件“哪些部位需要动、打开/关闭时序、手势结束阶段”；
- 在 Avalonia 中通过自己的 spring/effects runner 实现当前 M3 scheme；
- Legacy/Flutter duration 只能作为兼容基线，不能宣称等于最新 M3 Expressive。

## 2. 必须优先补动画的 Core 控件

优先级定义：P0 = 明显 jump cut 或破坏空间关系；P1 = 核心选择/布局反馈；P2 = 微交互或增强。

| 控件/区域 | 当前实现 | 应补 motion | Flutter/M3 对照 | 优先级 |
|---|---|---|---|---:|
| `MdSearchView` / `MdSearchBar` | Results 直接 `IsVisible`；容器尺寸不变 | Search bar → view 的 bounds/shape/width container transform；header、divider、results 分段 fade；关闭反向；Reduced 改为短 fade | M3 Expressive 明确 search focus 时 bar 变宽；Flutter `search_anchor.dart` 使用 600 ms route，并将 anchor、icons、divider、list 分段 fade | P0 |
| `MdDialogHost` / `MdSimpleDialog` | scrim 有 opacity transition，但 overlay 关闭立即隐藏；dialog surface 无 scale/transform；SimpleDialog 直接显隐 | scrim effects + surface scale/fade；open/closing 生命周期；full-screen 使用 platform forward/back 或 shared-axis，不与 basic dialog 共用动画 | M3 要求 enter/exit；Flutter dialog route 150 ms fade，inset size 100 ms | P0 |
| `MdSheetHost` | surface 只从 24px 处进入；scrim不动；关闭时 `PART_Layer` 立即隐藏；drag 回弹依赖固定 Transition | surface 从完整 extent 滑入/滑出；scrim effects；velocity/threshold settle；drag 期间 1:1、释放后 spatial spring；退出完成后才隐藏 | Flutter bottom sheet enter 250 ms、exit 200 ms；M3 transition 指南明确 bottom sheet 不应靠 cross-fade | P0 |
| `MdMenuAnchor` / submenu / ComboBox / AutoComplete / Dropdown | Popup 大多直接出现/消失；Combo 只有箭头和 label；Submenu 无进退时序 | origin-aware grow/scale/clip + opacity；menu item stagger/fade；submenu 横向来源关系；关闭延迟到动画完成；快速切换可反向 | Flutter `menu_anchor.dart` opening 500 ms、closing 150 ms，含 Fade/Size transition；popup menu 约 300 ms | P0 |
| `MdTooltipHost` | surface 有 150 ms fade/scale，但 Popup 一关闭视觉树即消失，exit 基本不可见 | host 保持 Popup 到 exit 完成；plain tooltip 短 fade，rich tooltip scale/fade；Reduced 只 fade | Flutter Tooltip exit 100 ms、FadeTransition | P0 |
| `MdSnackbar` | `IsVisible`直接开关 | height/translation + fade；队列替换和 dismiss 反向；不要只做 opacity | Flutter SnackBar 250 ms，M3 height/fade curves 分段 | P0 |
| `MdBanner` | root 直接隐藏 | 从顶部/布局边缘 slide；关闭反向并在完成后回收高度 | Flutter MaterialBanner 250 ms `SlideTransition` | P0 |
| `MdNavigationDrawer` | Drawer 有 opacity/+24px transition，但 scrim直接出现；close 立即 `IsVisible=False`；移动距离不是完整宽度 | full-width slide、scrim fade、drag/open progress、velocity settle、退出完成再卸载；RTL 原点 | Flutter Drawer 使用 controller 和约 246 ms settle | P0 |
| `MdFabMenu` | 已有 MaxHeight/opacity/scale，且代码延迟回收 220 ms | 改为 token 驱动；每个 action stagger；trigger icon rotation/shape morph；中途反向保持连续速度 | M3 FAB 可变形成菜单；当前属于“部分实现” | P1 |
| FAB / Extended FAB | 只有 ripple/state/elevation；没有出现/隐藏、FAB↔Extended、FAB→surface | 中心点 scale；icon motion；宽度、shape、label opacity 的 container transform；与 Scaffold/navigation destination 切换联动 | M3 明确 FAB 出现从中心展开、可 transition 到 extended/menu/surface；Flutter Scaffold 有 scale/rotation/fade controller | P1 |
| Navigation bar destinations | selected indicator、icon与label直接切换 | indicator width/position spatial；selected icon/label effects；目的地内容 fade-through；可配置 duration | Flutter `NavigationBar.animationDuration`，默认 destination transition 500 ms，内部 label/icon fade | P1 |
| Navigation rail | Width 96↔220 直接跳；label/layout瞬变 | rail 宽度、label opacity/position、FAB expansion、destination indicator；中断可 retarget | Flutter NavigationRail 使用 extended controller 与 FadeTransition | P1 |
| Tabs / TabItem | 指示器在旧 item 消失、新 item出现；没有跨 item slide；`MdTabView`仅 160 ms opacity | indicator bounds tween；内容 fade-through 或相邻页面 shared-axis；手势页切换进度 | Flutter TabController/TabBar 使用连续 animation；M3 tabs 应维持目的地连续性 | P1 |
| Carousel | `SelectedIndex`后 `ScrollIntoView`，无平滑滚动、snap、动态 item resize；autoplay 是跳转 | scroll physics、snap settle、large/medium/small item size morph、center/hero layout reflow、自动播放 motion；Reduced 禁用 ambient autoplay 或缩短 | M3 要求滚动时 item 自动变尺寸并 snap；现实现与 M3 carousel 最核心 motion 缺失 | P0 |
| ExpansionPanel | expand icon会旋转，但 body 直接显隐 | body height/clip/opacity；panel spacing/elevation；快速开关反向 | Flutter expansion panel 使用 `AnimatedContainer`，其内部 expansion/merge material也依赖 animation | P1 |
| Stepper | active content、indicator/icon直接切换 | `AnimatedSize`式内容高度；indicator颜色/shape/icon crossfade；connector progress | Flutter `stepper.dart` 使用 `AnimatedContainer` 和 `AnimatedSize` | P1 |
| Chips | selected/check icon、leading/trailing结构和删除直接跳 | selection 195 ms、checkmark 150/50 ms、avatar/delete drawer 150/100 ms、disable 75 ms；删除后 layout collapse | Flutter `chip.dart`有多个 controller 和 AnimatedSwitcher | P1 |
| Button groups / segmented buttons / split button | 大部分形状、选中和布局直接改变 | pressed child expansion、相邻 shape reflow、selected indicator/check/icon；split menu箭头和surface | 当前 M3 button groups依赖 color、motion、shape表达；单按钮只有 corner transition+ripple 不够覆盖 group choreography | P1 |
| Date/Range picker + Calendar | 日期页、月份、年/月/输入模式直接替换 | 月页 horizontal slide 200 ms；模式切换 dialog size；header selection crossfade；范围填充连续扩展 | Flutter `calendar_date_picker.dart` month 200 ms；date picker dialog size 200 ms | P1 |
| TimePicker / TimeDial | 手/数字状态直接更新，模式切换直接跳 | dial hand 200 ms、hour/minute crossfade、dialog size 200 ms、AM/PM effects | Flutter `_kDialAnimateDuration=200ms`、dialog size 200 ms | P1 |
| RefreshIndicator | PullOffset跟手，但 armed→refreshing 与完成消失直接换控件/高度 | pull跟手不做 lag；释放 snap 150 ms；完成 scale out 200 ms；spinner crossfade；异步取消时反向 | Flutter position controller + Size/Scale transition；API说明 indicator fade in/out | P1 |
| ReorderableList | 被拖 item直接 translate；其他项目不让位；无 proxy lift/settle | drag proxy elevation/scale；gap/items 250 ms reflow；drop settle；edge auto-scroll；Reduced 保留位置反馈 | Flutter widgets reorderable list 有 controller 和约 250 ms item animation | P1 |
| Dismissible | 非拖动时已有 200 ms transform/opacity；无 resize-collapse | fling/velocity settle、confirm等待态、dismiss 后约 300 ms size collapse；取消回弹 | Flutter `dismissible.dart` slide 200 ms、resize 300 ms | P1 |
| Slider / RangeSlider | pointer跟手正确，但 hover/pressed overlay、value indicator、keyboard/programmatic position大多跳变 | drag阶段1:1；keyboard/programmatic value用75 ms spatial；overlay/value label 75–100 ms effects；Range 双 thumb独立 | Flutter Slider/RangeSlider 有 overlay、value indicator、enable和position controller | P1 |
| TextBox / Combo field decoration | label transform已有150 ms；其他 outline、error/supporting/hint结构多为跳变 | label/effects统一167 ms语义；hint/error crossfade；error label shake只在合适场景；supporting row size；Reduced禁用shake | Flutter InputDecorator 主 transition 167 ms，含多个 Fade/AnimatedOpacity/AnimatedSwitcher | P1 |
| Top app bar / adaptive layout | scrolled、medium/large高度和 breakpoint content 直接跳 | scroll-driven title position/size和surface tonal elevation；breakpoint换栏可短 fade/shared-axis，Reduced只fade | 属于连续布局变化，不能只切pseudo class | P2 |
| Scrollbar | thumb颜色和 auto-hide直接变化 | auto-hide 600 ms pause + 300 ms fade、hover thickness/brush effects；drag保持1:1 | Flutter scrollbar源码包含 fade controllers | P2 |
| DataTable/Paginated table | selection/state snap；Core表头无真实sort arrow motion | sort arrow rotation+fade 150 ms；page row replace fade-through；loading占位避免layout shift | Flutter DataTable sort arrow有两个 controller、150 ms | P2 |
| Progress indicators | indeterminate已有逐帧动画；determinate value直接变化 | determinate value插值、M3 expressive wavy/shape；Reduced 保留进度但去掉装饰波动 | M3明确进度组件依赖motion；现状属于部分完成 | P2 |
| Checkbox/Radio/Switch | 已有主要 glyph/thumb transition | 接入统一 scheme/easing与Reduced；程序化反向/快速切换连续；Switch drag velocity | 不需要重写，属于基础设施迁移 | P2 |

### 通常不需要强行加结构动画的 Core 控件

`MdDivider`、纯 `MdSurface`、静态 `MdText`、只展示内容的普通 Toolbar/List 容器不应为了“看起来有动画”而运动。它们只需要共享的 hover/focus/pressed effects，以及在宿主执行页面/布局 transition 时参与整体动画。Badge 数字变化可以做短 scale/crossfade，但不是比 Search/Sheet/Navigation 更高的基线缺陷。

## 3. Ecosystem 需要补的动画

Ecosystem 38 个 `ControlTheme` 中只有 PIN cell 声明了 brush transition。Skeleton 和 `MdAnimationSequence` 在 C# 中有动画，但其余大多数状态是直接替换。

| 控件 | 当前缺口 | 建议 | 优先级 |
|---|---|---|---:|
| Popover / HoverCard | Popup直接出现/关闭 | origin scale+fade；关闭延迟；hover进入退出定时与animation取消统一 | P0 |
| CommandPalette | backdrop `IsVisible` jump；surface不动 | backdrop effects + surface scale/translate；结果列表 clean fade；打开焦点与关闭动画并行 | P0 |
| SlidableItem | drag后 Offset直接跳到0或ActionExtent | drag跟手；释放 velocity spring；action pane icon按进度scale；关闭可反向 | P1 |
| AsyncSelect / Cascader | popup和列路径直接变化 | menu enter/exit；列 forward/back shared-axis；loading/result clean fade | P1 |
| Calendar | 整个42日集合直接替换 | 月份方向 slide；selection/range effects；禁用日期不做过度motion | P1 |
| TreeView | expand/collapse子树直接增删 | height/clip/opacity；disclosure旋转；异步加载占位；diff insert/remove | P1 |
| Transfer | 移动项目瞬间从一列消失并出现在另一列 | source removal + target insertion；数量badge；drag/drop proxy；Reduced只做短fade | P1 |
| Chat | insert/delete/status/quote/selection工具栏全跳变 | 新消息enter、删除collapse、失败→retry状态crossfade、selection toolbar、typing indicator；历史prepend保持scroll anchor | P1 |
| DataGrid | sort/filter/edit refresh整表跳变 | sort arrow、行insert/remove/reorder、editor enter/exit；不要给滚动或pointer resize加lag | P2 |
| Timeline | Active/state颜色直接切换 | indicator color/scale effects、connector progress、追加item enter | P2 |
| TagInput | tag增删和suggestion popup jump | chip insert/remove+wrap layout animation；suggestions menu；错误信息size/fade | P2 |
| PIN | brush已有150 ms | active/error/filled glyph scale、paste fill stagger；遵守IME与Reduced | P2 |
| Chart |数据重绘 jump | point/bar/path tween，tooltip/selection effects；大数据下可关闭；必须有可访问文本替代 | P2 |
| Rating | fill直接跳 | keyboard/programmatic短tween；pointer drag保持1:1；commit时轻scale | P3 |
| Breadcrumb/Result/PagedItems |状态直接替换 | loading/error/data clean fade、breadcrumb overflow menu；避免加载导致布局抖动 | P3 |
| Skeleton / AnimationSequence |已有基础动画但不读统一 scheme | 接入 `MdMotion.Scheme`和系统Reduced；修正取消/重入、detach生命周期 | P1（基础设施） |

## 4. 推荐的动画基础设施顺序

不要直接在几十个 AXAML 文件里继续复制 `Duration="0:0:0.2"`。先补统一运行层，否则会得到更多不一致、不能Reduced、退出被切断的动画。

### M0：统一 motion runtime

1. `MdMotionContext`：解析继承的 scheme、系统 reduced-animation、应用级覆盖和测试clock。
2. `MdMotionSpec`：`Spatial/Effects × Fast/Default/Slow`，同时提供 legacy transition spec。
3. 可打断 spring runner：保存 current value、velocity、target；重定向时不中断连续性。
4. effects runner：opacity/color/shadow无overshoot；spatial runner：offset/size/rotation/corner可overshoot。
5. lifecycle state：`Closed → Opening → Open → Closing`；动画完成前不把Popup/overlay从视觉树移除。
6. pointer phase与settle phase分离：drag/slider/pull必须1:1，不得让Transition滞后；release才启动spring。
7. `MdThemeManager.Apply`真正传播MotionScheme，且与OS reduced-motion合并。

### M1：先修 P0 弹层和导航

Search、Dialog、Sheet、Menu/Combo/Tooltip、Snackbar/Banner、NavigationDrawer、Popover、CommandPalette。

### M2：再修选择与布局

NavigationBar/Rail/Tabs、Carousel、Expansion/Stepper、Chips、Date/Time、TextField。

### M3：再修手势 settle 与 Ecosystem

Refresh、Reorder、Dismissible、Slider、Slidable、Tree、Transfer、Chat、Grid、Timeline、Tag、Chart。

### M4：测试标准

每个动画至少验证：

- 起点、25/50/75%中间帧、终点；
- open过程中立即close，以及close过程中立即open；
- pointer跟手时无Transition lag；
- detach/re-template/cancel后无timer、task或handler泄漏；
- Expressive与Standard轨迹不同；Reduced只保留必要反馈；None立即到终态；
- 60/120Hz不依赖固定tick计数；
- Light/Dark、RTL、DPI、Android触摸；
- Popup必须在真实平台host验证，不用Headless伪装。

## 5. 无边框窗口为何没有达到 FluentAvalonia AppWindow 的效果

### 5.1 根因：当前是“控件级自绘标题栏工具箱”，不是“平台窗口集成”

`MdBorderlessWindow`目前主要完成了API外壳：WindowDecorations、ExtendClientArea、BeginMoveDrag、BeginResizeDrag、几个命令和伪类。Windows/macOS/Linux adapter 都是空派生类，实际逻辑仍来自同一个 `MdAvaloniaWindowPlatformAdapter`。

FluentAvalonia AppWindow 则是完整windowing vertical slice：

- Windows专用初始化；
- 自己的Window template、root border、default title bar、content presenter、content margin；
- TitleBar颜色资源；
- Win32 WndProc hook；
- native system menu、Alt+Space/right click；
- DWM theme/border属性；
- taskbar progress；
- splash lifecycle；
- dialog/full-screen状态；
- 其他平台graceful fallback。

因此当前两者根本不在同一完成层级。

### 5.2 当前实现中的具体断点

1. **`MdBorderlessWindow`没有自己的默认ControlTemplate。** 主题只是BasedOn `MdWindow`。`TemplateSettings.ContentMargin`、`IsMaximized`等计算值没有被窗口主题消费。
2. **Gallery的“四角、clip、outline”是页面代码临时拼出来的。** 它不是 `MdBorderlessWindow`默认能力；其他用户创建该窗口不会自动得到同样效果。
3. **TitleBar不是窗口自动组成的一部分。** 用户必须手工放置 `MdWindowTitleBar`；Window的Title/Icon、caption visibility也没有默认绑定过去。
4. **窗口和TitleBar各自有一套ShowMinimize/Maximize/Close属性。** 当前没有默认template把它们同步。
5. **TemplateSettings.TitleBarHeight只在TitleBar attached时读取一次。** 后续动态改变窗口TitleBarHeight不会自动更新现有TitleBar高度。
6. **Windows adapter没有Win32实现。** `MdWindowsWindowPlatformAdapter`是空类；`TryShowSystemMenu`始终false。Alt+Space、标题栏右键、菜单项enable状态、native close/move/size都依赖宿主fallback。
7. **Caption buttons是普通Avalonia Button。** 最大化按钮不能提供Windows 11原生Snap Layout hover，缺少native non-client hit-test语义；maximize后图标也没有切到restore glyph。
8. **没有DWM/系统主题集成。** 没有immersive dark titlebar、border color、corner preference、Mica/Acrylic选择或系统变化重应用。
9. **resize不是完整默认体验。** 只有用户手工放置的单个 `MdWindowResizeGrip`；没有默认8方向edge/corner区域。当PreserveNativeBorder失效或关闭时，窗口只剩手工grip。
10. **hard-coded geometry。** TitleBar 40、caption width 46，没有left/right system inset、DPI、平台caption布局、安全区或macOS traffic-light适配。
11. **active/inactive/pressed状态不完整。** caption仅有hover和close-hover，缺pressed、inactive button resources；Window maximize/fullscreen/dialog状态没有完整视觉模板。
12. **命中测试是控件树启发式。** 以Focusable/TextBox/SelectingItemsControl判断，不等于平台drag rect/non-client hit-test；复杂Tab strip、菜单、拖拽空白区容易出现差异。
13. **平台类名不等于平台能力。** macOS/Linux类目前也没有traffic lights、native menu/drag region、X11/Wayland差异处理。
14. **文档状态过度乐观。** `BORDERLESS_WINDOW_PLAN.md`写W0–W3 complete，但真实平台release matrix尚未签字，且源代码仍缺上述默认template和native Windows路径；更准确的状态应是“contract + generic fallback prototype complete”。

### 5.3 与 FluentAvalonia 源码的关键差异

- Fluent `FAAppWindow`在Windows启用专用template，并用 `WindowDecorationProperties.ElementRole="TitleBar"`标记原生拖拽区域；当前实现只在pointer event里调用`BeginMoveDrag`。
- Fluent `Win32WindowManager`替换/链式调用WndProc，处理右键系统菜单、`WM_SYSCOMMAND`和Alt+Space，并根据maximized/dialog状态启用或禁用系统菜单项；当前adapter直接返回false。
- Fluent强制重应用Win32 window theme，并通过DWM设置Windows 11 border color；当前没有平台interop。
- Fluent把native frame/caption能力与managed title content结合；当前使用自绘caption button模拟全部caption行为，所以snap、system menu、DPI/inset等自然有差距。
- Fluent template自己处理titlebar/content margin和active/inactive资源；当前这些工作落在Gallery或宿主上。

## 6. 无边框窗口建议实施顺序

### W0：先修正目标和文档

把目标拆成：

- Cross-platform generic custom chrome；
- Windows native-enhanced chrome；
- macOS/Linux adapter；
- Material视觉层。

不要再用一个“complete”覆盖四者。

### W1：先让窗口自包含

- 为 `MdBorderlessWindow`提供默认template：RootBorder、TitleBar、ContentPresenter、8方向resize regions。
- 实际消费TemplateSettings；maximized/fullscreen时正确margin、clip、corner。
- 自动绑定Title/Icon/CanMinimize/CanMaximize/CanResize/Show*。
- restore/maximize glyph和automation name随WindowState变化。
- 移除Gallery专属四角补丁，把它变成control能力。

### W2：使用Avalonia 12原生window-decoration角色

- 标记TitleBar和resize edge/corner角色；
- interactive区域标记为user/decorations content；
- 保留BeginMoveDrag/BeginResizeDrag作为平台fallback，而不是唯一机制。

### W3：Windows native adapter

- 获取HWND并在lifecycle中安全安装/卸载hook；
- native system menu：right-click、Alt+Space、状态enable；
- Windows 11 maximize/Snap Layout兼容：优先保留native caption hit-test，或实现正确`HTMAXBUTTON`路径；
- DWM immersive dark、border color、corner preference、可选Mica/Acrylic；
- DPI physical/logical坐标和monitor work area；
- active/theme/system color变化时重应用；
- 保持系统minimize/maximize/restore动画和shadow。

### W4：其他平台与真实验收

- macOS：traffic-light safe area、fullscreen、drag region；
- Linux：X11/Wayland和不同WM fallback；
- Windows 10/11、100/125/150/200% DPI、多屏、负坐标、snap、Alt+Space、Win+Arrow、触屏；
- headless只测试模板、命令和几何，不宣称验证compositor/native menu。

## 7. 最终判断

当前组件库的“motion token”和少量样板动画已经搭好，但绝大多数组件仍然是**状态正确、过程缺失**。首要工作不是给每个Border随手加一个200 ms Transition，而是先实现可继承、可Reduced、可打断、可反向、能维持Popup退出生命周期的统一motion runtime。

无边框窗口没有达到FluentAvalonia同等效果，也不是配色或圆角微调问题，而是当前只完成了managed control层；FluentAvalonia还完成了window template、native frame/caption协作和Win32 integration层。只继续调整Gallery中的Border/CornerRadius不会补齐这层差距。

## 8. 主要外部依据

- Material 3 Motion physics：https://m3.material.io/styles/motion/overview/how-it-works
- Material easing/duration：https://m3.material.io/styles/motion/easing-and-duration/tokens-specs
- Material transitions/reduced motion：https://m3.material.io/styles/motion/transitions/applying-transitions
- Material Search：https://m3.material.io/components/search
- Material FAB：https://m3.material.io/components/floating-action-button/guidelines
- Material Carousel：https://m3.material.io/components/carousel/guidelines
- Material Dialogs：https://m3.material.io/components/dialogs/guidelines
- Flutter source（本次逐文件检查）：
  - https://github.com/flutter/flutter/blob/master/packages/flutter/lib/src/material/search_anchor.dart
  - https://github.com/flutter/flutter/blob/master/packages/flutter/lib/src/material/chip.dart
  - https://github.com/flutter/flutter/blob/master/packages/flutter/lib/src/material/navigation_bar.dart
  - https://github.com/flutter/flutter/blob/master/packages/flutter/lib/src/material/navigation_drawer.dart
  - https://github.com/flutter/flutter/blob/master/packages/flutter/lib/src/material/navigation_rail.dart
  - https://github.com/flutter/flutter/blob/master/packages/flutter/lib/src/material/bottom_sheet.dart
  - https://github.com/flutter/flutter/blob/master/packages/flutter/lib/src/material/dialog.dart
  - https://github.com/flutter/flutter/blob/master/packages/flutter/lib/src/material/menu_anchor.dart
  - https://github.com/flutter/flutter/blob/master/packages/flutter/lib/src/material/refresh_indicator.dart
  - https://github.com/flutter/flutter/blob/master/packages/flutter/lib/src/material/input_decorator.dart
  - https://github.com/flutter/flutter/blob/master/packages/flutter/lib/src/material/snack_bar.dart
  - https://github.com/flutter/flutter/blob/master/packages/flutter/lib/src/widgets/dismissible.dart
  - https://github.com/flutter/flutter/blob/master/packages/flutter/lib/src/widgets/reorderable_list.dart
- FluentAvalonia windowing源码：
  - https://github.com/amwx/FluentAvalonia/tree/master/src/FluentAvalonia/UI/Windowing
  - https://github.com/amwx/FluentAvalonia/blob/master/src/FluentAvalonia/UI/Windowing/AppWindow/FAAppWindow.cs
  - https://github.com/amwx/FluentAvalonia/blob/master/src/FluentAvalonia/UI/Windowing/Win32/Win32WindowManager.cs
  - https://github.com/amwx/FluentAvalonia/blob/master/src/FluentAvalonia/UI/Windowing/Win32/Win32AppWindowFeatures.cs
  - https://github.com/amwx/FluentAvalonia/blob/master/src/FluentAvalonia/Styling/ControlThemes/FAControls/AppWindowStyles.axaml
- Avalonia 12 window management：https://docs.avaloniaui.net/docs/app-development/window-management
