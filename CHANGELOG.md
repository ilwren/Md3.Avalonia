# Changelog

## [0.4.1-preview.1] - 2026-10-06

### Added

- **`Md3.Avalonia.RichEditor`**, an opt-in Material Design 3 theme for the
  [AvaloniaRichEditor](https://github.com/centwon/AvaloniaRichEditor) control. It is the only
  package that pulls in that dependency, bringing the set to six packages.
- **`Md3.Avalonia.AvaloniaEdit`**, an opt-in Material Design 3 chrome theme for the
  [AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit) text editor: `TextEditor`, `TextArea`,
  the search and replace panel, the completion list and the completion tooltip, plus keyed
  `Md.Comp.AvaloniaEdit.*` themes for the inner controls the editor instantiates. It is the only
  package that pulls in that dependency, bringing the set to seven. The Gallery's `CodeExample`
  now uses it instead of scoping Fluent resources to the preview, so no project in the repository
  references `Avalonia.Themes.Fluent` any more.
- **`Md3.Avalonia.AvaloniaHex`**, an opt-in Material Design 3 theme for the
  [AvaloniaHex](https://github.com/Washi1337/AvaloniaHex) hex editor: `HexForegroundBase`,
  `OffsetForeground`, the header, current-line, caret and selection brushes and pens are filled
  from `Md.Sys` tokens, and the editor gets a Material surface, outline, corner and
  `MdScrollViewer` scrollbars. It is the only package that pulls in that dependency, bringing
  the set to eight.
- **`Md3.Avalonia.Dock`**, an opt-in Material Design 3 theme for the
  [Dock](https://github.com/wieslawsoltes/Dock) docking layout system. It retokenizes Dock's
  accent, surface, tab and chrome brushes onto `Md.Sys.Color` tokens and includes Dock's own
  control dictionaries by `avares://` URI instead of forking them, so Dock keeps owning its
  templates and part names. It is the only package that pulls in `Dock.Avalonia` and
  `Dock.Avalonia.Themes.Fluent`, bringing the set to nine. Because Dock's keyed chrome themes
  (`ButtonCreateDocument`, `ChromeButton`, `DockDrawnCaptionButton`) are
  `BasedOn="{StaticResource {x:Type Button}}"` and neither `MaterialTheme` nor an absent
  `FluentTheme` supplies that key, it also ships the one implicit native theme it cannot avoid -
  a Material text button in `Themes/Controls/MaterialButton.axaml` - the one native theme the
  package adds, and by design: without it Dock's chrome cannot load when no `FluentTheme` is
  present. Every other native control is still left to the host application.
- **`Md3.Avalonia.Tabalonia`**, an opt-in Material Design 3 theme for
  [Tabalonia](https://github.com/egorozh/Tabalonia) draggable tabs. It retokenizes the 19 brushes of
  Tabalonia's `Themes/Custom` onto `Md.Sys.Color` tokens - strip on `Surface`, idle tab ink
  `OnSurfaceVariant`, hover an `OnSurface` state layer, selected tab a `Primary` container with
  `OnPrimary` ink - and supplies the six `TabItemHeader*` keys Tabalonia's Custom templates actually
  read from Avalonia's `FluentTheme`, so the theme is standalone. Like the Dock theme it includes the
  pinned package's dictionaries by `avares://` URI instead of forking templates. It is the only
  package that pulls in `Tabalonia`, bringing the set to ten.
- **`Md3.Avalonia.Markdown`**, an opt-in Material Design 3 style collection for
  [Markdown.Avalonia](https://github.com/whistyun/Markdown.Avalonia) documents. It is shaped unlike
  the other theme packages, because upstream is: `MarkdownScrollViewer` chooses a style collection
  itself and inserts it into its own `Styles`, the five built-in collections are selected by
  reflection over `MarkdownStyle`'s static properties, and the `INamedStyle` marker they implement
  is internal - so neither `MarkdownStyleName` nor an application-level `StyleInclude` can restyle a
  viewer. `MaterialMarkdownTheme : Styles` is therefore assigned to the viewer's `MarkdownStyle`
  property (or `<md:MarkdownScrollViewer.MarkdownStyle>` in XAML) instead of being merged
  application-wide, one instance per viewer like the built-in collections. It mirrors the selector
  set of the package's `MarkdownStyleFluentTheme` retokenized onto `Md.Sys.*`: prose
  `OnSurfaceVariant` on `Surface`, headings and code `OnSurface`, links `Primary` (`Tertiary` on
  hover, where upstream asks for the complementary accent), the blockquote bar `Primary`, table
  rules and dividers `OutlineVariant`, code blocks `SurfaceContainerLow`, notes and table headers
  `SurfaceContainerHighest`, and the red marker upstream paints on an unhandled `:::` container the
  `Error` role. Sizes come from `Md.Sys.TypeScale` - `HeadlineLarge` for h1 down to `TitleSmall`
  for h6, `BodyLarge` for prose, `BodyMedium` for code and table cells, `LabelLarge` for table
  headers - so h5 and h6, which no built-in collection styles at all, are filled in. Headings follow
  the weights M3 gives those roles instead of Fluent's Light (Regular down to `TitleLarge`, Medium
  for `TitleMedium`/`TitleSmall` and table headers), the document is typeset in
  `Md.Sys.Typeface.Plain` rather than inheriting the host's font, and selection is
  `Md.Sys.Color.PrimarySelection.Brush` - the same wash the editor, hex viewer and text field themes
  use - so it never falls back to upstream's cyan; `mdc|Rule` gets the `OutlineVariant` foreground it
  otherwise inherits from the window. Layout follows upstream except where Material has a shape of
  its own: the code block is a `SurfaceContainerLow` card with an 8 DIP radius instead of Fluent's
  two hairlines, table cells get 10/6 DIP padding instead of 2, and the `Error` marker on an
  unhandled `:::` container is a 3 DIP frame. It is the only package that pulls in
  `Markdown.Avalonia.Tight` (whose assembly is `Markdown.Avalonia`), bringing the set to eleven.
- **`Md3.Avalonia.LiveCharts2`**, an opt-in Material Design 3 theme for
  [LiveCharts2](https://github.com/Live-Charts/LiveCharts2) charts. It is the only package that
  pulls in `LiveChartsCore.SkiaSharpView.Avalonia` (pinned at `2.1.0-dev-798`, the Avalonia 12 line;
  the stable `2.0.x` line is the Avalonia 11 build and does not load against Avalonia 12), bringing
  the set to twelve. It is also the only Material theme in the repository that is *not* an Avalonia
  style collection: LiveCharts has no per-control style seam, so the package's whole public surface
  is a configuration entry point - `LiveCharts.Configure(settings => settings.AddMaterialTheme())`,
  once, before the first chart view exists - and it contains no `Styles` or `ControlTheme` at all.
  The plane and series rules are appended to LiveCharts' own default theme, so its tooltip and
  legend factories, its animations and every geometry it already decides ship unchanged; what
  changes is the Material part. (The label, needle and angular-tick rules take over the one slot
  LiveCharts keeps for each visual-element type instead - it throws when a type is registered
  twice, which its own default theme does for those three.) Series take an eight-role palette (`Primary`, `Tertiary`, `Secondary`, `Error`,
  `PrimaryContainer`, `TertiaryContainer`, `InversePrimary`, `Outline` - the first four give a
  four-hue palette the way upstream's own does, the rest are further tones of the same hues). Axes
  use `LabelMedium`/`LabelLarge` in `OnSurfaceVariant` with an `OutlineVariant` grid at 12 DIP
  padding; a line is a 3 DIP stroke with upstream's area fill cleared and 8 DIP surface-filled
  markers ringed in the series colour; bars take a 4 DIP rounded end and 6 DIP between them while
  stacked bars stay square; pie slices are separated by the surface showing through rather than a
  border; a gauge is a `SurfaceContainerHighest` track with a `Primary` indicator and a needle in
  `OnSurfaceVariant`; candlesticks use
  `Tertiary` and `Error` instead of upstream's hardcoded green and red; tooltips and legends are
  inverse-surface cards with a shadow at `LabelLarge`, and the canvas clears to `Surface`. Every
  colour is read from `Md.Sys.Color.*` for the variant each chart draws in, so light/dark follow the
  application, and an application that does not register `MaterialTheme` gets the M3 baseline values
  the package carries. `MdChart` in the core package stays provider neutral.
- **`Md3.Avalonia.ScottPlot`**, an opt-in Material Design 3 theme for
  [ScottPlot](https://github.com/ScottPlot/ScottPlot) charts. It is the only package that pulls in
  `ScottPlot.Avalonia` (pinned at `5.1.59`, whose dependencies declare `Avalonia >= 12.0.0` and
  `ScottPlot >= 5.1.59`), bringing the set to thirteen. Like the LiveCharts2 package it is not an
  Avalonia style collection: a ScottPlot `Plot` carries its own colours and its own type, so the
  package's whole public surface is a plot-level entry point - `MaterialScottPlotTheme.Follow(avaPlot)`
  applies the rules for the variant the control actually has and re-applies them from the control's
  `ActualThemeVariantChanged` event, because ScottPlot bakes colours into a plot when they are set,
  and `Apply(plot, dark)` / the instance `Apply` overloads style a plot directly. The figure and the
  data background are both `Surface`; `Plot.Axes.Color` paints the frame, ticks, tick labels, axis
  names and title in `OnSurfaceVariant` in one call; major grid lines are `OutlineVariant` while minor
  lines are off; the legend is a `SurfaceContainerHigh` panel with an `OutlineVariant` outline and
  `OnSurfaceVariant` text; the type scale is `TitleMedium` for the title, `LabelLarge` for axis names
  and legend entries and `LabelMedium` for tick labels, all in the family `Md.Sys.Typeface.Plain`
  names; and series take the same eight-role palette the LiveCharts2 theme walks. ScottPlot's frames,
  tick generation, layout and interaction ship unchanged, a plot the theme was not handed keeps every
  default, and nothing process-wide is rewritten - the theme never assigns `Fonts.Default`, so it
  cannot change the type of plots it was not asked to style. `MdChart` in the core package stays
  provider neutral.
- **`Md3.Avalonia.PropertyGrid`**, an opt-in Material Design 3 theme for
  [bodong.Avalonia.PropertyGrid](https://github.com/bodong1987/Avalonia.PropertyGrid), the first of
  Wave 3. It is the only package that pulls in that dependency (pinned at `12.1.1.1`, which declares
  `Avalonia >= 12.1.1`), bringing the set to fourteen. The grid sets no colours of its own - its 604
  lines of embedded styles only lay out templates and host plain Avalonia controls - so in an
  application that ships no native control theme it draws its chrome and none of its cells.
  `MaterialPropertyGridTheme` supplies what is missing as 32 keyed `Md.Comp.PropertyGrid.*` control
  themes: the pass-through containers every cell is wrapped in, Material filled fields, buttons,
  filter chips, checkboxes, radio buttons, drop-downs and their items, the numeric field and its
  spinners, sliders and their thumbs, progress bars, the date and time fields with their wheel
  presenters, category expanders, the name/value splitter, the scroller, the context menu with its
  items and separators, and tooltips. Each one is handed to a control by a style whose selector
  requires a `PropertyGrid` ancestor, which is what keeps the package opt-in: registering it never
  replaces a native control theme for the rest of the application, and the grid needs no
  `FluentTheme`. Upstream keeps what only upstream can paint - the 0.5 DIP `Gray` hairlines its own
  templates and its expandable-object factory set as local values, and Avalonia's `ColorPicker`,
  whose style sheets the grid already loads for its own subtree - and the date and time presenters
  keep the control's wheel mechanics rather than becoming a Material calendar.
  `MdPropertyGridThemeTests` pins the cell and chrome themes, that every control theme the package
  ships is keyed rather than implicit, that a control outside a grid stays unthemed, the date
  flyout's parts and its confirm path, the options menu, and that the grid still edits, filters and
  writes values back.
- **`Md3.Avalonia.NodifyM`**, an opt-in Material Design 3 theme for
  [NodifyM.Avalonia](https://github.com/Maklith/NodifyM.Avalonia), the node editor that follows the
  property grid in Wave 3. It is the only package that pulls in that dependency (pinned at
  `12.0.0`, which declares `Avalonia >= 12.0.2`), bringing the set to fifteen. Upstream ships a
  single style file that carries both the brushes its templates read and the eleven sheets that
  style its controls, and `MaterialNodifyMTheme` takes those two halves apart: the eleven sheets are
  style-included exactly as upstream lists them, while this theme's resources merge upstream's own
  data templates and brush dictionary followed by an accent of ours that redeclares 43 of the 44
  brushes against `Md.Sys.Color.*`. Dictionaries are searched last-merged first, so the accent
  answers in upstream's place while the templates keep reading the keys they were written against -
  and because both variants resolve through `Md.Sys.Color.*`, the dictionary carries no
  `ThemeDictionaries` of its own. The canvas becomes a surface ruled in `OutlineVariant`; a node is
  a `SurfaceContainerHigh` card with a `SecondaryContainer` header and a `SurfaceContainer` footer;
  input pins are `SecondaryContainer` and output pins `PrimaryContainer`, so a wire's two ends are
  distinguishable without labels; wires take `OnSurfaceVariant`, with the circuit variant and the
  pending drag in `Primary`; and the alignment guides and the selection frame take the same
  `Primary`. Additive styles cover the values no accent dictionary can reach, because
  upstream sets them as literals: a node's `White` ink, the `LightBlue` and `Orange` borders it
  paints on hover and when selected, the `Azure` behind its items, a node group's four inline
  colours and the `Gainsboro` the grid rule's property registers with. Upstream keeps the node's
  3 DIP corner radius, which is baked into its template; the marquee's stroke and fill, which are
  written on the rectangle itself rather than in a style - a value set that way outranks every style
  setter, so the selection-rectangle brushes answer the minimap's viewport and nothing else; and the
  label a connection can carry, whose geometry upstream builds in C# from a hardcoded `Inter`
  typeface, so the gallery's wire template binds no `Text`.
  `MdNodifyMThemeTests` reads the brushes back off realized controls - the canvas, the node and both
  pin directions, the pending wire, the grid rule and all six minimap brushes - then pins the
  selection frame, the variant switch, and that a plain control in the same window keeps no theme.
- **`Md3.Avalonia.Mapsui`.** A Material 3 theme for maps drawn with
  [Mapsui](https://github.com/Mapsui/Mapsui), the last item in Wave 3. It is the only package that
  pulls in that dependency (pinned at `Mapsui.Avalonia12` `5.1.0`, the Avalonia 12 build, which
  declares `Avalonia >= 12.0.2`, `Avalonia.Skia >= 12.0.2` and `SkiaSharp >= 3.119.4`), bringing the
  set to sixteen. Mapsui has no style seam at all - `MapControl` draws its own surface from the
  colours a `Map` and its layers carry - so, like the two chart providers, this package is an entry
  point rather than a `Styles` collection. `MaterialMapsuiTheme` writes the canvas (`Surface`, where
  Mapsui ships white), the vector style an unstyled layer is given - an `Outline` hairline for a
  boundary, a heavier `OnSurfaceVariant` stroke for a line, `Primary` at twelve percent for a region,
  mixed into the colour because Mapsui's brush takes a colour rather than an opacity - and a label
  chip in `SurfaceContainer` with an `OutlineVariant` border, the `ExtraSmall` corner radius and
  `OnSurface` ink at `LabelMedium`. A layer whose style was chosen keeps it - and that is not the same as a null
  style, because `BaseLayer`'s constructor gives every layer a grey outline, a black line and a white
  fill, so a layer counts as unstyled while it still carries that default. Only the colours are
  replaced: a layer's own opacity and visible range travel across. `Follow(MapControl)` keeps a map in
  step with the light/dark switch, and `SeriesPalette(dark)` hands a thematic map the same eight
  Material roles both chart providers walk. Projection, tile fetching, navigation, hit testing and
  animation stay upstream's. `MdMapsuiThemeTests` pins the palette, the canvas, both styles, the
  type scale, the corner token and the dark-variant reads.
- **Native AOT.** All sixteen packages declare `IsAotCompatible` and build with the IL3xxx analyzers
  on alongside the existing IL2xxx trimming ones. `.github/workflows/aot-probe.yml` publishes the
  desktop gallery with `PublishAot=true` and does not accept a green step as the answer: it
  asserts the output is a native ELF executable with no managed assembly beside it, then runs that
  binary under xvfb. Native AOT fails at runtime on reflection that was trimmed away, not at
  compile time, so the smoke run is the part that carries the claim.
- `MdBreadcrumb` gains a `Powerline` variant.
- [`docs/DIALOG_SERVICE.md`](docs/DIALOG_SERVICE.md): how to show dialogs from a view model
  through `IMdDialogService`, including the composition-root wiring.

### Fixed

- **Markdown code could fail to shape, intermittently, on a host that advertises a font it cannot
  build.** The gallery's markdown page threw `Could not create glyphTypeface. Font family: Consolas
  (key: fonts:SystemFonts)` in CI on some runs and not others. `ColorTextBlock.Avalonia`'s `CCode`
  defaults its `MonospaceFontFamily` to whichever family `FontFamilyCollector.TryGetMonospace()`
  picked - the first system family whose name contains one of eight substrings, in enumeration order
  rather than preference order, so the pick differs between machines and between processes - and a
  family from that collection carries the `fonts:SystemFonts` key, for which `FontManager` has no
  fallback: a single family the host cannot shape throws at measure time. A family list is a
  composite key and does fall back, so `MaterialMarkdownTheme` now sets the property to one, cased as
  the families are named because the candidate match is ordinal. It has to be set on
  `MonospaceFontFamily` and not on `FontFamily`: `CCode` binds the latter to the former in its
  constructor, and a binding outranks a style setter, which is also why upstream's own stack never
  reached a span.
- **Two of the theme projects were missing from the solution's build configuration.** The Dock
  entry lost its `EndProject` line when Tabalonia was added after it, which swallowed Tabalonia's
  project block, and neither theme project had a `ProjectConfigurationPlatforms` entry - so
  `dotnet build Md3.Avalonia.sln` silently skipped them and only the packaging script's per-project
  builds kept them honest. Both are mapped now, with `Md3.Avalonia.Markdown` alongside them.
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
