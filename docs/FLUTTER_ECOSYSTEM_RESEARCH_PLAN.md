# Flutter ecosystem clean-room research and implementation plan

Status: **the documented Waves A–F scope is implemented** (hardened 2026-09-26). This is an Avalonia-native, provider-neutral subset; it is not a claim of API or feature completeness against every referenced Flutter package.

This document is the implementation ledger for `Md3.Avalonia.Extra` (published as `Md3.Avalonia.Ecosystem` before the rename; the old namespaces remain as aliases). It records behavior studied from public documentation, not copied implementation. The package depends on `Md3.Avalonia` and Avalonia only; it never requires `Md3.Avalonia.Icons`. Applications can inject their own glyphs, fonts, data engines and providers.

## Mandatory clean-room gate

For every external baseline:

1. Freeze the public package/version and license in `spec-snapshot/manifest.json` and `THIRD-PARTY-NOTICES.md`.
2. Record only public behavior, states, accessibility semantics and API concepts.
3. Do not copy source, private assets, package-specific naming, shaders or generated code.
4. Design an Avalonia-native API based on styled/direct properties, `ICommand`, events and provider interfaces.
5. Apply Material 3 tokens through the opt-in `EcosystemTheme`.
6. Complete the visual/interaction gate in `COMPONENT_QUALITY_CHECKLIST.md`.

Frozen initial baselines are GetWidget 7.0.2, flutter_rating_bar 4.0.1 and shadcn_ui 0.57.0. Later-wave controls use general public interaction conventions and provider-neutral contracts rather than cloned package code.

## Wave A — foundations and overlays (complete)

| Deliverable | Public implementation |
|---|---|
| Overlay placement and focus return | `MdOverlayPlacement`, `MdFocusReturnScope` |
| Shortcut contract | `MdShortcutBinding`; `MdCommandPalette.OpenGesture` and per-command gestures |
| Async request contract | `MdAsyncRequestState`, `MdPageRequest`, `MdPageResult<T>`, `IMdPageProvider<T>` |
| Density | inherited `MdDensity.Mode`: Comfortable, Compact, Touch |
| Popover | `MdPopover`: light dismiss, Escape (including focused popup content), initial popup focus, one-open coordination, focus return |
| Hover card | `MdHoverCard`: focus/hover triggers and deterministic delays |
| Command palette | `MdCommandPalette`: search, keyboard selection, click/Enter execution, backdrop dismissal and global shortcut routing |

Native popups are never forced into an overlay layer. This preserves Android/headless safety and avoids first-frame custom-surface shells.

## Wave B — collections, layouts and enterprise data (complete)

| Deliverable | Public implementation |
|---|---|
| Slidable actions | `MdSlidableItem`: pointer drag, start/end actions, RTL handling and keyboard alternatives |
| Paged/infinite items | `MdPagedItemsView`: provider delegate, cancellation, retry, refresh and explicit idle/loading/data/empty/error/completed states |
| Masonry/quilted/woven | `MdMasonryPanel` and `MdMasonryLayoutStrategy` |
| Adaptive input mode | five `MdAdaptiveBreakpoint` values, named breakpoint, orientation and Touch/Pointer/Keyboard input state in core `MdAdaptiveLayout` |
| Extensible virtualized grid | `MdDataGrid`, `MdDataGridColumn`: virtualized rows, pointer-sortable/aligned headers, sorting, predicate/text filtering, resize/reorder APIs, selection, editing and clipboard text |

`MdMasonryPanel` is a finite layout panel. Large feeds should use it through an item-realization host. `MdDataGrid` uses a virtualizing stack panel in its default theme.

## Wave C — selection, calendar and feedback (complete)

| Deliverable | Public implementation |
|---|---|
| Async searchable selection | `MdAsyncSelect`: debounce, cancellation, local/remote search, empty/error states and keyboard interaction |
| Calendar | `MdCalendar`: 42-day month grid, single/range/multiple selection, date enable predicate, badges and keyboard traversal |
| Timeline | `MdTimeline`, `MdTimelineItem`, `MdTimelineItemPresenter`: vertical/horizontal start/end connectors, first/last termination, item status visuals and `ActiveIndex` projection |
| Result and empty states | `MdResultView`: information/success/warning/error/empty kinds plus `ICommand` action |
| Cascader | `MdCascader`, `MdCascaderItem`: selected path and interactive columns |
| Transfer | `MdTransfer`: filters, checked moves and move-all APIs/buttons |

## Wave D — provider-neutral advanced surfaces (complete)

| Deliverable | Public implementation |
|---|---|
| Charts | `MdChart`, `MdChartSeries`, `MdChartPoint`, `IMdChartDataProvider`; line/area/bar presentation, axes/grid and pointer exploration |
| Rich editor UI | `MdRichEditor`/`MdRichTextEditor`, `IMdRichEditorAdapter`, `MdRichEditorCommand`; the package owns chrome, never a document engine |
| Chat | `MdChatView`, `MdChatMessage`: message source, composer, bubble-only selection hit testing, `ICommand`/event submit, history provider and suggestions |

No chart vendor, rich-text engine, network service, model API or media codec is embedded.

## Wave E — loading and motion plus core enhancements (complete)

| Deliverable | Public implementation |
|---|---|
| Skeleton | `MdSkeleton`/`MdSkeletonGroup`: rectangle/rounded/circle/text shapes, grouped state, base/highlight brushes, real pulse timer and reduced-motion switch |
| Sequencing | `MdAnimationSequence`: cancelable, awaitable stagger, reverse order and opacity transitions |
| Carousel | `MdCarouselController`, autoplay, hover pause, wrap/finite navigation and infinite-loop API |
| Image viewer | gesture presets, configurable zoom step, finite/wrapped navigation and accessible keyboard operations |
| Refresh | async cancelable `RefreshHandler`/`RequestRefreshAsync` in addition to `ICommand` and event paths |

## Wave F — structured input and hierarchy (complete)

| Deliverable | Frozen public behavior baseline |
|---|---|
| PIN/OTP input | `flutter_pinput` 1.0.3: fixed-length segmented entry, paste, masking, focused/filled/error/disabled states and completion |
| Tree view | `animated_tree_view` 2.3.0: arbitrary depth, expansion, indentation, utility APIs and RTL |
| Tag input | `chips_input_autocomplete` 1.2.2: dynamic/removable tags, suggestions, limits and validation |

Only the public behavior listed above was reviewed. The Avalonia API, implementation, Material 3 visuals, tests, and examples are designed independently. No third-party Dart source or private assets are used.

## Gallery and tests

- `EcosystemGalleryPage` provides live interaction for every wave, Light/Dark theme inheritance and selectable/copyable XAML/C# usage.
- `MdEcosystemWaveAndWindowTests` and `MdEcosystemWaveFTests` cover public state machines, providers, direct APIs, binding-friendly collections, RTL, rendering and Gallery construction.
- Current hardened baseline (2026-10-07): solution Release build with zero warnings/errors and 402/402 headless tests passing. The added tests exercise real pointer search commit, reorder-handle drag, data-grid header sorting, chat bubble hit testing, state-layer geometry/pressed state, keyboard form input and Tab focus cycling; native popup-host and Android checks remain separate release gates.

## Acceptance and future compatibility

The documented implementation scope is complete. Before each release, still run:

- desktop manual popup positioning/light-dismiss checks on Windows, macOS and Linux;
- Android workload build and touch smoke test when the workload is available;
- keyboard/screen-reader checks and 200% scaling;
- package dependency/content inspection and artifact cleanup.

These are release-validation obligations, not placeholders for unfinished controls.
