# 兼容性与支持范围

## 当前基线

| 项目 | 基线 |
|---|---|
| .NET | `.NET 10`，控件包目标 `net10.0` |
| Avalonia | `12.1.2` |
| Desktop Gallery | Windows、macOS、Linux；CI 分别构建 |
| Android Gallery | `net10.0-android`；构建需 .NET Android workload |
| NuGet 版本 | `0.4.1-preview.1` |

所有 Avalonia 依赖版本集中定义于 [`Directory.Packages.props`](../Directory.Packages.props)。核心包使用 Avalonia 跨平台 API，不依赖桌面主题；可选 DataGrid 与 RichEditor 包分别增加其目标控件依赖。

## 平台验证边界

桌面 Gallery 在 Linux、Windows 和 macOS CI 上构建；Android Gallery 由独立工作流构建。Android Emulator UI 回归为单独的可选工作流，构建成功不等同于真机验收。Windows 原生窗口 chrome、Android 生命周期和输入行为、屏幕阅读器及高 DPI 场景仍应按[发布验证清单](RELEASE_VALIDATION.md)在目标平台执行。

当前预览版没有为 iOS 或浏览器/WASM 提供独立宿主与完整发布验收承诺。

## API 与裁剪

所有包均为预览版；升级前请检查 [`CHANGELOG.md`](../CHANGELOG.md)。每个包生成 XML API 文档，所有六个项目启用 `IsTrimmable` 和 `IsAotCompatible` 分析选项。它们不代表每个平台、每个 RID 都已完成裁剪或 AOT 发布验证；应用仍应使用其目标运行时执行发布测试。

公共控件类型、属性与主题资源属于预览 API。首次稳定发布前可能调整，届时会在变更记录中说明。
