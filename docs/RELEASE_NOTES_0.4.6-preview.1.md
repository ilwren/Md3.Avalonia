# Md3.Avalonia 0.4.6-preview.1 发布说明（草案）

状态：项目源码版本基线已更新；NuGet/GitHub 发布及 `net8.0`/`net10.0` 多目标构建验证尚未确认。这里记录当前源码中的变化，不代表发布流程、CI 或桌面/设备人工验收已经通过。

本版将七个 NuGet 包统一到 `0.4.6-preview.1`。`0.4.5-preview.1` 的六包说明仍保留为历史记录，见 [`0.4.5-preview.1 发布说明`](RELEASE_NOTES_0.4.5-preview.1.md)。

## 新增

- 新增独立的 `Md3.Avalonia.Notifications` 包，为 Windows、macOS、Linux 和 Android 提供标题/正文通知 API；直接调用平台 API，不启动 PowerShell、`notify-send` 或其他外部 helper 进程。各平台的通知投递、权限和归属行为仍需人工验收。
- 新增 `MdBreadcrumbPathConverter`，可将路由路径转换为 `MdBreadcrumb` 路径项。

## 变更

- 七个 NuGet 包同时提供 `net8.0` 与 `net10.0` 资产，以保持与 Avalonia 12 的 .NET 支持范围同步；Notifications 另提供 Windows/Android 专用 TFM。Gallery（桌面和 Android 宿主）仍仅使用 .NET 10。
- Avalonia 及 `Avalonia.Controls.DataGrid` 依赖统一下调至首个稳定 Avalonia 12 版本 `12.0.0`。
- Extra 控件资源统一通过 `ExtraTheme` 暴露；移除旧 `EcosystemTheme` 名称，不保留兼容 shim。
- Android Gallery 的显示版本更新为 `0.4.6-preview.1`，基础 `ApplicationVersion` 更新为 `6`；发布工作流的版本码偏移同步为 `+6`。

## 修正与验证边界

- `MaterialRichEditorTheme` 的默认文本颜色改为使用动态 M3 `OnSurface` 令牌，以改善深色主题下未设置样式的正文可读性。
- `MdBeforeAfter` 增加首次边缘拖动的焦点/滚动保护与回归覆盖。Headless 覆盖不等同于桌面实机验收；首次拖动、定位和滚动问题在桌面端仍需复测，不在此宣称已解决。
- 通知包的 Windows 11 24H2 首次 Toast 注册与实际投递、macOS 归属、Linux X11/Wayland、Android 权限/API 行为仍未完成人工签署。当前源码的 CI/人工验证状态见 [`RELEASE_VALIDATION.md`](RELEASE_VALIDATION.md)；其中自动化证据标注为历史 `0.4.5-preview.1` 范围，不能当作本版验证结果。

安装示例中的 `0.4.6-preview.1` 仅在该版本发布到 NuGet 后可用。