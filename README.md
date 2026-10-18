# Md3.Avalonia

面向 Avalonia 12 的 Material Design 3 控件库，基于 `.NET 10`。提供独立的 `Md*` 控件类型、动态 HCT 主题及配套 Gallery，不覆盖 Avalonia 原生控件主题。

当前预览版本：`0.4.1-preview.1`。跨平台自动构建已接入 CI；Android 真机及屏幕阅读器验收仍需在目标设备上完成。

| Actions | Cards, chips and pickers |
|---|---|
| ![Material 3 button controls](docs/MdActionButtonsPreview.png) | ![Material 3 component preview](docs/MdNewComponentsPreview.png) |

## 功能概览

- 按钮、输入、选择、导航、菜单、对话框、列表、进度与日期/时间选择等控件；
- Light、Dark、System 主题，支持任意种子色、HCT 配色、对比度与形状设置；
- 可选 Material Symbols 图标包、扩展控件包，以及 DataGrid 和 RichEditor 主题包；
- Avalonia 原生绑定、命令、键盘与控件行为；桌面和 Android 使用同一套核心控件。

## NuGet 包

六个包版本均为 `0.4.1-preview.1`。

| 包 | 说明 |
|---|---|
| `Md3.Avalonia` | 核心控件与 `MaterialTheme` |
| `Md3.Avalonia.Icons` | 完整 Material Symbols Rounded 字体与图标目录 |
| `Md3.Avalonia.Icons.Lite` | 精简 Material Symbols 字体子集；与 Icons 二选一 |
| `Md3.Avalonia.Extra` | 可选扩展控件，依赖核心包 |
| `Md3.Avalonia.DataGrid` | Avalonia 原生 `DataGrid` 的 Material 主题 |
| `Md3.Avalonia.RichEditor` | AvaloniaRichEditor 的 Material 主题 |

仅核心包为必需依赖。DataGrid 与 RichEditor 主题各自携带对应的第三方控件依赖；不引用时不会加入项目。

## 快速开始

安装核心包：

```bash
dotnet add package Md3.Avalonia --version 0.4.1-preview.1
```

在 `App.axaml` 注册主题：

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:themes="using:Md3.Avalonia.Themes">
  <Application.Styles>
    <themes:MaterialTheme />
  </Application.Styles>
</Application>
```

在窗口中使用控件：

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:md="using:Md3.Avalonia.Controls">
  <StackPanel Spacing="16">
    <md:MdButton Content="保存" Variant="Filled" />
    <md:MdTextBox Label="项目名称" Text="{Binding ProjectName}" />
    <md:MdCheckBox Content="包含诊断信息"
                   IsChecked="{Binding IncludeDiagnostics}" />
  </StackPanel>
</Window>
```

Extra 控件需要额外安装 `Md3.Avalonia.Extra`，并在应用资源中注册 `ExtraTheme`。DataGrid、RichEditor 主题也应在 `MaterialTheme` 后注册。

### 动态主题

```csharp
using Avalonia;
using Avalonia.Styling;
using Md3.Avalonia.Themes.Dynamic;

var options = new MdThemeOptions
{
    SeedColor = "#006A6A",
    SchemeVariant = MdThemeSchemeVariant.Expressive,
    ContrastLevel = MdThemeContrastLevel.Standard,
    ThemeMode = MdThemeMode.System
};

var application = Application.Current!;
var isDark = application.ActualThemeVariant == ThemeVariant.Dark;
MdThemeManager.Apply(application, options, isDark);
```

## 文档

- [API Reference / API 参考](docs/API.md)：由 C# XML 注释生成的双语 API 参考；
- [对话框服务接入指南](docs/DIALOG_SERVICE.md)：ViewModel 调用与宿主配置；
- [兼容性说明](docs/COMPATIBILITY.md)；
- [发布验证清单](docs/RELEASE_VALIDATION.md)；
- [规范验证说明](docs/SPEC_VERIFICATION.md)；
- [Flutter 行为边界](docs/FLUTTER_PARITY_STATUS.md)；
- [变更记录](CHANGELOG.md)。

完整组件示例可运行 Gallery：

```bash
dotnet run --project gallery/Md3.Avalonia.Gallery.Desktop/Md3.Avalonia.Gallery.Desktop.csproj
```

## 构建与测试

需要 .NET 10 SDK。以下命令分别构建桌面 Gallery、运行 Headless 测试、检查 API 文档覆盖并打包六个 NuGet 包：

```bash
dotnet build gallery/Md3.Avalonia.Gallery.Desktop/Md3.Avalonia.Gallery.Desktop.csproj -c Release
dotnet test tests/Md3.Avalonia.HeadlessTests/Md3.Avalonia.HeadlessTests.csproj -c Release
python3 scripts/check-api-doc-coverage.py
scripts/build-nuget.sh
```

Android Gallery 位于 `gallery/Md3.Avalonia.Gallery.Android`，构建需要 .NET Android workload。
