# Gallery defect review — tracker

Source: the 2026-10-04 review of the gallery and component library. Numbering follows the
reported list. This file records the **root cause** for anything diagnosed, so the next pass does
not repeat the search.

Verification note: the development sandbox has no .NET SDK. Everything here is verified by
compilation and headless tests in CI, not by looking at the running app. Items marked *visual*
need confirmation on a real desktop run.

## Fixed

| # | Symptom | Root cause | Commit |
|---|---------|-----------|--------|
| 1 | Breadcrumb: the overlay covers the trail and no segment responds to a click | The ellipsis overflow popup was anchored to the whole control rather than to its own button, so it painted across the trail, and the segments were `TextBlock`s with no input surface. Both theme copies carried the same template and had to move together. | `8680142` |
| 2 | Carousel: the caption text overflows and collides with the next item | Items were measured against the viewport instead of against the slot they actually occupy. Added a `ContentExtent` attached property and `GetArrangement()` so the item knows its own width before its text wraps. | `8680142` |
| 5 | Segmented button: in the usage sample the selected item's label sits off-centre | Only the icon slot collapsed when an item had no icon, so a selected item reserved space for a check mark its neighbours did not. Keyed on a new `^:selected:has-selected-icon` state so the reservation matches what is drawn. | `8680142` |
| 7 | Settings expander: the header's padding does not match the cards around it | `Padding` was consumed by the inner `PART_ContentArea` (`16,8`) while the header's own root `Grid` ignored it, so the two surfaces indented differently. The ToggleButton root now takes `Padding` and the content area drops to `0,4`. | `8680142` |
| 27 | Container transform and animated visibility do not animate; they snap | The four motion controls toggled `IsVisible`, which is not a transition. Rebuilt on two new primitives: `MdRevealHost` yields layout space and clips as a fraction, `MdMorphPanel` interpolates between two children's measured extents. Shared axis now slides 30 DIP, fade-through scales from 0.92, container transform cross-fades 30/70 while its corner radius travels. Content that is already a `Control` is not re-parented, so it animates in place without an exit cross-fade. | `8680142` |
| 3 | Nav bar / rail: the pressed overlay has square corners over a rounded indicator | The ripple sat in a `Grid` with `ClipToBounds`, and a Grid clips to a **rectangle**. Now clipped by a `Border` carrying the pill's corner radius. Rail had the same template. | `0e30402` |
| 4 | Expanded search: after choosing a result, clearing and retyping never reopens the list | `MdSearchView` only hooked `KeyDown`; nothing watched the text. `CommitResult` writes the chosen text and dismisses, so only a host `Show()` could reopen it. Now reopens on header `TextChanged`, guarded by focus and by the committed text. | `6ec505a`, `bb36f45` |
| 6 | `FontSize` on a settings card / group / expander title does nothing | The templates wrote `FontSize="16"/"14"` **directly on the presenters**. A literal inside a template outranks any value from outside. Moved to `ControlTheme` setters + `{TemplateBinding FontSize}`; defaults unchanged, now overridable. | `0e30402` |
| 11 | Rich tooltip disappears before it can be reached (same on focus) | `HideDelay` is 100 ms for every variant. A rich tooltip is interactive, so the pointer must cross the gap to reach it; the timer expired mid-flight. Leaving now starts a variant-aware countdown with a 600 ms bridge for rich tooltips. | `ecc3d5f` |
| 14 | Simple dialog: clicking an item reports "cancelled" | Selection closed the dialog via `Dismiss()`, so one click raised **both** `ItemSelected` and `Dismissed`, and the cancel listener ran second and overwrote the result. A choice is no longer a dismissal. *(The styling half of this item is still open.)* | `ecc3d5f` |
| 16 | "Move avatar": a fragment of the content shows for a few frames | The page revealed the destination hero and started the flight **in the same pass**, so `MdHero` measured a hero with no layout yet, photographed a zero-sized rectangle and abandoned the transition — then the source was hidden immediately, reflowing the row under a transition that had already recorded the geometry. Now: reveal, let one layout pass run, await the flight, then hide the source. | `6189b95` |
| 18 | Load next page gives only a text message | Two causes. `MdPagedItemsView` had **no `ItemTemplate`**, so records rendered as bare `ToString` text, indistinguishable from the status label. And the demo provider returned synchronously, so the view went `Loading → Data` inside one dispatcher pass and the progress indicator was switched on and off before it could be painted. | `6189b95` |
| 17 | "Swipe this row" shows wrong colours at all four corners | `PART_Foreground` was rounded to 12 but the archive/delete layers behind it were **square**, and the clipping container was a `Grid` — rectangular clip. The primary and error containers showed through outside the foreground's rounded corners. One rounded `Border` now clips the whole row. | `bf55924` |
| 20 | Dragging a row between the transfer lists hides the row | The drag translated the **real `ListBoxItem`**, which stays inside its own `ListBox`, so the scroll viewport clipped it the moment it moved toward the other list. A picture of the row now rides in the overlay layer, which nothing clips. | `6189b95` |
| 28 | Numeric `−` is not centred in its target | The glyph `M3 8H15V10H3Z` is **12 wide by 2 tall**, but the `Path` declared a 12×12 box. `Stretch="Uniform"` scales by 1 and seats the geometry at the **top** of the box, so only the `+` — whose ink really is 12×12 — came out centred. This is also what made the pair look badly aligned. The numeric control also gained **its own gallery page**. | `bf55924` |
| 23① | Tree expander's hover area is a small square, not a full circle | `PART_State` was inside a centred grid, so it was sized by the 16 DIP chevron, and it had no `CornerRadius`. It now fills the 40 DIP circle. | `ecc3d5f` |
| 23② | Expanding one node rotates other nodes' chevrons | `Rebuild()` replaces the whole flattened row list, so every container rebinds to a different row — and the 0.2 s transition on the chevron's `Angle` animated each one to its new value. Rotation is now instant. | `ecc3d5f` |
| 24①② | Rating is cut off **and** can only be lowered, never raised | One cause. The gallery pinned `Width="176"` while the natural row is `5 × 48 + 4 × 4 = 256`: trailing stars were clipped away, and the positions still reachable were mapped against the unclipped 256 scale, so every visible star reported a lower value than it drew. Render and hit testing now share one `GetMetrics(arrangedWidth)`. | `0e30402` |
| 25 | Redundant breadcrumb under the rating | Removed; `MdBreadcrumb` already has its own page. | `0e30402` |

Regression tests added: `Rating_Arranged_Narrower_Than_Its_Natural_Row_Still_Reaches_Both_Ends`,
`Simple_Dialog_Selecting_An_Item_Does_Not_Also_Report_A_Dismissal`,
`Expanded_Search_Reopens_When_The_Header_Is_Typed_In_Again`.

`Rating_Hit_Test_Quantizes_Inside_Each_Star_And_Ignores_Spacing` was corrected: it used a 260 DIP
window for a row needing 288, so it pinned the overflow behaviour that caused #24.

19, 15 and 10 were fixed after that table was written.

**19 selection rendering** turned out to be four faults in one slot of `MdFormField`. The
supporting presenter had no visibility binding, so a field with nothing to say still reserved a
row; neither the supporting nor the error line was indented, while `MdTextBox` and `MdComboBox`
both indent to 16 DIP through `Md.Comp.TextField.Supporting.Row.Margin`; error and supporting
showed at the same time, where M3 has the error replace the supporting text; and
`MdDropdownFormField` added its own pair on top of the two `MdComboBox` already draws, so the
dropdown showed the supporting line twice, one copy unindented. The dropdown now forwards
`SupportingText` and `ErrorText` into the combo box that was already rendering them correctly.

**15 AboutDialog auto-fill** — the dialog made the application restate its own name, version and
copyright, all of which the entry assembly declares. Unset properties now fall back to
`AssemblyTitle`/`AssemblyProduct`, the informational version with SourceLink's `+sha` stripped,
and `AssemblyCopyright`, read through new `EffectiveApplicationName`/`EffectiveApplicationVersion`/
`EffectiveLegalese` properties so an explicit value is never overwritten. Absent icon, version and
legalese rows collapse.

**10 tabs have no content host** — correct as far as it went: `MdTabs` is a bar and `MdTabItem`'s
`Content` is its label, so pages had nowhere to go without hand-wiring `SelectionChanged`. Rather
than fold content into the bar, which Material places in app bars away from the pages, the missing
half is now `MdTabsView`: children are the pages, matched to the bar by position, with selection
synchronised both ways. Same split as Flutter's `TabBar`/`TabBarView`.

**13 `MdDataGrid` is not a drop-in for `DataGrid`** — true, and growing it into one was the
wrong answer: Avalonia already ships that control, and a rename would not have changed what
`MdDataGrid` can do. The decision taken was to theme the real thing. `Md3.Avalonia.DataGrid` is
a new opt-in package carrying the `Avalonia.Controls.DataGrid` dependency alone, so the other
four packages stay clean, and `MdDataGrid` keeps its job as a Material data table. The gallery
now shows both, one above the other, with the boundary written between them.

The theme is derived from the DataGrid package's own Fluent theme rather than hand-written,
because the control finds its parts by name and a fresh template would have drifted. Two things
had to change beyond colour: `MaterialTheme` is standalone and the app loads no `FluentTheme`,
so every `System*` brush and the Fluent-derived cell editor theme would have failed to resolve
and rendered an invisible grid.

### 12 · one page per component — batch 1 of the incremental split

`FlutterParityGalleryPage` carried eighteen unrelated components in one 305-line scroll, which is
the worst instance of the problem: nothing on it could be found, linked to, or reviewed on its own.
The eight most loosely coupled sections now have their own pages, nine in total because
`MdReorderableList` and `MdGridTile` were sharing a single two-column section:

Banner · Expansion panels · Data table · Paginated table · Stepper · Pull to refresh ·
Reorderable list · Grid tiles · Dismissible

Each one is a normal gallery page — h1, a description, the live demo, and a usage snippet checked
against the real API rather than copied from the old page. Four of the snippets were wrong before
they were checked: the data table one referenced an `MdDataColumn` type that does not exist, and
the stepper one used `StepChanged` and `Title` instead of `ActiveStepChanged` and `Header`.

Splitting also surfaced a real cross-section bug: `DismissibleDismissed` wrote its status into
`DialogStatus`, the status line of the *simple dialog* section several components further down,
so dismissing the row appeared to do nothing. Each page now owns its own status line.

The page lost its dev-era "Phase 3 ·" / "Phase 4 ·" heading prefixes and the stale
`MinHeight="5200"` that sized it for content it no longer held.

One pre-existing gap turned up next door: `NumericNav` was in the gallery index but missing from
`_navigationButtons`, so its selected state was never cleared when navigating away. Added.

### 12 · batch 2 — the parity page is gone

The six sections left behind shared `ActiveDialogHost`, `_simpleDialog`, `_licenses` and the hero
state, which is why they moved together rather than piecemeal. Eight more pages, because two of
those sections were again two demos sharing one container:

Form validation · Simple dialog · About and licenses · Picker restoration ·
Draggable sheet · Keyboard avoidance · Adaptive controls · Focus, shortcut and Hero

With nothing left on it, `FlutterParityGalleryPage` was deleted rather than kept as an empty
shell — seventeen components now answer for themselves, which was the point of #12. Its trailing
usage block was by then describing expansion panels, refresh indicators, steppers and data
tables, none of which had been on the page since batch 1.

The two dialog pages keep a local `MdDialogHost` for standalone preview and prefer the shell's
host when running inside the gallery, the arrangement the parity page used.

### 12 · batch 3 — the ecosystem page is gone

`EcosystemGalleryPage` was the other dumping ground: eight "Wave A…F" sections, twenty-eight
controls, one 4400-DIP scroll, and headings named after the development phase that produced them
rather than after anything a reader is looking for. It is now twenty-eight pages:

Popover · Hover cards · Command palette · Density · Slidable item · Data grids · Masonry panel ·
Paged items · Async select · Calendar · Timeline · Cascader · Transfer · Result view · Chart ·
Rich editor · Chat view · Before and after · Animated text · Spin kit · Staggered panel ·
Skeleton · Animation sequence · PIN input · Tree view · Tag input · Avatar · Rating

Two of those are deliberate judgement calls rather than a mechanical one-section-one-page split:

- **`MdDataGrid` and the stock `DataGrid` stay on one page.** Defect #13 put them side by side on
  purpose, with the "when you need the full spreadsheet control" paragraph between them. Splitting
  them would have deleted the only place that says which of the two to reach for.
- **`MdPopover` and `MdHoverCard` are separate pages**, even though they shared a `WrapPanel` and
  one status line. They are different controls with different dismissal rules, and batch 1 already
  set the precedent by separating `MdReorderableList` from `MdGridTile`.

Splitting exposed the same class of cross-section coupling as batch 1, in three places:

- The command palette's three commands drove the **skeleton, the animation sequence and the paged
  items view** — three sections elsewhere on the page. A command palette that can only be
  demonstrated by scrolling to the thing it secretly moved is not a demonstration. Its page now
  owns an `MdSkeleton` and three commands that act on its own state.
- `OverlayStatus` was shared by the popover, the hover card *and* the command palette, so the last
  thing you touched overwrote the report of the other two. Each page now has its own status line.
- `CalendarStatus` sat under a `WrapPanel` holding both the calendar and the timeline, implying
  the timeline wrote to it. It never did — the timeline is static and has no handler at all.

`PagedRecord` and `GridRow`, both private to the old code-behind, moved with their sections
(`GridRow` to the data-grid page, where both grids use it). `_messages`, `_editorAdapter`, `_tags`
and `_sortAscending` likewise went with chat, rich editor, tag input and the grid.

The trailing usage block was a single snippet covering seven unrelated controls; each page now
carries the snippet for its own control, and the page-level `MinHeight="4400"` that sized the old
scroll is gone. `EcosystemGalleryPage` was deleted rather than left as an empty shell, which is
what #12 was asking for. `MdEcosystemWaveAndWindowTests` kept its render assertion by pointing at
`ChatViewGalleryPage`, the heaviest of the twenty-eight.

### 12 · batch 4 — the last two crowded pages, and what stays whole

`DesktopAdaptersGalleryPage` grouped four unrelated controls under "they are desktop-ish", and
`AdvancedSelectionGalleryPage` ("Segmented & range") grouped three. Seven more pages:

Autocomplete · Surfaces and type scale · Responsive content · Scrolling surface ·
Segmented buttons · Range slider · Date range picker

Single and multiple selection stayed on one page: they are the same `MdSegmentedButtonGroup` with
`AllowMultiple` flipped, so separating them would describe one control twice. The desktop-adapters
usage snippet was stale — it still showed `MdNumericBox`, which had already moved to its own page.

Two pages were examined and deliberately **not** split:

- **`MotionGalleryPage`** is a topic page, not a dumping ground. Spring schemes, container
  transform and animated visibility are three views of one subject — the M3 motion system — and
  the spring comparison only means anything next to the transitions it parameterises.
- **`ComponentsOverviewGalleryPage`** has no demos at all; it is the category index behind the
  "Components" tab. Splitting an index is meaningless.

#### The overview index had dead links

The overview links to pages *by title*, through `MainWindow.NavigateToIndexedPage`. Nothing checked
those titles, so the splits rotted it silently: **"Flutter parity" and "Flutter ecosystem" were
still listed after both pages had been deleted**, and clicking them did nothing at all. It was also
badly incomplete — 35 of what are now 93 pages.

It is now generated from the gallery index itself, grouped into nine categories (the six M3
categories plus Data, Motion and style, and Sample apps), with every indexed page appearing exactly
once. `MdGalleryIndexTests` makes the rot a build failure:

- every overview link resolves through `NavigateToIndexedPage`, and
- every indexed page is linked from the overview, so a new page cannot be added without appearing
  there.

One more pre-existing gap closed on the way: `NumericGalleryPage` was in the desktop index but
missing from the Android single-view shell, so numeric input was unreachable on Android.

#### Where #12 ended up

| Batch | Page removed | Pages created |
|---|---|---|
| 1 | — (`FlutterParityGalleryPage` thinned) | 9 |
| 2 | `FlutterParityGalleryPage` | 8 |
| 3 | `EcosystemGalleryPage` | 28 |
| 4 | `DesktopAdaptersGalleryPage`, `AdvancedSelectionGalleryPage` | 7 |

Fifty-two components that could only be reached by scrolling a shared page now have their own
page, nav entry, search keywords and usage snippet. The gallery went from 48 pages to 97.

## Checked and found correct — no change made

| # | Item | Evidence |
|---|------|----------|
| 8 | Slider's water-drop is dark in light mode | This is correct M3. material-components-android's `Slider.md` lists the value label's style as `@style/Widget.Material3.Tooltip` and notes "The value label is a Tooltip"; independently spec-sourced M3 implementations record the value indicator as `inverseSurface` / `inverseOnSurface`. Dark in light mode is the intent, exactly like a tooltip or snackbar. |
| 28b | Numeric `+`/`−` pair sits too low | **I changed this and was wrong.** The hand-tuned 15 DIP top margin looked like 8 too much, since the input row is 56 and the panel 42 (centring at 7). The regression test reported a button origin of −1 relative to `PART_Container`, which proves the container starts 8 DIP down, under the label: 15 = 8 + 7, already centred. Reverted, with the arithmetic now written into the template. |

## Diagnosed, not yet fixed

| # | Item | Finding |
|---|------|---------|
| 9 | Date / time / range pickers don't apply the selection | The modal rollback is **deliberate and covered by a test** (`Modal_Pickers_Roll_Back_Provisional_Values_When_Dismissed`): closing a modal picker without pressing OK restores the value held at open. Docked mode commits immediately. Needs a precise repro — which picker, which mode, and whether OK was pressed — before changing tested behaviour. The picker gallery page also shows no bound value, so the outcome is invisible either way. |
| 26 | Borderless window corners missing | Depends on OS-level window shaping (transparency hints, DWM rounded corners, the platform adapters) — not observable or testable headless. Needs a desktop run to diagnose rather than a guess. |

## Not yet investigated

21 rich text (adapter-based) · 22 image compare (adapter-based)

## P0 mobile platform gaps

Not gallery defects — these came out of the component audit and block Android use outright.

| # | Gap | Resolution | Commit |
|---|-----|-----------|--------|
| P0-1 | 23 `Key.Escape` dismissals across 18 files, and nothing listening to `TopLevel.BackRequested`. On Android the system back button and the predictive back gesture could not close a single modal surface; back popped the activity instead. | `MdBackNavigation` keeps a per-`TopLevel` handler stack (`ConditionalWeakTable`, so nothing is kept alive) and offers a back request to the most recently registered surface first, marking the routed event handled once one consumes it. `MdBackScope` registers a surface only while it is both open and attached. Wired into `MdDialogHost`, `MdSheetHost`, `MdNavigationDrawer`, `MdSearchView`, `MdMenuAnchor`, `MdFabMenu`, `MdDatePicker`, `MdTimePicker` — each dismissing exactly where it dismisses on Escape, so a non-modal sheet still lets the request fall through. | `26eb03d` |
| P0-2 | No use of `IInsetsManager` anywhere. Avalonia's automatic root padding is all-or-nothing, so a top app bar could not paint behind the status bar and a bottom bar could not paint behind the gesture handle. | `MdSafeArea` insets per edge, with a `MinimumPadding` floor and a settable `SafeAreaPadding` so a layout can be previewed and tested without a device. Edge-to-edge is `TopLevel.AutoSafeAreaPadding="False"` plus this control on the parts that must stay clear. | `26eb03d` |

Both are covered by `tests/Md3.Avalonia.HeadlessTests/MdMobilePlatformTests.cs` (22 tests). Gesture
animation on a real device and cutout geometry still need manual sign-off.

### Found while doing this

- **A modal surface opened before it is attached deadlocks the UI thread.** With `IsOpen = true`
  set in an object initializer, `MdModalFocusController` arms a focus redirect that re-posts
  itself, and `Dispatcher.RunJobs()` never drains — the headless suite ran 45 minutes against a
  1m48s baseline instead of failing. Every existing test happens to open these surfaces after the
  window is shown, which is why it had never been hit. Worth fixing in the controller: the
  redirect should give up rather than re-arm when the scope cannot take focus.
- **CI had no job timeouts**, so that deadlock would have held a runner for GitHub's six-hour
  default. Both jobs are now bounded (`timeout-minutes: 8` / `10`).

## API gaps found while fixing

`MdPagedItemsView` gained `ItemTemplate`; without it a consumer could not style loaded rows at
all. That is the same shape of problem as #13 — these controls expose too little to be used for
real. Worth a sweep of the Extra controls for missing template/presentation hooks.

## Agreed approach

- **#12 page split**: incremental — pull each component onto its own page as it gets fixed, rather
  than one big-bang restructure. `FlutterParity` is fully split (seventeen pages over two
  batches) and the page is gone. Still crowded, in descending order: `Ecosystem` (9 sections),
  `ComponentsOverview` (7), `DesktopAdapters` (5), `AdvancedSelection` (5), `Motion` (4).
- **#21 / #22**: adapter-based. No third-party dependency in the library; ship the Material UI and
  an adapter seam, and let the gallery demo one implementation. `MdRichEditor` already has one.
