# Md3.Avalonia 0.3.0-preview.1 release notes

Preview date: 2026-10-03.

`0.3.0-preview.1` is the first 0.3 preview. It consolidates the P0/P1/P2 remediation recorded in the 2026-10-02 full-component audit and the subsequent Android, FAB menu, ColorPicker and regression fixes.

## Highlights

- Android single-view popup support: `EmbeddableControlRoot` now owns the named `VisualLayerManager` required by Avalonia's overlay popup fallback. `MdComboBox` does not force overlay mode on desktop and can fall back safely on Android.
- Complete Android Gallery navigation: all registered Start, Samples, Components and Library pages are reachable through a grouped, scrollable modal drawer.
- Outlined fields use real transparent stroke notches instead of painting a fixed surface-colored label patch.
- `MdFabMenu` retains independent `ExpansionDirection` geometry and adds `IsInitiallyOpen`; `IsOpen` remains the authoritative two-way live state.
- `MdColorPicker` scrolls inside constrained dialogs/popups and exposes independent visibility controls for its preview, mode selector, Material palette, spectrum and recent-color panels.
- Audited accessibility and input behavior now covers modal focus, menus, search, date/time/range semantics, table/tree/chart/rating automation, 48-DIP targets, RTL and reduced motion.
- App bar, toolbar, button-group and split-button tokens were aligned with the audited Material/Flutter baselines.
- Tabs use a velocity-preserving spatial spring runner; responsive CommandPalette, LicensePage and Transfer layouts avoid previous fixed desktop widths.
- Core and Extra default visible/automation strings use inherited English/Simplified Chinese localization for the audited component families.

## Packages

- `Md3.Avalonia` 0.3.0-preview.1
- `Md3.Avalonia.Icons` 0.3.0-preview.1
- `Md3.Avalonia.Icons.Lite` 0.3.0-preview.1
- `Md3.Avalonia.Extra` 0.3.0-preview.1

The Android Gallery reports `0.3.0-preview.1` with numeric application version `3`.

## Validation

Release gates are:

1. Release Desktop Gallery build with warnings treated as errors.
2. Full `Md3.Avalonia.HeadlessTests` run with zero failures.
3. Independent NuGet pack/metadata/dependency verification for all four packages.
4. Multi-platform Gallery build workflow.

Android Maestro/logcat evidence remains available in the Android workflow, but physical Android hardware, Narrator, VoiceOver and Orca acceptance must still be signed off with the device/OS details recorded in `docs/RELEASE_VALIDATION.md`. This preview does not convert unexecuted manual checks into compliance claims.

## Compatibility notes

- Targets .NET 10 and Avalonia 12.1.2.
- The Icons packages remain optional. Core and Extra do not require an icon provider.
- API-shell Flutter parity types remain explicitly experimental/partial as listed in `docs/FLUTTER_PARITY_STATUS.md`.
- Existing `MdFabMenu.IsOpen` bindings continue to work; `IsInitiallyOpen` is opt-in and applied only on first visual-tree attachment.
