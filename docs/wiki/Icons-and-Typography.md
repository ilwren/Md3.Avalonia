# Icons & Typography

Md3.Avalonia includes first-class support for **Material Symbols Rounded** with embedded TTF font resources that work 100% offline with zero manual downloads.

---

## 1. Choosing an Icon Package

### Package Comparison:

| Feature | `Md3.Avalonia.Icons` | `Md3.Avalonia.Icons.Lite` |
| :--- | :--- | :--- |
| **Glyph Count** | 4,405 mapped code points / 6,646 glyphs | 45 curated code points / 66 glyphs |
| **Binary Size** | ~14.5 MiB official variable TTF | ~98 KiB real-outline subset |
| **Best For** | Full-featured apps requiring specialized icons | Mobile, embedded, or bandwidth-conscious applications |
| **Class Name** | `MdSymbols.<IconName>` | `MdSymbolsLite.<IconName>` |

---

## 2. Using Icons in XAML

### Using `MdIcon`
```xml
<md:MdIcon Glyph="{x:Static md:MdSymbols.Favorite}" Size="24" Foreground="{DynamicResource Md.Sys.Color.Primary.Brush}" />
```

### Using Icons on Buttons & FABs
```xml
<!-- Filled button with icon -->
<md:MdButton Content="Add Item" Icon="{x:Static md:MdSymbols.Add}" Variant="Filled" />

<!-- Floating Action Button -->
<md:MdFloatingActionButton Icon="{x:Static md:MdSymbols.Edit}" />

<!-- Icon Button -->
<md:MdIconButton Icon="{x:Static md:MdSymbols.Settings}" />
```

---

## 3. Direct Font Family Usage in XAML
You can also use the embedded font directly with unicode character strings:
```xml
<TextBlock FontFamily="{DynamicResource MaterialSymbolsRounded}" Text="&#xe87d;" FontSize="24" />
```
