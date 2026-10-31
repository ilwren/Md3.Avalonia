# Direct-path file-system picker research

Research dates: 2026-10-25 (behavior), 2026-10-26 (visual redesign)

This note records the required pre-implementation sequence for `MdFileSystemPicker`. The control is
an Md3.Avalonia project decision, not an official Material component.

## 1. Avalonia control studied first

The closest reusable Avalonia design is its managed file chooser:

- [`ManagedFileChooserViewModel`](https://github.com/AvaloniaUI/Avalonia/blob/12.1.2/src/Avalonia.Dialogs/Internal/ManagedFileChooserViewModel.cs)
- [`ManagedFileChooserSources`](https://github.com/AvaloniaUI/Avalonia/blob/12.1.2/src/Avalonia.Dialogs/Internal/ManagedFileChooserSources.cs)
- [Fluent managed chooser template](https://github.com/AvaloniaUI/Avalonia/blob/12.1.2/src/Avalonia.Themes.Fluent/Controls/ManagedFileChooser.xaml)

Useful behavior retained: direct `DirectoryInfo.EnumerateFileSystemInfos`, parent/refresh/location
navigation, folders-first sorting, hidden-item filtering, file-name patterns, selectable rows,
multiple selection, and file/folder modes.

Behavior deliberately not copied: `IStorageFolder`, `TryGetLocalPath`, `IStorageProvider`, native
picker options, global service lookup, desktop-only dense columns, and silently swallowing
`UnauthorizedAccessException`. `MdFileSystemPicker` reports inaccessible locations and exposes only
normalized `System.IO` paths.

## 2. Material redesign from that baseline

The managed chooser was re-composed as an independent `MdFileSystemPicker : TemplatedControl` with
a scoped `ControlTheme` and `PART_` contract. Its template uses the existing `MdTextBox`, `MdList`,
`MdButton`, `MdIconButton`, `MdCheckBox`, and progress controls. Component geometry and spacing live
under `Md.Comp.FileSystemPicker.*`; colors and type use runtime `DynamicResource` system tokens.

The desktop column table was replaced with touch rows, a compact detail line, and an explicit
open-folder affordance. The second visual pass gives each entry a 48-DIP tonal icon tile, a clear
selected container and trailing check, breadcrumb-first navigation, an on-demand raw path editor,
a fixed confirmation surface, and an expanded-width quick-access rail. The rail collapses on phones,
where Home moves into the app-bar actions and the picker remains suitable for a full-screen dialog.
Loading, empty, and permission/error states remain visible. Enter/double-click activates a row; F5
refreshes; Back goes up when focus is not editing the path. Folder mode can choose the current
location, avoiding a touch-hostile distinction between selecting and opening the same row.

## 3. Flutter equivalent studied last

Flutter's maintained equivalent is the federated
[`file_selector`](https://github.com/flutter/packages/tree/main/packages/file_selector/file_selector)
plugin:

- [`file_selector.dart`](https://github.com/flutter/packages/blob/main/packages/file_selector/file_selector/lib/file_selector.dart)
- [`XTypeGroup`](https://github.com/flutter/packages/blob/main/packages/file_selector/file_selector_platform_interface/lib/src/types/x_type_group.dart)
- [directory example](https://github.com/flutter/packages/blob/main/packages/file_selector/file_selector/example/lib/get_directory_page.dart)

The public semantics informed separate file, multi-file, and directory outcomes; an initial
directory; confirm text; cancellation represented by no result; and extension groups that normalize
a leading dot. The transport did **not** carry over: Flutter delegates to platform dialogs (and on
Android therefore platform document-provider behavior), while this control intentionally browses
only process-accessible real file-system paths.

The visual-redesign pass also studied Flutter implementations that draw their own browser:

- [`andyduke/filesystem_picker`](https://github.com/andyduke/filesystem_picker), especially its
  [`picker_page.dart`](https://github.com/andyduke/filesystem_picker/blob/master/package/lib/src/picker_page.dart),
  [`breadcrumbs.dart`](https://github.com/andyduke/filesystem_picker/blob/master/package/lib/src/breadcrumbs.dart),
  and full-screen/dialog/bottom-sheet presentation. Its hierarchy and shortcuts are useful, while
  its older Material 2 styling was not copied.
- [`Senzme/NFile`'s internal picker](https://github.com/Senzme/NFile/blob/main/lib/ui/screens/internal_file_picker_screen.dart)
  informed the title/path app-bar hierarchy, 48-DIP tonal icon tiles, selected row treatment,
  storage shortcut surface, and selection-aware confirmation. NFile is GPL-3.0: no source, custom
  icons, textures, or assets were copied; these generic interaction ideas were independently
  re-composed from existing Md3.Avalonia controls and tokens.
- [`DevsOnFlutter/file_manager`](https://github.com/DevsOnFlutter/file_manager) reinforced the
  separation between file-system enumeration and replaceable list/grid presentation. ZenFile was
  reviewed as a richer NFile derivative, but its neon/glass brand treatment and dual-pane manager
  chrome were deliberately excluded from this focused MD3 picker.

The resulting visual direction is therefore NFile's mobile information hierarchy plus
`filesystem_picker`'s breadcrumb clarity, constrained to standard Material 3 surfaces, typography,
state colors, touch targets, and this repository's own icon geometry.

## Follow-up interaction and presentation decisions

The responsive redesign was subsequently hardened in four areas:

- every glyph is drawn in a fixed 24×24 transparent `Canvas` and scaled by a centered `Viewbox`, so asymmetric path bounds cannot move the visual viewport off-center;
- single selection uses Avalonia's native `SelectionMode.Single | SelectionMode.Toggle`, matching multiple mode's ability to clear an accidental selection;
- `BreadcrumbStyle` exposes `Standard` and connected `Powerline` presentations without changing path semantics. The Powerline treatment now uses the same complementary 14-DIP notch/body/point geometry and overlap as the original `MdBreadcrumb`, with a scoped zero-minimum-width button theme so generic button targets cannot create gaps between segments;
- folder activation resolves the entry from the actual double-tap source before consulting selection. This is required because the intentional Toggle selection mode can deselect the row on the second click;
- visible labels, state messages, entry type/date/number formatting, tooltips and automation names follow inherited `MdLocalization.Culture` (English/Chinese). Public text properties such as `Title`, `PathLabel`, action labels and state labels remain locally overridable and are not overwritten by later culture changes.

These are additive clean-room decisions built from the same Avalonia selection/localization primitives; no reference package source or icon asset was copied.

## Platform boundary

Not using SAF does not defeat Android scoped storage. App-private directories work directly. Shared
or protected locations still require whatever access the OS grants to the process; some locations
cannot be made available as paths without a platform-specific permission or policy exception. The
control surfaces that failure rather than turning a content URI into a misleading pseudo-path.
