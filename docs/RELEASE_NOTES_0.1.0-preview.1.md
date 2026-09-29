# Md3.Avalonia 0.1.0-preview.1 release notes

## Highlights

- Independent Avalonia 12 Material 3/Expressive control library with no global native-control theme replacement.
- 49-role HCT dynamic theming from any seed color, six palette variants, Standard/Medium/High contrast and Light/Dark/System.
- Theme Lab with live cross-component preview, contrast diagnostics and JSON/file import/export.
- Website-style Gallery with five official Material width classes, compact modal navigation, indexed search and dynamic table of contents.
- Desktop adapters, pointer-origin ripple, inherited motion schemes, localization, virtualized lists/carousels and automation hardening.
- NuGet-ready metadata and an Android single-view sample host.

## Upgrade and compatibility

This is the first packaged preview, so there is no earlier package migration. The three release packages target `net8.0;net10.0` and Avalonia `12.1.2`; see `COMPATIBILITY.md`. Public control/property/resource and theme-JSON names should be treated as preview API until the first stable release.

## Known limitations

- The Material Symbols Rounded binary is intentionally not committed. `Md3.Avalonia.Icons` now requires the publisher to supply the official font offline at its reserved path (or through `MaterialSymbolsRoundedFontFile`) and embeds it into the package; compilation fails when it is absent.
- Android source host is present, but Android workload installation and ARM64 device/emulator validation were not available in this environment.
- Narrator, VoiceOver and Orca acceptance remain manual external gates.
- iOS and browser are not preview-release targets.

## Validation

With an offline font path supplied, the desktop solution builds in Release with 0 warnings and 0 errors and all 196 headless tests pass. Core, Icons and Ecosystem packages were generated with both `lib/net8.0` and `lib/net10.0` assets and inspected outside the workspace, then excluded from delivery. See `RELEASE_VALIDATION.md` for automated evidence and the unsigned manual matrix.
