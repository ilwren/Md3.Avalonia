# Flutter 行为边界

Md3.Avalonia 使用 Avalonia 控件、属性与生命周期。相似的类型名称不代表 Flutter API、绘制、路由、平台适配或辅助功能完全一致。`Md3.Avalonia.Extra` 是可选扩展包，不属于官方 Material 组件目录。

下表说明当前公开的有限实现范围；应以控件源码和测试为准。

| API | 状态 | 已实现范围与边界 |
|---|---|---|
| `MdHero` | 实现子集 | 提供源/目标之间的快照过渡与取消处理；不承诺覆盖 Flutter 的全部剪裁、阴影及多窗口情形。 |
| `MdReorderableList` | 实现子集 | 支持拖动排序、自动滚动与键盘替代操作；不复刻 Flutter 的全部 delegate API。 |
| `MdDismissible` | 实现子集 | 支持手势阈值、确认、速度与收起生命周期；键盘操作和通知覆盖仍有限。 |
| `MdFocusTraversalGroup` | 实现子集 | 提供视觉顺序、TabIndex 和几何阅读顺序策略；使用 Avalonia 焦点原语，不接收 Flutter 策略对象。 |
| `MdKeyboardAvoidingHost` | 实现子集 | 使用 Avalonia `TopLevel.InputPane` 报告的高度增加底部内距，并可滚动聚焦控件；不计算矩形重叠，未报告遮挡的浮动键盘不会触发内缩。 |
| `MdSimpleDialog` | 部分实现 | 提供 Material 风格选项列表与关闭交互；不是 Flutter Navigator route，也不是 `showDialog<T>` 的替代 API。 |
| `MdPaginatedDataTable` | API 子集 | 提供基础分页列表；不包含 Flutter `DataTable` 的完整列、单元格与排序模型。 |
| `MdAdaptiveSwitch` | API 子集 | `:cupertino` 状态调整最小尺寸，其余复用 `MdSwitch`；不等同于 Flutter Cupertino switch 的完整外观与行为。 |
| `MdAdaptiveProgressIndicator` | API 子集 | 复用 Material 圆形进度控件；Cupertino 状态仅调整笔画与轨道，不实现 Cupertino activity indicator。 |
| `MdAboutDialog` | API 子集 | 显示应用元数据并通过事件请求许可内容；不包含 Flutter 的路由和完整许可流程。 |
| `MdContainerTransform`、`MdAnimatedVisibility`（Extra） | 实现子集 | 提供容器或可见性过渡；不等同于 Flutter 的完整共享元素或中断速度模型。 |
| `MdSharedAxis`、`MdFadeThrough`（Extra） | 部分实现 | 通过保留 presenter 实现内容切换；旧内容为 `Control` 时无法进行同时呈现，跳过其退出/重叠阶段，但新内容仍执行入场过渡。 |

`[MdExperimental]` 标记的类型不作为已完成的 Flutter parity 证据。项目不发布整体 parity 百分比；行为对齐声明必须限定到表中 API、已测试的情形及其边界。
