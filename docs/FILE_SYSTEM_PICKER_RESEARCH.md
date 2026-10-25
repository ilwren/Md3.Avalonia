# Direct-path file-system picker research

Research date: 2026-10-25

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

The desktop column table was replaced with 56-DIP touch rows, a compact detail line, and an explicit
open-folder affordance. The layout reflows without a sidebar, supports full-screen phone dialogs,
and has visible loading, empty, and permission/error states. Enter/double-click activates a row;
F5 refreshes; Back goes up when focus is not editing the path. Folder mode can choose the current
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

## Platform boundary

Not using SAF does not defeat Android scoped storage. App-private directories work directly. Shared
or protected locations still require whatever access the OS grants to the process; some locations
cannot be made available as paths without a platform-specific permission or policy exception. The
control surfaces that failure rather than turning a content URI into a misleading pseudo-path.
