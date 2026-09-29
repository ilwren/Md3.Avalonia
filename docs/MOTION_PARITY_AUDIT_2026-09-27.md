# Md3.Avalonia 动画与动效逐控件审计

**日期：** 2026-09-27  
**审计对象：** `Md3.Avalonia`、Flutter parity 控件、`Md3.Avalonia.Ecosystem`  
**审计方式：** Headless 连续帧截图 + 像素差异 + Visual Tree/Transition 状态 + 源码逐项核对

## 0. P1/P2 修复复验（2026-09-28）

本报告保留 2026-09-27 的原始缺陷证据；以下复验结果覆盖并取代原 P1/P2 的“未修复”判定。

| 编号 | 复验状态 | 修复结果 |
|---|---|---|
| P1-01 | **已关闭** | 13 个主题中的固定 AXAML Transition 已清零；Button/Card/selection/TextBox/Combo/FAB/Tab/PIN 等改由运行时 `MdMotionTransitions` 构建。Reduced 无 spatial，None 无 Transition。 |
| P1-02 | **已关闭** | Circular/Linear/Loading/Ripple 均响应运行中 scheme 变化；Reduced 停止 ambient/spatial/morph，Ripple 仅保留 100ms 全尺寸淡出；None 立即停表。 |
| P1-03 | **已关闭** | Hero 已实现 `OverlayLayer` 双快照 shared-element flight、bounds 插值、cross-fade、同 Tag 替换、完成/取消、scheme 切换及 source/destination detach 清理。 |
| P1-04 | **已关闭** | Draggable sheet 已分离可等待/可取消 `AnimateToAsync`、`AnimateTo` 与即时 `JumpTo`；拖拽期间无 Transition，release 后才按 scheme settle。 |
| P1-05 | **已关闭（Headless 进程内）** | Snackbar/Dialog 的挂载与 open 目标拆到不同 render turn；退出延迟卸载，快速反转版本化，None 同步完成。真实窗口组合效果仍属于平台复核范围。 |
| P2-01 | **已关闭（Popup host 受限）** | Cascader/AsyncSelect 均有 desired-state/presence、真实 enter/exit 初始帧、快速反转、surface/arrow scheme motion；并接入跨 Core/Ecosystem popup owner replacement。 |
| P2-02 | **已关闭** | RangeSlider、Slidable 已补 pointer capture-lost，统一恢复 drag 状态和 release settle。 |
| P2-03 | **已关闭** | FAB menu 的 close duration 改由 resolver 提供；None 即时卸载；关闭 timer 和延迟 open frame 在 detach 时取消，异步帧不再回写 detached visual。 |
| P2-04 | **已关闭** | Tree/Transfer 的待执行与进行中 FLIP 在 scheme 变化时版本失效并清除残留；AnimationSequence 立即取消并恢复稳定终态。 |

复验命令使用 .NET SDK 10.0.401，并为不随仓库分发的 Icons 字体契约显式传入测试替代路径：

- Release solution build：**0 warnings / 0 errors**；
- 正式 Headless 完整测试：**204/204 passed**；
- `MdMotionLifecycleTests`：由 24 项扩展为 **32/32 passed**；
- 源码静态复查：`src` 内固定 AXAML Transition **0**，统一工厂外直接构造 Transition **0**；
- 最终构建产物清理结果见文末复验记录。

> 上述结果仍只证明进程内 Visual Tree、状态机、timer/cancellation 与 Headless 可渲染路径；不把它表述为真实设备精确时长、GPU 性能、native Popup/window 或 Android 触摸表现已经验证。

## 1. 结论

可以用 Headless 连续截图看到并核对大部分**进程内 Visual Tree 动画**，但不能把它当作真机渲染性能测试。

本轮已证明可以捕获：

- 选择态的颜色、透明度、形状及位置中间帧；
- Dialog、Drawer、Sheet、Banner、FAB menu、Search、Expansion 等进场方向；
- Sheet、Snackbar 等退场期间内容保留；
- 快速 open → close → open 的反向切换；
- Slider、RangeSlider 拖拽时的逐点直跟，以及 release 后的 settle；
- indeterminate/loading/skeleton 等持续动画；
- `Expressive`、`Reduced`、`None` 三种 scheme 的实际画面差异。

2026-09-27 的原始审计判定是“尚不能视为完整 parity”，并以 `MdSwitch` 的 None 中间帧证明了固定 AXAML Transition 绕过。2026-09-28 复验确认这些 P1/P2 实现缺口已全部关闭；原始证据继续保留，用于说明修复动机和回归边界。

### 总体判定（修复复验后）

| 维度 | 判定 |
|---|---|
| Headless 能否看到中间帧 | **能** |
| token-aware 控件的方向/中间态 | **P1/P2 范围通过** |
| transient surface 的 presence | **挂载/进入/退出/反转状态通过；native host 仍需平台复核** |
| 快速反向切换 | **Drawer、Snackbar、Dialog、Cascader、AsyncSelect 代表路径通过** |
| 直接操控 1:1 | **核心路径及 RangeSlider/Slidable capture-lost 回归通过** |
| Reduced/None | **P1/P2 范围通过：Reduced 无空间运动，None 即时完成** |
| 定时器生命周期 | **P1/P2 范围通过：runtime scheme 与 detach 均有停止/重启/取消路径** |
| native Popup/window/Android | **Headless 不能完整验证，必须真机/真实桌面 host 验收** |

## 2. 审计规模与测试结果

- 枚举了 **114 个 Core/Flutter public control/helper class** 和 **32 个 Ecosystem class**，共 **146 个**。
- Core 中约 **35 个 code-behind motion-aware 文件**。
- Ecosystem 中 **6 个包含 motion 逻辑的文件**。
- 原始审计找到 **13 个含固定 AXAML Transition 的主题文件**；修复复查为 **0**。
- 临时连续帧测试：**6/6 passed**。
- 原始审计时既有 motion lifecycle 测试：**24/24 passed**；修复复验后为 **32/32 passed**，完整套件 **204/204 passed**。
- 每组序列通常包含 `before`、`t0`、`t35`、`t70`、`t120`、`t220`、`t400`；直接操控序列按每次 pointer move 截图。
- 所有帧均生成 SHA-256、相邻帧 changed-pixel 数、变化包围盒和 RGB mean absolute delta。

> Headless 的动画时钟与墙钟并不等价。本环境中很多声明为 150–435ms 的 Transition 在约 70ms 墙钟后已稳定，因此这些帧只用于证明“是否存在中间态、方向是否正确、内容是否保留”，**不能**反推真实 duration。

## 3. 视觉证据

证据文件位于 [`docs/motion-audit-evidence/`](motion-audit-evidence/)：

- [选择控件连续帧](motion-audit-evidence/selection-timeline.jpg)
- [Surface 进场矩阵](motion-audit-evidence/surface-entrances.jpg)
- [直接操控与 release](motion-audit-evidence/direct-manipulation.jpg)
- [Expressive / Reduced / None 对照](motion-audit-evidence/motion-scheme-switch.jpg)
- [退场内容保留与反向切换](motion-audit-evidence/exit-and-reversal.jpg)
- [持续动画与值变化](motion-audit-evidence/ambient-value.jpg)
- [逐帧像素指标](motion-audit-evidence/frame-metrics.tsv)

代表性结果（原始审计帧，保留作修复前证据）：

1. **Switch**：`before → t0 → t35 → t70` 的 thumb 位置和 track 颜色均不同，证明不是只截到起止帧。
2. **Selection 组合**：Toggle、Checkbox、Radio、Switch、Filter chip、Segmented button 均有状态变化；中间帧可见。
3. **Dialog/Drawer/Sheet/Banner**：可见 scale/translate/opacity 进场；Drawer 的 placement 方向和 Sheet 的底部进场方向正确。
4. **Drawer reversal**：打开中途关闭、关闭中途重开均连续，没有旧内容重新挂载造成的空白帧。
5. **Slider/RangeSlider**：每次 pointer move 后 `VisualValue == Value`，截图中 handle 与轨道立即到新位置；拖拽期间 `Transitions == null`。
6. **Scheme=None 的 Switch**：仍能看到 t0/t35/t70 三个不同位置，直接证明固定 AXAML Transition 绕过了 `MdMotionScheme.None`。
7. **Snackbar**：进场在首个截图已到终态，未捕获到可见中间帧；退场则能看到保留内容的淡出中间帧。
8. **Dialog**：源码中的 `MdPresenceController` 保持 `:present`，结构测试也通过，但 Headless 退场 t0 已不可见；这不是“已验证通过”，而是需要真实 host 复核的异常。

## 4. Motion token、duration 与 easing

`src/Md3.Avalonia/Motion/` 的主干设计是正确的：

- `MdMotion.Resolve` 区分 `Spatial` 与 `Effects`；
- `None` 返回 disabled/zero duration；
- `Reduced` 禁用 spatial，只保留 100ms effects；
- Standard spatial：225/320/485ms；
- Expressive spatial：360/435/600ms；
- Effects：150/230/325ms；
- `MdSpringEasing` 使用冻结 spring token，并在 endpoint 精确落到 1；
- `MdMotionTransitions` 统一创建 Double/Vector/Brush/Transform Transition。

这与当前 Material 3 以 spring/physics 为主的方向一致。问题不在 resolver，而在一批控件完全绕过 resolver。

### 固定 AXAML Transition 清单（修复前证据）

以下主题在原始审计时存在 80–250ms 固定时长，且多数没有 `:reduced-motion`/`:no-motion` 覆盖；2026-09-28 复查时这些固定 Transition 已全部迁出，源码 grep 为 0：

- `MdButton.axaml`
- `MdCard.axaml`
- `MdCheckBox.axaml`
- `MdComboBox.axaml`
- `MdFab.axaml`
- `MdFlutterAdvanced.axaml`
- `MdIconButton.axaml`
- `MdRadioButton.axaml`
- `MdSwitch.axaml`
- `MdTabView.axaml`
- `MdTextBox.axaml`
- `MdToggleButton.axaml`
- `Md3.Avalonia.Ecosystem/Themes/WaveFControls.axaml`

`MdComboBox.axaml` 的 popup surface 本身另有 code-behind token-aware Transition，但 label/arrow 等内部固定 Transition 仍不遵循 scheme。

## 5. 高优先级问题

### P1-01：固定 AXAML Transition 绕过 Reduced/None

**修复状态：已关闭（2026-09-28）。** 固定 AXAML Transition 已清零并迁移到继承 scheme 感知的统一工厂；正式测试覆盖 Standard → Reduced → None 运行时切换。

**原始影响控件：** Button、Card、Checkbox、Radio、Switch/AdaptiveSwitch、IconButton/ToggleIconButton、ToggleButton、TextBox/SearchBar/Numeric/部分 Combo 内部、FAB menu、TabViewItem、DraggableScrollableSheet、PIN cell 等。

**证据：** `MdSwitch` 在 `MdMotionScheme.None` 下仍存在 thumb travel 中间帧。

**影响：**

- `None` 不能真正关闭动画；
- `Reduced` 仍进行位移、缩放、旋转、corner morph；
- 同一应用内 token-aware 和固定 Transition 的节奏不一致。

**建议：** 把模板中的固定 Transition 移到 control code 的 `UpdateMotion()`，或提供按继承 scheme 选择的 theme resource；`Reduced` 只允许短 effect fade，`None` 必须移除所有 Transition。

### P1-02：indeterminate/loading/ripple 的运行时 scheme 生命周期不完整

**修复状态：已关闭（2026-09-28）。** 四类控件均订阅运行中 scheme 变化；正式测试验证 timer 停止、重新启用及 detach 清理。

**原始涉及文件：**

- `MdCircularProgressIndicator.cs`
- `MdLinearProgressIndicator.cs`
- `MdLoadingIndicator.cs`
- `MdRipplePresenter.cs`

前三个进度/加载控件没有订阅 `MdMotion.SchemeProperty.Changed`：

- attach 时为 `Expressive` 后切到 `None`，Circular/Linear 的 timer 不会停止，画面继续旋转/移动；
- attach 时为 `None` 后切回 `Expressive`，timer 不会自动启动；
- Loading 在 `Reduced` 下仍进行 shape morph + rotation，只是速度乘 0.55，与“Reduced 移除 spatial/morph”不一致；
- Ripple 在 Reduced 下仍从原点扩张半径，而不是只保留短 effect；动画中切到 None 也不会立即停止。

**定时器 detach 行为本身正确**，问题是运行时 scheme 变化。

### P1-03：`MdHero` 不是实际 shared-element 动画

**修复状态：已关闭（2026-09-28）。** 现已由 `OverlayLayer` 承载 source/destination 快照，支持空间 flight、Reduced cross-fade、None 即时完成、同 Tag 串行替换、取消、完成事件和双方 detach 清理；Headless 正式测试验证 overlay child 的创建与回收。

原始实现中的 `MdHero.RequestTransitionTo()` 只发出 `TransitionRequested` 事件并传递 source/destination bounds；库内没有 overlay clone、路径插值、cross-fade、取消/反转或 lifecycle controller。

因此它只能归类为 **host integration hook**，不能宣称 Flutter `Hero` 动画已实现。

### P1-04：`MdDraggableScrollableSheet` 的 API 与 motion 语义不正确

**修复状态：已关闭（2026-09-28）。** `AnimateToAsync` 可等待/取消，`AnimateTo` 启动 token-aware settle，`JumpTo` 同步跳转；drag 无 Transition，release 后才恢复 settle。

原始问题：

- `AnimateTo()` 与 `JumpTo()` 都只调用同一个 `SetExtent()`；
- 模板对 Height 使用固定 250ms Transition；
- 因此 `JumpTo()` 也会动画，`AnimateTo()` 没有独立可等待/可取消动画；
- Reduced/None 不能移除这段固定 settle；
- 拖拽阶段通过 `:dragging` 去除 Transition，1:1 思路正确。

### P1-05：Snackbar 进场和 Dialog 退场的可见帧异常

**修复状态：已关闭（进程内 Headless，2026-09-28）。** present 与 open 目标已拆成两个 render turn，退出延迟卸载并以版本号保护快速反转；None 同步完成，detach 后待执行帧失效。native window 最终组合效果仍保留平台验收限制。

原始问题：

- `MdSnackbar` 将 `:present` 与 `:open` 在同一同步更新中设置，Headless 首帧已经到终态，没有捕获到 enter transition；退出淡出正常。
- `MdDialogHost` 的 presence controller 和 Transition 声明均存在，但关闭后的第一张截图已不可见；Sheet/Snackbar 的相同审计方法能捕获退场中间帧。

需要在真实桌面 host 上确认是否同样跳变；若复现，应把“挂载”和“进入目标状态”拆到两个 render turn，并核对 nested overlay/presenter opacity 的退出顺序。

### P2-01：Ecosystem popup 族动效不一致

**修复状态：已关闭（状态机/Visual Tree，2026-09-28）。** AsyncSelect/Cascader 已统一 desired state、presence、分帧 enter、延迟 exit、反转、scheme surface motion 和 popup owner replacement；native Popup 合成仍受 Headless 能力限制。

原始对比：

- `MdPopover`/`MdHoverCard`/`MdCommandPalette`：token-aware，presence 与 reduced 处理完整。
- `MdAsyncSelect`：native Popup 直接开关，没有进退场状态和 presence。
- `MdCascader`：内联 Border 直接 `IsVisible`，没有进退场动画，也没有 `MdMotion` scheme。

同一 ecosystem 的 transient surface 行为不一致。

### P2-02：pointer capture 丢失路径不完整

**修复状态：已关闭（2026-09-28）。** RangeSlider 与 Slidable 的 capture-lost 均复用 release/settle 清理路径，正式 Headless pointer capture 转移测试通过。

原始问题：

- `MdRangeSlider` 没有 `OnPointerCaptureLost`，系统取消 capture 时可能残留 `_dragging=true`、`Transitions=null`。
- `MdSlidableItem` 同样没有 capture-lost reset/settle。
- `MdTimeDial`、`MdRefreshIndicator`、`MdTransfer`、Draggable sheet 已处理 capture-lost。

### P2-03：FAB menu 的 fixed timer 与 detach

**修复状态：已关闭（2026-09-28）。** close timer 改用 resolver exit duration，None 即时卸载；detach 显式停表并取消版本化 open frame，测试验证等待超过原退出时长后 detached visual 不再变化。

原始实现中的 `MdFabMenu` 使用固定 220ms close timer，与 scheme 无关；`None` 下仍延迟卸载。类中没有 detach 时停止 `_closeTimer`。虽然 timer 是一次性并会自行停止，仍不符合严格 lifecycle 要求。

### P2-04：Tree/Transfer/AnimationSequence 的 mid-flight scheme 切换

**修复状态：已关闭（2026-09-28）。** Tree/Transfer 在 scheme 变化和 detach 时让待执行帧失效并清除容器残留；AnimationSequence 取消 token、移除 Transition 并恢复稳定 opacity。

原始实现中的这些控件会在动画启动时读取 scheme，通常能正确决定下一次动画；但正在进行的 FLIP/sequence 在 scheme 切为 None 时不会统一取消。`MdAnimationSequence` 的普通 cancellation/detach 路径是正确的。

## 6. Core / Flutter parity 逐控件核对

图例：

- **通过-V**：连续截图看到中间帧，且源码路径合理；
- **通过-S**：源码 + lifecycle/状态测试通过，未依赖 native host；
- **受限-H**：native Popup/window/平台能力，Headless 只能验证状态与源码；
- **瞬切-I**：没有独立动画；对该类控件可接受或不属于规范必需动画；
- **缺口-G**：存在已确认问题。

| 控件 | 动画职责与核对结果 | 判定 |
|---|---|---|
| `MdButton`, `MdFloatingActionButton`, `MdExtendedFloatingActionButton` | Ripple 与 pressed corner morph 均由 scheme 接管；Reduced 去 corner spatial，None 无 Transition | **修复-通过-S** |
| `MdIconButton`, `MdToggleIconButton`, `MdToggleButton` | 选择态颜色/shape 已迁移统一工厂；运行时 Reduced/None 回归通过 | **修复-通过-S** |
| `MdCard`, `MdGridTile`, `MdGridTileBar` | Card 状态层 opacity 已接入 scheme；GridTileBar 为静态内容条 | **修复-通过-S** |
| `MdCheckBox`, `MdRadioButton`, `MdSwitch`, `MdAdaptiveSwitch` | check/dot/thumb 已迁移 scheme-aware；Reduced 仅 effects，None 无 thumb travel | **修复-通过-V/S** |
| `MdChip`, `MdAssistChip`, `MdFilterChip`, `MdInputChip`, `MdSuggestionChip` | Brush/opacity 使用 `MdMotionTransitions`；Reduced 保留 effects，None 取消；组合帧通过 | **通过-V/S** |
| `MdSegmentedButton`, `MdSegmentedButtonGroup` | 选中图标、背景和前景 effect transition；连续帧可见，scheme-aware | **通过-V/S** |
| `MdSplitButton`, `MdStandardButtonGroup`, `MdConnectedButtonGroup` | 子按钮负责 ripple/state；容器本身无独立布局动画 | **瞬切-I** |
| `MdFabMenu`, `MdFabMenuItem` | resolver 时长、Reduced/None、退出 presence、反转及 detach timer/frame 取消均已覆盖 | **修复-通过-S** |
| `MdRipplePresenter` | Reduced 为全尺寸短淡出；mid-flight None 与 detach 均立即停表 | **修复-通过-S** |
| `MdStateLayer`, `MdFocusRing` | 状态/焦点即时反馈，无独立 motion controller | **瞬切-I** |
| `MdTextBox`, `MdSearchBar`, `MdNumericBox` | floating label/input/icon Transition 已迁移运行时 scheme | **修复-通过-S** |
| `MdComboBox` | Popup presence/surface 及 arrow/label 均 scheme-aware；native Popup 仍受 Headless 限制 | **修复-通过-S/H** |
| `MdAutoCompleteBox` | Popup desired state 与 visual presence 分离，旧 popup replacement 测试通过；真实 native surface 受限 | **通过-S/H** |
| `MdDatePicker`, `MdDateRangePicker`, `MdTimePicker`, `MdDatePickerDialog`, `MdTimePickerDialog` | surface scale/translate/fade、presence、replacement、Reduced/None 源码和测试通过；native Popup 受限 | **通过-S/H** |
| `MdTimeDial` | pointer drag 直跟、release/set value 的 token motion、Reduced/None 测试通过 | **通过-S** |
| `MdBanner` | -16px → 0 + fade；进场中间帧可见，presence/controller scheme-aware | **通过-V/S** |
| `MdDialog`, `MdDialogHost` | 分帧 enter、延迟 exit、快速反转与 None 即时卸载通过；native host 仍需平台复核 | **修复-通过-S/H** |
| `MdDropdownMenu`, `MdMenuAnchor`, `MdMenu`, `MdMenuItem`, `MdSubMenuItem` | surface scale/translate/fade 由 token 控制，旧 popup 同轮关闭；native Popup 受限 | **通过-S/H** |
| `MdExpansionPanel`, `MdExpansionPanelList` | 内容淡入/位移、展开高度中间帧可见；退出挂载测试通过；列表只协调子项 | **通过-V/S** |
| `MdSearchView` | open/present/fade/transform 为 token-aware；截图可见中间态，快捷键路径独立 | **通过-V/S** |
| `MdSheetHost` | placement-aware translate、scrim fade、拖拽禁 Transition、release settle；进入/退出中间帧和内容保留可见 | **通过-V/S** |
| `MdSnackbar` | present/open 分帧，enter/exit presence、快速反转、None 与 detach 通过 | **修复-通过-S** |
| `MdTooltip`, `MdTooltipHost` | surface scale/translate/fade，hover/focus/long-press timer detach 正确；native Popup 受限 | **通过-S/H** |
| `MdSimpleDialog` | token-aware surface + presence，结构 exit 测试通过 | **通过-S** |
| `MdAboutDialog` | 静态 dialog 内容，由外部 host 提供 motion | **瞬切-I** |
| `MdCarousel`, `MdCarouselItem` | 大/中/小 item width morph、selection settle、wheel/gesture cancel、autoplay scheme/lifecycle 均有实现 | **通过-S** |
| `MdNavigationBar`, `MdNavigationBarItem`, `MdNavigationRail`, `MdNavigationRailItem` | indicator scale、icon fade、label/selection effects token-aware；Reduced 去空间 motion | **通过-S** |
| `MdNavigationDrawer` | 左/右 placement-aware 位移 + scrim；进场和 open-close-open 反转连续帧通过 | **通过-V/S** |
| `MdNavigationSuite` | compact/rail/drawer 模式切换为布局瞬切；子导航自身有 selection motion | **瞬切-I** |
| `MdTabs`, `MdTabItem` | shared indicator X/width FLIP，Reduced 改为 fade，None 立即提交 | **通过-S** |
| `MdTabView`, `MdTabViewItem` | 内容 fade-through/shared-axis 保留；item indicator 已迁移运行时 scheme | **修复-通过-S** |
| `MdStep`, `MdStepper` | active content 位移/淡入、退出 presence、Reduced/None 测试通过；Stepper 只协调 | **通过-S** |
| `MdSlider`, `MdSliderThumb` | programmatic settle 使用 token；pointer drag 时 Transition=null；逐点截图和数值相等断言通过 | **通过-V/S** |
| `MdRangeSlider` | 双 handle 直跟、programmatic settle 与 capture-lost reset 均通过 | **修复-通过-S** |
| `MdRefreshIndicator` | pull 1:1、armed/refresh、release settle、async cancellation、Reduced/None 路径均存在 | **通过-S** |
| `MdReorderableList` | drag 直跟、相邻项 reflow、边缘 auto-scroll、FLIP settle 测试通过 | **通过-S** |
| `MdDismissible` | swipe direct manipulation、确认后 dismiss/reject settle、Reduced 语义测试通过 | **通过-S** |
| `MdDraggableScrollableSheet` | drag 1:1；Animate/Jump 分离，可等待/取消，release 才按 scheme settle | **修复-通过-S** |
| `MdHero` | Overlay 双快照 shared-element flight、cross-fade、替换、取消与 detach 清理 | **修复-通过-S** |
| `MdCircularProgressIndicator`, `MdLinearProgressIndicator` | runtime scheme 启停、Reduced/None 静止与 detach stop 回归通过 | **修复-通过-V/S** |
| `MdLoadingIndicator`, `MdAdaptiveProgressIndicator` | Expressive 保留形变；Reduced/None 禁 morph/rotate，runtime timer 响应已补齐 | **修复-通过-S** |
| `MdPaginatedDataTable` | page 切换和 loading 状态为即时替换；无独立页切换动画 | **瞬切-I** |
| `MdForm`, `MdFormField`, `MdDropdownFormField` | validation/focus/error 状态依赖内部 input；没有额外容器动画 | **瞬切-I** |
| `MdLicensePage` | 搜索/选择/滚动内容即时更新，不需要独立 motion | **瞬切-I** |
| `MdAdaptiveLayout`, `MdScaffold` | breakpoint/slot 布局协调；当前为即时 re-layout | **瞬切-I** |
| `MdBadge`, `MdBadgedBox` | 数字/内容更新即时，无规范必需独立 motion | **瞬切-I** |
| `MdBottomAppBar`, `MdTopAppBar`, `MdToolbar` | 静态 surface/slot 容器；子 action 自带交互 motion | **瞬切-I** |
| `MdDataTable`, `MdDataTableRow`, `MdList`, `MdListItem` | selection/sort/state 层即时变化；无行插入/重排动画 | **瞬切-I** |
| `MdDivider`, `MdSurface`, `MdIcon`, `MdSymbolPresenter`, `MdText` | 纯绘制/排版原语，不需要独立动画 | **瞬切-I** |
| `MdScrollBar`, `MdScrollViewer` | 保留 Avalonia 原生滚动/滚动条行为，库内无新增 motion | **瞬切-I** |
| `MdFocusTraversalGroup`, `MdShortcutScope` | 输入/焦点策略容器，无视觉 motion 职责 | **瞬切-I** |
| `MdBorderlessWindow`, `MdWindow`, `MdWindowTitleBar`, `MdCaptionButton`, `MdWindowCaptionButton`, `MdWindowDragRegion`, `MdWindowResizeGrip` | caption/state 可在 Headless 看样式，但 native move/resize/maximize/window transition 必须真实平台验证 | **受限-H** |

## 7. Ecosystem 逐控件核对

| 控件 | 动画职责与核对结果 | 判定 |
|---|---|---|
| `MdPopover`, `MdHoverCard` | token-aware scale/translate/fade、exit presence、旧 host replacement、timer detach 完整；native Popup 受限 | **通过-S/H** |
| `MdCommandPalette` | backdrop + surface + item host Transition，presence、focus、Reduced/None 测试通过 | **通过-S** |
| `MdTransfer` | pointer drag、FLIP/fade、Reduced fade 及 mid-flight scheme/detach 清理均通过 | **修复-通过-S** |
| `MdTreeView` | reveal/reflow、Reduced effects 及 mid-flight scheme/detach 清理均通过 | **修复-通过-S** |
| `MdSlidableItem` | drag 直跟、release token settle 与 capture-lost 共用稳定收尾 | **修复-通过-S** |
| `MdSkeletonGroup`, `MdSkeleton` | pulse 只在 Expressive/Standard 开启，Reduced/None 和 detach 会停止 timer | **通过-V/S** |
| `MdAnimationSequence` | stagger/reverse/cancel/detach 保留；mid-flight scheme 会立即取消并稳定终态 | **修复-通过-S** |
| `MdPinInput`, `MdPinCellPresenter` | 连续 editor 保留；cell brush Transition 已迁移 scheme-aware | **修复-通过-S** |
| `MdCascader` | desired state/presence、enter/exit、反转、arrow/surface Reduced/None 已实现 | **修复-通过-S** |
| `MdAsyncSelect` | Popup desired/presence、surface enter/exit、反转、owner replacement 与 detach async 取消已实现 | **修复-通过-S/H** |
| `MdMasonryPanel` | masonry re-layout 瞬时，没有 item reflow/FLIP | **瞬切-I；若要求动态重排则为缺口** |
| `MdPagedItemsView` | loading/empty/error/data 状态即时切换；内部 indeterminate progress 继承 Core 问题 | **瞬切-I / 继承缺口** |
| `MdAvatar`, `MdAvatarGroup`, `MdBreadcrumb` | 静态展示/选择与 overflow；无规范必需独立 motion | **瞬切-I** |
| `MdDataGrid`, `MdDataGridHeaderPresenter` | sort/select/edit 状态即时更新；无行插入/排序 reflow 动画 | **瞬切-I** |
| `MdTimeline`, `MdTimelineItemPresenter`, `MdChart` | 数据重新绘制即时，没有 path/point tween；当前不应宣称 animated chart | **瞬切-I** |
| `MdRichEditor`, `MdRichTextEditor` | 编辑/选择/格式状态，无独立 motion | **瞬切-I** |
| `MdChatView`, `MdChatMessagePresenter` | 多选/引用/删除/重试交互真实；message add/remove 没有 entry/exit animation | **瞬切-I** |
| `MdRating` | pointer/keyboard 直接设置 rating，直跟而非 settle；适合直接操控 | **通过-S** |
| `MdCalendar`, `MdCalendarDayPresenter` | 月份/范围拖选状态即时更新；没有月份 shared-axis/page transition | **瞬切-I** |
| `MdResultView` | 空/错/成功内容静态呈现，无独立 motion | **瞬切-I** |
| `MdTagInput` | tag 增删真实，但 chip entry/exit 为即时更新 | **瞬切-I** |

## 8. 进入、退出、反转与内容保留

### 已确认

- `MdPresenceController` 会在关闭时延迟清除 `:present`，重开会停止 pending exit timer。
- Drawer、Sheet、Snackbar、Banner、Search、Expansion、Step、Picker、Popover、CommandPalette 等都采用 desired state 与 visual presence 分离的设计。
- Drawer 快速 open → close → open 连续帧显示：关闭方向正确，重开从当前视觉状态继续，没有旧内容空壳。
- Sheet/Snackbar 退场 t0/t35 仍有内容，随后才卸载。
- Combo/AutoComplete/Picker/Popover 的“旧 popup 自动关闭”结构测试通过。

### 未完全确认

- Dialog 的结构 presence 正确，但 Headless 可见 exit 没有中间帧。
- native Popup surface 的真实 compositor 退场无法在当前 Headless Window 中截图。
- Android back、触摸取消、系统窗口 deactivation 对反转和 focus return 的影响未验证。

## 9. 直接操控与 settle

### 已通过

- Slider：drag 时 `Transitions=null`，`VisualValue == Value`，release 后恢复 token Transition。
- RangeSlider：drag 时双 handle 直跟，release 恢复 settle。
- Refresh：pull offset 直接跟随，release 后才启用 settle。
- Carousel：gesture/wheel 开始时取消 settle，结束后 snap。
- Sheet、Draggable sheet、Dismissible、Reorder、Transfer、TimeDial：源码均区分直接操控与动画 settle。

### 修复后剩余边界

- RangeSlider、Slidable 的 capture-lost 清理已补齐并通过 Headless capture 转移回归。
- Headless 鼠标事件仍不能证明 Android 触摸采样、velocity 质量、多指取消和 nested scrolling 仲裁。

## 10. 定时动画生命周期

### 正确路径

- Carousel：autoplay、wheel snap、settle cleanup 均在 detach 停止；Reduced/None 禁止 autoplay。
- Snackbar、TooltipHost、HoverCard：timer 在 detach 停止。
- Skeleton：Reduced/None/detach 停止 pulse timer 和 stopwatch。
- Refresh：detach 会取消 refresh token 并停止 settle timer。
- Circular/Linear/Loading/Ripple：detach 会停止 timer。
- AnimationSequence：detach 会 cancel sequence。

### 修复复验

- Circular/Linear/Loading/Ripple 已对**运行中 scheme 变化**立即启停或降级。
- FAB menu close timer 已按 resolver 定时并在 detach 显式停止；延迟 open frame 可取消。
- Transfer/Tree/AnimationSequence 的 mid-flight scheme 变化会统一取消并清除残留状态。

## 11. Headless 能证明与不能证明的边界

### 能证明

- 是否存在可见中间帧；
- 位移、缩放、透明度、颜色和 shape 的方向；
- enter/exit 的 visual presence；
- open/close/open 是否可反转；
- 直接操控是否立即更新视觉值；
- Reduced/None 最终是否仍有 Transition；
- timer/cancellation/detach 的进程内状态。

### 不能证明

- 真实设备上的精确 duration；
- 60/90/120Hz 是否流畅、是否丢帧；
- GPU/compositor 性能和功耗；
- native Popup、系统 Window 动画的最终组合效果；
- Android 触摸采样、nested scroll、back gesture、窗口 inset；
- 触觉反馈；
- 屏幕阅读器和平台 accessibility 动画偏好映射。

因此不能用“Headless 在约 70ms 后稳定”断言源码 200/320/435ms duration 错误，也不能据此宣称真机流畅。

## 12. 修复执行记录与后续平台矩阵

1. **已完成：唯一 motion 入口**——13 个主题文件中的固定 Transition 已移除，统一由 `MdMotionTransitions` 和 inherited scheme 驱动。
2. **已完成：自动化不变量**——正式测试覆盖 None 无 Transition、Reduced 无 spatial/geometry、运行时切换和 detach。
3. **已完成：ambient 生命周期**——Circular/Linear/Loading/Ripple 在 None 立即停止，重新启用时恢复，detach 停表。
4. **已完成：Flutter 语义**——Hero 有实际 overlay flight；Draggable sheet 已分离 `JumpTo` 与可取消 `AnimateToAsync`。
5. **已完成：enter/exit**——Snackbar/Dialog 分帧进入、延迟退出、快速反转和 None 即时路径均已覆盖。
6. **已完成：capture-lost**——RangeSlider、Slidable 恢复交互状态并重新启用 settle。
7. **已完成：Ecosystem popup**——AsyncSelect、Cascader 使用 desired-state/presence/token motion，并参与 popup owner replacement。
8. **仍需真实平台执行，不属于 Headless 可证明范围**——Windows/Linux/macOS/Android 上验证 native Popup、窗口、触摸、刷新率、系统 Reduce Motion 映射和性能。

## 13. 最终审计意见

2026-09-28 复验后，报告列出的 P1/P2 已全部关闭：固定 AXAML Transition 与并行 motion 系统已消除；Reduced/None 契约、ambient timer、Hero、Draggable sheet、transient presence、capture-lost、FAB lifecycle 和 Ecosystem mid-flight 取消均已有生产实现与正式 Headless 回归。

Release solution build 为 **0 warnings / 0 errors**，正式 Headless 完整套件为 **204/204 passed**。验证完成后已删除全部 `bin`、`obj`、`TestResults`，最终扫描剩余目录数为 **0**。因此可以在限定语义下表述为：**本报告所列 P1/P2 的进程内 Visual Tree 与生命周期问题已修复并通过自动化复验。**

仍不能把这一定义扩大为真实设备性能或所有平台集成已经验证。native Popup/window 的最终合成、真实设备精确时长、GPU/compositor 性能、Android 触摸与 nested scrolling 仍必须在对应平台执行矩阵验收。
