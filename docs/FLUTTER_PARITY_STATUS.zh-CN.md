---
_lang: zh-CN
---

# Flutter 行为边界

[English](FLUTTER_PARITY_STATUS.md) | [简体中文](FLUTTER_PARITY_STATUS.zh-CN.md)

Md3.Avalonia 使用 Avalonia 控件、属性与生命周期。相似的类型名称不代表 Flutter API、绘制、路由、平台适配或辅助功能完全一致。`Md3.Avalonia.Extra` 是可选扩展包，不属于官方 Material 组件目录。

下表说明当前公开的有限实现范围；应以控件源码和测试为准。

| API 家族 | 状态 | 已支持行为 | 已知边界 |
|---|---|---|---|
| `MdHero` | 实现子集 | 快照浮层、源/目标插值、取消与 Reduced Motion 回退 | 未验证多窗口、全部剪裁/阴影情形或 Flutter 像素级一致性 |
| `MdReorderableList` | 实现子集 | 指针排序、自动滚动、键盘替代操作与 Reduced Motion | 不复刻 Flutter 的全部 delegate 与 drag decorator API |
| `MdDismissible` | 实现子集 | 轴向指针拖动、保留释放速度的 spring settle、逻辑方向背景、确认与收起生命周期 | 键盘操作和通知覆盖仍有限 |
| `MdFocusTraversalGroup` | 实现子集 | 视觉顺序、`TabIndex`、几何阅读顺序、循环和跳过模式 | 使用 Avalonia 焦点原语，不接收 Flutter `FocusTraversalPolicy` 对象 |
| `MdKeyboardAvoidingHost` | 实现子集 | 订阅 Avalonia `TopLevel.InputPane`、应用遮挡高度并滚动聚焦内容 | 未报告遮挡的浮动键盘不会触发避让 |
| `MdSimpleDialog` | 部分实现 | 遮罩、Escape、焦点约束/恢复、可选列表和取消操作 | 必须放在铺满宿主的浮层中；不是 Navigator route 或 `showDialog<T>` 的复刻 |
| `MdPaginatedDataTable` | API 骨架 | 基于列表行的基础分页 | 没有 Flutter 的完整列/单元格/表头排序模型，不应描述为 DataTable parity |
| `MdAdaptiveSwitch` | API 骨架 | Material switch 行为与平台尺寸选择 | 未实现 Cupertino 的绘制与动画 |
| `MdAdaptiveProgressIndicator` | API 骨架 | Material 进度语义与平台化尺寸 | 未实现 Cupertino activity indicator 绘制 |
| `MdAboutDialog` | API 骨架 | 应用元数据界面与许可回调 | 不包含内置 dialog route 或完整 Flutter 许可流程 |
| `MdContainerTransform`, `MdAnimatedVisibility`（Extra） | 实现子集 | Material 30/70 交叉淡入下的边界/形状变换，以及不触发布局回流的剪裁入退场 | 未建模 elevation morph 和中断速度；反转时会重启动画而不是重定向 |
| `MdSharedAxis`, `MdFadeThrough`（Extra） | 部分实现 | 对模板或 ViewModel 内容执行带平移/缩放的分段入场与保留退出 | `Control` 不能同时属于两个 presenter，因此旧内容为控件时跳过退出阶段，仅执行新内容入场 |
| `MdStepper` | 实现子集 | 垂直与水平（`Orientation`）步骤流、每步 `State` 样式、索引导航和 `ActiveStepChanged` | 不承诺 Flutter `controlsStep`/`currentStep` 的动画时序一致；水平内容布局更简化 |
| `MdPagination` | 无 Flutter 对应控件 | 数字与圆点模式、上一页/下一页、`NumberOfPages` 与双向 `SelectedPageIndex` | Flutter 没有第一方 pagination 控件；本控件基于 Avalonia `PipsPager`，不是 Flutter 移植 |
| `MdMaskedTextBox` | 无 Flutter 对应控件 | mask 限制输入、自动填充字面量、`PromptChar`、`HidePromptOnLeave` 与 `MdTextBox` 字段状态 | Flutter 通过 `TextInputFormatter` 处理 mask，没有第一方控件；本控件扩展 Avalonia `MaskedTextBox` |
| `MdCommitGraph` | 无 Flutter 对应控件 | 分道提交历史、分支/合并连接线、head 标记、每道 Material 色、`HeadId` 与只读 `LaneCount` | Flutter 没有第一方 commit graph；布局使用 GitHub 风格自定义 lane 算法，宿主提供普通 `MdCommitNode` 模型 |
| `MdCarousel` | 映射到 `PageView` | weighted、hero、center-aligned 与 uncontained 变体，含 controller 与 autoplay | 未验证 Flutter `PageView` physics/滚动位置一致性 |

标记 `[MdExperimental]` 的类型不作为已完成的 Flutter parity 证据。项目不发布整体 parity 百分比；行为对齐声明必须限定到表中 API、已测试的情形及其边界。
