# Changelog

## [0.3.5-preview.1] - 2026-10-04

Previously numbered `3.1.0-preview.1`, which was a typo for `0.3.1-preview.1`. The packages
were never tagged or published under it, so this entry is renumbered in place rather than
leaving a version that reads as a major release.

### Added

- `MdBorderlessWindow` now inherits `Window.Icon` into the Material title bar by default.
- Added independent title-bar icon visibility and caption-button state controls.
- `MdPagedItemsView.ItemTemplate`, so loaded records can be presented as something other than
  their `ToString()`.
- A dedicated "Numeric input" Gallery page covering the stepper variants, a suppressed stepper,
  fractional steps and read-only values.

### Changed

- Preserved native Windows DWM minimize, maximize, and restore animations.
- Unified the title bar with the window `Surface` and removed the visible divider line.
- Updated desktop and Android Gallery behavior and release documentation.
- The transfer control drags a picture of the row in the overlay layer instead of translating
  the row itself.

### Fixed

- Prevented native and Material window frames from being rendered as a double border.
- Corrected headless template expectations for native-frame behavior.
- A row dragged between the transfer lists is no longer clipped away by its own scroll viewport.
- `MdHero` flights started from a just-revealed destination now measure it after layout instead
  of photographing a zero-sized rectangle and abandoning the transition.
- `MdSlidableItem` clips its swipe actions to the row's rounded shape, so the action colours no
  longer show through at the four corners.
- The numeric stepper's minus glyph is centred in its target; its 12x2 ink had been declared in
  a 12x12 box, which seated it at the top.
- Expanded search reopens its result list when the header is typed in again after a commit.
- `MdSimpleDialog` no longer reports a dismissal when an item is chosen.
- Rating, breadcrumb and tree-view rendering and hit-testing defects from the Gallery review.

### Documentation

- Corrected `docs/API.md`: the `MdPagedItemsView` and `MdResultView` samples used properties
  that do not exist, `MdRangeSlider` is `LowerValue`/`UpperValue` rather than
  `StartValue`/`EndValue`, and the search controls expose `Text`/`SearchSubmitted`/
  `ResultCommitted` rather than `Query`/`SearchResultCommitted`.
- Corrected the borderless-window icon default in both `README.md` and `docs/API.md`: a
  borderless window shows its icon by default, which both documents had stated backwards.
- Refreshed the token-dictionary listing in `README.md`, which named 7 of the 11 dictionaries.

For details, see [`docs/RELEASE_NOTES_0.3.5-preview.1.md`](docs/RELEASE_NOTES_0.3.5-preview.1.md).

All notable changes follow Keep a Changelog. The project intends to use Semantic Versioning after the preview cycle.

## [0.3.0-preview.1] - 2026-10-03

### Added

- Added an Android `EmbeddableControlRoot` visual-layer host so ComboBox and other popup controls can use Avalonia's overlay fallback in a single-view lifetime.
- Added a complete, grouped and scrollable Android Gallery navigation shell covering every registered sample page.
- Added `MdFabMenu.IsInitiallyOpen` while preserving two-way `IsOpen` and independent up/down expansion.
- Added configurable ColorPicker preview, palette, spectrum and recent-color panels plus an internal constrained-height scroll viewport.
- Added focused P0/P1/P2 accessibility, responsive, localization, motion and token regression tests.
- Added the complete official Material Symbols Rounded variable TTF and a real-outline Lite subset, both pinned and verified from the Google upstream asset.
- Added a standalone Material theme for native Avalonia `ItemsControl`, so `ItemsSource` plus `ItemTemplate` renders without requiring FluentTheme.

### Changed

- Completed the 2026-10-02 audit remediation across menu/modal focus, picker and range semantics, RTL, reduced motion, touch targets, automation peers, overlay behavior, responsive layouts and inherited en/zh localization.
- Replaced fixed outlined-field label patches with transparent outline notches and kept host backgrounds visible.
- Retargeted tab indicator spatial motion with a velocity-preserving spring runner; reduced/none schemes snap without spatial animation.
- Updated app bars, toolbars, button groups and split buttons to audited Material component tokens and logical RTL geometry.
- Made Cascader a true popup, Transfer and CommandPalette responsive, and Breadcrumb overflow behavior invokable.
- Improved failed-test diagnostics so GitHub annotations include the exact headless test name.
- Aligned breadcrumb overflow with the audited Flutter package model: wrap, horizontal scroll, or explicit collapse, with no trailing divider or persistent parent selection by default.
- Changed carousel geometry to follow Material keyline/navigation state rather than pointer selection; small items now stay in the official 40–56 DIP range.
- Reworked Gallery navigation selection so exactly one destination owns the active indicator, and added 360/412 DIP Android page constraint/reflow handling.

### Fixed

- Fixed connected/standard button-group defaults so group-managed style-trigger values never overwrite explicit local child values.
- Fixed compact rating hit testing to retain full 48-DIP input cells around smaller visible stars.
- Fixed chat selection so only presses inside the visual message bubble select a row.
- Fixed Cascader open staging and prevented initial keyboard focus from committing a hierarchy selection.
- Fixed the split-button two-DIP visible gap while preserving the independent 48-DIP trailing touch target.
- Fixed Windows custom chrome to preserve Full native caption style bits for Windows 11 DWM state animations, use the Win32 system menu, and avoid drawing a second outer border.

### Validation status

- Desktop Gallery build, NuGet package verification and the full headless suite are required green release gates.
- Physical Android, Narrator, VoiceOver and Orca sign-off remain explicit external validation items; this preview does not claim those checks without recorded hardware/OS evidence.

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

- Physical Android ARM64, Narrator, VoiceOver and Orca acceptance require external hardware/OS sign-off; see `docs/RELEASE_VALIDATION.md`.
