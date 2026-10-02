# Md3.Avalonia 全组件 Material Design / Flutter 规范复核

> 审计日期：2026-10-02  
> 审计基线：`b2a67d66f7bfd71905998b768dea960e6cc4327e`  
> 范围：`Md3.Avalonia`、`Md3.Avalonia.Extra`、主题、motion runtime、Headless/Maestro 测试与既有合规报告  
> 结论类型：源码与模板静态审计 + 既有测试证据复核；**不是** Narrator/VoiceOver/Orca、真机触控或视觉金图认证

## 1. 结论摘要

当前实现已经具备较完整的 Material token、原生 Avalonia 基类复用、动态色、状态层和统一 motion API 基础；按钮、Checkbox、Radio、Switch、普通 Slider、Snackbar、TreeView 的部分键盘/RTL 行为以及若干 popup exit 生命周期实现值得保留。

但是，仓库目前**不能声明“100% Material Design 3 / Flutter 规范合规”**。主要原因不是少量像素误差，而是以下发布阻断问题：

1. 规范验证脚本没有读取多数实现或渲染结果，大量项目无条件写入 `passed=True`；甚至会把不存在/拼写不一致的类型判定为“完整主题、跨平台支持”。
2. Menu、Dialog、modal drawer/sheet、Search、FAB menu 等关键临时界面缺少完整焦点进入、焦点约束、关闭后恢复以及 Automation 状态。
3. `MdRangeSlider` 的两个 thumb 在辅助技术中不是两个可独立操作的范围对象。
4. 日期/时间选择器没有实现 Material 要求的完整日历网格、dial 数据点语义和键盘模型。
5. Extra 中 `MdSharedAxis`、`MdFadeThrough`、`MdAnimatedVisibility` 基本只有状态属性，默认主题甚至不存在；`MdContainerTransform` 只是瞬时内容切换，并非容器变换。
6. 一批 Flutter parity API 只是 Material 风格外壳，尚未实现 API 名称暗示的 Flutter 行为，例如 `MdFocusTraversalGroup.Policy`、`MdKeyboardAvoidingHost` 的“自动监视”、Cupertino adaptive 控件和 `MdSimpleDialog` 的 dialog route 行为。
7. 触控目标、RTL、本地化和可访问错误语义仍有系统性缺口。

### 严重度统计

| 严重度 | 数量 | 含义 |
|---|---:|---|
| P0 | 7 | 阻止发布“规范合规”声明，或使核心操作对键盘/辅助技术用户不可完成 |
| P1 | 20 | 组件主要能力、跨输入、RTL、Reduced Motion 或公开 API 存在明显不完整 |
| P2 | 8 | 视觉/token、响应式、文案与工程治理问题；应在宣称高保真前完成 |

**建议的当前对外措辞**：

> “Md3.Avalonia 提供 Material 3 风格的 Avalonia 控件，并覆盖部分 M3 Expressive 与 Flutter 交互模式；完整的跨平台可访问性、RTL、真机 motion 和所有组件规范一致性仍在验证中。”

而不是：

> “100% Material Design 3 and Flutter compliant.”

---

## 2. 审计基准与分类原则

### 2.1 本轮重新核对的官方依据

Material 页面处于持续更新状态，本轮以 2026-10-02 可访问的当前页面为准，尤其注意 2025 年 M3 Expressive 更新：

- [Material 3 Components](https://m3.material.io/components)
- [M3 motion physics](https://m3.material.io/styles/motion/overview/how-it-works)
- [M3 Expressive motion theming](https://m3.material.io/blog/m3-expressive-motion-theming)
- [All buttons](https://m3.material.io/components/all-buttons)
- [Buttons specs](https://m3.material.io/components/buttons/specs)
- [Icon buttons guidelines](https://m3.material.io/components/icon-buttons/guidelines)
- [FAB](https://m3.material.io/components/floating-action-button)
- [FAB menu guidelines](https://m3.material.io/components/fab-menu/guidelines)
- [FAB menu accessibility](https://m3.material.io/components/fab-menu/accessibility)
- [Button groups](https://m3.material.io/components/button-groups/guidelines)
- [Split button accessibility](https://m3.material.io/components/split-button/accessibility)
- [Menus accessibility](https://m3.material.io/components/menus/accessibility)
- [Dialogs guidelines](https://m3.material.io/components/dialogs/guidelines)
- [Bottom sheets accessibility](https://m3.material.io/components/bottom-sheets/accessibility)
- [Search accessibility](https://m3.material.io/components/search/accessibility)
- [Chips accessibility](https://m3.material.io/components/chips/accessibility)
- [Carousel accessibility](https://m3.material.io/components/carousel/accessibility)
- [Date pickers accessibility](https://m3.material.io/components/date-pickers/accessibility)
- [Time pickers accessibility](https://m3.material.io/components/time-pickers/accessibility)
- [Cards accessibility](https://m3.material.io/components/cards/accessibility)
- [Switch specs](https://m3.material.io/components/switch/specs)
- [Text fields accessibility](https://m3.material.io/components/text-fields/accessibility)
- [Tooltips guidelines](https://m3.material.io/components/tooltips/guidelines)
- [Navigation bar](https://m3.material.io/components/navigation-bar)
- [Navigation rail](https://m3.material.io/components/navigation-rail)
- [Toolbars guidelines](https://m3.material.io/components/toolbars/guidelines)

Flutter parity 以公开 API/源码行为为比较对象，而不是把 Flutter 独有控件自动归入官方 M3：

- [Flutter Material API](https://api.flutter.dev/flutter/material/)
- [showDialog](https://api.flutter.dev/flutter/material/showDialog.html)
- [SimpleDialog](https://api.flutter.dev/flutter/material/SimpleDialog-class.html)
- [MenuAnchor](https://api.flutter.dev/flutter/material/MenuAnchor-class.html)
- [DropdownMenu](https://api.flutter.dev/flutter/material/DropdownMenu-class.html)
- [Dismissible](https://api.flutter.dev/flutter/widgets/Dismissible-class.html)
- [DraggableScrollableSheet](https://api.flutter.dev/flutter/widgets/DraggableScrollableSheet-class.html)
- [ReorderableListView](https://api.flutter.dev/flutter/material/ReorderableListView-class.html)
- [FocusTraversalGroup](https://api.flutter.dev/flutter/widgets/FocusTraversalGroup-class.html)

### 2.2 分类

| 分类 | 判断规则 | 示例 |
|---|---|---|
| 当前官方 M3 | 当前 Material 组件目录或当前 Expressive 更新中的组件 | Button、FAB、Menu、Dialog、Navigation、Picker、Carousel |
| Flutter parity 扩展 | Flutter Material/widgets 有明确 API，但当前 M3 目录不一定存在同名组件 | Dismissible、ReorderableList、Form、Hero、FocusTraversalGroup |
| Material-style Extra | Material/Flutter 没有一方的同名一等组件，只要求 token、状态、AX、RTL、平台行为一致 | ColorPicker、Cascader、Transfer、Chart、Chat、Rating |
| 基础设施 | 不应被计算为一个“官方 Material 组件” | Scaffold、AdaptiveLayout、StateLayer、RipplePresenter、FocusRing |

`MdColorPicker` 自身注释已经正确写明 Material 与 Flutter 没有一方 ColorPicker；因此本报告只审核其 Material 风格、公开 API 一致性和无障碍，不把它称为官方 M3 Color Picker。

---

## 3. P0：发布阻断问题

### P0-01：现有“100% 合规”证据无效

**证据**

- 项目规范要求先生成 `spec-snapshot/manifest.json`：`MaterialDesign3_Avalonia12_设计规范.md:69`；仓库中该文件不存在。
- `scripts/run-spec-verification.py:34-58` 的触控检查不读取 AXAML、不实例化控件、不测 `Bounds`，而是对一个常量数组逐条 `passed=True`。
- 同脚本 `:149-163` 的 swipe threshold 测试同样无条件通过。
- `:166-210` 的 inventory 检查不检查类型、主题或目标平台，仍无条件通过；列表中还有实际 API 不一致的 `MdAutocompleteBox`、`MdFocusTrap`、`MdShortcut`。
- `docs/reports/test-spec-execution-results.json` 记录的是 123/123 项在 0.001 秒内通过；`docs/reports/material-design-3-and-flutter-spec-verification-report.md:14,75,83` 却写成 128 Passed 并据此声明 100% 合规，证据数量本身也不一致。
- `tests/maestro/01_material_design_specs.yaml` 只启动、按文本点击和截图，没有读取触控目标矩形或 Automation tree。

**影响**

- 生成的 123 项结果和报告中宣称的 128 项都不是实现级规范验证，不能支持触控、对比度、motion、Flutter 行为或跨平台结论。
- 它还掩盖了真实的 32/36/40dp 交互目标和未实现 API。

**必须修复**

1. 立即把既有报告标记为“历史、非认证”；CI 不再把脚本结果称为 spec compliance。
2. 生成带 URL、抓取日期、版本、哈希和许可信息的 `spec-snapshot/manifest.json`。
3. 每条自动检查必须能回溯到：实例化控件、实际 `Bounds`、Automation peer/pattern、输入事件或截图测量；不能对常量自证。
4. CI 分开报告 unit/headless、视觉、平台、屏幕阅读器和人工项目。

### P0-02：Menu 不是完整的菜单交互/语义模型

**证据**

- `MdMenu` 只是空的 `ItemsControl` 子类：`src/Md3.Avalonia/Controls/MdMenu.cs`。
- `MdMenuItem` 只是 `Button`，`IsSelected` 只切换视觉伪类：`MdMenuItem.cs:8-38`。
- `MdMenuAnchor` 只处理 Enter/Space/Escape：`MdMenuAnchor.cs:91-105`；打开后没有把焦点移到第一个可用项，也没有方向键、Home/End、首字母搜索、禁用项跳过或关闭后恢复焦点。
- `MdSubMenuItem` 只在点击/hover 时切换 bool：`MdSubMenuItem.cs:14-24`；没有 Left/Right（RTL 镜像）、延时、父子焦点路径或 expand/collapse Automation 状态。
- 子菜单固定 `Placement="RightEdgeAlignedTop"`，尾部 glyph 固定 `›`：`Themes/Controls/MdMenuAnchor.axaml:43-75`。

**官方差距**

Material menu 要求打开后的初始焦点、Up/Down、Home/End、首字符、Space/Enter、Escape 和子菜单焦点关系。当前鼠标可点击，但键盘与辅助技术用户无法获得等价菜单体验。

**必须修复**

- 采用 roving focus/selection controller；实现 Menu/MenuItem/Submenu 的正确 Automation role、expanded/checked/disabled 状态。
- 打开时聚焦首个可用项；关闭或 Escape 后恢复触发器。
- 子菜单用 start/end 和 `FlowDirection` 决定展开侧及箭头。
- 增加键盘与 Automation tree 测试。

### P0-03：Dialog、modal drawer/sheet 和 `MdSimpleDialog` 没有真正的 modal focus 行为

**证据**

- `MdDialogHost` 只有 Escape 和 scrim pointer 关闭；`UpdateOpenState()` 不捕获当前焦点、不请求 dialog 初始焦点、不设置 focus scope/trap、不恢复触发器：`MdDialogHost.cs:113-225`。
- `MdNavigationDrawer` 和 `MdSheetHost` 同样只有 Escape/scrim/drag；背景内容仍可通过 Tab 到达：`MdNavigationDrawer.cs:87-154`、`MdSheetHost.cs:108-294`。
- `MdSimpleDialog` 只是页面内的一个 surface；没有 scrim、Escape、焦点进入/闭环/恢复：`MdFlutterPhase3.cs:191-299`。
- Flutter `showDialog` 默认使用 closed-loop traversal；Material Dialog 明确要求出现时禁用背景功能。

**影响**

- 键盘用户可在“模态”层打开时进入背景；屏幕阅读器也没有可靠的 dialog 边界和标题关系。
- Escape 是否生效取决于事件是否经过 host，而不是全局 modal scope。

**必须修复**

- 建立统一 `MdModalFocusScope`：capture → initial focus → cycle/contain → restore。
- modal 展示时从可访问树/键盘路由中隔离背景，但 standard sheet/drawer 不应错误地变成 modal。
- Dialog 关联 title/description，Automation role/state 正确暴露。
- 所有关闭路径（按钮、scrim、Escape、binding、取消 token、替换 dialog）都必须恢复焦点。

### P0-04：日期/时间选择器缺少官方要求的键盘与辅助语义

**证据**

- 核心 `MdDatePicker` 的日期是 42 个普通 Button 放在 ItemsControl 中，没有 grid/row/column header 语义，也没有 roving focus 和箭头/PageUp/PageDown/Home/End 模型：`MdDatePicker.cs:259-268,497-547`，`Themes/Controls/MdDatePicker.axaml:126-180`。
- 打开时没有聚焦当前/已选日期；外部绑定 `IsOpen=true` 也没有初始焦点路径。
- 日期按钮实际 `Width=36 Height=36`，注释所称“ItemsControl remains the 48-DIP hit host”并不成立：ItemsControl 不是日期 Button 的命中区域，见 `MdDatePicker.axaml:14-43`。
- `MdTimeDial` 是一个自绘、单焦点 Control；屏幕阅读器看不到每个 hour/minute selector 的 Button/selected/value 语义：`MdTimeDial.cs:128-192,228-288`。
- Time dial 键盘只按单步调整值；没有官方文档所示“Hour 7 of 12”等 selector 语义。

**官方差距**

Material 日期选择器要求手工输入替代、48×48dp 目标、Month grid、weekday column header、完整日期名称和网格键盘导航。Time picker 要求 Hour/Minute 文本输入与 dial selector 的明确 role/name/value。

**必须修复**

- 日历实现可访问 Grid/GridItem 或等价 peer，单一活动日期 + roving focus；选择与“只移动活动日期”分开。
- 打开后聚焦已选日期、今天或首个有效日期。
- 为 dial 数据点提供虚拟/真实 Automation children 与选择状态，或使用一组可访问 button。
- 将日期、月份、AM/PM、按钮名称全部接入 localization provider。

### P0-05：`MdRangeSlider` 对辅助技术只有一个 Control，不是两个 thumb

**证据**

- `MdRangeSlider : Control` 自行绘制整条 range；仓库没有任何自定义 `AutomationPeer`。
- 两个值只存在于 `LowerValue` / `UpperValue` 属性：`MdRangeSlider.cs:14-74`。
- 键盘操作依靠内部 `_activeLower`；Tab 只是翻转该字段后继续调用 base，并不会形成两个可单独聚焦的 thumb：`MdRangeSlider.cs:164-175`。
- 整个仓库静态扫描没有 `OnCreateAutomationPeer` 或 `class ... AutomationPeer`。

**影响**

屏幕阅读器无法发现两个范围值、分别命名它们、读取上下限或选择要调整的 thumb。这不是给父控件加 `AutomationProperties.Name` 可以解决的问题。

**必须修复**

- 创建具有两个虚拟子 peer 的 range-slider peer，分别实现 RangeValue pattern、名称、值、最小/最大和 bounds。
- 明确 thumb 切换、交叉限制、Home/End/Page 和 RTL 键位。
- 添加 UI Automation pattern 测试，而不是只断言 peer 非 null。

### P0-06：Extra 的四个命名 motion 控件未实现其公开承诺

**证据**

- `MdSharedAxis` 只有 `Direction/IsForward/Duration` 与伪类，没有内容切换 controller：`MdMotionControls.cs:175-211`。
- `MdFadeThrough` 只有 `Duration` 属性：`MdMotionControls.cs:216-224`。
- `MdAnimatedVisibility` 只有 `IsVisible/Duration` 状态：`MdMotionControls.cs:288-318`。
- Extra 主题中没有 `MdSharedAxis`、`MdFadeThrough`、`MdAnimatedVisibility` 的 `ControlTheme`；默认情况下甚至不能据此保证内容被渲染。
- `MdContainerTransform` 的主题只在两个 presenter 之间切换 `IsVisible` 并瞬间改变 CornerRadius：`Extra/Themes/AdvancedControls.axaml:909-931`；没有 bounds、位置、尺寸、shape、elevation 与内容 crossfade 的同步 morph。
- 四个 `Duration` 属性都没有接入真正的动画 runner。

**影响**

公开类型名和 XML 注释会让用户合理期待 Material transition pattern，但当前行为是 no-op、瞬时切换或依赖外部未知主题。

**必须修复**

- 在实现前将这些类型标记 experimental；不能继续计入“已完成组件”。
- 实现可打断、可反向、保留退出生命周期的 shared-axis/fade-through/container-transform/visibility controller。
- Reduced 仅保留短 effects fade，None 瞬时完成；正常 scheme 必须消费 `MdMotion` token，而不是各自硬编码 duration。

### P0-07：若干 Flutter parity 类型只有名称/伪类，没有对应行为

**证据**

- `MdFocusTraversalGroup.Policy` 只切换伪类；主题只消费 `Cycle` 和 `SkipTraversal`，`VisualOrder/TabIndex/ReadingOrder` 三种 policy 实际没有不同策略：`MdFlutterPhase4.cs:568-600`、`MdFlutterAdvanced.axaml:316-321`。
- `MdKeyboardAvoidingHost` 的注释声称“monitors soft keyboard insets”，但实现只是消费者手工设置 `KeyboardHeight`，没有平台 inset/IME 订阅：`MdFlutterPhase4.cs:632-731`。
- `MdAdaptiveSwitch` 的 Cupertino 分支只改 `MinWidth/MinHeight`，仍沿用 M3 switch 模板；`MdAdaptiveProgressIndicator` 仍是 `MdCircularProgressIndicator`，只改 stroke/track：`MdFlutterPhase4.cs:300-345`、`MdFlutterAdvanced.axaml:308-313`。
- `MdPaginatedDataTable` 只是分页 ListBox，没有 Flutter DataTable 的 columns/cells/header sort 模型：`MdFlutterPhase2.cs:19-155`。
- `MdAboutDialog` 只是 ContentControl surface，`ShowLicenses()` 在默认主题中没有触发按钮；`MdSimpleDialog` 缺少 showDialog route 行为。

**必须修复**

- 将 parity 文档拆成 `implemented / partial / API-shell / not implemented`。
- API-shell 不得计为完成；若名称保留，XML 文档必须准确写明宿主责任。
- 为真正的 parity 建立行为测试，直接对照对应 Flutter 稳定版本源码常量和状态机。

---

## 4. P1：高优先级问题

| ID | 组件/证据 | 问题与影响 | 修复方向 |
|---|---|---|---|
| P1-01 | `MdChip.axaml:5`、`MdSegmentedButton.axaml:3`、`MdDatePicker.axaml:14-43`、`MdDesktopAdapters.axaml:168-180`、Extra `AdvancedControls.axaml:235-241`、`MdRating.cs` | Chip 32dp、segmented 40dp、日期 36/40dp、NumericBox spinner 约 32×20dp、rating 默认约 32dp；不是 48dp 触控包围盒。Small FAB 也没有独立 48dp 外层 hit target。 | 保留视觉容器尺寸，但在触控场景为控件根提供至少 48×48dp 的透明命中/焦点区域；相邻目标仍需足够分离。桌面高密度 spinner 应有明确 density/input-mode 策略。 |
| P1-02 | `MdFabMenu.cs:116-124` | 展开方向已有独立 `ExpansionDirection`，默认向上并可向下，符合既定约束；但打开不聚焦首项、无方向键/首字母/关闭后焦点恢复，也不限制 2–6 项。 | 保留独立方向属性；增加 menu focus controller、Automation expanded/state 与 2–6 项诊断。 |
| P1-03 | `MdSearchView.cs:62-138` | `Show()` 立即尝试聚焦 Header，但绑定打开路径不聚焦；没有结果 roving navigation、初始交互元素策略、焦点恢复和背景隔离。 | 对 header + results 建统一 search focus scope，支持官方 Tab/方向键/Enter/Escape 行为。 |
| P1-04 | `MdNavigationDrawer.cs`、`MdSheetHost.cs`、`MdScaffold.axaml` | placement/FAB 使用物理 Left/Right；没有 start/end 默认映射。Modal 已在 P0，此外 side sheet/drawer 和 Scaffold 在 RTL 下不会自动交换主边。 | 公开 Start/End 语义或根据 `FlowDirection` 映射；物理 Left/Right 仅作为显式 override。 |
| P1-05 | `MdCarousel.cs:156-160,174-202,316-327` | Left/Right 与 margin 全按 LTR；Reduced Motion 虽取消 transition/auto-play，却仍按选中项改变大中小 item 宽度，违反 Material reduced-motion“所有 item 同尺寸”。缺少 container/current-of-total Automation label 与 Show all 支持。 | RTL 镜像；Reduced 下固定 item width；提供容器和项目位置信息及可选 ShowAll command。 |
| P1-06 | `MdRangeSlider.cs:242-244`、`MdRating.cs:97-103`、`MdTimeDial.cs:180-192` | 自绘输入把 Left/Right 当物理增减，没有统一 RTL value-direction 规则。Rating/TimeDial 还缺少独立焦点视觉/peer。 | 建立可复用方向映射与 AX peer；逐控件规定逻辑增减。 |
| P1-07 | `MdTextBox.cs:28-59,95-123`、`MdFlutterPhase3.cs:69-151`、`MdPinInput.cs:79-134`、`MdTagInput.cs:105-205` | Label、supporting/error、counter 没有系统关联到输入的 Automation name/help/error；错误变化没有 live announcement。Pin/Tag 默认名称和验证文案硬编码英文。 | 将可字符串化 Label 映射为 Name，error/support 映射 HelpText/描述或可访问关系；错误更新 live announce；所有默认文案本地化。 |
| P1-08 | `MdTooltipHost.cs:30-35,72-87` | 默认离开目标后 100ms 隐藏，官方 guidance 为约 1.5s；tooltip 没有与 trigger 的 HelpText/description 关系。 | 默认 hide delay 对齐官方值；保证 pointer 可移入 rich tooltip；同步辅助描述。 |
| P1-09 | `MdSheet.axaml:16-18`、`MdFlutterAdvanced.axaml:306` | Bottom sheet handle 只有 32/36dp 高，非可聚焦 Button，没有 Space/Enter 或键盘展开/关闭。 | 48dp handle action，正确 name/role，Space/Enter 与 Escape；拖拽只作为补充输入。 |
| P1-10 | `MdDataTable.cs`、`MdPaginatedDataTable`、Extra `MdDataGrid.cs` | 都主要继承/组合 ListBox；没有 table/grid、row、column header、cell、sort direction 的 Automation tree。Extra DataGrid 也没有 cell 键盘导航模型。 | 为 table/grid 建专用 peers/pattern，header sort 状态、row/column index、edit state；测试真实 Automation tree。 |
| P1-11 | Extra `MdTreeView.cs:60-68,109,186-211` | 键盘与 RTL 实现较好，但继承 ListBox 只保留 list semantics；节点没有 treeitem level/expanded/posinset/setsize。默认名称 “Hierarchy” 硬编码。 | 提供 Tree/TreeItem peers 或采用原生 TreeView 语义；名称由宿主/本地化提供。 |
| P1-12 | Extra `MdChart`：`MdDataVisualization.cs:128-282` | `BuildAccessibleTable()` 仅返回字符串，未自动进入 Automation tree；没有键盘数据点导航、series/point 名称、焦点视觉；fallback palette 硬编码；文本固定 LTR 绘制。 | 自动生成隐藏但可访问的数据表/虚拟 children；方向键导航与 hover/focus 同源；token palette；RTL/culture。 |
| P1-13 | Extra `MdRating.cs` 与主题 | 自绘 `RangeBase` 没有 rating/value peer；无可见 focus ring；默认 32dp；左右键不处理 RTL。 | 48dp hit target、RangeValue 或 selection peer、focus ring、RTL 与只读语义。 |
| P1-14 | Extra `MdCalendar`：`MdSelectionAndFeedback.cs:435-563` | 方向键立即修改 `SelectedDate`，没有“活动日期”和“提交选择”分离；Home/End 固定 Sunday 周边界，忽略已用于布局的 `FirstDayOfWeek`；没有范围键盘选择/完整 Grid 语义；40dp target。 | 按 DatePicker 同一日历 controller 重构；Home/End 使用文化周首日；Shift range；48dp hit；RTL。 |
| P1-15 | Extra `MdColorPicker.cs:111-301`、`AdvancedControls.axaml:665-873` | `SelectedHex` 注册为 TwoWay，但外部修改不会解析回 `SelectedColor`；`IsAlphaEnabled` 变化不重新同步 alpha/HEX。模式按钮只是视觉 Variant，不暴露 tab/selection；HSV/alpha slider 缺 name。 | 为 Hex/Alpha 属性添加防递归同步与验证状态；mode 用 Tab/Radio 语义；命名每条 slider、swatch 与当前值；弹层管理焦点。 |
| P1-16 | Extra `MdBreadcrumb.cs:29-81`、`ExtraControls.axaml:22` | `Href` 不参与导航；`MaxDisplayedItems`、`ItemsBeforeCollapse`、`ItemsAfterCollapse` 不产生 overflow collapse；item 高度 36dp。公开 API 与行为不符。 | 实现可访问的 overflow menu/ellipsis、current-page state、48dp target；或在实现前移除/标记这些属性。 |
| P1-17 | Extra `MdCascader`：`MdCascaderTransfer.cs:20-206`、`AdvancedControls.axaml:286-327` | “DropDown” 实际在 StackPanel 布局流内展开，不是 Popup；打开会推开页面。没有首列/已选项初始焦点和跨列 Left/Right 路径。 | 使用 Popup/Overlay、窗口边缘碰撞与 RTL placement；实现 columns 的 roving focus 和关闭恢复。 |
| P1-18 | Extra `MdTransfer` / `MdSlidableItem` | Transfer 是固定三列 desktop 布局、箭头和文案按 LTR；缺少 compact 布局与移动公告。Slidable 虽对 drag delta 做 RTL 反转，但模板仍把 StartActions 固定在左、EndActions 固定在右。 | 逻辑 start/end 模板镜像；compact 改堆叠/单面板；移动完成使用 live status。 |
| P1-19 | Extra `MdCommandPalette.cs:111-180`、`MdOverlays.cs:127-205` | CommandPalette 无焦点 capture/trap/restore，背景可 Tab；命令 `CanExecute=false` 未反映 row disabled。Popover 对任何 top-level wheel 都关闭，包括 popup 内滚动。HoverCard 在 hover 打开时强制移动焦点，pointer 从 anchor 移向独立 PopupRoot 又可能触发 120ms 关闭。 | Palette 使用 modal focus scope；Popover 只在 owner viewport 发生外部滚动时关闭；HoverCard 区分 hover preview 与键盘/点击交互。 |
| P1-20 | Extra `MdChatView` / `MdRichEditor`：`MdDataVisualization.cs:686-1010` | 新消息、发送失败/重试、选择数、busy 状态没有 live region；message presenter 未构造 sender/time/status 的可访问名称。Rich editor toolbar 只用 enum 作为 DataContext，未同步 toggle/disabled state 和本地化名称。 | Chat log/live region、消息结构化名称与状态；toolbar command descriptors 包含 name、shortcut、checked、enabled。 |

---

## 5. P2：视觉、motion 与工程治理问题

| ID | 证据 | 问题 | 建议 |
|---|---|---|---|
| P2-01 | `Themes/Controls/MdAppBar.axaml:5-20,102-115` | Top/Bottom app bar 默认 `BorderThickness=1`、`CornerRadius=20`，表现成 outlined rounded card；当前 app bar 通常是边到边 surface。Bottom app bar 又是当前不再推荐的 compatibility component。 | Top app bar 默认无 outline/圆角；Bottom app bar 明确标注 legacy，并推荐 docked/floating toolbar。 |
| P2-02 | `MdToolbar.axaml:5-15` | 默认 Padding 8、item spacing 4；与当前 toolbar 关于边缘和操作间隔的指导明显偏离。 | 从当前 toolbar token 建 component token；分别定义 floating/docked 和 compact density。 |
| P2-03 | `MdStandardButtonGroup.cs`、`MdConnectedButtonGroup.cs` | Group `Size` 主要改变 group pseudo/spacing，没有可靠传播给所有子 `MdButton`；connected group 扫描视觉后代，可能误改嵌套按钮。 | 只管理直接 item containers；生成 container 时设置 size/shape，并保留用户 local value。 |
| P2-04 | `MdSplitButton` 与其 theme | trailing 区除 medium 外与冻结尺寸不一致；尾部按钮无内建 Automation name；物理圆角未显式 RTL 镜像。 | 使用冻结 component tokens；两个操作分别命名；start/end corner mapping。 |
| P2-05 | `Motion/MdMotion.cs`、`MdSpringEasing.cs` | 已有 spatial/effects spring token，这是正确方向；但用固定 settling duration 把 spring 采样成 Avalonia Transition easing，打断时通常从当前值重新起步，不能保留速度。 | 对关键 spatial motion 使用可保留 position/velocity 的 runner；Transition easing 留给低风险 effects。 |
| P2-06 | `MdFlutterAdvanced.axaml:80,152`、Extra `AdvancedControls.axaml:90-92` | 仍有 `#66000000`、`#B3000000` 和硬编码 shadow，而不是 scrim/elevation token。 | 新增 semantic scrim/overlay token；阴影统一引用 elevation token。 |
| P2-07 | 多个 AXAML 的固定 Width/MinWidth、物理 Margin；`MdLicensePage` 固定 `260,*`、CommandPalette `Width=600` | 未证明 360dp、200% 字体、长德语/阿拉伯语及窄 Popup 下无裁剪。`MaxWidth` 绑定自身 Bounds 的写法也不能代替可用窗口宽度。 | 添加窄宽/大字模式；用可用 viewport 计算 popup；文本允许 wrap、reflow。 |
| P2-08 | 多个默认字符串与 Automation name | Core picker、chip、loading；Extra command/chat/transfer/tag/pin/tree/result 等仍硬编码英文。 | 所有库提供的可见文案和默认 Automation 文案走 `MdLocalization` 或由宿主必填。 |

---

## 6. 全组件覆盖矩阵

本轮枚举到 Core 141 个、Extra 56 个公开 `Md*` class/record/struct/interface 声明（共 197 个）。其中 event args、record model、converter、controller、platform adapter 和仅供模板使用的 presenter 不作为独立视觉组件重复评分，而是并入其所属组件族。下列矩阵覆盖所有独立用户界面组件与基础设施。

状态含义：

- **基础可用**：静态审计未发现本轮 P0，但不代表已通过真机/屏幕阅读器认证。
- **部分实现**：主要视觉存在，但至少有一项 P1/P2 或官方行为未覆盖。
- **阻断**：存在本报告 P0。
- **结构/基础设施**：不应作为官方 M3 组件计数。

### 6.1 当前官方 M3 / Expressive 对齐组件

| 组件族 | 仓库类型 | 状态 | 主要结论 |
|---|---|---|---|
| Common buttons | `MdButton`、`MdToggleButton` | 基础可用 | XS–XL、shape、48dp root 与 token 基础较完整；仍需真机焦点/对比度/长文本。 |
| Icon buttons | `MdIconButton`、`MdToggleIconButton` | 基础可用 | 尺寸体系已扩展；图标按钮仍依赖使用者设置动作名称。 |
| Split button | `MdSplitButton` | 部分实现 | 双操作存在；尾部尺寸、名称和 RTL shape 待修。 |
| Button groups | `MdStandardButtonGroup`、`MdConnectedButtonGroup` | 部分实现 | 当前 Expressive 组件方向正确；size 传播与子项边界不可靠。 |
| FAB / Extended FAB | `MdFloatingActionButton`、`MdExtendedFloatingActionButton` | 部分实现 | medium 与新色调已存在；small 已不推荐且 hit target 需补 48dp；默认不应鼓励 surface/legacy。 |
| FAB menu | `MdFabMenu`、`MdFabMenuItem` | 部分实现 | **已保留默认向上，且使用独立 `ExpansionDirection` 支持向下**；焦点/键盘/2–6 项约束不完整。 |
| Toolbar | `MdToolbar` | 部分实现 | floating/docked 外观存在；padding/spacing token 偏离当前指导。 |
| Top app bar | `MdTopAppBar` | 部分实现 | 多尺寸 API 存在；默认 outline/rounding 不符合 app bar surface 角色。 |
| Bottom app bar | `MdBottomAppBar` | compatibility | 当前官方不再推荐；应明确 legacy 并引导 toolbar。 |
| Badge | `MdBadge`、`MdBadgedBox` | 基础可用 | 需补 RTL anchor、超长数字与 screen-reader 合并名称测试。 |
| Snackbar | `MdSnackbar`、Host/Service/Message | 基础可用 | 已设置 Polite live setting，队列/exit 生命周期较完整；仍需平台 announcement 验证。 |
| Tooltip | `MdTooltip`、`MdTooltipHost` | 部分实现 | hover/focus/long-press/Escape 存在；hide delay 与辅助描述关系不符。 |
| Progress / loading | Linear、Circular、`MdLoadingIndicator` | 基础可用 | 使用原生 ProgressBar peer、Reduced/None 停止 ambient motion；“Loading”需本地化。 |
| Card | `MdCard` | 部分实现 | Button 基类可保留交互 card 键盘路径；展示卡片/交互卡片焦点策略需按用法验证。 |
| Carousel | `MdCarousel`、Item/Controller | 部分实现 | 多种布局与直接操纵存在；Reduced、RTL、AX container/item 不完整。 |
| Divider / List | `MdDivider`、`MdList`、`MdListItem` | 基础可用 | 依赖 ItemsControl/ListBox 原生语义；复杂 list item 操作仍需测试。 |
| Dialog | `MdDialog`、`MdDialogHost` | **阻断** | 视觉与 presence 有实现；modal focus/Automation 边界缺失。 |
| Bottom/side sheet | `MdSheetHost` | **阻断** | pointer drag 与 exit motion 较好；modal focus 和 drag-handle keyboard 缺失。 |
| Navigation bar | `MdNavigationBar`、Item | 基础可用 | Flexible 默认、Baseline compatibility 处理合理；跨断点焦点/selection 保持需验证。 |
| Navigation rail | `MdNavigationRail`、Item | 部分实现 | collapsed/expanded API 存在；宽度 transition 与 start/end/焦点保持需加强。 |
| Navigation drawer | `MdNavigationDrawer` | **阻断** | modal focus 缺失；placement 为物理左右。 |
| Navigation suite | `MdNavigationSuite` | 部分实现 | 自适应切换存在；三个独立 navigation view 的 selection/focus 由用户绑定，默认未同步。 |
| Tabs | `MdTabs`、`MdTabItem`、`MdTabView`、`MdTabViewItem` | 部分实现 | 原生 selection 基础存在；需验证 roving arrows、RTL、tab/tabpanel 关系与动态增删焦点。 |
| Checkbox / Radio / Switch | `MdCheckBox`、`MdRadioButton`、`MdSwitch`、`MdAdaptiveSwitch` | 基础可用/部分 | 核心三者原生基类和 48dp target 较稳健；adaptive Cupertino 不是真正 parity。 |
| Chips | Assist/Filter/Input/Suggestion | 部分实现 | 视觉变体存在；32dp 实际 hit、组方向键、Delete 与 remove-action 独立语义不完整。 |
| Segmented button | `MdSegmentedButton`、Group | 部分实现 | selection 基础存在；40dp hit target、组语义/方向键需补。 |
| Slider | `MdSlider`、`MdSliderThumb` | 基础可用 | 原生 Slider 保留 range peer；需补 RTL/vertical/高对比真机矩阵。 |
| Range slider | `MdRangeSlider` | **阻断** | 双值视觉存在，但双 thumb Automation 模型不存在。 |
| Text field | `MdTextBox` | 部分实现 | 保留原生编辑、IME、password；label/error/support/counter AX 关系不足。 |
| Combo/dropdown/autocomplete | `MdComboBox`、`MdDropdownMenu`、`MdAutoCompleteBox` | 部分实现 | 原生选择/输入基础较好；popup 初始焦点、方向键细节、名称/错误关系需补。 |
| Search | `MdSearchBar`、`MdSearchView` | 部分实现 | 输入基础存在；expanded search 的完整 focus/result 模型不足。 |
| Date picker/range | `MdDatePicker`、`MdDateRangePicker`、dialog | **阻断** | 本地化日期生成和月切换已有；Grid/keyboard/48dp/initial focus 不合格。 |
| Time picker | `MdTimePicker`、`MdTimeDial`、dialog | **阻断** | 提供 dial 与文本输入是正确方向；dial AX selector 语义和 focus 模型不足。 |

### 6.2 Flutter parity / Flutter-inspired 组件

| 组件族 | 状态 | 主要结论 |
|---|---|---|
| Banner | 部分实现 | Flutter 风格 compatibility；不是当前官方 M3 一等组件。需确认 dismiss/action AX 与队列策略。 |
| ExpansionPanel/List | 部分实现 | 展开视觉和 header 按钮存在；expanded state/组键盘/焦点需补。 |
| DataTable/PaginatedDataTable | 部分/阻断 AX | ListBox 外壳不是完整 table/grid；Paginated 类型缺 Flutter columns/cells 模型，文案硬编码。 |
| ReorderableList | 基础可用 | pointer drag、auto-scroll、Ctrl+Arrow alternative、Reduced motion 均有基础；移动结果缺 live announcement。 |
| GridTile/GridTileBar | 部分实现 | 可点击 tile 与 focus visual 存在；favorite/activated state 未完整语义化。 |
| Dismissible | 部分实现 | gesture、confirm、velocity、collapse 已较完整；默认没有键盘可执行 dismiss action/公告。 |
| Form/FormField/DropdownFormField | 部分实现 | 验证协调存在；required 文案、error relationship/live announcement 不完整。 |
| SimpleDialog | **阻断** | 只有 surface 和 selection，缺 showDialog modal route 行为。 |
| AboutDialog/LicensePage | API-shell/部分 | About 不是 dialog host，license action 未默认连接；LicensePage 固定双栏不适配窄宽。 |
| DraggableScrollableSheet | 部分实现 | drag/snap/scroll handoff 基础存在；36dp 非键盘 handle，且不是 modal route。 |
| AdaptiveSwitch/Progress | API-shell | Cupertino 仅少量尺寸/画笔变化，实际仍使用 M3 template/animation。 |
| Hero | 基础可用 | snapshot overlay、取消替换、Reduced fade 较完整；仍需真机 DPI、多窗口、clip/shape 验证。 |
| FocusTraversalGroup | **阻断 parity** | Cycle/Skip 生效；三种 Policy 仅元数据，没有策略实现。 |
| ShortcutScope | 基础可用 | 局部 ICommand 路由存在；冲突、文本输入和平台 Meta 键策略需说明。 |
| KeyboardAvoidingHost | **阻断 parity** | 布局计算存在，但不监视 IME inset，必须由宿主手工赋值。 |
| RefreshIndicator | 部分实现 | pointer pull、async、settle 和 Reduced 基础存在；没有键盘替代/live state。 |
| Stepper/Step | 部分实现 | 视觉和操作按钮存在；不是当前 M3 目录组件；current/completed/error AX 状态不足。 |
| Adaptive layout/scaffold | 基础设施 | breakpoints 基础合理；不是官方组件合规项，仍需 200%/安全区/重复 content fallback 验证。 |

### 6.3 Material-style Extra

| 组件族 | 状态 | 主要结论 |
|---|---|---|
| Avatar/AvatarGroup | 基础可用 | 非交互展示；图片 alt、重叠顺序和剩余数量名称由宿主补充。 |
| Breadcrumb | 部分实现 | collapse/link API 尚未实现；36dp target。 |
| Cascader | 部分实现 | 列选择逻辑存在；不是 Popup，焦点/跨列键盘/RTL placement 不完整。 |
| Transfer | 部分实现 | direct/button/drag 路径存在；RTL、compact、公告、本地化不足。 |
| SlidableItem | 部分实现 | gesture/velocity/命令/keyboard 有基础；RTL 模板未镜像。 |
| PagedItemsView | 部分实现 | async/cancel/retry/empty/error 状态存在；状态 announcement 与默认文案本地化不足。 |
| MasonryPanel | 结构控件 | 布局逻辑可用；不等于虚拟化，超大集合性能需由宿主验证。 |
| ColorPicker/Button | 部分实现 | HCT tonal palette 是正确方向；TwoWay Hex/alpha 同步、mode/slider AX、popup focus 不完整。 |
| CommandPalette | 部分实现 | shortcut/filter/arrow/Enter/Escape 存在；modal focus 与 disabled command 状态不完整。 |
| DataGrid | 部分实现 | sort/filter/edit/resize API 存在；没有 grid/cell/header Automation 和完整键盘编辑。 |
| Timeline | 基础可用 | 主要为展示；水平模式 RTL、时间/状态合并名称和长文本需验证。 |
| Chart | 部分实现 | 绘制与 pointer probe 存在；可访问数据和键盘 probe 缺失。 |
| RichEditor | API-shell | adapter 架构合理；toolbar command 语义、checked/enabled/shortcut 反馈不足。 |
| ChatView | 部分实现 | composer/history/selection/quote/retry 基础存在；chat log/live announcements 和消息语义不足。 |
| Skeleton/Sequence | 部分实现 | Reduced/None 有处理；需要确认 skeleton 不进入 AX tree并避免隐藏真实内容名称。 |
| SharedAxis/FadeThrough/ContainerTransform/AnimatedVisibility | **阻断** | 见 P0-06。 |
| Popover/HoverCard | 部分实现 | Popup/focus return 基础存在；wheel 与 hover/focus 生命周期有冲突风险。 |
| PinInput | 基础可用 | 使用真实 TextBox，输入/粘贴/password 较稳健；error announcement 与默认名称本地化不足。 |
| Rating | 部分实现 | pointer/keyboard/value 存在；AX/focus/48dp/RTL 不完整。 |
| AsyncSelect | 部分实现 | debounce/cancel/loading/popup 基础存在；combobox relationship、结果公告、初始焦点需补。 |
| Calendar/Day | 部分实现 | 月生成和 range 视觉存在；键盘模型、文化周边界、Grid AX、RTL 和 target 不完整。 |
| ResultView | 基础可用 | 展示/行动结构简单；empty/error/success live announcement 由宿主补充。 |
| TagInput | 部分实现 | 真实文本输入和 removable chip 路径存在；Down 直接提交首建议，无 suggestion highlight/Up/Escape/listbox 语义；error 未公告。 |
| TreeView | 部分实现 | 键盘与 RTL 是 Extra 中较好的实现；Tree/TreeItem AX 层级缺失。 |

### 6.4 基础设施、桌面适配与并入父组件的类型

| 组件族 | 仓库类型 | 状态 | 主要结论 |
|---|---|---|---|
| Adaptive composition | `MdAdaptiveLayout`、`MdScaffold` | 基础设施 | 五档 width class 与输入模式伪类存在；Scaffold 的 FAB 固定物理右侧，安全区、IME 和 200% 字体仍需平台验证。Large/ExtraLarge fallback 复用同一 Control content 时也应增加 logical/visual parent 回归。 |
| Surface/content primitives | `MdSurface`、`MdText`、`MdIcon`、`MdSymbolPresenter` | 基础可用 | 主要为 token 化展示；不应计为独立官方组件数量。Icon/Symbol 在作为唯一动作内容时仍必须由父 Button 提供 name。 |
| Interaction primitives | `MdStateLayer`、`MdRipplePresenter`、`MdFocusRing` | 基础设施 | 已从可访问树隐藏，方向正确；ripple 和 focus 外观仍需高对比/Reduced/指针来源的视觉与真机验证。 |
| Scrolling | `MdScrollViewer`、`MdScrollBar` | 部分实现 | 原生滚动语义仍在；自定义 inertia、auto-hide、pointer/touch 和平台滚轮行为需要 Windows/macOS/Linux 实测。 |
| Numeric input | `MdNumericBox` | 部分实现 | 保留原生 NumericUpDown 输入基础；默认 spinner 约 32×20dp，只适合显式 desktop density，不满足触控目标。 |
| Settings surfaces | `MdSettingsCard`、`MdSettingsExpander`、`MdSettingsGroup` | 部分实现 | 属于 Material-style/应用模式而非当前 M3 一等组件；展开状态、标题关系、嵌套操作与大字模式需补证据。 |
| Desktop window chrome | `MdWindow`、`MdBorderlessWindow`、`MdWindowTitleBar`、`MdCaptionButton`/`MdWindowCaptionButton`、`MdWindowDragRegion`、`MdWindowResizeGrip`、各 platform adapter | 平台验证待完成 | 模板、caption 名称和 adapter 架构已有测试；snap/system menu、多屏负坐标、100/125/150/200% DPI、macOS/Linux 原生行为尚未由本轮静态审计批准。 |
| Popup/lifetime infrastructure | `IMdPopupOwner`、`MdPopupCoordinator`、`MdPresenceController` 等 | 基础设施 | 多个 exit 生命周期已比瞬时卸载稳健；不能替代每个组件各自的 focus、Automation、placement 和 light-dismiss 规范。 |
| Models/controllers/presenters | `MdCalendarDay`、`MdSliderThumb`、`MdCarouselController`、各 EventArgs、Extra 的 row/cell/day/message/pin/timeline presenter、converter/record | 并入父组件 | 不作为独立官方组件评分；其触控、Automation 和视觉行为已在父组件对应条目中评估。 |

---

## 7. 横向问题分析

### 7.1 Automation

仓库静态扫描没有自定义 AutomationPeer。继承自 `Button`、`TextBox`、`ListBox`、`Slider`、`ProgressBar` 的控件可以保留部分原生 peer，这是优点；但以下自定义交互模型不能靠基类自动表达：

- 两 thumb range slider；
- 自绘 rating、time dial、chart；
- calendar grid/day/range；
- tree level/expanded；
- data grid row/column/cell/sort；
- dialog/modal boundary；
- menu/submenu expanded/checked；
- chat log/new-message announcement。

现有 `MdReleaseHardeningTests.Accessibility_Metadata_And_Native_Automation_Peers_Are_Preserved` 只检查 peer 非 null、Snackbar live setting 和少量名称。**“peer 非 null”不等于 role、pattern、children、state 和 bounds 正确。**

### 7.2 RTL

Core 控件中显式 `FlowDirection` 处理几乎只出现于 `MdTimeDial` 的文本绘制，而且还固定为 `LeftToRight`。Extra 只有 TreeView 和 Slidable 部分处理。Avalonia 可以自动镜像部分布局，但不会自动修正：

- 自绘 X 坐标；
- Left/Right 键的逻辑意义；
- Popup `RightEdgeAlignedTop`；
- `ChevronLeft/Right` 的上一页/下一页；
- 物理 CornerRadius；
- start/end action pane；
- 固定 `HorizontalAlignment=Left/Right` 与四值 Margin。

因此不能用“FlowDirection 会自动继承”作为全库 RTL 通过证据。

### 7.3 Motion

积极点：

- `MdMotionScheme` 提供 Expressive/Standard/Reduced/None；
- motion token 分为 spatial/effects 和 fast/default/slow；
- 多个 Popup 用 `MdPresenceController` 保留 exit 生命周期；
- direct manipulation 期间部分控件会关闭 transition。

仍有三个系统性问题：

1. 不少组件只有状态切换，没有 transition choreography；Extra motion controls 最严重。
2. 固定 duration 的 sampled spring 不能保证打断/重定向时保持速度；与当前 physics system 的核心收益仍有差距。
3. `:reduced-motion` selector 的存在不等于合规。Carousel 在 Reduced 下仍改变 item size，就是反例。

### 7.4 本地化和长文本

核心 picker 已引入 `MdLocalization`，这是正确基础；但仍有大量默认英文：

- “Select date”、Previous/Next month、Open picker；
- Remove chip、Clear text、Show or hide password；
- Rows per page；
- Pull/Release/Refreshing；
- Extra 的 Retry、Cancel、Quote、Delete、No results、Filter、Move selected、Tags、Hierarchy、Verification code。

Automation 名称同样需要本地化。只本地化可见按钮而保留英文 name 仍不完整。

### 7.5 200% 字体和紧凑宽度

`docs/RELEASE_VALIDATION.md:33-36` 中 Windows 200% + Narrator、macOS VoiceOver、Linux Orca 仍是未勾选项。本轮未找到可以替代这些项目的自动化证据。固定宽度 dialog/palette/license page、单行按钮组、transfer 三列、breadcrumb 和 app bar 是高风险区域。

---

## 8. 测试与验证边界

### 已有且可作为回归证据

- 当前基线曾通过 CI、NuGet 和 multi-platform gallery 三条 workflow。
- Headless 测试约 233 个（10 个 `[Fact]`、223 个 `[AvaloniaFact]`），覆盖许多模板加载、几何、属性、输入和 motion 生命周期回归。
- 已有 gallery 截图审计可证明部分固定 viewport 下能够渲染。

### 不能由上述证据推出

- 不能推出所有交互目标 ≥48dp；核心 DatePicker Button 明确为 36dp。
- 不能推出 Automation role/pattern 正确；peer 非 null 不够。
- 不能推出 Narrator、VoiceOver、Orca announcement 正确。
- 不能推出 Windows/macOS/Linux/Android 的 Popup、IME、窗口装饰一致。
- 不能推出 200% 字体、OS high contrast、触控/笔、120Hz motion 或 RTL 全通过。
- 不能推出视觉与当前 M3 Expressive token 逐像素一致。

### 修复后必须新增的测试层

1. **源码/资源约束**：禁止未批准 hex、缺失 token、公开未消费属性、缺主题的 concrete control。
2. **Headless 几何**：真实模板 `Bounds`、48dp 命中、popup placement、360/600/840/1200/1600、200% 字体。
3. **输入状态机**：pointer/touch/pen/keyboard、焦点进入/恢复、快速反转、取消、disabled。
4. **Automation tree**：role/name/value/state/pattern/children/bounds/live setting。
5. **视觉金图**：Light/Dark、contrast levels、LTR/RTL、normal/reduced/no motion 的稳定帧。
6. **平台人工矩阵**：Narrator、VoiceOver、Orca、Android TalkBack；Windows 100/125/150/200% DPI；macOS trackpad；Linux Wayland/X11。
7. **motion trace**：enter/exit 生命周期、retarget velocity、Reduced 禁止 spatial、None 无延时。

---

## 9. 建议修复顺序

### 阶段 A：先修证据与语义，不动大面积视觉

1. 撤销/标记旧 100% 报告，补 spec manifest。
2. 建 Automation test harness 和 modal focus scope。
3. 修 Menu、Dialog/Drawer/Sheet、RangeSlider、Date/Time Picker。
4. 将未实现 motion/parity 类型标 experimental 或从“完成清单”移除。

### 阶段 B：交互目标、RTL、Reduced Motion

1. Chip、Segmented、日期、Rating、small FAB、sheet handle 的 48dp hit target。
2. 所有 custom-drawn/custom-gesture 组件逐项 RTL。
3. Carousel Reduced 固定尺寸；清理所有 spatial motion 泄漏。
4. Search/FAB menu/Tooltip/Popover/CommandPalette 焦点与生命周期。

### 阶段 C：Extra 的公开 API 可信度

1. ColorPicker TwoWay Hex/alpha。
2. Breadcrumb collapse/link。
3. Cascader Popup 和跨列键盘。
4. Grid/Tree/Chart/Rating/Calendar/Chat 的 Automation model。
5. Container transform/shared axis/fade through/animated visibility 真正实现。

### 阶段 D：视觉与平台验证

1. App bar、toolbar、split/group component tokens。
2. scrim/shadow token 化。
3. 360dp + 200% 字体 + 长文案 + RTL 视觉金图。
4. Windows/macOS/Linux/Android 屏幕阅读器与真实输入验证。

---

## 10. 合规声明的退出标准

只有同时满足以下条件，才建议恢复强合规措辞：

- 每个“官方 M3”组件都有对应官方 URL/快照、token 映射、交互/AX/RTL/motion 测试。
- Flutter parity 组件锁定明确 Flutter stable 版本与源码 commit，且标明完全/部分 parity。
- Extra 不再被宣传为官方 M3 组件。
- P0 全部关闭，P1 至少没有可访问性、键盘、触控、RTL 和公开 API 不一致项。
- 三大桌面平台至少完成 keyboard-only + screen reader；Android 完成 touch + TalkBack。
- 360dp 与 200% 字体无关键内容裁剪；LTR/RTL、Light/Dark、contrast levels 有金图。
- Reduced Motion 移除 spatial/parallax/动态尺寸；None 没有动画等待或延迟卸载。
- 规范脚本的每一条 PASS 都能指向真实实现或运行证据，且没有无条件通过项。

## 最终判断

**当前状态：功能覆盖广、基础架构有进展，但属于“Material 3 风格组件库 + 部分 Expressive/Flutter parity”，不是已经认证的完整 M3/Flutter 合规实现。**

最优先的工作不是继续增加组件数量，而是让现有公开类型的键盘、Automation、RTL、Reduced Motion、焦点生命周期与测试证据变得可信。
