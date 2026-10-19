# Quick start / 快速上手

This guide takes a new Avalonia app from package references to its first Material Design 3 screen. For detailed package choices, see [NuGet packages / NuGet 包](NUGET_PACKAGES.md); for more patterns, see [Usage examples / 使用示例](USAGE_EXAMPLES.md).

本指南从 NuGet 引用开始，带你在新的 Avalonia 应用中搭建第一个 Material Design 3 页面。包的详细选择请参阅 [NuGet 包指南](NUGET_PACKAGES.md)，更多用法请参阅[使用示例](USAGE_EXAMPLES.md)。

## Requirements / 环境要求

- Avalonia 12. The repository baseline is Avalonia `12.1.2`.
- .NET 10 SDK is recommended. The core controls, Extra, icons, DataGrid, and RichEditor packages also target .NET 8; third-party theme add-ons target .NET 10.
- The current source version is `0.5.0-preview.1`. The repository README marks it as not yet published; package commands below work once that version is available on your configured NuGet feed.

- Avalonia 12；仓库当前基线为 `12.1.2`。
- 推荐使用 .NET 10 SDK。核心控件、Extra、图标、DataGrid 与 RichEditor 包也支持 .NET 8；第三方主题扩展包以 .NET 10 为目标框架。
- 当前源码版本为 `0.5.0-preview.1`。仓库 README 标注该版本尚未发布；以下 NuGet 命令需在对应版本已发布到当前 NuGet 源后使用。

## 1. Create an Avalonia app / 创建 Avalonia 应用

If the Avalonia templates are not installed yet:

如果尚未安装 Avalonia 项目模板：

```bash
dotnet new install Avalonia.Templates
dotnet new avalonia.app -o Md3Quickstart
cd Md3Quickstart
```

## 2. Add the packages / 添加 NuGet 包

The core package supplies controls, theme tokens, and motion. Add one icon package if you want the bundled Material Symbols catalog and font. `Icons` is the full catalog; `Icons.Lite` is a smaller alternative—choose one, not both.

核心包提供控件、主题令牌与动效。若要使用随包提供的 Material Symbols 目录和字体，再添加一个图标包。`Icons` 提供完整目录，`Icons.Lite` 是较小的替代方案；二者任选其一，不要同时安装。

```bash
dotnet add package Md3.Avalonia
dotnet add package Md3.Avalonia.Icons
```

Pin every Md3.Avalonia package to the same version in production. If the preview version is not on NuGet yet, use a published version or build the packages from this repository; see [NuGet packages / NuGet 包](NUGET_PACKAGES.md).

生产项目请将所有 Md3.Avalonia 包固定到同一版本。如果预览版尚未上架，请使用已发布版本，或从本仓库构建 NuGet 包；详见 [NuGet 包指南](NUGET_PACKAGES.md)。

## 3. Register the theme / 注册主题

Add `MaterialTheme` once in `App.axaml`. `RequestedThemeVariant="Default"` follows Avalonia's default light/dark theme selection. The theme styles Md3 controls; it does not globally replace Avalonia's native control themes.

在 `App.axaml` 中注册一次 `MaterialTheme`。`RequestedThemeVariant="Default"` 会沿用 Avalonia 默认的浅色/深色主题选择。该主题负责 Md3 控件，不会全局替换 Avalonia 原生控件的主题。

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:themes="using:Md3.Avalonia.Themes"
             x:Class="Md3Quickstart.App"
             RequestedThemeVariant="Default">
  <Application.Styles>
    <themes:MaterialTheme />
  </Application.Styles>
</Application>
```

## 4. Build a screen / 构建页面

Use the controls in XAML like other Avalonia controls. The example below includes a text field, a filled action, and an icon from the optional Icons package.

像使用其他 Avalonia 控件一样在 XAML 中使用 Md3 控件。下面的示例包含文本输入框、实心按钮，以及来自可选 Icons 包的图标。

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:md="using:Md3.Avalonia.Controls"
        x:Class="Md3Quickstart.MainWindow">
  <StackPanel Margin="24" Spacing="16">
    <TextBlock Text="Create a workspace" />
    <md:MdTextBox Label="Workspace name" Text="Material sample" />
    <md:MdButton Content="Create workspace" Variant="Filled" />
    <md:MdIcon Glyph="{x:Static md:MdSymbols.Favorite}" Size="24" />
  </StackPanel>
</Window>
```

The icon catalog is available after installing `Md3.Avalonia.Icons` (or `Md3.Avalonia.Icons.Lite`). Its embedded font provider is discovered automatically; no manual font download or `ConfigureFonts` call is required. Remove the `MdIcon` line if you are using only the core package.

安装 `Md3.Avalonia.Icons`（或 `Md3.Avalonia.Icons.Lite`）后即可访问图标目录。内嵌字体 provider 会自动发现，无需手动下载字体或调用 `ConfigureFonts`。若只安装核心包，请删除 `MdIcon` 这一行。

## 5. Build and explore / 构建并继续探索

```bash
dotnet build
```

Next, choose an optional package in the [NuGet guide](NUGET_PACKAGES.md), explore the [usage examples](USAGE_EXAMPLES.md), or browse the [generated API reference](https://ilwren.github.io/Md3.Avalonia/).

接下来可在 [NuGet 包指南](NUGET_PACKAGES.md)中选择扩展包，查看[使用示例](USAGE_EXAMPLES.md)，或浏览[自动生成的 API 参考](https://ilwren.github.io/Md3.Avalonia/)。
