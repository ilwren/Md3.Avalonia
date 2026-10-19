# Flutter behavior boundaries / Flutter 行为边界

Md3.Avalonia uses Avalonia controls, properties, and lifecycles. Similar type names do not imply identical Flutter APIs, rendering, routing, platform adaptation, or accessibility. `Md3.Avalonia.Extra` is an optional extension package, not part of the official Material component catalog.

Md3.Avalonia 使用 Avalonia 控件、属性与生命周期。相似的类型名称不代表 Flutter API、绘制、路由、平台适配或辅助功能完全一致。`Md3.Avalonia.Extra` 是可选扩展包，不属于官方 Material 组件目录。

The table describes the limited scope currently exposed by the public API. Verify behavior against the control source and tests.

下表说明当前公开的有限实现范围；应以控件源码和测试为准。

| API | Status / 状态 | Implemented behavior and limits / 已实现范围与边界 |
|---|---|---|
| `MdHero` | Implemented subset / 实现子集 | Provides snapshot transitions between source and destination and supports cancellation; does not cover every Flutter clipping, shadow, or multi-window case. / 提供源/目标之间的快照过渡与取消处理；不承诺覆盖 Flutter 的全部剪裁、阴影及多窗口情形。 |
| `MdReorderableList` | Implemented subset / 实现子集 | Supports drag reordering, auto-scroll, and keyboard alternatives; it does not reproduce Flutter's complete delegate API. / 支持拖动排序、自动滚动与键盘替代操作；不复刻 Flutter 的全部 delegate API。 |
| `MdDismissible` | Implemented subset / 实现子集 | Supports gesture thresholds, confirmation, velocity, and dismissal lifecycle; keyboard operation and notification coverage remain limited. / 支持手势阈值、确认、速度与收起生命周期；键盘操作和通知覆盖仍有限。 |
| `MdFocusTraversalGroup` | Implemented subset / 实现子集 | Provides visual order, `TabIndex`, and geometric reading-order strategies using Avalonia focus primitives; it does not accept Flutter strategy objects. / 提供视觉顺序、TabIndex 和几何阅读顺序策略；使用 Avalonia 焦点原语，不接收 Flutter 策略对象。 |
| `MdKeyboardAvoidingHost` | Implemented subset / 实现子集 | Uses the height reported by Avalonia `TopLevel.InputPane` to add bottom padding and can scroll the focused control into view. It does not compute rectangle overlap; a floating keyboard that reports no occlusion causes no inset. / 使用 Avalonia `TopLevel.InputPane` 报告的高度增加底部内距，并可滚动聚焦控件；不计算矩形重叠，未报告遮挡的浮动键盘不会触发内缩。 |
| `MdSimpleDialog` | Partial implementation / 部分实现 | Provides a Material-style option list and dismissal interaction; it is not a Flutter Navigator route or a replacement for `showDialog<T>`. / 提供 Material 风格选项列表与关闭交互；不是 Flutter Navigator route，也不是 `showDialog<T>` 的替代 API。 |
| `MdPaginatedDataTable` | API subset / API 子集 | Provides basic paging; it does not include Flutter `DataTable`'s complete column, cell, and sorting model. / 提供基础分页列表；不包含 Flutter `DataTable` 的完整列、单元格与排序模型。 |
| `MdAdaptiveSwitch` | API subset / API 子集 | The `:cupertino` state adjusts minimum size while the other states reuse `MdSwitch`; it is not a complete visual or behavioral match for Flutter Cupertino switch. / `:cupertino` 状态调整最小尺寸，其余复用 `MdSwitch`；不等同于 Flutter Cupertino switch 的完整外观与行为。 |
| `MdAdaptiveProgressIndicator` | API subset / API 子集 | Reuses the Material circular progress control; the Cupertino state adjusts stroke and track only and does not implement the Cupertino activity indicator. / 复用 Material 圆形进度控件；Cupertino 状态仅调整笔画与轨道，不实现 Cupertino activity indicator。 |
| `MdAboutDialog` | API subset / API 子集 | Displays application metadata and raises an event to request license content; it does not include Flutter routing or the full license flow. / 显示应用元数据并通过事件请求许可内容；不包含 Flutter 的路由和完整许可流程。 |
| `MdContainerTransform`, `MdAnimatedVisibility` (Extra) /（Extra） | Implemented subset / 实现子集 | Provide container or visibility transitions; they do not match Flutter's complete shared-element or interruptible-velocity models. / 提供容器或可见性过渡；不等同于 Flutter 的完整共享元素或中断速度模型。 |
| `MdSharedAxis`, `MdFadeThrough` (Extra) /（Extra） | Partial implementation / 部分实现 | Switch content using a retained presenter. When the old content is a `Control`, both contents cannot be presented simultaneously, so its exit/overlap phase is skipped while the new content still enters. / 通过保留 presenter 实现内容切换；旧内容为 `Control` 时无法进行同时呈现，跳过其退出/重叠阶段，但新内容仍执行入场过渡。 |

Types marked `[MdExperimental]` are not evidence of completed Flutter parity. The project does not publish an overall parity percentage; every compatibility claim must be limited to the APIs, tested cases, and boundaries listed here.

标记 `[MdExperimental]` 的类型不作为已完成的 Flutter parity 证据。项目不发布整体 parity 百分比；行为对齐声明必须限定到表中 API、已测试的情形及其边界。
