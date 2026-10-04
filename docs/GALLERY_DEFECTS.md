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
| 13 | `MdDataGrid` is not a drop-in for `DataGrid` | It exposes 10 styled properties in total. The gap is real and needs an explicit API decision, not a rename. |

## Not yet investigated

12 one page per parity component · 21 rich text (adapter-based) · 22 image compare (adapter-based)

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
  than one big-bang restructure.
- **#21 / #22**: adapter-based. No third-party dependency in the library; ship the Material UI and
  an adapter seam, and let the gallery demo one implementation. `MdRichEditor` already has one.
