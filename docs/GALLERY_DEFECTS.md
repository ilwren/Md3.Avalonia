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
| 3 | Nav bar / rail: the pressed overlay has square corners over a rounded indicator | The ripple sat in a `Grid` with `ClipToBounds`, and a Grid clips to a **rectangle**. Now clipped by a `Border` carrying the pill's corner radius. Rail had the same template. | `0e30402` |
| 4 | Expanded search: after choosing a result, clearing and retyping never reopens the list | `MdSearchView` only hooked `KeyDown`; nothing watched the text. `CommitResult` writes the chosen text and dismisses, so only a host `Show()` could reopen it. Now reopens on header `TextChanged`, guarded by focus and by the committed text. | `6ec505a`, `bb36f45` |
| 6 | `FontSize` on a settings card / group / expander title does nothing | The templates wrote `FontSize="16"/"14"` **directly on the presenters**. A literal inside a template outranks any value from outside. Moved to `ControlTheme` setters + `{TemplateBinding FontSize}`; defaults unchanged, now overridable. | `0e30402` |
| 11 | Rich tooltip disappears before it can be reached (same on focus) | `HideDelay` is 100 ms for every variant. A rich tooltip is interactive, so the pointer must cross the gap to reach it; the timer expired mid-flight. Leaving now starts a variant-aware countdown with a 600 ms bridge for rich tooltips. | `ecc3d5f` |
| 14 | Simple dialog: clicking an item reports "cancelled" | Selection closed the dialog via `Dismiss()`, so one click raised **both** `ItemSelected` and `Dismissed`, and the cancel listener ran second and overwrote the result. A choice is no longer a dismissal. *(The styling half of this item is still open.)* | `ecc3d5f` |
| 23① | Tree expander's hover area is a small square, not a full circle | `PART_State` was inside a centred grid, so it was sized by the 16 DIP chevron, and it had no `CornerRadius`. It now fills the 40 DIP circle. | `ecc3d5f` |
| 23② | Expanding one node rotates other nodes' chevrons | `Rebuild()` replaces the whole flattened row list, so every container rebinds to a different row — and the 0.2 s transition on the chevron's `Angle` animated each one to its new value. Rotation is now instant. | `ecc3d5f` |
| 24①② | Rating is cut off **and** can only be lowered, never raised | One cause. The gallery pinned `Width="176"` while the natural row is `5 × 48 + 4 × 4 = 256`: trailing stars were clipped away, and the positions still reachable were mapped against the unclipped 256 scale, so every visible star reported a lower value than it drew. Render and hit testing now share one `GetMetrics(arrangedWidth)`. | `0e30402` |
| 25 | Redundant breadcrumb under the rating | Removed; `MdBreadcrumb` already has its own page. | `0e30402` |

Regression tests added: `Rating_Arranged_Narrower_Than_Its_Natural_Row_Still_Reaches_Both_Ends`,
`Simple_Dialog_Selecting_An_Item_Does_Not_Also_Report_A_Dismissal`,
`Expanded_Search_Reopens_When_The_Header_Is_Typed_In_Again`.

`Rating_Hit_Test_Quantizes_Inside_Each_Star_And_Ignores_Spacing` was corrected: it used a 260 DIP
window for a row needing 288, so it pinned the overflow behaviour that caused #24.

## Diagnosed, not yet fixed

| # | Item | Finding |
|---|------|---------|
| 9 | Date / time / range pickers don't apply the selection | The modal rollback is **deliberate and covered by a test** (`Modal_Pickers_Roll_Back_Provisional_Values_When_Dismissed`): closing a modal picker without pressing OK restores the value held at open. Docked mode commits immediately. Needs a precise repro — which picker, which mode, and whether OK was pressed — before changing tested behaviour. The picker gallery page also shows no bound value, so the outcome is invisible either way. |
| 10 | Tabs have no content host | `MdTabs`/`MdTabItem` are separate types from `MdTabView`/`MdTabViewItem`; only the latter pair carries content. |
| 13 | `MdDataGrid` is not a drop-in for `DataGrid` | It exposes 10 styled properties in total. The gap is real and needs an explicit API decision, not a rename. |

## Not yet investigated

1 breadcrumb overlay + no interaction · 2 carousel text layout · 5 usage button alignment ·
7 expander margins · 8 slider handle dark in light mode · 12 one page per parity component ·
15 AboutDialog auto-fill · 16 avatar first-frame glitch · 17 snackbar corners ·
18 load-more gives no visible change · 19 selection rendering · 20 dragged item hidden ·
21 rich text (adapter-based) · 22 image compare (adapter-based) · 26 borderless window corners ·
27 container transform / animated visibility don't animate · 28 numeric +/- centring and its own page

## Agreed approach

- **#12 page split**: incremental — pull each component onto its own page as it gets fixed, rather
  than one big-bang restructure.
- **#21 / #22**: adapter-based. No third-party dependency in the library; ship the Material UI and
  an adapter seam, and let the gallery demo one implementation. `MdRichEditor` already has one.
