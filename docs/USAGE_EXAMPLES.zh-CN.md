---
_lang: zh-CN
---

# 使用示例

[English](USAGE_EXAMPLES.md) | [简体中文](USAGE_EXAMPLES.zh-CN.md)

以下示例展示如何将控件和主题包组合到真实页面中。截图来自项目 Gallery；瀑布流示意图为本指南专门绘制。完整可运行示例可查看 [Gallery 源码](https://github.com/ilwren/Md3.Avalonia/tree/main/gallery/Md3.Avalonia.Gallery/Pages)。

## 1. 简单表单

Md3 控件可直接使用 Avalonia 标准绑定与命令。和原生控件一样，将属性绑定到 ViewModel 即可。

```xml
<StackPanel Margin="24" Spacing="16">
  <TextBlock Text="New workspace" />
  <md:MdTextBox Label="Workspace name"
                Text="{Binding WorkspaceName}" />
  <md:MdComboBox Label="Platform"
                 Variant="Outlined"
                 ItemsSource="{Binding Platforms}"
                 SelectedItem="{Binding SelectedPlatform}" />
  <md:MdButton Content="Create workspace"
               Variant="Filled"
               Command="{Binding CreateCommand}" />
</StackPanel>
```

上例中的 `md` 对应 `Md3.Avalonia.Controls` namespace。先在 `App.axaml` 中注册 `MaterialTheme`；详见[快速上手](GETTING_STARTED.zh-CN.md)。

![Material 3 卡片、Chip、轮播、日期选择器与时间选择器的 Gallery 预览。](MdNewComponentsPreview.png)

## 2. 图标操作按钮

安装 `Md3.Avalonia.Icons` 或 `Md3.Avalonia.Icons.Lite` 之一，再通过公开图标目录为核心图标按钮指定 glyph。图标包会自动注册内嵌字体。

```xml
<StackPanel Orientation="Horizontal" Spacing="12">
  <md:MdIconButton Icon="{x:Static md:MdSymbols.Edit}"
                   Variant="Filled"
                   Command="{Binding EditCommand}"
                   AutomationProperties.Name="Edit workspace" />
  <md:MdButton Content="Create"
               Variant="Tonal"
               Command="{Binding CreateCommand}" />
</StackPanel>
```

`MdSymbols` 与 `MdIconButton` 位于相同的 `Md3.Avalonia.Controls` XAML namespace。若安装 Lite 包，则使用 `MdSymbolsLite`。

![Md3 操作按钮与图标按钮的 Gallery 预览。](MdActionButtonsPreview.png)

## 3. 应用动态配色

从 seed color 生成色彩角色，并在应用级别应用。请在注册 `MaterialTheme` 后调用；主题资源为动态资源，角色变化时控件会随之更新。

```csharp
using Avalonia.Controls;
using Avalonia.Styling;
using Md3.Avalonia.Themes.Dynamic;

var application = Application.Current!;
var options = new MdThemeOptions
{
    SeedColor = "#006A6A",
    SchemeVariant = MdThemeSchemeVariant.Expressive,
    ContrastLevel = MdThemeContrastLevel.High,
    ThemeMode = MdThemeMode.System
};

var useDarkScheme = application.ActualThemeVariant == ThemeVariant.Dark;
MdThemeManager.Apply(application, options, useDarkScheme);
```

`ThemeMode.System` 也会跟踪系统后续的浅色/深色变化。各 scheme、对比度、动效与形状选项详见 [API 参考](https://ilwren.github.io/Md3.Avalonia/)。

## 4. 使用 Extra 排列错落卡片

添加 `Md3.Avalonia.Extra`，并在核心主题后注册 `ExtraTheme`。`MdWaterfall` 会把子项放入当前最短的列。设置 `Columns` 可固定列数；保持默认值 0 时，则根据 `MinColumnWidth` 自适应计算列数。

```xml
<eco:MdWaterfall MinColumnWidth="220"
                 ColumnSpacing="12"
                 ItemSpacing="12">
  <md:MdCard Variant="Elevated" Height="120" />
  <md:MdCard Variant="Filled" Height="190" />
  <md:MdCard Variant="Outlined" Height="90" />
  <md:MdCard Variant="Elevated" Height="160" />
  <md:MdCard Variant="Filled" Height="110" />
  <md:MdCard Variant="Outlined" Height="200" />
</eco:MdWaterfall>
```

使用 `xmlns:eco="using:Md3.Avalonia.Extra.Controls"` 和 `xmlns:md="using:Md3.Avalonia.Controls"`。`MdWaterfall` 也支持 `ItemsSource` 绑定集合。可选包详情请见 [NuGet 包指南](NUGET_PACKAGES.zh-CN.md)。

![瀑布流项目依次放入最短列的示意图。](images/waterfall-layout.svg)

## 5. 由 ViewModel 驱动的对话框

让 ViewModel 不持有控件引用：注入 `IMdDialogService`，显示对话框模型并等待结果。完整指南涵盖 Host、DataTemplate、取消行为与弹层所有权协调。

完整示例请参阅[对话框服务指南](DIALOG_SERVICE.zh-CN.md)及对应 API 参考。
