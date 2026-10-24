# Md3.Avalonia 0.4.6-preview.1 中间源码基线（已并入 0.5.0-preview.1）

> 本页记录一组曾以 `0.4.6-preview.1` 为版本目标的中间源码变化；这些变化已并入 `0.5.0-preview.1`，该版本于 2026-10-19 发布到 [NuGet.org](https://www.nuget.org/packages/Md3.Avalonia/0.5.0-preview.1)。本页仅供追溯，不是当前安装指南；请使用 `0.5.0-preview.1`，并以[正式发行说明](RELEASE_NOTES_0.5.0-preview.1.md)为准。

该基线原计划将七个包扩展为 `net8.0` 与 `net10.0` 资产。`0.4.5-preview.1` 的六包说明仍保留为历史记录，见 [`0.4.5-preview.1 发布说明`](RELEASE_NOTES_0.4.5-preview.1.md)。

## 新增

- 新增独立的 `Md3.Avalonia.Notifications` 包，为 Windows、macOS、Linux 和 Android 提供标题/正文通知 API；直接调用平台 API，不启动 PowerShell、`notify-send` 或其他外部 helper 进程。各平台的通知投递、权限和归属行为仍需人工验收。
- 新增 `MdBreadcrumbPathConverter`，可将路由路径转换为 `MdBreadcrumb` 路径项。

## 变更

- 七个包（核心、Extra、Icons、Icons.Lite、DataGrid、RichEditor 与 Notifications）提供 `net8.0` 和 `net10.0` 资产；Notifications 还包含 Windows 专用 TFM。其余 11 个第三方主题包仅以 `net10.0` 为目标框架。桌面和 Android Gallery 宿主仍使用 .NET 10。
- 最终发行版本使用 Avalonia `12.1.2`，包括 `Avalonia.Controls.DataGrid`；此前草案中的 `12.0.0` 已不是发行依赖版本。
- Extra 控件资源统一通过 `ExtraTheme` 暴露；移除旧 `EcosystemTheme` 名称，不保留兼容 shim。
- 最终 Android Gallery 的应用显示版本为 `0.5.0-preview.1`，基础 `ApplicationVersion` 为 `6`；发布工作流的版本码偏移为 `+6`。

## 修正与验证边界

- `MaterialRichEditorTheme` 的默认文本颜色改为使用动态 M3 `OnSurface` 令牌，以改善深色主题下未设置样式的正文可读性。
- `MdBeforeAfter` 增加首次边缘拖动的焦点/滚动保护与回归覆盖。Headless 覆盖不等同于桌面实机验收；首次拖动、定位和滚动问题在桌面端仍需复测，不在此宣称已解决。
- 通知包的 Windows 11 24H2 首次 Toast 注册与实际投递、macOS 归属、Linux X11/Wayland、Android 权限/API 行为仍未完成人工签署。当前源码的 CI/人工验证状态见 [`RELEASE_VALIDATION.md`](RELEASE_VALIDATION.md)；其中自动化证据标注为历史 `0.4.5-preview.1` 范围，不能当作本版验证结果。

当前 NuGet 安装示例使用已发布的 `0.5.0-preview.1`；`0.4.6-preview.1` 是中间源码基线，不应作为当前安装版本。