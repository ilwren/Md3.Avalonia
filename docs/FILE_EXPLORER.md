# Provider-backed Material 3 file explorer

`Md3.Avalonia.Extra` provides a reusable file-browser surface and provider-neutral storage contract. The browser uses Material 3 controls on compact phone layouts and expanded desktop layouts; it does not switch to a platform-specific Fluent or macOS visual language.

## Quick start

```csharp
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;

var provider = new MdLocalFileSystemProvider(
[
    new MdFileLocation("documents", "Documents", documentsDirectory),
    new MdFileLocation("downloads", "Downloads", downloadsDirectory)
]);

var explorer = new MdFileExplorer
{
    Provider = provider,
    ViewMode = MdFileViewMode.Details,
    IsPreviewPaneOpen = true,
    GlyphSelector = entry => entry.IsDirectory ? MdSymbols.Folder : MdSymbols.Description
};

explorer.EntryOpened += (_, entry) => OpenInYourDocumentWorkflow(entry);
```

The host supplies the roots explicitly. `MdLocalFileSystemProvider` does **not** enumerate every disk, automatically expose a user's whole home folder, or ask mobile apps for broad storage permissions. Configure only roots the host is allowed to expose. The gallery uses a temporary demo directory for this reason.

## Provider contract

`IMdFileSystemProvider` owns storage discovery, directory listing, operations and previews. The UI addresses a `MdFileLocation` by its opaque `Id`; every `MdFileEntry.RelativePath` is relative to that location and uses `/` separators for portable display. An implementation may instead map those relative paths to document IDs, content URIs, cloud objects or another non-filesystem identifier. `IMdFileThumbnailProvider` is an optional extension for providers that can load image thumbnails independently from full previews; the grid requests up to 24 image entries in the current folder and otherwise uses its glyph/text fallback.

A provider implements:

- `GetLocationsAsync` for host-approved roots;
- `GetEntriesAsync(locationId, relativePath, includeHidden, cancellationToken)`;
- `ExecuteAsync(request, progress, cancellationToken)` for create, rename, delete, copy and move;
- `GetPreviewAsync` for provider-owned preview content.

Android Storage Access Framework/document-tree access should be implemented as an `IMdFileSystemProvider` that retains the granted tree/document URI. Do not convert SAF URIs into guessed local paths. The stock local provider intentionally rejects traversal outside a configured root and refuses to follow reparse points/symbolic links; symlinks are listed as non-navigable entries. A final link can be deleted, renamed or moved natively on the same filesystem as a link entry; copying it (including a cross-volume copy-based move) or traversing through it is rejected. Host applications remain responsible for choosing trusted root directories and any platform-specific permissions or URI grants.

## Included UI

- `MdFileExplorer`: responsive shell, location list, back/forward/up/refresh, path navigation, current-folder search, hidden-item toggle, list/grid/details modes, visible sort direction, multiselection, Material confirmation/conflict dialogs, operation queue and status feedback.
- `MdFileLocationBar`: location picker, up navigation, collapsing breadcrumbs and an editable location-relative path field. It only raises navigation requests; it does not touch storage itself.
- `MdFileItemsView`: a `ListBox` with multiple selection, list/grid/details item templates, Enter/double-click activation, compact touch folder activation, long-press selection, F2 rename, Delete and Escape behavior. The virtualizing stack panel is used for list/details; grid mode uses a wrapping panel.
- `MdFileSelectionToolbar` and `MdFileContextMenu`: Material command surfaces that can be extended with `MdFileAction` and `ICommand`; the selection surface exposes Open for a single item and a clear-selection action.
- `MdFilePreviewPane`: image, bounded text and metadata preview with a clear Open action for files. Disposable images returned by a provider transfer ownership to the consumer; `MdFileExplorer` releases replaced previews and thumbnails.
- `MdFileOperationQueue` and `MdFileOperationCenter`: serialized provider calls, cancellation, determinate/indeterminate progress and completed/conflict/error state.

Set `GlyphSelector` to return a symbol codepoint when the host references an icon-font provider such as `Md3.Avalonia.Icons`. Without an icon provider, file tiles display a text fallback, so the Extra package does not acquire a mandatory font dependency.

## Touch, keyboard and navigation workflow

The interaction model was checked against the clean-room-relevant flows in [`filesystem_picker`'s file tile and picker page](https://github.com/andyduke/filesystem_picker/tree/master/package/lib/src) and [`FileX`'s home controller and item widgets](https://github.com/yatinrana01/FileX/tree/main/lib/app). Those Flutter sources provide useful concrete examples of tapping folders to navigate, selectable file rows, breadcrumb navigation, go-up actions, item long-press affordances, and path history. No Dart source or package assets are copied.

- In compact layouts, one touch tap opens a navigable folder. A file tap selects it; the preview and selection actions then provide explicit Open. Touch long-press starts multi-selection, and subsequent taps toggle entries. Clear selection (×) in the action bar or Escape exits the mode. Desktop continues to use double-click/Enter to activate items and standard keyboard modifiers for multi-selection.
- Back, forward, and up buttons stay enabled only when their destinations exist. Back/forward keep separate history stacks; a new path navigation clears the forward stack. Alt+Left/Right navigates history, Alt+Up goes up, F5 refreshes, Ctrl/Cmd+L edits the path, Ctrl/Cmd+F focuses search, Ctrl/Cmd+A selects all, F2 renames, Delete requests deletion, and Escape clears search/selection.
- Copy and Move open a folder navigator instead of requiring users to type a destination path. Locations, breadcrumbs and the up button can be used to browse; tap/double-click a folder to enter it, then choose **Use this folder**. The currently sorted details column displays an ascending/descending arrow.

The file-open action raises `MdFileExplorer.EntryOpened`; the host decides whether to open a document in-app or delegate it to the operating system. The long-press affordance is adapted to selection for this explorer rather than copying FileX's host-specific share-file callback.

## Local preview and operation behavior

The local provider supports text/configuration previews capped at 16 Ki characters, and decodes grid thumbnails to a 256-pixel target width for common bitmap formats understood by Avalonia. A selected image preview uses the full decoded image. Other file types return a descriptive no-inline-preview result; opening an external application is deliberately delegated through `EntryOpened` to the host.

Local copy stages into a unique sibling and moves it into place only after the full copy completes. Cross-volume local moves fall back to copy-then-delete; if source cleanup fails after the full destination copy, the complete copy is preserved and any replaced destination is preserved under a keep-both recovery name rather than rolling back the copy. Such failures set `MdFileOperationResult.HasPartialChanges`, prompting the explorer to refresh the visible directory despite reporting the operation error. Name conflicts are data (`MdFileOperationResult.HasConflict`) so the UI can ask for Skip, Keep both, Replace or Cancel; the keep-both policy generates a unique suffix. Replace first moves the old target to a unique sibling backup, restoring it if the new operation fails. The Explorer asks before permanent deletion. Recursive deletion reports item progress and checks cancellation between entries; cancelling mid-delete can leave a directory partially emptied. Errors are returned to the operation center/status area rather than hidden.

Search currently filters the entries in the open directory; recursive indexing/search is a provider-level extension point, not an implicit full-disk crawl.

## Gallery and tests

Open **File explorer** in the Gallery for a temporary local-provider example. The headless test suite covers path traversal and symbolic-link protection, hidden-item filtering, conflict/keep-both behavior and rollback, bounded previews/thumbnails, cancellable delete progress, late provider-result isolation, copy/move results, compact/expanded layouts, Material rendering and Gallery indexing.
