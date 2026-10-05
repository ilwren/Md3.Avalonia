# Md3.Avalonia 0.4.0-preview.1 release notes

发布日期：2026-10-05

这是 0.4.0 预览版。本版的重点是**移动端平台能力**（Android 系统返回、安全区）、**裁剪安全**、
一个新的 opt-in `DataGrid` 主题包，以及一轮针对 Gallery 评审清单的系统性缺陷整改 —— 28 项里
现在只剩 1 项（borderless window 圆角）需要真机桌面环境才能判定。

> 次版本号从 0.3.x 进到 0.4.0，是因为本版新增了包（`Md3.Avalonia.DataGrid`）和跨越多个控件的
> 新能力面（返回导航、安全区、裁剪），不只是修补。所有包仍为预览版，API 可能在首个稳定版前
> 继续做保持兼容的打磨。

## 重要变更

### Android 平台缺口

- **系统返回键 / 预测式返回手势。** `MdBackNavigation` 把 `TopLevel.BackRequested` 路由到最上层
  打开的 Material surface（后开先关），并在被消费后标记事件已处理，避免 activity 被一起弹出。
  共 13 个面接入：`MdDialogHost`、`MdSheetHost`、`MdNavigationDrawer`、`MdSearchView`、
  `MdMenuAnchor`、`MdFabMenu`、`MdDatePicker`、`MdTimePicker`、`MdSimpleDialog`、
  `MdCommandPalette`、`MdPopover`、`MdCascader`、`MdAsyncSelect`。`MdBackScope` 可供自定义 surface
  接入。
  - 刻意不接入的：`MdSearchBar`（Escape 是清空文本）、`MdSwipeAction` 与 `MdChatView`
    （Escape 取消编辑或选择）、`MdSubMenuItem` 与 `MdMenu`（由 `MdMenuAnchor` 代管，重复注册会
    双重 unwind）、`MdTooltipHost`（tooltip 是瞬态的，吞掉返回手势会抢走用户给页面的操作）。
- **安全区。** `MdSafeArea` 让内容避开状态栏、挖孔、导航栏和手势条，支持按边控制
  （`MdSafeAreaEdges`）、`MinimumPadding` 下限，以及可直接赋值的 `SafeAreaPadding`，便于在没有
  设备的情况下预览和测试布局。边到边的做法是 `TopLevel.AutoSafeAreaPadding="False"` 配合本控件
  用在需要避让的局部。

### 裁剪（trimming）

五个包均声明 `IsTrimmable` 并开启 IL2xxx 分析器。`MdThemeJson` 改用源生成的序列化上下文；所有
接受字符串属性路径的控件都增加了免反射的委托入口：`MdDataGridColumn.ValueSelector` /
`.ValueParser` / `.ValueSetter`、`MdAsyncSelect.DisplaySelector`、`MdSearchView.ResultDisplaySelector`。

> **本版不承诺 AOT。** 只做裁剪安全化，`PublishAot` 不在验收范围内。

### 新包：`Md3.Avalonia.DataGrid`

Avalonia 原生 `DataGrid` 的 Material Design 3 主题，派生自上游 `12.1.2` 的 Fluent 模板并保留
`PART_` 命名。它是唯一引入 `Avalonia.Controls.DataGrid` 依赖的包，不引用就不会被拖进来。用法是在
`MaterialTheme` 之后加入 `MaterialDataGridTheme`。

### Gallery 与控件缺陷整改

- **一个组件一个页面。** 52 个此前只能靠滚动共享页面才能看到的组件拆出了独立页面，三个堆积页被
  删除，Gallery 从 48 页增加到 97 页。概览页由索引生成，并有双向断言的测试防止死链。
- 评审清单 28 项中已处理 27 项，含 breadcrumb 溢出、carousel 测量、segmented button 图标占位、
  settings expander 内边距、container transform / animated visibility 动画、simple dialog 选择被
  误报为取消、tree expander 命中区、numeric stepper 字形居中等。逐项根因见
  [`docs/GALLERY_DEFECTS.md`](GALLERY_DEFECTS.md)。
- 两项被报为「功能缺失」的，实际是 Gallery 演示本身不可观测，已修正演示而非改行为：
  - **日期 / 时间选择器**：规则本身正确但页面不显示任何绑定值，提交与回滚看起来一模一样。
    现在每个 picker 都打印实时值并标注自己的提交规则。规则是**非对称的且是故意的** ——
    `MdDatePicker` 的 Docked 模式即选即提交，Modal 模式未确认则回滚；`MdTimePicker` 两种模式
    都回滚，因为 M3 没有给时间选择器 docked 变体，它永远是对话框。
  - **图像对比**：`MdBeforeAfter` 当时在 `"BEFORE"` 和 `"AFTER"` 两个字符串之间拖分割线。现在
    比较的是同一张照片的两个调色版本，并附带绑定到 `Position` 的滑块、纵向示例和非图像示例。

### 控件修复（本版发现于整改过程中）

- `MdModalFocusController` 不再自我补给 dispatcher 队列。守卫标志此前在重定向任务开始时就被清除，
  于是聚焦尝试自身产生的焦点事件会 arm 下一次重定向，`Dispatcher.RunJobs()` 永远无法排空。现在
  标志在聚焦尝试之后清除，并且连续三次落不进作用域即放弃重定向；containment、isolation 和
  Escape 不受影响。
- `MdPopover` 关闭时会从「当前打开」协调器注销。此前已关闭的 popover 仍被记为打开，下一个打开的
  popover 会反向操作它，轻则切断退出动画，重则在跨 UI 线程时抛
  `InvalidOperationException`。
- `MdBeforeAfter`：`DividerBrush` 不再被模板里的字面量覆盖；`PositionChanged` 改为跟随属性，
  方向键和绑定变更同样会触发；`IsInteractive="False"` 现在同时意味着不可 tab 聚焦、方向键无效。
- `MdSimpleDialog` 选项改为通栏，采用 M3 间距（标题 24/24/24/0、内容 0/12/0/16、选项 24/8）。

## 包

五个包版本统一为 `0.4.0-preview.1`：

- `Md3.Avalonia`（核心）
- `Md3.Avalonia.Icons`、`Md3.Avalonia.Icons.Lite`（二选一的图标 provider）
- `Md3.Avalonia.Extra`（依赖核心）
- `Md3.Avalonia.DataGrid`（新增，opt-in）

均生成 `.nupkg` 与 `.snupkg`，包含 XML API 文档、README 和第三方声明。

## 已知限制

- **borderless window 圆角（评审清单第 26 项）仍未判定。** 它依赖 OS 层的窗口整形（透明度提示、
  DWM 圆角、平台 adapter），headless 既观察不到也测不了，需要一次真实桌面运行来定位。
- 不承诺 AOT；本版只做裁剪安全化。
- 无障碍（a11y）与 RTL 的完整自动化验收不在本预览版范围内。
- Windows 11 原生窗口视觉效果仍需在实际 Windows 设备上确认。
- Android Emulator UI 回归测试可能受 GitHub macOS runner 启动稳定性影响。
- [`docs/RELEASE_VALIDATION.md`](RELEASE_VALIDATION.md) 中的人工平台矩阵仍未签署。
