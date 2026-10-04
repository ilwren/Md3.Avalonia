# Md3.Avalonia 3.1.0-preview.1 release notes

发布日期：2026-10-04

这是 3.1 预览版，重点完善桌面窗口 chrome、图标、caption buttons、跨平台 Gallery 与发布验证流程。

## 重要变更

### MdBorderlessWindow

- 标题栏默认继承 `Window.Icon`，并在 Material 标题栏中显示窗口图标。
- 新增 `ShowIcon`，默认值为 `true`，可以使用 `ShowIcon="False"` 手动隐藏标题栏图标。
- `ShowMinimizeButton`、`ShowMaximizeButton`、`ShowCloseButton` 可以独立控制按钮可见性。
- `IsMinimizeButtonEnabled`、`IsMaximizeButtonEnabled`、`IsCloseButtonEnabled` 可以独立控制按钮启用状态。
- 保留 Windows 原生窗口状态、拖拽、resize、系统菜单和 DWM 最小化/最大化/还原动画。
- 避免原生 frame 与 Material frame 叠加产生双边框。
- 标题栏背景统一使用 `Surface`，移除明显的标题栏分隔线。

### Gallery 与平台

- Desktop Gallery 保留 Windows 原生窗口动画验证场景。
- Android Gallery 不暴露不支持的桌面窗口 chrome 页面。
- 更新窗口模板和 headless tests，覆盖标题栏图标、caption button 状态和 native frame 行为。

### 发布与验证

- 四个包统一版本为 `3.1.0-preview.1`：
  - `Md3.Avalonia`
  - `Md3.Avalonia.Icons`
  - `Md3.Avalonia.Icons.Lite`
  - `Md3.Avalonia.Extra`
- NuGet 包继续生成 `.nupkg` 和 `.snupkg`。
- GitHub Actions 负责构建、测试、打包和多平台 Gallery 验证。

## 已知限制

- Windows 11 原生窗口视觉效果仍需在实际 Windows 设备上确认。
- RTL 和完整辅助功能自动化验证不属于本预览版的完整验收范围。
- Android Emulator UI 回归测试可能受到 GitHub macOS runner 启动稳定性的影响。
