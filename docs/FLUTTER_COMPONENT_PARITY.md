# Flutter ecosystem parity and desktop development plan

Reference baseline (checked 2026-09-23):

- Flutter Material component catalog: https://docs.flutter.dev/ui/widgets/material
- Flutter Material 2 catalog for still-supported widgets: https://docs.flutter.dev/ui/widgets/material2
- Flutter Material library API: https://api.flutter.dev/flutter/material/
- Flutter package registry: https://pub.dev/

The goal is behavioral/API parity where that behavior fits Avalonia, not a Dart API clone. Every control remains an independent `Md*` type, uses scoped themes, supports direct manipulation and binding/commands, and avoids global native-control replacement.

## Package architecture

| Package | Responsibility | Dependency rule |
|---|---|---|
| `Md3.Avalonia` | Official Flutter Material-aligned controls, Material tokens, motion, localization, and platform-neutral infrastructure | Does not reference a concrete icon/font provider |
| `Md3.Avalonia.Icons` | Optional Material Symbols catalog, checked-in complete official variable TTF, codepoints, license, and `Md.Icon.*` injection | Standalone optional provider; no manual font download |
| `Md3.Avalonia.Extra` | Clean-room controls based on audited third-party Flutter interaction documentation (also carries the older `Md3.Avalonia.Ecosystem` namespaces as aliases) | References `Md3.Avalonia`; does not reference Icons |

Core icon slots resolve `Md.Sys.Typeface.Symbols.Rounded` and `Md.Icon.*`. Core supplies visually empty fallbacks; when referenced, the optional Icons package is discovered automatically on first symbol use and injects its verified Material Symbols resources. Applications can still replace every token with their own icon font or objects.

The third-party packages below are **research baselines, not runtime dependencies**. Their interaction models and public documentation may inform clean-room Avalonia implementations, but the library keeps Material 3 visuals, Avalonia-native input/automation, and the existing token system. No Dart source, package assets, fonts, trademarks, or screenshots are copied into the product without a separate license review and required notices.

## Implemented before the parity program

- Actions: buttons, toggle/icon/split/connected buttons, FAB/extended FAB/FAB menu.
- Containment: card, dialog/dialog host, divider, lists, bottom/side sheets.
- Navigation: app bars, navigation bar, navigation drawer, navigation rail, tabs, adaptive suite.
- Selection/input: checkbox, radio, switch, chips, segmented buttons, ComboBox, autocomplete, numeric input, text fields, slider/range slider, date/date-range/time pickers.
- Communication/status: badge, menus, progress, expressive loading, snackbar, tooltip, search.
- M3 extensions: carousel, toolbar, scaffold, material scrolling surface, dynamic theme lab.

## Phase 1 — implemented

| Flutter reference | Avalonia control | Key behavior |
|---|---|---|
| `MaterialBanner` | `MdBanner` | Leading/content/actions slots, inline/below actions, two-way open state, dismiss command/event |
| `ExpansionPanelList` / `ExpansionPanel` | `MdExpansionPanelList`, `MdExpansionPanel` | Single or multiple expansion, tappable header, two-way state, animated indicator |
| `DataTable` | `MdDataTable`, `MdDataTableRow` | Header slot, `ItemsSource`, item template, multi-selection, keyboard behavior, striping/dividers, sort request API |
| `Stepper` / `Step` | `MdStepper`, `MdStep` | Linear/non-linear navigation, step states, active-step binding, continue/cancel commands and events |
| `RefreshIndicator` | `MdRefreshIndicator` | Pull resistance, armed/refreshing states, command/event, direct begin/complete API |

Gallery page: **Flutter parity · phase 1**.

## Popular open-source Flutter libraries added to the research backlog

“Popular” is a discovery signal rather than a permanent ranking. The snapshot uses visible pub.dev activity, likes/downloads, current releases, cross-platform reach, and the distinct interaction patterns a package can contribute. Counts change continuously; package URLs and licenses are the authoritative records. A package being listed does not mean every widget will be cloned.

### Full UI systems and platform design systems

| Flutter package | Open-source signal at the 2026-09-23 review | What enters the plan | Disposition |
|---|---|---|---|
| [GetWidget](https://pub.dev/packages/getwidget) | MIT; about 2.5k pub likes; broad pre-composed UI kit | Avatar/group, rating, accordion composition, sticky content, loading and empty-state patterns | High-priority behavior audit; retain Material 3 appearance |
| [shadcn_ui](https://pub.dev/packages/shadcn_ui) | MIT; about 950 likes; actively released, six-platform package | Command palette, popover, hover card, context menu, breadcrumb, code snippet and composable form patterns | High-priority desktop interaction audit |
| [shadcn_flutter](https://pub.dev/packages/shadcn_flutter) | BSD-3-Clause; about 470 likes; standalone component ecosystem | Overlay coordination, command/context menus, drawer/sheet APIs, density and desktop keyboard behavior | High-priority API/interaction audit |
| [Forui](https://pub.dev/packages/forui) | MIT/OFL; about 430 likes; active desktop/touch design system | Touch/desktop density, composable controls, form feedback and responsive component patterns | High-priority behavior audit; do not import its font assets |
| [fluent_ui](https://pub.dev/packages/fluent_ui) | BSD-3-Clause; about 3.2k likes; Windows-oriented, cross-platform | Desktop navigation, command surfaces, focus/keyboard details and window chrome behavior | Desktop reference only; no Fluent skin in the Material core |
| [macos_ui](https://pub.dev/packages/macos_ui) | MIT; about 1k likes; macOS-focused | Unified title bar, toolbar/window safe areas, sidebar sizing and platform-aware window behavior | macOS/windowing reference only |
| [Yaru](https://pub.dev/packages/yaru) | MPL-2.0; Ubuntu-maintained desktop widget suite | Linux client-side decorations, window controls, split layouts and desktop platform adaptation | Behavior/documentation reference; MPL source is not copied |
| [TDesign Flutter](https://pub.dev/packages/tdesign_flutter) | MIT; Tencent package and repository, mobile/web component suite | Enterprise form composition, indexes, cascaded selection, result/empty states and mobile density | Secondary audit, especially for Chinese/localized workflows |
| [Bruno](https://pub.dev/packages/bruno) | MIT; enterprise mobile component suite; last stable activity is older | Form groups, filters, search/result and mobile business-flow patterns | Watchlist/reference only until maintenance resumes |
| [ant_design_flutter](https://pub.dev/packages/ant_design_flutter) | MIT; desktop/web-oriented but stable pub release is old and a rewrite is in progress | Cascader, transfer, descriptions, enterprise table/form patterns | Monitor-only; no implementation gate until an active stable release exists |

### Specialized component packages

| Area | Flutter references | Planned Avalonia outcome |
|---|---|---|
| Responsive composition | [responsive_framework](https://pub.dev/packages/responsive_framework) | Extend `MdAdaptiveLayout` with named breakpoints, orientation/input-mode conditions and testable layout decisions |
| Swipe actions | [flutter_slidable](https://pub.dev/packages/flutter_slidable) | `MdSlidableItem` with start/end action panes, thresholds, dismiss confirmation, commands, keyboard alternatives and RTL |
| Masonry/quilted layouts | [flutter_staggered_grid_view](https://pub.dev/packages/flutter_staggered_grid_view) | Virtualization-aware `MdMasonryPanel` and quilted/woven layout strategies, without infinite-height measurement traps |
| Incremental paging | [infinite_scroll_pagination](https://pub.dev/packages/infinite_scroll_pagination) | `MdPagedItemsView` state model with loading/empty/error/retry/refresh states and provider-agnostic page requests |
| Data tables | [data_table_2](https://pub.dev/packages/data_table_2), [PlutoGrid](https://pub.dev/packages/pluto_grid) | Enhance `MdDataTable`; later add `MdDataGrid` for sticky headers, resize/reorder, virtualization, filtering, editing and keyboard navigation |
| Searchable selection | [dropdown_search](https://pub.dev/packages/dropdown_search), [flutter_typeahead](https://pub.dev/packages/flutter_typeahead) | Enhance `MdComboBox`/`MdAutoCompleteBox` with async cancellation, debounce, paging, multi-select and menu/dialog/sheet presentation |
| Calendar | [table_calendar](https://pub.dev/packages/table_calendar) | `MdCalendar` with event markers, multiple/range selection, locale-first week rules and month/week/two-week formats |
| Carousel | [carousel_slider](https://pub.dev/packages/carousel_slider) | Enhance `MdCarousel` with infinite mode, autoplay policies, center enlargement, pause-on-interaction and controller commands |
| Refresh/load-more | [easy_refresh](https://pub.dev/packages/easy_refresh) | Extend `MdRefreshIndicator` with load-more state, configurable trigger positions and nested-scroll coordination |
| Motion composition | [flutter_animate](https://pub.dev/packages/flutter_animate) | Declarative `MdAnimationSequence`/effect descriptors using the existing Material motion tokens and reduced-motion policy |
| Skeleton loading | [skeletonizer](https://pub.dev/packages/skeletonizer) | `MdSkeleton` and `MdSkeletonGroup`, content-shape placeholders, shimmer/pulse modes and reduced-motion fallback |
| Charts | [fl_chart](https://pub.dev/packages/fl_chart) | Provider-agnostic `MdChart` family: line, bar, pie/donut, scatter and radar, with Material tokens and accessible data summaries |
| Rich text | [flutter_quill](https://pub.dev/packages/flutter_quill) | `MdRichTextEditor` UI, document/selection contracts, toolbar and adapter interfaces; persistence/import formats remain pluggable |
| Chat | [flutter_chat_ui](https://pub.dev/packages/flutter_chat_ui) | Backend-agnostic `MdChatView`, message grouping, composer, attachments/retry events and incremental history loading |
| Rating | [flutter_rating_bar](https://pub.dev/packages/flutter_rating_bar) | `MdRating` with fractional values, indicator/read-only mode, keyboard/pointer input and screen-reader value text |
| Timeline | [timeline_tile](https://pub.dev/packages/timeline_tile) | `MdTimeline`/`MdTimelineItem` with start/end connectors, alternating layout, status semantics and virtualization guidance |

### License and clean-room gate

Before any third-party-inspired item moves from research to implementation:

1. Freeze the reviewed package version, source URL, public documentation URL and license in the spec snapshot.
2. Write an independent behavior matrix and Material 3 visual mapping before looking at implementation details.
3. Prefer MIT/BSD/Apache documentation and public demos as behavioral references. MPL material remains documentation-only unless file-level obligations are explicitly reviewed.
4. Do not copy Dart source, generated assets, icon sets, fonts, screenshots, package names or visual trade dress into the Avalonia assembly.
5. Record any required attribution in `THIRD-PARTY-NOTICES.md`; a package with unclear, changed or “pending” license metadata blocks implementation until resolved.
6. Re-check project activity and platform support at the start of each wave. Stale/watchlist projects can inform requirements but cannot be the sole acceptance baseline.

## Official Flutter Phases 2–4 — implemented

Gallery page: **Flutter Material parity · phases 1–4**. The controls use Avalonia-native binding, commands/events/direct methods and Android-safe public APIs; navigation services, codecs, native platform switches, and persistence providers remain host adapters.

### Phase 2 — data and collection utilities

- `MdPaginatedDataTable`: lazy-host-compatible page index/first-row API, rows-per-page metadata, page command/event, loading state, and card/table presentation.
- `MdReorderableList`: pointer drag, Ctrl+Up/Down alternative, mutable-list/direct move, start/request/complete events, and command interception.
- `MdGridTile` / `MdGridTileBar`: content plus header/footer overlays with leading/title/subtitle/trailing composition.
- `MdDismissible`: bidirectional drag, thresholds, confirmation event, command, direct dismiss/restore, and distinct directional backgrounds.
- `MdScrollBar`: interactive policy plus automatic/always/hidden thumb and track-visibility APIs.

### Phase 3 — form and route helpers

- `MdForm` / `MdFormField`: validation coordination, required/custom validators, autovalidation, reset/submit, command/event and direct APIs.
- `MdDropdownFormField`: Material ComboBox adapter with the same validation contract.
- `MdSimpleDialog`: selectable option surface with command/event/direct open APIs.
- `MdAboutDialog` and `MdLicensePage`: application metadata, filterable license model, and host-routed license presentation.
- `MdPickerRestorationStore`: provider-agnostic stable-ID serialization helpers for date/time picker state. Hosts decide where the state dictionary is persisted.

### Phase 4 — advanced surfaces and platform adaptation

- `MdDraggableScrollableSheet`: min/initial/max extents, drag-resize, boundary-aware nested wheel coordination, snapping, and controller-style direct methods.
- `MdAdaptiveSwitch` / `MdAdaptiveProgressIndicator`: explicit/automatic Material-Cupertino adaptation metadata, with Material as the safe fallback.
- `MdHero`: tagged shared-element transition request hooks; host navigation and animation orchestration remain external.
- `MdFocusTraversalGroup` and `MdShortcutScope`: Avalonia `KeyboardNavigation.TabNavigation` cycle/skip behavior, policy metadata, and real `KeyGesture` → `ICommand` routing.

### 2026-09-26 hardening pass

- `MdComboBox` now applies its 72-DIP option theme during container preparation as well as through `ItemContainerTheme`; the headless suite verifies the preparation/measure path, while an opened native-popup measurement remains a desktop/Android release check.
- Expanded search has an explicit selected-result/commit contract, command/event notification, display-member resolution, query backfill, and close-on-commit. The Gallery path is verified with a real pointer click.
- Flutter/Ecosystem list, reorder, generated-table, transfer, and tree item themes keep padding in the content layer so hover/pressed/focus/selected state layers use the full rounded surface.
- Reorder pointer initiation in default-handle mode is limited to `PART_DragHandle`; refresh feedback overlays content instead of changing content layout.
- Form `OnUserInteraction` validation ignores untouched fields, dropdown selection marks a field touched, and the invalid dropdown receives the error visual state.
- `MdPaginatedDataTable` exposes a working rows-per-page selector in its footer.

The repeated visual/interaction verification gate is maintained in [`COMPONENT_QUALITY_CHECKLIST.md`](COMPONENT_QUALITY_CHECKLIST.md) and is required after each new component.

## Third-party ecosystem implementation waves

Status: **Waves A–F complete.** The separate `Md3.Avalonia.Extra` package has an opt-in `EcosystemTheme` (the styles entry keeps its original name), depends on core, and never depends on Material Symbols. The complete control/API matrix, clean-room gate, frozen baselines and acceptance obligations are maintained in [`FLUTTER_ECOSYSTEM_RESEARCH_PLAN.md`](FLUTTER_ECOSYSTEM_RESEARCH_PLAN.md).

- **Wave A:** overlay/focus/shortcut/async/density contracts, avatar/rating/breadcrumb, popover, hover card and command palette.
- **Wave B:** slidable, incremental paging, masonry/quilted/woven layout, five-breakpoint adaptive input state and virtualized editable data grid.
- **Wave C:** cancelable searchable single/multi selection, calendar, timeline, result/empty, cascader and transfer.
- **Wave D:** provider-neutral chart with text-table alternative, rich-editor adapter chrome and chat shell.
- **Wave E:** skeleton/group, cancelable animation sequencing, carousel controller/autoplay/loop and async refresh.
- **Wave F:** segmented PIN/OTP input with native edit/paste semantics; arbitrary-depth tree selection/expansion/keyboard/RTL; Material input-chip tag entry with suggestions, limits and validation.

The Gallery **Flutter ecosystem** page contains live interactions for all waves. Automated APIs/rendering are covered by `MdEcosystemWaveAndWindowTests`. Real-platform popup, IME, screen-reader, 100k-row performance and Android workload checks remain release-validation gates rather than missing source controls.

## Borderless window development plan

Status: **W0–W3 complete.** The existing `MdWindow` still preserves native decorations; `MdBorderlessWindow` is opt-in. The detailed implementation and real-platform release matrix are maintained in [`BORDERLESS_WINDOW_PLAN.md`](BORDERLESS_WINDOW_PLAN.md).

Delivered APIs include `MdBorderlessWindow`, `MdWindowTitleBar`, `MdCaptionButton`/`MdWindowCaptionButton`, `MdWindowDragRegion`, `MdWindowResizeGrip`, MVVM commands/events/direct methods, capability flags, injectable `IMdWindowPlatformAdapter`, named Windows/macOS/Linux/Android adapters and a resolver. The desktop Gallery opens a real secondary custom-chrome window; Android resolves to a safe no-op adapter. Headless tests cover adapter application and state operations. OS compositor, native system-menu, snap, multi-monitor and DPI behavior still require the documented Windows/macOS/X11/Wayland manual release matrix.

## Definition of done for every future phase/wave

A phase or wave is not “implemented” until it includes:

- independent `Md*` controls or opt-in adapters; no global replacement of native Avalonia controls;
- Gallery AXAML and C# examples with selectable/copyable code;
- headless interaction, rendering and API regression tests;
- Light/Dark, high-contrast, arbitrary seed-color and reduced-motion verification;
- localization, RTL, keyboard, pointer/touch and automation behavior;
- CommunityToolkit.MVVM-compatible properties/commands/events plus direct operation APIs;
- Android-safe public APIs, with platform-specific capabilities isolated behind adapters;
- updated API documentation, licenses/notices and phase status;
- Release build with zero warnings/errors and a workspace cleaned of `bin`, `obj`, screenshots, packages and other generated artifacts.
