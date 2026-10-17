# Md3.Avalonia 0.4.5-preview.1 发布说明

发布日期：2026-10-12

这是一个预览版更新，整合自 `0.4.1-preview.1` 之后的组件、Gallery 和跨平台修复。六个 NuGet 包统一为 `0.4.5-preview.1`。本版没有计划中的公共 API 破坏性变更；预览期 API 仍可能在首个稳定版前调整。

## 主要变化

### Material 规范与组件体验

- 修正多处 M3 几何与状态细节：导航栏 active indicator、Snackbar 圆角、Tabs 选中指示线、Surface 基础色；Pressed state layer 使用 M3 的 0.10 透明度，线性 indeterminate progress 改用 M3 双段扫动动画。
- `MdTabView` 新增 `Variant`，统一 Primary/Secondary tabs 的外观；Secondary 指示线和 Primary 全宽指示线分别遵循对应规范。
- `MdCarousel` 在桌面端点击折叠 keyline 后会聚焦并展开该卡片；`MdBanner` 窄屏下自动将 actions 移至文本下方。
- `MdAboutDialog` 在 compact 宽度下改为上下排列；`MdSettingsExpander` 使用可逆 motion 展开/收起。

### 桌面交互与绘制修复

- `MdScrollViewer` 滚动条在首次布局即同步 range、viewport 和可见状态；校准后交回 Avalonia 原生 offset 绑定，拖动时不再有两条写入路径竞争。
- `MdTabView` 修复 Windows 非 100% 缩放下首次绘制底部 tab strip 缺失；Gallery 桌面托盘菜单现在包含显示窗口、主题切换和退出操作。
- Gallery 的共享 Dialog scrim 覆盖整个桌面窗口。Rich Tooltip 在触发器 hover 且发生 focus 转移时不启动隐藏计时；pointer 与 focus 均离开后仍按正常 grace period 隐藏。两者均有 headless 回归测试。
- Picker 修复提交后字段显示未刷新的问题，并在页面滚动时关闭桌面 popup；桌面复验仍待用户确认。Cascader 标签/下拉框裁切已修正并通过 headless/CI，真实桌面复验仍待用户确认。
- `MdBeforeAfter` 修复首次从边缘开始拖动时的指针偏移，并新增 Gallery 首次交互回归测试；实际桌面验收仍待用户确认。
- TreeView 展开箭头的实际绘制墨迹现在居中于 40×40 DIP 透明状态区域；新增基于渲染像素质心的回归测试，而非只断言布局对齐属性。
- Gallery 的第三方 RichEditor 打开后即展示格式化 HTML 示例；测试确认初始文档确实产生编辑器区域像素输出。

### Gallery、Android 与发布

- Gallery 示例代码按内容自适应高度；主题演示开关会实际切换 Light/Dark/System；对话框与响应式演示在窄屏下修正了布局。
- Android Gallery 项目重新纳入 `Md3.Avalonia.sln`；CI 构建 `net10.0-android` APK。Android `ApplicationDisplayVersion` 为 `0.4.5-preview.1`，数值版本号递增至 5。
- 六个包声明 trimming/AOT 兼容性；Linux x64 Gallery 原生 AOT 发布会验证产物为本机可执行文件，并通过 xvfb 启动 smoke test。此结果不等同于 Windows/macOS/Android AOT 或物理设备验收。
- README 与 API 参考补充安装、包选择、主题注册、富文本编辑器说明和当前版本信息。

## NuGet 包

| 包 | 用途 |
|---|---|
| `Md3.Avalonia` | 核心 Material 3 控件与主题 |
| `Md3.Avalonia.Icons` | 完整 Material Symbols Rounded 字体与图标目录 |
| `Md3.Avalonia.Icons.Lite` | 精简 Material Symbols Rounded 字体子集，与完整 Icons provider 二选一 |
| `Md3.Avalonia.Extra` | Flutter-inspired 扩展控件 |
| `Md3.Avalonia.DataGrid` | Avalonia 原生 DataGrid 的 Material 主题 |
| `Md3.Avalonia.RichEditor` | AvaloniaRichEditor 的 Material 主题 |

安装核心包：

```bash
dotnet add package Md3.Avalonia --version 0.4.5-preview.1
```

按需添加其他包时也使用同一版本。可选主题分别为 `ExtraTheme`、`MaterialDataGridTheme` 和 `MaterialRichEditorTheme`；只引用所需包和主题。

## 验证范围

- GitHub Actions CI：桌面 Gallery 构建、headless 行为/渲染测试和静态检查。
- Gallery workflow：Linux、Windows、macOS 桌面发布以及 Android APK 构建。
- NuGet workflow：六个 `.nupkg` 与 `.snupkg` 构建。
- AOT probe：Linux x64 原生发布与 xvfb smoke test。AOT workflow 会报告诊断，具体应用仍需检查自身 trim/AOT 警告。
- Narrator、VoiceOver、Orca、TalkBack、Android 旋转/生命周期和其他物理设备操作仍需按 [`RELEASE_VALIDATION.md`](RELEASE_VALIDATION.md) 人工签署；自动化构建不视为设备验收。

完整组件与 API 列表见 [`API.md`](API.md)，版本历史见 [`../CHANGELOG.md`](../CHANGELOG.md)。
