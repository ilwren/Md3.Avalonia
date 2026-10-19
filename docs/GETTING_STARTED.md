# Quick start

[English](GETTING_STARTED.md) | [简体中文](GETTING_STARTED.zh-CN.md)

This guide takes a new Avalonia app from package references to its first Material Design 3 screen. For detailed package choices, see [NuGet packages](NUGET_PACKAGES.md); for more patterns, see [Usage examples](USAGE_EXAMPLES.md).

## Requirements

- Avalonia 12. The repository baseline is Avalonia `12.1.2`.
- .NET 10 SDK is recommended. The core controls, Extra, icons, DataGrid, and RichEditor packages also target .NET 8; third-party theme add-ons target .NET 10.
- The current source version is `0.5.0-preview.1`. The repository README marks it as not yet published; package commands below work once that version is available on your configured NuGet feed.

## 1. Create an Avalonia app

If the Avalonia templates are not installed yet:

```bash
dotnet new install Avalonia.Templates
dotnet new avalonia.app -o Md3Quickstart
cd Md3Quickstart
```

## 2. Add the packages

The core package supplies controls, theme tokens, and motion. Add one icon package if you want the bundled Material Symbols catalog and font. `Icons` is the full catalog; `Icons.Lite` is a smaller alternative—choose one, not both.

```bash
dotnet add package Md3.Avalonia
dotnet add package Md3.Avalonia.Icons
```

Pin every Md3.Avalonia package to the same version in production. If the preview version is not on NuGet yet, use a published version or build the packages from this repository; see [NuGet packages](NUGET_PACKAGES.md).

## 3. Register the theme

Add `MaterialTheme` once in `App.axaml`. `RequestedThemeVariant="Default"` follows Avalonia's default light/dark theme selection. The theme styles Md3 controls; it does not globally replace Avalonia's native control themes.

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

## 4. Build a screen

Use the controls in XAML like other Avalonia controls. The example below includes a text field, a filled action, and an icon from the optional Icons package.

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

## 5. Build and explore

```bash
dotnet build
```

Next, choose an optional package in the [NuGet guide](NUGET_PACKAGES.md), explore the [usage examples](USAGE_EXAMPLES.md), or browse the [generated API reference](https://ilwren.github.io/Md3.Avalonia/).
