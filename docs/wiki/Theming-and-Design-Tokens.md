# Theming & Design Tokens

## 1. Material Design 3 HCT Color System
Md3.Avalonia generates complete 30-token Material Design 3 color schemes dynamically using Google's HCT (Hue, Chroma, Tone) color model from a single seed color.

### Core Color Roles:
- **Primary & OnPrimary**: Key component accents and high-emphasis actions.
- **PrimaryContainer & OnPrimaryContainer**: Low-contrast backgrounds for containers, pills, and active states.
- **Secondary & Tertiary**: Distinct accent hues for secondary accents, tags, and tertiary controls.
- **Surface & OnSurface**: Main application window background and body text.
- **SurfaceContainer / SurfaceContainerHigh / Highest**: Layered surfaces and floating cards.
- **Outline & OutlineVariant**: Borders, dividers, and inactive focus outlines.
- **Error & OnError**: Destructive actions and validation feedback.

---

## 2. Dynamic Theme Switching in XAML and C#

In `App.axaml`:
```xml
<md:MaterialTheme SeedColor="#3F51B5" PaletteMode="Light" />
```

To switch palettes dynamically:
```csharp
var theme = Application.Current?.Styles.OfType<MaterialTheme>().FirstOrDefault();
if (theme != null)
{
    theme.SeedColor = Color.Parse("#006A60"); // Teal Green
    theme.PaletteMode = MdPaletteMode.Dark;   // Switch to Dark Mode
}
```

---

## 3. Elevation & Shadow Tokens
Md3.Avalonia defines standard MD3 elevation levels:
- `Md.Sys.Elevation.Level0` (0 dp)
- `Md.Sys.Elevation.Level1` (1 dp)
- `Md.Sys.Elevation.Level2` (3 dp)
- `Md.Sys.Elevation.Level3` (6 dp)
- `Md.Sys.Elevation.Level4` (8 dp)
- `Md.Sys.Elevation.Level5` (12 dp)
