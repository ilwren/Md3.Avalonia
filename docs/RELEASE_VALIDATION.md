# Release validation record

Version: `0.4.5-preview.1`
Automated baseline date: 2026-10-12

## Automated gates

| Gate | Command or evidence | Expected |
|---|---|---|
| Font provenance | `python3 scripts/verify-fonts.py` | Pinned complete Google variable TTF and real Lite subset pass SHA/table/glyph gates |
| Restore | `dotnet restore Md3.Avalonia.sln` | Success |
| Release compile | CI `Build Gallery Desktop` plus multi-platform Gallery workflow | 0 warnings, 0 errors for .NET 10 release projects |
| Tests | `dotnet test tests/Md3.Avalonia.HeadlessTests/Md3.Avalonia.HeadlessTests.csproj -c Release` | 0 failed; no unexplained skipped tests |
| Package | `bash scripts/build-nuget.sh` or `scripts/build-nuget.ps1` for Core, Icons, Icons.Lite, Extra, DataGrid, RichEditor and Notifications (requires the Android workload) | Seven `.nupkg` and seven `.snupkg`; `0.4.5-preview.1` metadata/readme/notices/XML docs and one-way dependencies present |
| Immediate artifact cleanup | after every build/test/pack, scan and remove `bin`, `obj`, `TestResults`, DLL/PDB and packages before continuing | No generated workspace artifacts |
| Dynamic color | golden seed vectors, arbitrary seed/variant, contrast pairs, JSON round-trip | Pass |
| Trim analysis | CI/package build with `EnableTrimAnalyzer` on all seven packages | 0 IL2xxx warnings; every suppression carries a written justification |
| Trim escape hatches | `MdTrimmingTests` | Selector delegates on `MdDataGrid`, `MdAsyncSelect` and `MdSearchView` return without reflection; `IsTrimmable` present on shipped assemblies |
| Responsive Gallery | 599/600/839/840/1199/1200/1599/1600 boundary behavior | Pass |
| Visual directions/themes | LTR/RTL, Light/Dark, Standard/Medium/High | Render without exception |
| Virtualization | large `MdList` and `MdCarousel` data sets | Bounded realized containers |
| Lifecycle | repeated theme replacement and attach/detach | No resource-count/timer growth |

## Current automated result

Automated evidence for code commit [`258abb6`](https://github.com/ilwren/Md3.Avalonia/commit/258abb666c63df7cac051f6119bc53dc0dbd84e6):

- [CI run `38224349059`](https://github.com/ilwren/Md3.Avalonia/actions/runs/38224349059) passed static checks, Gallery/test builds and the full headless suite, including the interaction regressions below.
- [NuGet run `38224348930`](https://github.com/ilwren/Md3.Avalonia/actions/runs/38224348930) passed the six-package build, pack and artifact upload. It also verified embedded fonts.
- [Multi-platform Gallery run `38224348901`](https://github.com/ilwren/Md3.Avalonia/actions/runs/38224348901) passed Linux, Windows and macOS desktop publish plus Android APK build/upload. APK compilation is not physical-device sign-off.
- [Linux x64 AOT probe `38224348903`](https://github.com/ilwren/Md3.Avalonia/actions/runs/38224348903) passed native executable verification and xvfb smoke test. It reported `Binding(String)` trim/dynamic-code warnings in `gallery/Md3.Avalonia.Gallery/Pages/DataGridGalleryPage.axaml.cs:23–25`; this is not a zero-diagnostic result and does not sign off other platforms or consuming-app behavior.

## Interaction regression acceptance

Keep these scenarios in the headless suite; a generic focused-control drag or a layout-property assertion is not a substitute for the actual regression:

- `MdReportedIssuesTests.Carousel_Clicking_A_Collapsed_Card_Makes_It_The_Focal_Keyline`: the first click on a collapsed Carousel keyline focuses and expands that card.
- `MdDialogAndSnackbarHostTests.Gallery_Simple_Dialog_Scrim_Covers_The_Whole_Desktop_Window`: the SimpleDialog scrim spans the Gallery's entire desktop root, including the app bar and navigation rail.
- `MdMotionLifecycleTests.Rich_Tooltip_Uses_Owner_Overlay_And_Remains_Open_While_Attended`: verify the tooltip popup actually uses the owner's overlay layer and shares its `TopLevel`, move onto the interactive action, then leave both trigger and tooltip to check the grace period. The root-cause fix targets native-popup owner deactivation; physical desktop verification remains pending.
- `MdDesktopPopupAndComparisonTests.Comparison_First_Arrange_Uses_Final_Size_For_Initial_Position`: before any pointer input or Position change, check that the default 0.5 handle is centered after the first arrange in both orientations.
- `MdComparisonDragTests.First_Gallery_Drag_From_Left_And_Top_Edges_Does_Not_Focus_Or_Scroll_During_Drag`: exercise the first inward-and-return drag from clipped left/top handles in both orientations, while checking the Gallery ScrollViewer stays fixed and restores its focus behavior. The focus/scroll guard in `22488b5` still failed on the reported Windows 11 / 150% setup; the initial-geometry correction now has headless coverage, but physical desktop confirmation remains pending.
- `MdRichEditorTreeVisualRegressionTests.RichEditor_Gallery_Starts_With_Rasterized_Sample_Content` and `TreeView_Chevron_Painted_Ink_Is_Centered_In_The_Transparent_State_Target`: verify actual rendered pixels and the chevron's rasterized-ink centroid inside its transparent state area.

Picker stale-field behavior and Cascader label/dropdown clipping have headless/CI coverage, but their actual desktop appearance still requires manual confirmation. Do not infer desktop sign-off from the headless result.

## Manual platform matrix — not signed in this environment

Do not mark an item complete without recording device/OS, assistive technology version, tester and date.

- [ ] Windows x64: pointer, keyboard-only, 100/150/200% scale, Narrator. Specifically recheck `MdBeforeAfter` at 150%: the latest report on `22488b5` showed Position 0.5 with the horizontal/vertical handle at the left/top and a first-drag jump; no desktop sign-off yet.
- [ ] System notifications: verify registered-AUMID Windows toast; macOS delivery/attribution; Linux X11 and Wayland with `notify-send`; Android API 32 vs 33+ with permission denied/granted, channel settings, and app-specific small icon. This new package currently has no platform sign-off.
- [ ] Windows ARM64: launch, Gallery navigation, popup/picker/dialog smoke.
- [ ] macOS Apple Silicon: keyboard, trackpad, VoiceOver, Light/Dark/System.
- [ ] Linux x64: Wayland/X11 launch, keyboard, Orca, font fallback.
- [ ] Android ARM64 physical device API 26+: touch targets, scroll, IME, password reveal, popup light-dismiss, back, rotation, resume, low-memory recreation.
- [ ] Android emulator compact/medium/expanded widths: theme switch and dynamic seed.
- [ ] High-contrast/forced-color platform settings where exposed by the OS.
- [ ] Localization review for English and Simplified Chinese, including picker culture behavior.

## Embedded official font gate

The repository contains the complete Google Material Symbols Rounded variable TTF and a real-outline Lite subset. `scripts/verify-fonts.py` pins upstream commit `737e3324305806514d7909874fa1818ae1808232`, the full-font SHA-256, the deterministic Lite SHA-256, required variable-font tables, and minimum glyph counts. CI and both packaging scripts run this check; missing, placeholder, or substituted fonts fail before compilation. Both icon packages embed the applicable font and Apache-2.0 license. No look-alike Unicode fallback is used.
