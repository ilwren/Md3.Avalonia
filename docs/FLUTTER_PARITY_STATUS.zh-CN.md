---
_lang: zh-CN
description: "Md3.Avalonia 与 Flutter Material 组件在哪些方面一致、哪些方面有意不同，以及原因。"
canonical: "https://ilwren.github.io/Md3.Avalonia/docs/FLUTTER_PARITY_STATUS.zh-CN.html"
hreflang_en: "https://ilwren.github.io/Md3.Avalonia/docs/FLUTTER_PARITY_STATUS.html"
hreflang_zh: "https://ilwren.github.io/Md3.Avalonia/docs/FLUTTER_PARITY_STATUS.zh-CN.html"
hreflang_x_default: "https://ilwren.github.io/Md3.Avalonia/docs/FLUTTER_PARITY_STATUS.html"
comments: true
---

# Flutter 行为边界

[English](FLUTTER_PARITY_STATUS.md) | [简体中文](FLUTTER_PARITY_STATUS.zh-CN.md)

Md3.Avalonia 使用 Avalonia 控件、属性与生命周期。相似的类型名称不代表 Flutter API、绘制、路由、平台适配或辅助功能完全一致。`Md3.Avalonia.Extra` 是可选扩展包，不属于官方 Material 组件目录。

下表说明当前公开的有限实现范围；应以控件源码和测试为准。

| API 家族 | 状态 | 已支持行为 | 已知边界 |
|---|---|---|---|
| `MdHero` | 实现子集 | 快照浮层、源/目标插值、取消与 Reduced Motion 回退、`RectTween`/`MdRectTweens` 飞行形状、`FlightShuttleBuilder`、`PlaceholderBuilder`、`TransitionOnUserGestures` | 未验证多窗口、全部剪裁/阴影情形或 Flutter 像素级一致性；手势飞行仅在两侧 Hero 都显式开启时才会启动 |
| `MdReorderableList` | 实现子集 | 指针排序、自动滚动、键盘替代操作与 Reduced Motion | 不复刻 Flutter 的全部 delegate 与 drag decorator API |
| `MdDismissible` | 实现子集 | 轴向指针拖动、保留释放速度的 spring settle、逻辑方向背景、确认与收起生命周期、`Vertical`/`Up`/`Down` 方向、按方向的 `DismissThresholds`、`ResizeDuration`、`CrossAxisEndOffset` 与 `UpdateOffset` 逐帧回调 | 键盘操作和通知覆盖仍有限 |
| `MdFocusTraversalGroup` | 实现子集 | 视觉顺序、`TabIndex`、几何阅读顺序、循环和跳过模式 | 使用 Avalonia 焦点原语，不接收 Flutter `FocusTraversalPolicy` 对象 |
| `MdKeyboardAvoidingHost` | 实现子集 | 订阅 Avalonia `TopLevel.InputPane`、应用遮挡高度并滚动聚焦内容 | 未报告遮挡的浮动键盘不会触发避让 |
| `MdSimpleDialog` | 部分实现 | 遮罩、Escape、焦点约束/恢复、可选列表和取消操作 | 必须放在铺满宿主的浮层中；不是 Navigator route 或 `showDialog<T>` 的复刻 |
| `MdPaginatedDataTable` | 已实现子集 | 列表行分页、可选等宽列头、由宿主处理的排序请求、表头操作槽位和首末页导航 | 行与单元格对齐仍由宿主的 `ItemTemplate` 提供；没有自动生成单元格、行选择 API 或自动排序，不是完整 DataTable parity |
| `MdAdaptiveSwitch` | API 骨架 | Material switch 行为与平台尺寸选择 | 未实现 Cupertino 的绘制与动画 |
| `MdAdaptiveProgressIndicator` | API 骨架 | Material 进度语义与平台化尺寸 | 未实现 Cupertino activity indicator 绘制 |
| `MdAboutDialog` | API 骨架 | 应用元数据界面与许可回调 | 不包含内置 dialog route 或完整 Flutter 许可流程 |
| `MdContainerTransform`, `MdAnimatedVisibility`（Extra） | 实现子集 | Material 30/70 交叉淡入下的边界/形状变换、不触发布局回流的剪裁入退场，以及 `ClosedElevation`/`OpenElevation` 阴影渐变 | 中途反转会从当前进度重定向并按剩余行程比例缩放时长；反转瞬间的释放速度由该缩放近似，阴影为 M3 umbra/penumbra 数值的近似实现 |
| `MdSharedAxis`, `MdFadeThrough`（Extra） | 实现子集 | 对模板、ViewModel 与 `Control` 内容执行带平移/缩放的分段入场与保留退出 | `Control` 退出时展示冻结的 `MdVisualSnapshot` 图像——不含实时绑定、命中测试或无障碍树；无法截取快照时跳过退出阶段，仅执行新内容入场 |
| `MdStepper` | 实现子集 | 垂直与水平（`Orientation`）步骤流、每步 `State` 样式、索引导航和 `ActiveStepChanged`；`ControlsBuilder`、`StepIconBuilder`、`ConnectorBrush`/`ConnectorThickness` | 不承诺 Flutter `controlsStep`/`currentStep` 的动画时序一致；水平内容布局更简化；builder 在导航时重建，不会在每步状态变化时重跑 |
| `MdDraggableScrollableSheet` | 实现子集 | 拖拽手柄改尺寸、吸附点与轻弹吸附、`ExtentChanged`/`AnimationCompleted`、`ShouldCloseOnMinExtent`/`MinExtentReached`、`SnapAnimationDuration`、`MdDraggableScrollableController` | 不承诺 Flutter 滚动物理或 `ScrollController` 注入一致性；最小尺寸收起请求以事件形式上报，由宿主决定是否关闭 |
| `MdSlideToConfirm`（Extra） | 实现子集 | 逻辑方向拖动并松开后一次确认、弹簧回弹、双向状态、键盘和无障碍调用 | 独立实现；不含命令、异步等待、自定义模板或安全/授权保证 |
| `MdEmojiPicker`（Extra） | 实现子集 | 可替换的小型示例集、搜索、类别、内存最近使用、双向选择和无障碍按钮 | 独立实现；不含完整 Unicode/CLDR 数据、肤色选择、持久化、字体可用性检测、插入或虚拟化网格 |
| `MdWheelPicker`（Extra） | 实现子集 | 有界整数步长、跟手拖动与速度惯性吸附、点击行、聚焦后滚轮、键盘和数值范围自动化 | 独立实现；不含小数、循环、水平布局、自定义条目模板或 Flutter 滚动物理一致性 |
| `MdConfetti`（Extra） | 实现子集 | 有界一次性粒子、方向/散布、速度、重力/阻尼、主题画刷、渲染帧生命周期和减少动效抑制 | 参考公开文档独立实现；不含持续发射、循环控制器、自定义路径、任意调色板、声音或触觉反馈 |
| `MdMarquee`（Extra） | 实现子集 | 水平溢出滚动、方向、速度、间距、轮间暂停、RTL、减少动效静止回退及完整文本无障碍 | 参考公开文档独立实现；不含垂直滚动、有限循环、加减速曲线、富文本及文本交互 |
| `MdExpandableText`（Extra） | 实现子集 | 纯文本按行或 Unicode 文本元素折叠、独立本地化展开/收起按钮、双向状态及 RTL | 参考公开文档独立实现；不含富文本、注解、选择、行内按钮及展开动画 |
| `MdAutoSizeText`（Extra） | 实现子集 | 有界纯文本字号缩放、下限及步进、最大行数、换行、裁剪、RTL 和文本无障碍 | 参考公开文档独立实现；不含富文本、选择、分组、预设字号及替代内容 |
| `MdMediaProgressBar`（Extra） | 实现子集 | 宿主提供的秒数、已播放/缓冲段、时间标签、指针/键盘/无障碍 seek 与 RTL | 参考公开文档独立实现；不含播放、解码、流媒体或自主计时器；宿主处理 seek 请求 |
| `MdPageIndicator`（Extra） | 实现子集 | 宿主驱动的小数页位置、滑动/拉伸标记、水平/垂直与 RTL 布局、指针/键盘/无障碍请求 | 参考 smooth_page_indicator 公开文档独立实现；翻页和动效由宿主管理；不含控制器自动绑定、自运动、滚动圆点虚拟化及其他效果 |
| `MdPagination` | 无 Flutter 对应控件 | 数字与圆点模式、上一页/下一页、`NumberOfPages` 与双向 `SelectedPageIndex` | Flutter 没有第一方 pagination 控件；本控件基于 Avalonia `PipsPager`，不是 Flutter 移植 |
| `MdMaskedTextBox` | 无 Flutter 对应控件 | mask 限制输入、自动填充字面量、`PromptChar`、`HidePromptOnLeave` 与 `MdTextBox` 字段状态 | Flutter 通过 `TextInputFormatter` 处理 mask，没有第一方控件；本控件扩展 Avalonia `MaskedTextBox` |
| `MdCommitGraph` | 无 Flutter 对应控件 | 分道提交历史、分支/合并连接线、head 标记、每道 Material 色、`HeadId` 与只读 `LaneCount` | Flutter 没有第一方 commit graph；布局使用 GitHub 风格自定义 lane 算法，宿主提供普通 `MdCommitNode` 模型 |
| `MdCarousel` | 映射到 `PageView` | weighted、hero、center-aligned 与 uncontained 变体，含 controller 与 autoplay | 未验证 Flutter `PageView` physics/滚动位置一致性 |

标记 `[MdExperimental]` 的类型不作为已完成的 Flutter parity 证据。项目不发布整体 parity 百分比；行为对齐声明必须限定到表中 API、已测试的情形及其边界。
