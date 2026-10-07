# UI parity audit — Material Design 3 and Flutter Material

**Audit date:** 2026-09-27  
**Scope:** `Md3.Avalonia`, `Md3.Avalonia.Icons`, `Md3.Avalonia.Extra`, Gallery shell and all Gallery component destinations  
**Baseline:** current Material Design 3 component site and Flutter 3.47 Material catalog/API

## 1. Executive conclusion

The library now has broad **catalog coverage**, but it is not yet honest to call it complete visual/behavioral parity with either the current Material Design 3 site or Flutter Material.

- The desktop Light/Dark baseline is coherent. The Gallery shell, surfaces, typography hierarchy, dynamic colors and most resting component states render without desktop horizontal overflow.
- The current Material top-level component catalog is represented: actions, containment, communication, navigation, selection and text input are all present.
- Several controls are visually close in their resting state: buttons, badges, checkbox/radio, cards, divider, loading/progress, snackbar, switch and the dynamic theme surface.
- The largest visible defect is **compact layout**. At 360 dp, fixed-width rows and non-wrapping headings are clipped on most complex pages. This is a real rendered defect, not merely a source-code concern.
- Native/platform surfaces remain unverified: popup-hosted ComboBox/menu/autocomplete/picker content, OS window chrome, Android, IME, screen reader, touch and compositor behavior cannot be proven by these headless screenshots.
- The official Material Symbols font was not available for this audit. The temporary DejaVu build correctly failed font validation, so icon-dependent components rendered intentionally empty. Their icon alignment and official glyph rendering therefore remain unverified.
- Accessibility semantics are incomplete for custom controls. Base Button/TextBox/ListBox-derived controls inherit useful Avalonia peers, but there are no custom `OnCreateAutomationPeer` implementations for controls such as range slider, date/time picker, PIN input, chart, transfer, sheet or composite navigation.

### Severity summary

| Severity | Finding |
|---|---|
| **P0** | 360 dp compact pages clip headings, fields, action rows, menus and demo surfaces. |
| **P0 release gate** | Official Material Symbols rendering has not been captured with the required official font. |
| **P0 release gate** | Native popup, Android, IME, screen-reader and real window-compositor paths remain unverified. |
| **P1** | Custom automation semantics/value/range/selection patterns are incomplete. |
| **P1** | Chip, segmented, slider and some trailing-action hit regions do not consistently provide a 48×48 dp touch target. |
| **P1** | Current M3 carousel additions (full-screen and uncontained multi-aspect-ratio) and current dynamic shape/parallax behavior are missing. |
| **P1** | M3 Expressive focused-search expansion is missing; the clear action is 40×40 rather than a 48 dp target. |
| **P1** | Bottom sheet lacks the current 640 dp wide-layout cap and its drag-handle target is 64×32 rather than at least 48 dp high. |
| **P1** | Gallery Chinese localization is incomplete; a coarse static scan found 392 untranslated candidates among 717 unique static labels. |
| **P2** | Gallery demos do not expose every implemented variant/state, so the Gallery understates or cannot verify the API surface. |
| **P2** | No explicit Flutter-stable compatibility preset separates Flutter defaults from newer M3 Expressive defaults. |

## 2. What headless screenshots can and cannot prove

### What was inspected

A temporary headless audit harness rendered the actual Avalonia visual tree and captured:

- **39 Gallery destinations**;
- **89 Light desktop frames** at `1700×1000`, including scrolled sections;
- **39 Dark desktop top frames**;
- **12 representative pages at 580 dp**;
- **12 representative pages at 360 dp**;
- **152 screenshots total**.

Automated geometry collection also recorded:

- no shell-level horizontal extent overflow at the 1700 dp desktop width;
- the scroll extent and viewport for every destination;
- interactive controls below 48 dp;
- likely text clipping candidates.

All 39 Dark top frames remained readable at rest. This does **not** replace contrast measurement for every state/seed.

### What a static headless frame does not prove

- Native `PopupRoot` placement, first-frame behavior or platform shadows/corners;
- pointer hover timing, long press, drag velocity, fling, overscroll or haptics;
- real animation cadence, frame pacing or GPU-compositor artifacts;
- Android density, status/navigation insets, soft keyboard and touch behavior;
- Windows/macOS/Linux window chrome, DWM, Snap Layout, native system menus or multi-monitor DPI;
- screen-reader announcements and full UI Automation patterns;
- IME composition, candidate windows and platform text selection;
- official icon glyphs when the official Material Symbols font is absent.

Headless screenshots are therefore a strong **layout/rest-state regression tool**, not a complete release certification tool.

## 3. Reference baselines

- Material component catalog: <https://m3.material.io/components>
- Material layout breakpoints and scaffold guidance: <https://m3.material.io/foundations/layout/layout-overview/overview>
- Material target-size guidance: <https://m3.material.io/foundations/designing/structure>
- Current Material carousel: <https://m3.material.io/components/carousel>
- Material search specs: <https://m3.material.io/components/search/specs>
- Material chip guidelines: <https://m3.material.io/components/chips/guidelines>
- Flutter Material catalog (Flutter 3.47 at audit time): <https://docs.flutter.dev/ui/widgets/material>
- Flutter Material API: <https://api.flutter.dev/flutter/material/>
- Flutter `MaterialTapTargetSize`: <https://api.flutter.dev/flutter/material/MaterialTapTargetSize.html>

Material and Flutter are not always pixel-identical. Current M3 Expressive guidance has moved ahead of some Flutter stable defaults. A strict product should offer two documented modes instead of blending them silently:

1. **Current M3 / Expressive** visual target;
2. **Flutter stable compatibility** target.

## 4. Gallery shell and responsive layout

### Desktop

The current desktop scaffold is materially closer to the official site:

- primary destination rail;
- utility header;
- contextual Components navigation;
- centered documentation body;
- “On this page” pane;
- independent scrolling and page-top reset after navigation.

The rendered desktop composition is coherent and usable in Light and Dark.

### Medium and compact

The navigation strategy is sound in principle: primary rail at medium width, modal contextual navigation below 1200 dp, and compact top chrome below 600 dp.

The page bodies are not yet fully responsive:

- At **580 dp**, long H1/H2 labels such as “Navigation rail & adaptive” and ecosystem section titles clip because they do not wrap.
- At **360 dp**, 11 of 12 inspected complex pages had visible clipping or off-screen content.
- Fixed horizontal groups remain two-column or fixed-width instead of stacking.
- Button, dialog, sheet and menu action rows can lose their trailing actions.
- Picker fields place the second/third field outside the visible width.
- The Flutter parity banner collapses its text into an unusably narrow vertical strip.
- Navigation-bar, adaptive and ecosystem titles are cut off.
- Code examples remain wider than the compact content viewport.

**Required correction:** introduce a reusable responsive Gallery section/grid primitive; wrap H1/H2/body text; replace fixed horizontal `StackPanel` groups with width-aware wrap/stack layouts; make demos choose compact templates at 360 dp, not only resize their outer control.

## 5. Component-by-component Material audit

Legend:

- **Close** — resting visual is broadly aligned; remaining work is validation or edge-state depth.
- **Partial** — core control exists and looks plausible, but a material visual/behavior/state gap remains.
- **Gap** — visible defect, current-spec omission or unverified critical surface.

| Component | Resting visual | Material/Flutter gap still present |
|---|---|---|
| Buttons / toggle buttons | **Close** | API has five sizes, round/square and selected morphing, but Gallery shows only XS–Large and does not demonstrate Extra Large, square or all pressed/selected morph states. Split-button demo uses a text `＋` instead of the official icon pipeline. A Flutter-stable snapshot preset is absent. |
| Standard / connected button groups | **Partial** | Group shapes and selection are present. The visible 40 dp group has no separate 48 dp touch target. Keyboard roving-focus/semantics need dedicated automation validation. |
| Icon buttons / toggle icon buttons | **Unverified** | Containers, color styles, shapes and sizes exist, but official glyph rendering was intentionally blank in this audit. Cannot approve optical centering, selected-icon substitution or icon contrast without the official font. |
| FAB / extended FAB / FAB menu | **Partial** | Size/color/menu surfaces are present. Icon-dependent states were blank. FAB-menu container transform, action count edge cases, accessibility ordering and Android touch behavior still need real-font/runtime capture. |
| Top and bottom app bars | **Partial** | Small, centered, medium, large and bottom bars are present. They read as bordered demo boxes rather than complete screen-edge bars. Flutter `SliverAppBar`-style pinned/floating/snap/stretch/collapse and scroll-under elevation behavior are not represented. |
| Badges | **Close** | Small/large/error and standalone anatomy render correctly. Needs official-icon placement capture, live-region semantics for changing counts and overflow/max-count examples. |
| Text fields | **Partial** | Filled/outlined, label, support/error, clear/password and context-menu behavior exist. At 360 dp paired fields clip. Prefix/suffix layout is tight. Multiline, counter/max-length, dense Flutter `InputDecoration`, IME and selection-toolbar parity are not fully demonstrated. |
| Checkbox | **Close control / Partial page** | Unselected, selected, indeterminate, disabled and error states are present. Gallery headings have constrained-height artifacts. Screen-reader checked/indeterminate/error announcements still rely mostly on the base peer. |
| Radio button | **Close** | Resting and disabled/error states look aligned. Needs radio-group semantics/name/value verification and Android touch capture. |
| Switch | **Close** | Resting dimensions visually match the 52×32 M3 track family. Requires official selected/unselected icons, 48 dp target verification across density modes and adaptive Flutter-platform behavior beyond metadata. |
| Chips | **Gap** | Assist/filter/input/suggestion visuals are present, but the whole controls measure 32 dp high. Current Material guidance requires a minimum 48 dp interaction target even when the visible container is 32 dp. Remove actions also need independent 48×48 targets and collision-free semantics. |
| ComboBox / exposed dropdown | **Partial** | Closed filled/outlined/error/editable fields render correctly. Native popup height, anchor contact, item 72 dp layout, corners, old-popup dismissal, keyboard focus return and Android host behavior are not visible in headless. At 360 dp paired examples clip. |
| Autocomplete | **Partial** | Native editing and async/filter contracts exist. Suggestions popup remains a native-host release gate; cancellation/race tests do not prove real popup geometry. |
| Numeric input | **Close for desktop** | Spinner now stays within the field on left/right and is visually compact. Each half is only 32×20, which is suitable for explicit desktop density but not a touch target; a separate touch-density template is needed for Android. |
| Carousel | **Gap** | Multi-browse, hero, center-aligned and uncontained APIs exist with snapping and item resizing. Current M3 additionally includes full-screen and uncontained multi-aspect-ratio layouts. Dynamic shape change and content parallax are not implemented. Gallery omits center/uncontained examples and compact adaptive layouts. |
| Cards | **Close baseline** | Elevated/filled/outlined surfaces and interactive state are present. Gallery lacks rich media/action anatomy, collection/grid responsive examples and complete semantics for interactive versus presentation-only cards. |
| Date picker / date range picker | **Partial / runtime gate** | Docked/modal APIs, 42-day calendar data, range selection and localization exist. Closed fields render correctly. Open calendar surfaces, focus loop, compact adaptation, scroll/keyboard month navigation and Android popup behavior were not visually certified here. |
| Time picker / time dial | **Partial / runtime gate** | Dial/input and 12/24-hour paths exist. Open dial, number placement, localized period controls, keyboard traversal and compact/full-screen adaptation were not captured. |
| Dialogs | **Partial** | Basic and full-screen contracts exist with scrim/host. At 360 dp Gallery action rows clip. Full-screen dialogs should be compact-only and basic dialogs should remain bounded on larger widths; this needs explicit responsive assertions and open-state screenshots. |
| Divider | **Close** | Full-width, inset and vertical examples are visually aligned. Needs RTL inset semantics and high-contrast checks. |
| Lists / list items | **Partial** | Generated rows have a 56 dp minimum and rich `MdListItem` uses 64 dp. Material/Flutter one-, two- and three-line heights should be explicit (roughly 56/72/88 families) instead of one general minimum. Leading/trailing/status semantics need stronger peers. |
| Loading indicator | **Close to current M3** | Expressive shape variants and motion schemes render. Static screenshots cannot certify morph cadence; Flutter stable does not have a one-to-one equivalent for every expressive shape. |
| Linear/circular progress | **Close** | Determinate, indeterminate and expressive wave forms render well. Needs exact current token measurements, high-contrast/disabled states and Flutter-stable compatibility snapshots. |
| Menus / submenu / menu anchor | **Partial / runtime gate** | Inline menu surface and API exist. Native popup placement, nested submenu hover delay, collision handling, light dismiss, RTL, screen edges and focus restoration are not certified. A dedicated Flutter `MenuBar`/desktop menubar pattern is absent. |
| Navigation bar | **Partial** | The destination surface and state indicator exist. Official icons were blank. Compact Gallery title/demo text clips at 360 dp. Selection semantics, destination announcements and 3–5 destination constraints need stronger automation validation. |
| Navigation drawer | **Partial** | Standard/modal states and placement exist. Header text has sizing artifacts; open modal geometry, edge/RTL behavior and screen-reader modal isolation remain runtime gates. |
| Navigation rail | **Partial** | Rail and destination layout render plausibly. Official icons were blank. Expanded/collapsed semantics, RTL, keyboard roving focus and medium-window touch behavior need more work. |
| Adaptive suite / scaffold | **Partial** | Real mode changes and finite layouts exist. The Gallery’s own 360 dp demo clips titles and controls. Flutter/Avalonia navigation-state restoration and route integration remain host responsibilities rather than parity APIs. |
| Search bar / search view | **Gap** | Basic 56 dp search surfaces and selected-result contract exist. Current M3 Expressive focus expansion (24 dp margins to 12 dp) is absent. Theme minimum width is 240 rather than the current 360 dp search guidance, and clear is 40×40. Open view geometry/motion needs real capture. |
| Bottom/side sheets | **Gap** | Modal/standard placement and real drag handling exist. Bottom surface has no 640 dp max width on expanded layouts. Drag handle target is 64×32 rather than at least 48 dp high. Static Gallery top frame shows only page content, not an opened sheet. Nested scroll/velocity and Android gestures remain unverified. |
| Slider | **Partial** | Continuous/discrete, stop/value indicators and disabled state render. Interactive template height measured 44 dp, below Flutter’s padded 48 dp target. Exact current M3 handle/track/pressed morphing and touch behavior need validation. |
| Range slider | **Partial** | Two-handle values and date-range sample exist. The Gallery fixed-width group clips at 360 dp. Handle crossing, overlap disambiguation, semantics for two values and Android touch require deeper tests. |
| Segmented buttons | **Partial** | Single/multi selection and group geometry exist. Visible/interactive group height is 40 dp with no outer 48 dp target; compact groups use fixed widths and clip. |
| Snackbar | **Close baseline** | Inverse surface, action and configurations render well. Timer pause on hover/focus, queue behavior, safe-area placement, screen-reader live announcement and Android bottom inset remain runtime checks. |
| Tabs / tab view | **Partial** | Primary, secondary, scrollable and content-view patterns are present. Needs compact overflow controls, complete keyboard/RTL semantics, official icon capture and Flutter indicator/animation compatibility snapshots. |
| Toolbars | **Unverified/Partial** | Floating, vibrant, docked, contextual and vertical containers exist, but most toolbar content was visually empty because icons were unavailable. Current Material requires 48×48 targets; contextual transition and vertical ordering need official-font capture. |
| Tooltips | **Partial** | Plain/rich surfaces render and delayed close logic exists. Hover, focus, long-press timing, action focus and Android long-press placement are not proven by static frames. |
| Material Symbols | **Release gate** | Invalid-font state correctly hides glyphs and explains the failure. The successful embedded official-font path must be rendered before release. |
| Motion / ripple | **Partial** | Standard, expressive, reduced and none schemes exist. Static frames cannot approve spring cadence, pointer-origin ripple, interruption/reversal or 60/120 Hz behavior. |
| Dynamic theme | **Close baseline** | HCT seed, variants, Light/Dark, contrast diagnostics, typography/shape/motion and JSON flow render coherently. OS high-contrast auto-detection and complete per-state contrast audits are not present. |
| Borderless window | **Runtime gate** | The simulated chrome is readable. Native menu, DWM corners/shadow, Snap Layout, DPI, resize hit testing and macOS/X11/Wayland behavior cannot be approved headlessly. |

## 6. Flutter-specific parity audit

### Core Flutter Material widgets

The common Flutter Material catalog is broadly mapped through independent `Md*` controls. The remaining gaps are mainly exact defaults, platform services and semantics rather than missing names.

| Flutter family | Current mapping | Gap |
|---|---|---|
| Filled/Elevated/Outlined/Text/Icon buttons | `MdButton`, `MdIconButton`, toggles | No explicit Flutter-stable theme preset; current implementation follows newer M3 Expressive sizes/shapes. |
| `FloatingActionButton` | FAB controls | Official icon/runtime gate; mini/padded target and Hero route transition are not integrated as Flutter does. |
| `SegmentedButton` | Segmented group | 40 dp target without Flutter’s mobile padded-target behavior. |
| `Badge` | Badge controls | Live semantics/overflow examples incomplete. |
| `Card`, `Divider`, `ListTile` | Card/divider/list | List height families and semantics need closer Flutter snapshot coverage. |
| `AppBar`, `BottomAppBar` | App bar controls | No `SliverAppBar` equivalent for pinned/floating/snap/stretch/collapse. |
| `NavigationBar`, drawer, rail, tabs | Navigation controls | Route/history restoration and destination semantics are weaker; official icon capture is missing. |
| Checkbox/radio/switch/chips | Selection controls | Chips lack padded touch target; adaptive factories are not comprehensive. |
| Date/time pickers | Picker controls | Native/open-surface and restoration certification incomplete. |
| Menu/`MenuAnchor` | Menu controls | Native popup/collision/menu-bar behavior remains a release gate. |
| Slider/range slider | Slider controls | 44 dp versus Flutter padded 48 dp; two-value automation semantics incomplete. |
| `TextField`, search | Text/search controls | Flutter `InputDecoration` breadth and current M3 focused-search expansion are incomplete. |

### Flutter parity phases 1–4

| Control group | Visual result | Remaining gap |
|---|---|---|
| Material banner | **Partial** | Surface/actions render; compact 360 layout collapses banner text into a narrow vertical strip. Dismiss/focus/live semantics need work. |
| Expansion panels | **Close baseline** | Visual stack is coherent. Animated header semantics, focus and large-content virtualization need validation. |
| Data table | **Partial** | Rows/header/sort selection render. Flutter column sizing, numeric alignment, horizontal overflow and accessibility table semantics are incomplete. |
| Stepper | **Partial** | Vertical flow renders and horizontal API exists. Compact and error/editing state snapshots are missing; no specialized automation peer. |
| Pull-to-refresh | **Behavior gate** | Real state machine/tests exist, but static screenshots cannot prove pull resistance, armed threshold and nested-scroll behavior on touch. |
| Paginated data table | **Partial** | Footer and rows-per-page flow exist. Large-data performance, keyboard/table semantics and width adaptation need validation. |
| Reorderable list | **Partial** | Handle and keyboard alternatives exist. Real touch drag/autoscroll, pointer capture and Android behavior remain unverified. |
| GridTile/GridTileBar | **Close baseline** | Visual composition exists; semantics and large-grid behavior need tests. |
| Dismissible | **Partial** | Bidirectional drag logic exists. Velocity, confirm/cancel animation and RTL touch need real-platform validation. |
| Form/FormField/dropdown form | **Partial** | Validation behavior exists. IME submit, autofill and full error semantics remain incomplete. |
| Simple/About/License dialogs | **Partial** | APIs and sample flow exist. Modal focus isolation and routed navigation are not Flutter-equivalent host services. |
| Picker restoration | **Partial** | Serialization helper exists; host persistence/navigation restoration is intentionally external. |
| Draggable scrollable sheet | **Partial** | Resize/nested wheel behavior exists. Touch velocity/snap and real nested scrolling remain release gates. |
| Adaptive switch/progress | **Partial** | Metadata and policy exist; only switch/progress are covered, not the broader set of current Flutter adaptive factories. |
| Hero | **Gap versus Flutter** | Emits transition requests, but does not provide Navigator-integrated shared-element flight, overlay, clipping interpolation or route lifecycle. |
| Focus traversal / shortcuts | **Partial** | Real Avalonia keyboard routing exists. Flutter focus-policy breadth, directional traversal and semantics need expanded tests. |

### Flutter components/utilities still absent or only approximated

These are not all appropriate for the core library, but they are gaps if “Flutter parity” is interpreted literally:

- `SliverAppBar` and sliver/scroll-coordinated header behavior;
- `MenuBar` and full desktop menu traversal/collision behavior;
- Navigator/restoration-integrated Hero flights;
- complete adaptive factories beyond switch/progress;
- Flutter `ScaffoldMessenger` queue/scope semantics;
- `SelectionArea` / adaptive text-selection toolbar breadth;
- Material page-route, predictive-back and route-transition services;
- a Flutter-stable default-token compatibility theme.

## 7. Ecosystem package audit

| Area | Current result | Remaining gap |
|---|---|---|
| Overlay/popover/hover card/command palette | Surfaces and commands exist | Native popup collision, multi-monitor placement, focus trapping and screen reader modal behavior are unverified. |
| Slidable/paged/masonry/data grid | Real controls and visible data changes exist | 100k-row performance, sticky header, column resize/reorder, touch slidable thresholds and screen-reader table semantics remain release gates. |
| Async select/calendar/timeline/result | Search/calendar/timeline surfaces render | Calendar supports month plus single/range/multiple selection, but planned week/two-week formats are absent; open async popup remains unverified. |
| Cascader/transfer | Real selection/transfer exists | Compact layout and RTL/touch drag need work; composite selection semantics are absent. |
| Chart | Axes, line/area/bar and pointer hover exist | Planned pie/donut/scatter/radar are absent. No data-table automation peer or keyboard point navigation. |
| Rich editor | Toolbar and provider-neutral adapter exist | It remains a shell/contract, not Flutter Quill document, selection, embed, clipboard, import/export or undo-history parity. |
| Chat | Composer, history, multi-select, quote/delete/retry exist | No first-class attachment model, grouping/date separators, delivery/read semantics or proven incremental virtualization at production scale. |
| Skeleton/animation sequence | Real states/motion policies exist | Long-running allocation/frame pacing and platform reduced-motion integration need profiling. |
| PIN/OTP | Native edit/paste shell exists | IME, password manager/autofill, accessibility value and Android keyboard tests remain. |
| Tree view | Expand/select/keyboard/RTL exists | Virtualization and very deep/large hierarchy performance remain. |
| Tag input | Add/remove/suggestion behavior exists | Popup/IME/compact wrapping and accessible remove-action targets need validation. |
| Avatar/rating/breadcrumb | Baseline controls render | Avatar image loading/error semantics, breadcrumb compact overflow, and fractional-rating accessibility/target geometry need deeper checks. |

## 8. Cross-cutting deficits

### Accessibility

The source contains attached automation names/live settings in selected places, but no custom automation-peer implementations. Composite controls therefore do not expose complete range/value/selection/expand-collapse/table semantics.

Priority peers:

1. `MdRangeSlider` — two independent range values;
2. date/time/range pickers — value plus expand/collapse;
3. sheet/dialog/search view — modal/open state and focus scope;
4. stepper, expansion panel, tree, cascader and transfer — position/expanded/selection state;
5. chart/data table/data grid — table/data summaries and keyboard navigation;
6. PIN/tag/chat — coherent value/selection/action semantics.

### Touch targets and density

Material generally recommends 48×48 dp interaction targets; Flutter uses padded 48 px targets on mobile and platform-appropriate shrink-wrap defaults on desktop.

Observed genuine target gaps include:

- chips: 32 dp high outer control;
- segmented groups: 40 dp high;
- search clear: 40×40;
- slider interaction height: 44 dp;
- bottom-sheet drag handle host: 64×32;
- numeric spinner: 32×20 per action — acceptable only for documented desktop density.

The correct fix is not to enlarge every visual container. Add a density/input-mode policy with invisible hit padding while preserving the specified visible geometry.

### Localization

The Gallery localization system is functional, but incomplete. Static AXAML analysis found:

- 717 unique static labels;
- 359 dictionary keys;
- 392 untranslated candidates before excluding proper nouns, data values and code.

Many candidates are genuine headings, descriptions and actions. Chinese mode therefore cannot yet be called complete.

### RTL

Avalonia mirrors many basic layouts automatically, and tree/reorder tests cover selected RTL paths. Directional behavior is not systematic across navigation, sliders, carousel, drawers/sheets, menus, pickers, app bars and directional icons.

### High contrast and scale

The theme generator has user-selectable contrast levels and diagnostic ratios, but there is no demonstrated automatic OS high-contrast mapping. This audit did not render 200% scaling; that remains a required matrix.

## 9. Recommended remediation order

### Phase A — visible compact and target defects

1. Fix every 360 dp Gallery page using reusable responsive section/grid primitives.
2. Add wrapping/ellipsis policy for H1/H2/body and TOC labels.
3. Stack paired fields/actions below their minimum widths.
4. Add input-mode-aware 48 dp hit regions for chips, segmented controls, search clear, sliders and sheet handle.
5. Add bottom-sheet 640 dp expanded-width cap.

### Phase B — current M3 deltas

1. Add full-screen and uncontained multi-aspect-ratio carousel layouts.
2. Add carousel dynamic shape/parallax behavior with reduced-motion fallback.
3. Add focused-search expansion and exact current spacing tokens.
4. Complete Gallery examples for all button sizes/shapes/morph states and all carousel variants.
5. Capture every icon-dependent component with the official offline font.

### Phase C — semantics and Flutter compatibility

1. Add custom automation peers for composite controls.
2. Add a documented Flutter-stable compatibility theme/preset.
3. Add SliverAppBar-like scroll-coordinated app bar behavior if Flutter parity remains a goal.
4. Define which route/navigation services are intentionally out of scope rather than labeling them complete.
5. Complete adaptive-control policy and mobile padded targets.

### Phase D — real-platform release matrix

1. Desktop native popup host: ComboBox, autocomplete, menu/submenu, date/time picker, tooltip.
2. Android: build, install, font, IME, touch, back, insets and popup matrix.
3. Windows/macOS/X11/Wayland: borderless chrome, resize, DPI, shadows/system menus.
4. NVDA/VoiceOver/Orca and keyboard-only traversal.
5. 100%, 150%, 200% scaling and 360/600/840/1200/1600 dp breakpoints.
6. Arbitrary seed, Light/Dark/high-contrast and reduced-motion screenshots for every state.

## 10. Audit verdict

The current implementation is a substantial, testable Material-themed Avalonia library, not an empty skin. It already covers more component families than Flutter’s short public Material catalog page. However:

- **desktop rest-state coverage is the strongest area**;
- **compact layout, semantics and real-platform surfaces are the weakest areas**;
- **current M3 carousel/search/sheet details and Flutter route/sliver behavior are concrete parity gaps**;
- **official-font, Android, native popup and screen-reader validation must remain explicitly unverified until performed**.

This document should be treated as the next correction backlog, not as a claim that every component is complete.
