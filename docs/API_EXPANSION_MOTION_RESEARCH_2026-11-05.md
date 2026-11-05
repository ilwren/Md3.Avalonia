# 已有控件 API 扩充与复杂动效补充调研（2026-11-05）

本文件回答两个问题：**一、现有控件哪些值得扩充 API，补什么；二、还可以补充哪些复杂效果的动画。** 参照系是 Flutter 官方 API、Flutter 第三方生态（pub.dev）与 Material Design 官方设计（m3.material.io / M3 Expressive / AndroidX tokens）。

数据日期 **2026-11-05**。差距判定以仓库现状为准（`docs/FLUTTER_PARITY_STATUS.md` 的「已知边界」列、`README.md` Known limitations、`docs/API.md` 的 API 面），对照物为 api.flutter.dev 公开文档、pub.dev 包页与 Material motion 官方指南；均为文档级调研，不含任何第三方源码。

---

## 1. 结论速览

- **API 扩充 20 项**：其中 10 项直接补 `FLUTTER_PARITY_STATUS.md` 明示的边界（§3 A1），10 项是对照 Flutter/官方组件后的体验增强（§3 A2）。全部是既有 `Md*` 控件的属性/事件/模板槽位追加，不需要新控件。
- **复杂动效 12 项**，分四层（§4）：
  1. **补齐 M3 四大转场的已知边界**（退出 cross-fade、elevation morph、中断重定向、Hero 弧线）；
  2. **M3 Expressive 官方能力**：形状 morphing（squircle/波浪形形状库）、emphasized 缓动曲线与短/中/长时长档 token（现只有弹簧 token）、速度续接；
  3. **滚动联动物理动效**：collapsing app bar、stretch 过滚回弹、视差、滚动驱动 reveal；
  4. **布局/内容动画原语**：`MdAnimatedList`、`MdAnimatedSwitcher`、效果链 API（flutter_animate 形态）、`MdAnimatedNumber`、粒子庆祝、主题切换过渡。
- Lottie/Rive 类播放引擎仍按「有第三方控件就做 Material 美化」原则走 vendor 主题包路线（§5），不进自研清单。

---

## 2. 判定依据

| 依据 | 内容 |
|---|---|
| `docs/FLUTTER_PARITY_STATUS.md` | 每个控件的「已知边界」列是最直接的 API 扩充来源（如 `MdDismissible`「键盘操作和通知覆盖仍有限」、`MdPaginatedDataTable`「没有 Flutter 的完整列/单元格/表头排序模型」） |
| `README.md` Known limitations | Motion 段明示「Control content does not cross-fade on exit」——四大转场的退出相是已知缺口 |
| api.flutter.dev | `Dismissible`/`Hero`/`ReorderableList`/`Stepper`/`DraggableScrollableSheet`/`PaginatedDataTable` 等构造函数与属性表（2026-11 复核） |
| Material motion / M3 Expressive | 弹簧物理为主模型、~35 个 morph 形状（squircle、scallop、burst…）、emphasized/expressive 缓动 cubic-bezier、short1-4/medium1-4/long1-4 时长档 |
| pub.dev | flutter_animate 4.5.2（4.05k likes，BSD-3-Clause）等效果链/动画生态 |

实现纪律不变：独立 `Md*` 类型 + scoped `ControlTheme`、token 驱动、Reduced/None 动效回退、AOT/裁剪、Gallery + 双语、clean-room 与 `THIRD-PARTY-NOTICES.md`/`spec-snapshot/manifest.json` 登记。

---

## 3. Part A —— 已有控件的 API 扩充

### A1. 直接补 parity 表明示的边界

| # | 控件 | 现有 API 面 | 建议追加 | 对照来源 |
|---|---|---|---|---|
| 1 | `MdDismissible` | `BackgroundContent`/`SecondaryBackgroundContent`、`Direction`（横三值）、单一 `DismissThreshold`、`ConfirmDismissAsync`、`DragOffset`、`Dismiss/DismissAsync/Restore` | ① `DismissThresholds` 按方向映射；② 垂直方向（`Up`/`Down`/`Vertical`）；③ `ResizeDuration` + 折叠期回调（Flutter `onResize`）；④ `UpdateOffset` 拖动进度回调（Flutter `onUpdate`）；⑤ `CrossAxisEndOffset`；⑥ `MovementDuration` 可调；⑦ 键盘删除/撤销通路 | parity：「键盘操作和通知覆盖仍有限」；Flutter `Dismissible` 构造函数（`resizeDuration`/`dismissThresholds`/`movementDuration`/`crossAxisEndOffset`/`onUpdate`/`onResize`） |
| 2 | `MdReorderableList` | `ReorderCommand`、`IsReorderEnabled`、`BuildDefaultDragHandles`、Started/Requested/Completed 事件 | ① `ProxyDecorator` 拖拽代理装饰（Flutter `ReorderItemProxyDecorator(child, index, animation)`，动画 0→1 抬起/落下）；② `ReorderAnimationDuration`；③ item 级禁拖与 `DragBoundaryProvider`；④ `AutoScrollerVelocityScalar` | parity：「不复刻 Flutter 的全部 delegate 与 drag decorator API」——drag decorator 正是缺口 |
| 3 | `MdPaginatedDataTable` | `Header`、`ItemsSource`、`ItemTemplate`、`PageIndex`、`RowsPerPage`、`AvailableRowsPerPage`、`TotalItemCount`、`IsLoading`、`PageChanged` | 列模型 `Columns`（`MdDataColumn`: Label/Tooltip/IsNumeric/OnSort）、`SortColumnIndex`/`SortAscending`/排序事件、行选择（`OnSelectAll`/`ShowCheckboxColumn`/`SelectedRows`）、`Actions` 槽位、`ShowFirstLastButtons`、行高/列间距 token 化 | parity：「API 骨架……没有 Flutter 的完整列/单元格/表头排序模型」；Flutter `PaginatedDataTable` 的 `DataColumn`/`sortColumnIndex`/`actions`/`showFirstLastButtons` |
| 4 | `MdHero` | `IsTransitionEnabled`、`TransitionRequested`/`TransitionCompleted` | ① `CreateRectTween`（含 Material 风格弧线 `RectTween`——Flutter `MaterialRectArcTween`）；② `FlightShuttleBuilder` 自定义飞行内容；③ `PlaceholderBuilder` 起飞占位；④ `TransitionOnUserGestures` 手势跟手转场 | parity：「未验证多窗口、全部剪裁/阴影情形」；Flutter `Hero` 构造函数四参数 |
| 5 | `MdStepper` | `ActiveStep`、`Orientation`、`IsLinear`、`ContinueContent`/`CancelContent`+Command、三事件 | ① `ControlsBuilder`（每步操作区模板，含 details）；② `StepIconBuilder` + 图标尺寸/margin；③ `ConnectorColor`/`ConnectorThickness`；④ 水平布局内容区增强；⑤ 步骤切换转场时序 API | parity：「不承诺 Flutter `controlsStep`/`currentStep` 的动画时序一致；水平内容布局更简化」；Flutter `Stepper` 的 `controlsBuilder`/`stepIconBuilder`/`connectorColor`/`connectorThickness` |
| 6 | `MdDraggableScrollableSheet` | `MinimumExtent`/`InitialExtent`/`MaximumExtent`/`Extent`、`Snap`/`SnapSizes`、`Expand`、`Header`、`ExtentChanged`/`AnimationCompleted` | ① `Controller` 对象（`AnimateTo`/`JumpTo`/`Reset`/`IsAttached`——Flutter `DraggableScrollableController`）；② `ShouldCloseOnMinExtent`；③ `SnapAnimationDuration`；④ 内容滚动联动契约（Flutter `ScrollableWidgetBuilder` 提供的 `ScrollController` 等价物，sheet 拖动与内容滚动单手势衔接） | Flutter `DraggableScrollableSheet` 构造函数 |
| 7 | `MdContainerTransform` | `IsExpanded`、`Closed/OpenContent`、`Closed/OpenCornerRadius`、`ContainerBackground`、`Duration` | ① `ClosedElevation`/`OpenElevation` + 阴影插值（elevation morph）；② 中断重定向：反转时从当前进度继续并把释放速度交给 `MdSpatialSpringRunner`（现为重启）；③ `FadeMode`（30/70 之外补 fade-through/scale-through 配方） | parity：「未建模 elevation morph 和中断速度；反转时会重启动画而不是重定向」 |
| 8 | `MdSharedAxis`/`MdFadeThrough` | `Axis`/`Forward`/`Duration`、`MdContentPhaser` 分段淡入 | 退出相位渲染（现旧内容为 `Control` 时跳过退出）：视觉快照或双槽 presenter；`Parallax` 系数（官方 shared axis 有轻微视差） | parity：「旧内容为控件时跳过退出阶段，仅执行新内容入场」 |
| 9 | `MdAdaptiveSwitch`/`MdAdaptiveProgressIndicator` | `Platform`（Automatic/Material/Cupertino） | Cupertino 形态的绘制与动画：iOS 轨道/拇指、activity indicator 转圈 | parity：「未实现 Cupertino 的绘制与动画」 |
| 10 | `MdSimpleDialog`/`MdAboutDialog`/`MdLicensePage` | 遮罩、焦点约束、列表、取消；元数据与许可回调 | dialog 生命周期钩子（`Opened`/`Closed` 时序）、许可页滚动定位与搜索 | parity：「不是 Navigator route 的复刻」「不包含……完整 Flutter 许可流程」 |

### A2. 对照 Flutter/官方组件的体验增强

| # | 控件 | 建议追加 | 参照 |
|---|---|---|---|
| 11 | `MdCarousel` | `Orientation`（垂直滚动）、`CurrentPage`（小数页码）/`ScrollOffset` 只读（供视差）、`PageSnapping`、滚动物理钩子、item 视差/缩放系数（M3 carousel 的 item transformation：邻项按距离缩放/淡出） | Flutter `PageView`（`scrollDirection`/`physics`/`pageSnapping`/`padEnds`、`PageController.viewportFraction`/`page`）；M3 Carousel 规范的多浏览/hero/uncontained 变换 |
| 12 | `MdExpansionPanelList` | `ExpansionCallback`、`AnimationDuration`、`DividerColor`、`MaterialGapSize`、panel 级 `IsExpanded` 双向与 `CanExpand` | Flutter `ExpansionPanelList`/`ExpansionPanel` |
| 13 | `MdTabs`/`MdTabsView` | 弹性指示器微动效（M3 Expressive selection indicator）、tab 级 `Badge` 槽位、`OnTabLongPressed` 等行为钩子 | M3 Tabs 规范、Compose `TabIndicator` 弹簧 |
| 14 | `MdTreeView` | 节点拖拽重排、三态复选（`IsChecked`/`IsNullState`）、异步子节点加载、过滤+高亮 | animated_tree_view/flutter_fancy_tree_view 行为域 |
| 15 | `MdForm`/`MdFormField` | `INotifyDataErrorInfo` 校验集成、异步校验状态、字段依赖联动、`AutoValidateMode` 等价物 | Flutter `Form`/`TextFormField` 的 `validator`/`autovalidateMode` |
| 16 | `MdChatView` | 输入中指示（typing）、消息回应/回复线程、回到底部按钮、消息时间戳分组 | flutter_chat_ui 行为域 |
| 17 | `MdRating` | item builder（自定义星形/图标）、点击模式（整星/半星）、glow 动效开关 | flutter_rating_bar 的 `itemBuilder`/`tapOnlyMode`/`allowHalfRating` |
| 18 | `MdWaveform` | 流式追加峰值接口（`AppendPeaks`）、缩放/窗口滚动、选区（region） | audio_waveforms 的实时录制波形场景 |
| 19 | `MdColorPicker` | 动态色和谐化（harmonize：把外部色拉进当前 scheme——官方 dynamic color 的 color harmonization，仓库已内置 MaterialColorUtilities 可直采）、最近使用色、对比度诊断展示 | Material dynamic color 官方指南 |
| 20 | `MdCommandPalette` | 分类分组、最近/常用排序、快捷键提示列、模糊匹配分数回调 | VS Code/Raycast 类命令面板惯例、shadcn_ui 等生态 |

> 注：A1/A2 都是属性/事件/模板槽位级别的增量，遵循现有 `Md*` + scoped `ControlTheme` 机制即可，无包结构变化。

---

## 4. Part B —— 可补充的复杂动效

### B1. 先补四大转场的已知边界（官方 M3 motion 完整化）

| # | 动效 | 现状 | 补充 | 依据 |
|---|---|---|---|---|
| 1 | **退出 cross-fade** | `MdSharedAxis`/`MdFadeThrough`/`MdContainerTransform` 的退出相缺失或跳过（README Known limitations） | 旧内容淡出+位移离场与新内容入场同时进行（Material motion 的 enter/exit 双相定义）；`Control` 内容用视觉快照或双槽 presenter 解决单宿主限制 | Material motion：容器变换/共享轴/淡入淡出均定义了 exit phase |
| 2 | **Container transform 的 elevation morph + 中断重定向** | 边界与形状插值已有；elevation/阴影不插值，反转重启 | 阴影随 30/70 进度插值；中断时反向续接并传递释放速度 | Material container transform 规范含 elevation 变化；parity 边界列明示 |
| 3 | **Hero 弧线飞行** | 线性矩形插值 | `MaterialRectArcTween` 式弧线（对角飞行时先抬升再落下）+ 手势跟手 | Flutter `Hero.createRectTween` 惯例 |

### B2. M3 Expressive 官方能力（m3.material.io / AndroidX）

| # | 动效 | 说明 | 形态建议 |
|---|---|---|---|
| 4 | **形状 morphing** | M3 Expressive 的核心新能力：~35 个 morph 形状（squircle、scallop、burst…），按钮按压形状变形、loading 形状连续 morph、FAB→菜单展开变形 | `MdShapeMorph`/`MdSquircleClip`（RoundedPolygon 级多边形插值）；`MdButton`/`MdFabMenu` 按压变形；扩展现有 7 个 loading 形状路径为可 morph 序列 |
| 5 | **emphasized/expressive 缓动曲线集** | `cubic-bezier(0.05,0.7,0.1,1)`（expressive/emphasized 减速）、`cubic-bezier(0.3,0,0.8,0.15)`（加速）、standard 两条；现仓库只有弹簧 token（`MdMotionTokens` 9 个冻结弹簧），无 cubic-bezier 资源 | `MdEasing` 静态资源 + token 字典条目，供非弹簧 Transition 与 XAML 引用 |
| 6 | **短/中/长时长档 token** | short1-4（50-200ms）/medium1-4（250-400ms）/long1-4（450-600ms）官方时长档；现 `MdMotionSpeed` 仅 Fast/Default/Slow 三档（150/230/325ms 等） | 扩展 token 层为 12 档，`MdMotionSpec` 保留三档别名 |
| 7 | **速度续接（interruptible motion）** | Expressive 弹簧动效应可被手势中断并带速度续接；现 `MdSpatialSpringRunner` 只在 `MdDismissible`/`MdSlidableItem` 用 | 通用化为 `MdInterruptibleTransition`：sheet 吸附、switch 拖动、carousel 翻页、FAB 展开都把手势速度交给 spring |

### B3. 滚动联动/物理动效

| # | 动效 | 说明 | 形态建议 |
|---|---|---|---|
| 8 | **Collapsing app bar（FlexibleSpaceBar 级）** | SliverAppBar 的 pin/float/**parallax**/stretch 四模式；现 `MdTopAppBar` 静态 | `MdCollapsingTopAppBar`：滚动折叠 + 标题视差缩放 + stretch 触发回调，折叠进度走 `MdRevealHost` 式 0-1 |
| 9 | **Stretch 过滚回弹** | Android 12+ 标准 stretchy overscroll（拉伸+回弹，取代光晕）；Avalonia 原生无 | `MdScrollViewer` 过滚拉伸效果：边缘内容随拉伸缩放、释放 spring 回弹（用 Expressive spring） |
| 10 | **视差（parallax）** | carousel 邻项视差、滚动图片视差 | `MdParallaxHost` 附加行为：子元素按滚动偏移比例位移；配合 §3 #11 的 `ScrollOffset` |
| 11 | **滚动驱动 reveal** | `MdRevealHost` 现由 `Fraction` 手动驱动 | 增加滚动适配器（flutter_animate 的 adapters 概念：由 ScrollPosition 驱动动画），实现滚动进场/退场 |

### B4. 布局/内容动画原语（参考 Flutter 生态）

| # | 动效 | 参照 | 形态建议 |
|---|---|---|---|
| 12 | **列表项插入/删除/移动动画** | Flutter `AnimatedList`/`SliverAnimatedList`（`insertItem`/`removeItem` + builder 动画） | `MdAnimatedList`：条目增删时高度折叠 + 邻项位移动画（与 `MdReorderableList` 的落位动画共用布局动画引擎） |
| 13 | **内容切换器** | Flutter `AnimatedSwitcher`（key 驱动 cross-fade、`transitionBuilder`/`layoutBuilder`/`switchInCurve`） | `MdAnimatedSwitcher`：任意子内容换代时的 enter/exit 双相动画——顺带是 B1 #1 退出 cross-fade 的通用解 |
| 14 | **效果链 API** | **flutter_animate 4.5.2（4.05k likes，BSD-3-Clause）**：fade/slide/scale/rotate/blur/tint/saturate/shake/swap/flip/follow-path + `then`/`also` 时序编排 + adapters（滚动/通知驱动） | `MdAnimate` 附加行为或 builder 包装器：声明式效果列表（`Fade,Slide,Scale`…）编排到单个时间轴，全部走 `MdMotion` scheme（Reduced/None 自动回退）；blur/tint/saturate/shake 是现库没有的效果 |
| 15 | **数字滚动** | animated_flip_counter（653 likes，MIT） | `MdAnimatedNumber`：翻牌/滚动数字过渡（仪表盘、统计数据） |
| 16 | **粒子庆祝** | confetti 1.65k likes（上轮候选表 3） | `MdConfetti`：方向/重力/阻力可控的彩纸粒子，装饰族与 `MdSpinKit` 同列 |
| 17 | **波浪/液体装饰背景** | wave 包（正弦波循环背景）、液体转场类 | `MdWaveBackground`：两层贝塞尔波循环滚动（空态/庆祝背景），纯装饰、Reduced 下静止 |
| 18 | **主题/动态色切换过渡** | 官方 dynamic color 的 scheme 切换过渡 | `MdThemeManager.Apply` 增加过渡选项：token brush 在旧/新 scheme 间 cross-fade（利用 `BrushTransition`） |

### B5. 不自研、走 vendor 主题路线

| 对象 | 路线 |
|---|---|
| Lottie/Rive 矢量动画播放 | Avalonia.Labs Lottie 等第三方播放器按「五关」做 `Md3.Avalonia.*` 主题包 |
| 视频/相机/着色器特效 | 平台/引擎能力，非控件库范畴；shader 类效果（flutter_animate 的 ShaderEffect）待 Skia 层面需求明确再评估 |

---

## 5. 优先级建议

| 优先 | 项 | 理由 |
|---|---|---|
| 高 | B1 #1 退出 cross-fade、#2 elevation morph/中断重定向 | 直接消掉 README Known limitations 与 parity 边界，是声明完整性的缺口 |
| 高 | A1 #1 `MdDismissible` 阈值/方向/回调、#4 `MdHero` 三件套 | 面向使用方的高频 API 缺口，改动局部、可测 |
| 高 | B2 #5/#6 缓动曲线与时长档 token | 纯 token 层增量，是其他所有动效的公共依赖 |
| 中 | B4 #12 `MdAnimatedList`、#13 `MdAnimatedSwitcher`、#14 效果链 | 新原语，收益面广；效果链需先定 clean-room 边界（flutter_animate 为行为参考，登记 notices） |
| 中 | B3 #8 collapsing app bar、#9 stretch 过滚 | 桌面/移动都受益；工作量中等 |
| 中 | A1 #3 `MdPaginatedDataTable` 列模型、#6 sheet controller | 边界明示的 API 骨架补全 |
| 低 | B2 #4 形状 morphing、B4 #15-#18 | 效果惊艳但独立性强，可按需排期 |

---

## 6. 纪律与登记

- 任何以第三方包为行为参考的实现（flutter_animate、animated_flip_counter、confetti、wave 等）沿用 `THIRD-PARTY-NOTICES.md`「Documentation-only ecosystem research」流程：冻结版本、只写独立行为需求、clean-room 实现、记 `spec-snapshot/manifest.json`。
- 每项动效必须有 Reduced/None 方案下的确定性回退（静止或短淡入），并在 `docs/FLUTTER_PARITY_STATUS.md` 增补边界行；`[MdExperimental]` 不作为 parity 证据。
- 新 token（缓动曲线/时长档）进 `spec-snapshot/tokens.json` 的 L1 校验范围，改动需过 `scripts/lint-design-tokens.py`。
- 本文是调研与建议，不构成排期承诺；行为对齐声明只限定到具体 API 与已测试情形。
