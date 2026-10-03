# Changelog

All notable changes follow Keep a Changelog. The project intends to use Semantic Versioning after the preview cycle.

## [0.2.0] - 2026-10-02

- Fixed Gallery diagnostics restore, Android deployment mappings, scrolling input parity, FAB menu alignment, breadcrumb rendering, and color-picker selection.
- Updated all NuGet packages to `0.2.0` and added Material 3 HCT tonal-palette generation for color picking.
- Added independent up/down FAB-menu expansion and refreshed the custom color picker with current Material surfaces, connected mode controls, accessible swatches, and generated role previews.
- Fixed `MdScrollBar` hover sizing so the track-computed thumb length remains stable and mouse thumb dragging works.
- Added type-selected `MdDialogHost.DataTemplates` support and an `MdSnackbarHost`/`IMdSnackbarService` queue for ViewModel-driven transient messages.
- Changed desktop `MdScrollViewer` mouse panning to opt-in and made child direct-manipulation gestures take precedence over page dragging.
- Fixed the Android Gallery startup theme to inherit from AppCompat, as required by `AvaloniaMainActivity`.
- Added standalone Material `Window`, `PopupRoot`, and `OverlayPopupHost` themes so Android can use Avalonia's popup fallback while desktop retains native popups.
- Replaced fixed-color outlined field label patches with transparent stroke notches for `MdTextBox` and `MdComboBox`.

## [0.1.0-preview.1] - 2026-09-24

### Added

- Independent Material 3 and Material 3 Expressive control types with scoped Avalonia 12 `ControlTheme` resources.
- Buttons, icon buttons, FABs, fields, selection controls, cards, carousel, chips, pickers, dialog, lists, progress, menus, navigation, search, sheets, slider, snackbar, tabs, toolbars and tooltips.
- HCT dynamic theme generation for arbitrary seed colors with six scheme variants, three contrast settings, Light/Dark/System, motion, font and shape options.
- Theme JSON import/export, contrast diagnostics and interactive Gallery Theme Lab.
- Five Material layout breakpoint bands, compact modal Gallery navigation, searchable component index and page-derived table of contents.
- English/Simplified Chinese localization infrastructure, pointer-origin ripple and inherited motion schemes.
- Virtualized list/carousel panels, accessibility metadata and an Android single-view Gallery host project.
- NuGet metadata, XML documentation, third-party notices, API overview, compatibility and release-validation documentation.
- Three independently packable libraries: core Material controls, optional Material Symbols provider, and clean-room Flutter ecosystem controls with one-way dependency direction.
- Ecosystem Waves A–F: overlay/focus/shortcut/async/density contracts; popover, hover card and command palette; slidable/paged/masonry/data-grid controls; async selection/calendar/timeline/result/cascader/transfer; provider-neutral chart/editor/chat; skeleton and animation sequencing; segmented PIN/OTP input, hierarchical tree view, and Material tag input.
- Borderless-window W0–W3: Material title bar/caption buttons, drag/resize regions, replaceable platform adapters, Android-safe fallback, Gallery and headless coverage.

### Changed

- Gallery shell now supports compact `<600`, medium `600–839`, expanded `840–1199`, large `1200–1599` and extra-large `>=1600` layouts.
- `MdAdaptiveLayout` now exposes the same five named content breakpoints, orientation and adaptive input mode.
- `MdCarousel` adds a controller, autoplay, hover pause and finite/infinite navigation; `MdRefreshIndicator` adds a cancelable async handler.
- `MdLoadingIndicator` now derives from `ProgressBar` to expose native progress automation semantics.
- `MdSnackbar` announces updates as a polite live region; decorative ripple, state and focus layers are excluded from automation control/content views.
- `MdTimePicker` dial mode now uses a draggable M3 clock face with one-minute precision and a 24-hour inner ring; localized date/calendar collection hosts no longer depend on a global Fluent theme.
- Ecosystem collection, calendar, transfer, command, rich-editor and chat templates now use complete Material primitives; chart axes include data labels and bar bands remain inside the plot.
- Reorderable list, GridTile, dialog/license, adaptive/focus/shortcut, selection/feedback and chat Gallery sections now expose observable task flows instead of static decoration.
- Rapid `MdTabView` changes use one selected-content presenter, `MdRating` quantizes inside each star, and numeric spinner glyphs use compact vertically centered action areas.
- `MdBorderlessWindow` preserves the platform border/resize frame by default while extending Material content into the caption area.
- The experimental media-preview controls and Gallery page were removed.

### Known limitations

- Material Symbols Rounded TTF is intentionally excluded from this preview; unresolved symbol slots remain visually empty until the optional Icons package or a host-provided font/resource mapping is registered.
- Physical Android ARM64, Narrator, VoiceOver and Orca acceptance require external hardware/OS sign-off; see `docs/RELEASE_VALIDATION.md`.
