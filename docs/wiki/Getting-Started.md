# Getting Started with Md3.Avalonia

## 1. Installation

Install the required NuGet packages into your Avalonia project via the .NET CLI or Package Manager:

```bash
# Core Material Design 3 controls (Required)
dotnet add package Md3.Avalonia

# Optional: Extra extended controls (Rich Editor, Chat View, Timeline, DataGrid, etc.)
dotnet add package Md3.Avalonia.Extra

# Choose ONE icon font package:
# Option A: Full icon font catalog (4,000+ glyphs)
dotnet add package Md3.Avalonia.Icons

# Option B: Lightweight icon font subset (~25 KB)
dotnet add package Md3.Avalonia.Icons.Lite
```

---

## 2. Configuring `App.axaml`

Add the `MaterialTheme` and optional `ExtraTheme` to your application styles in `App.axaml`:

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:md="using:Md3.Avalonia.Themes"
             xmlns:extra="using:Md3.Avalonia.Extra.Themes"
             x:Class="MyApp.App">
  <Application.Styles>
    <!-- Core Material Design 3 Theme -->
    <md:MaterialTheme SeedColor="#6750A4" PaletteMode="Light" />

    <!-- Optional: Extra Controls Theme (Rich Editor, Chat, Timeline, etc.) -->
    <extra:ExtraTheme />
  </Application.Styles>
</Application>
```

---

## 3. Dynamic Color & Theme Switching

You can change the seed color and theme mode dynamically at runtime in C#:

```csharp
using Avalonia;
using Avalonia.Media;
using Md3.Avalonia.Themes;

public static void UpdateTheme(Color newSeedColor, bool isDarkMode)
{
    var theme = Application.Current?.Styles.OfType<MaterialTheme>().FirstOrDefault();
    if (theme != null)
    {
        theme.SeedColor = newSeedColor;
        theme.PaletteMode = isDarkMode ? MdPaletteMode.Dark : MdPaletteMode.Light;
    }
}
```

---

## 4. Basic Window Example

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:md="using:Md3.Avalonia.Controls"
        x:Class="MyApp.MainWindow"
        Title="Material 3 App"
        Width="800" Height="600"
        Background="{DynamicResource Md.Sys.Color.Surface.Brush}">
  <StackPanel Margin="24" Spacing="16">
    <TextBlock Classes="h1" Text="Welcome to Md3.Avalonia" />
    
    <md:MdTextBox Label="Username" Variant="Outlined" ShowClearButton="True" />
    
    <StackPanel Orientation="Horizontal" Spacing="12">
      <md:MdButton Content="Filled Button" Variant="Filled" />
      <md:MdButton Content="Tonal Button" Variant="Tonal" />
      <md:MdButton Content="Outlined Button" Variant="Outlined" />
    </StackPanel>
  </StackPanel>
</Window>
```
