# Md3.Avalonia 0.2.0 release notes

Released 2026-10-02.

## Highlights

- Fixed `MdScrollViewer` input ownership: Avalonia handles wheel, touch/pen gestures, inertia, snap points, and chaining, while the optional desktop path handles mouse dragging without suppressing ordinary child clicks.
- Corrected trailing-edge alignment and anchored expansion for Material 3 FAB menus.
- Restored visible rich breadcrumb labels, icons, separators, and current-item state in both Extra theme entry points.
- Reworked `MdColorPicker` interaction with executable swatch commands, visible selection state, mode controls, Material-styled HSV/alpha controls, HEX synchronization, and Material Color Utilities HCT tonal palettes.
- Restored the compact `MdColorPickerButton` popup interaction.
- Removed the obsolete Gallery `Avalonia.Diagnostics` dependency and enabled Android Build/Deploy solution mappings.
- Versioned all four NuGet packages and the Android Gallery as `0.2.0`.

## Packages

- `Md3.Avalonia` 0.2.0
- `Md3.Avalonia.Icons` 0.2.0
- `Md3.Avalonia.Icons.Lite` 0.2.0
- `Md3.Avalonia.Extra` 0.2.0
