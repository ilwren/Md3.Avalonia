# Changelog

## [Unreleased]

### Added

- **Tray icon menu** for the desktop Gallery: show/restore window, Light/Dark/System theme
  radio items, and an Exit item that owns shutdown; closing the main window now hides it while
  the tray is present. Desktop-only — the Android single-view lifetime never installs it.
  Icon asset is `gallery/Md3.Avalonia.Gallery/Assets/tray-icon.png`.
- **Native AOT publish scripts**: `scripts/publish-gallery-aot.sh` and
  `scripts/publish-gallery-aot.ps1` publish the desktop Gallery with `PublishAot=true`, assert
  the output is a native executable with no managed app assembly beside it, optionally
  smoke-run it under xvfb (`--smoke`), and clean bin/obj afterwards.
  `.github/workflows/aot-probe.yml` now runs the same script on `main` and `arena/**` instead
  of being pinned to a stale session branch.
- Regression tests pinning the `MdScrollViewer` scrollbar calibration on the first layout pass
  (`MdScrollBarFirstFrameTests`): overflow, virtualized and no-overflow compositions must show
  correct `Maximum`/`ViewportSize`/visibility without a window resize.

### Fixed

- **`MdScrollViewer`'s scrollbar was wrong on the very first frame (desktop platforms; a window
  resize repaired it).** The template relied solely on `ScrollBar.AttachToScrollViewer`, whose
  self-bindings are created when the bar attaches to the visual tree; with unlucky ordering the
  bar rendered RangeBase defaults (a full-track thumb) until the next invalidation — classically
  a window resize. `MdScrollViewer` now mirrors `Maximum`/`Value`/`ViewportSize`/`Visibility`
  onto both `MdScrollBar` parts directly and synchronously on every
  Extent/Viewport/Offset/visibility change (plus once at template application), so the bars
  track the owner from the very first frame on every platform, with no resize and no
  binding/attach ordering involved; `AttachToScrollViewer` skips properties that are already
  set, so the two mechanisms cannot fight. Regression tests cover the first layout pass and a
  content-grows-after-layout resync.
- **`MdTabView` now exposes `Variant` (`MdTabVariant.Primary`/`Secondary`)**, unifying the style
  dimension previously spread across `MdTabs`/`MdTabsView`: primary renders the text-first M3
  look with a full-width active indicator, secondary (default) keeps icon+label with the short
  centered indicator. The value is pushed to every `MdTabViewItem` container.
- **`MdTabView` works around upstream Avalonia issue #22094**: at non-100% Windows display
  scaling (125/150/200%), `DockPanel.Dock="Bottom"` children render invisible on the first
  frame - the fill sibling is arranged over the whole client area and paints above the strip,
  while the strip's own bounds can look correct; only a manual resize repairs it. This is the
  exact reported "bottom tab strip missing on open with Min/Max window constraints, appears
  after resize" defect. `MdTabView` now verifies, after the first layout, both the strip rect
  and the strip/host overlap, and when either is wrong applies an imperceptible one-time,
  one-device-pixel width nudge that forces the same full relayout a manual resize would.
- **`MdTabView`'s tab strip now scrolls through an `MdScrollViewer`** instead of the native
  `ScrollViewer`. The native control carries no template in apps that load only MaterialTheme,
  which made the strip's first-frame desktop layout depend on whether the app merged a base
  theme; the themed `MdScrollViewer` is deterministic and calibrated from its own first layout
  pass.
- **Tray icon right-click menu rendered empty** in apps that only load the Material themes: the
  Win32 tray popup is a bare `Window` hosting a `MenuFlyoutPresenter` whose containers are
  native `MenuItem`/`Separator` controls, and with no base theme those types had no
  ControlTheme. `MdDesktopAdapters.axaml` now ships compact Material themes for
  `MenuFlyoutPresenter`, `MenuItem` (incl. radio/checkbox indicators and submenus) and
  `Separator`, so the Gallery tray menu (Show / theme radios / Exit) is visible.

### Changed

- All six NuGet packages now ship the repository logo as their package icon: the root
  `logo.png` is downscaled to a 128×128 `assets/logo-128.png` and packed via `PackageIcon`
  in every package.
- `Md3.Avalonia.Gallery.Android` removed from `Md3.Avalonia.sln`, restoring the documented
  design: developers without the Android workload can build the solution; CI builds the Android
  csproj directly.
- Documentation corrections: package counts updated to six (`Md3.Avalonia.RichEditor` was
  missing), `samples/Md3.Avalonia.Gallery.Android` paths corrected to `gallery/`, the stale
  `Md3.Avalonia.Themes`/`Color`/`Motion`/`tools` solution sketch in the design specification
  replaced with the real structure, the Material Symbols DoD item closed (the official TTF is
  committed and verified), and `docs/API.md` trimming section updated for `IsAotCompatible`.

## [0.4.1-preview.1] - 2026-10-06

### Added

- **`Md3.Avalonia.RichEditor`**, an opt-in Material Design 3 theme for the
  [AvaloniaRichEditor](https://github.com/centwon/AvaloniaRichEditor) control. It is the only
  package that pulls in that dependency, bringing the set to six packages.
- **Native AOT.** All six packages declare `IsAotCompatible` and build with the IL3xxx analyzers
  on alongside the existing IL2xxx trimming ones. `.github/workflows/aot-probe.yml` publishes the
  desktop gallery with `PublishAot=true` and does not accept a green step as the answer: it
  asserts the output is a native ELF executable with no managed assembly beside it, then runs that
  binary under xvfb. Native AOT fails at runtime on reflection that was trimmed away, not at
  compile time, so the smoke run is the part that carries the claim.
- `MdBreadcrumb` gains a `Powerline` variant.
- [`docs/DIALOG_SERVICE.md`](docs/DIALOG_SERVICE.md): how to show dialogs from a view model
  through `IMdDialogService`, including the composition-root wiring.

### Fixed

- **The carousel ignored how wide it was.** The arrangement produced one large item and one medium
  one at every window size, so from the third item on everything collapsed to the 56 DIP small
  keyline: four items covering 584 DIP of an 1188 DIP track, with the rest empty. Correct on a
  phone, which is the only width the rule was written for. `MdCarousel` now solves for how many
  large items the viewport has room for, and tapers **forward** from the focal item rather than
  symmetrically - a symmetric window hands out `2 * largeCount - 1` large items and overflows the
  arrangement it just solved. At a count of one the result is identical to before, so narrow
  layouts are untouched.
- **Borderless windows had square corners.** The theme makes the window itself transparent so the
  compositor can cut the rounded corners out of the client area, and leaves `PART_WindowFrame` to
  draw the rounded surface - but that border bound its `Background` straight back to the window's,
  which is the transparent one, so it painted nothing. Where transparency was refused the opaque
  square `TransparencyBackgroundFallback` filled the whole window instead, which is the square
  corner that was reported. The frame now paints `TransparencyBackgroundFallback`, which is this
  window's opaque colour and stays overridable.
- **`MdBeforeAfter` drifted away from the pointer** when the divider was grabbed at the very start
  or end. The handle straddles the divider, so at those positions half of it is clipped outside the
  control and every pixel the pointer can reach lies on the same side, making the grab offset
  one-directional and as large as half a handle. Dragging is delta-based on purpose, so that offset
  survived the whole gesture. The preserved offset is now clamped to the divider's distance from
  the nearer edge: mid-track grabs behave exactly as before, and the ends collapse to a snap.
- **Wheel and key input never reached a picker popup on desktop.** The popup is a separate
  `PopupRoot` there, so events raised inside it end at that root instead of bubbling to the
  control; Android renders popups into the `TopLevel` overlay, which is why scroll-to-adjust
  worked there and was dead on Windows. The popup surface now forwards into the same handlers, and
  `MdTimePicker` picks the hour or minute column from whichever the pointer is over.
- Desktop popups are dismissed when the page behind them scrolls, rather than staying put while
  their anchor moves away.
- The cascader dropdown aligns with its anchor's left edge instead of centring on it.

### Gallery

- Usage sections no longer show placeholder code. 32 pages carried XAML only and 7 carried C#
  only, and the absent tab rendered as an empty box; the missing tab is now hidden.
- 29 of the 34 pages that overflowed a 480 DIP window now reflow, by replacing fixed `Width` with
  `MaxWidth` in about 50 places. The five that still do not are pinned in the test with a
  two-way assertion, so they cannot be quietly forgotten or quietly fixed.
- 366 Chinese strings added to the localisation table.

### Testing

- Rendered pixels are now actually read. `UseHeadlessDrawing=false` means these tests run real
  Skia, but every one of the thirty-odd `CaptureRenderedFrame()` calls asserted only that the
  frame was not null. Corner rounding had been written off as untestable headless on that basis;
  it is not, and that is how the borderless window defect above was found.

## [0.4.0-preview.1] - 2026-10-05

### Added

- **Android system back.** `MdBackNavigation` routes `TopLevel.BackRequested` to the top-most
  open Material surface, newest first, and marks the event handled so the activity is not popped
  behind it. `MdDialogHost`, `MdSheetHost`, `MdNavigationDrawer`, `MdSearchView`, `MdMenuAnchor`,
  `MdFabMenu`, `MdDatePicker` and `MdTimePicker` opt in automatically; `MdBackScope` lets any
  other surface join.
- **Safe area insets.** `MdSafeArea` insets content past the status bar, a display cutout, the
  navigation bar and the gesture handle, with per-edge control (`MdSafeAreaEdges`), a
  `MinimumPadding` floor and a settable `SafeAreaPadding` for previewing a layout without a
  device.
- `MdRevealHost` and `MdMorphPanel`, the layout primitives behind the rebuilt motion controls.
- `MdTabsView`, the content area for an `MdTabs` bar. Children are the pages, matched to the
  bar's tabs by position, with selection kept in step in both directions.
- **`Md3.Avalonia.DataGrid`**, a new opt-in package with a Material Design 3 theme for
  Avalonia's own `DataGrid`. It is the only package that depends on `Avalonia.Controls.DataGrid`.
  Add `MaterialDataGridTheme` after `MaterialTheme`; it replaces the control's Fluent theme and,
  like the rest of the library, does not require Avalonia's `FluentTheme`.
- **Trimming support.** All five packages declare `IsTrimmable` and build with the IL2xxx
  analyzer enabled. `MdThemeJson` moved to a source-generated serializer context, and every
  control that takes a string property path gained a reflection-free delegate:
  `MdDataGridColumn.ValueSelector` / `.ValueParser` / `.ValueSetter`,
  `MdAsyncSelect.DisplaySelector`, `MdSearchView.ResultDisplaySelector`.

### Changed

- **Gallery: one page per component.** Five pages that each held a scroll of unrelated controls —
  `FlutterParityGalleryPage`, `EcosystemGalleryPage`, `DesktopAdaptersGalleryPage` and
  `AdvancedSelectionGalleryPage` — were split into 52 pages, one per component, each with its own
  nav entry, search keywords and usage snippet checked against the real API. The gallery went from
  48 to 97 pages. `MotionGalleryPage` and the components overview were kept whole: one is a topic
  page, the other is the category index.
- **Gallery: the components overview is generated from the gallery index.** It had accumulated
  links to two deleted pages and covered only a third of the gallery. `MdGalleryIndexTests` now
  fails the build if a link stops resolving or a page is missing from the index.


- The four motion controls (`MdSharedAxis`, `MdFadeThrough`, `MdContainerTransform`,
  `MdAnimatedVisibility`) now animate rather than toggling `IsVisible`, and are no longer marked
  experimental.
- Gallery fixes: breadcrumb overflow anchoring and clickable segments, carousel item measurement,
  segmented-button icon reservation, settings-expander header padding.

### Fixed
- `MdBeforeAfter`: `DividerBrush` is honoured instead of being overridden by a literal in the
  template, `PositionChanged` is raised for keyboard and binding changes rather than only for
  pointer drags, and `IsInteractive="False"` now makes the control a non-tab-stop that ignores the
  arrow keys as well as the pointer. The gallery page compares two photographs instead of the
  words "BEFORE" and "AFTER" (gallery defect 22).
- Simple dialog options run edge to edge with the M3 spacing (title 24/24/24/0, content
  0/12/0/16, option 24/8). The surface's own padding used to inset the list, so a row's hover and
  selection layers stopped short of the dialog edge. `MdFlutterListItemTheme` now takes its corner
  radius from a setter rather than a literal in the template, so it can be squared off; defaults
  are unchanged.
- Five more surfaces answer the Android back gesture: `MdSimpleDialog`, `MdCommandPalette`,
  `MdPopover`, `MdCascader` and `MdAsyncSelect`. Each handled Escape but never registered with
  `MdBackNavigation`, so on Android back popped the activity out from under an open surface.
  `MdSimpleDialog` reports exactly one dismissal for a back request, like its cancel button.
- The picker gallery pages print their bound values. `MdDatePicker` in `Docked` mode commits as you
  pick while modal date pickers and both time-picker modes stay provisional until confirmed; with
  no value shown anywhere, the two outcomes were indistinguishable and read as "the picker doesn't
  apply the selection" (gallery defect 9). Behaviour is unchanged and now pinned in both
  directions.
- `MdModalFocusController` no longer re-arms its focus redirect from inside the redirect itself. The
  pending-redirect guard is now cleared after the focus attempt rather than before it, and three
  consecutive attempts that cannot land focus inside the modal scope stop the redirect — a scope
  that is unfocusable (typically one opened before it was attached) used to keep the dispatcher
  queue refilling itself, so `Dispatcher.RunJobs()` never returned. Containment, isolation and
  Escape are unaffected, and the counter resets once focus reaches the scope.
- `MdPopover` deregisters from the open-popover coordinator when it closes. A closed popover used
  to stay on record as the open one for as long as it was alive, so the next popover to open
  anywhere in the process reached back into it and cut its exit animation short — and threw a
  cross-thread `InvalidOperationException` when that stale popover belonged to another UI thread.
- `MdFormField` supporting and error text: the supporting line no longer reserves a row when
  empty, both lines indent to the field's inner edge, the error replaces the supporting text
  instead of stacking under it, and `MdDropdownFormField` no longer draws a second supporting
  line on top of the one `MdComboBox` already renders.
- `MdAboutDialog` now fills `ApplicationName`, `ApplicationVersion` and `Legalese` from the entry
  assembly when they are not set, and collapses the icon, version and copyright rows when empty.

- CI jobs are bounded by a timeout; a deadlocked test previously held a runner for six hours.

For details, see [`docs/RELEASE_NOTES_0.4.0-preview.1.md`](docs/RELEASE_NOTES_0.4.0-preview.1.md).

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
