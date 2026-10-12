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
| Package | `bash scripts/build-nuget.sh` or `scripts/build-nuget.ps1` for Core, Icons, Icons.Lite, Extra, DataGrid and RichEditor | Six `.nupkg` and six `.snupkg`; `0.4.5-preview.1` metadata/readme/notices/XML docs and one-way dependencies present |
| Immediate artifact cleanup | after every build/test/pack, scan and remove `bin`, `obj`, `TestResults`, DLL/PDB and packages before continuing | No generated workspace artifacts |
| Dynamic color | golden seed vectors, arbitrary seed/variant, contrast pairs, JSON round-trip | Pass |
| Trim analysis | CI `Build tests` with `EnableTrimAnalyzer` on all six packages | 0 IL2xxx warnings; every suppression carries a written justification |
| Trim escape hatches | `MdTrimmingTests` | Selector delegates on `MdDataGrid`, `MdAsyncSelect` and `MdSearchView` return without reflection; `IsTrimmable` present on shipped assemblies |
| Responsive Gallery | 599/600/839/840/1199/1200/1599/1600 boundary behavior | Pass |
| Visual directions/themes | LTR/RTL, Light/Dark, Standard/Medium/High | Render without exception |
| Virtualization | large `MdList` and `MdCarousel` data sets | Bounded realized containers |
| Lifecycle | repeated theme replacement and attach/detach | No resource-count/timer growth |

## Current automated result

- GitHub CI builds the Desktop Gallery with warnings as errors, runs the complete xUnit v3 headless suite, emits named failure annotations, and verifies all NuGet packages.
- The multi-platform workflow publishes Linux, Windows and macOS Desktop Gallery outputs and builds the Android APK with the .NET Android workload.
- Package verification covers Core, Icons, Icons.Lite, Extra, DataGrid and RichEditor at `0.4.5-preview.1`, including `.nupkg`/`.snupkg`, XML docs, README/notices and dependency direction. Temporary validation packages are deleted after inspection.
- The 0.4.5 preview CI run must report **0 failed tests**; use its own GitHub Actions summary/TRX rather than copying historical test totals.
- The Gallery workflow builds Linux, Windows and macOS desktop outputs and compiles/uploads the Android APK. Physical-device interaction is **not** inferred from compilation and remains in the unsigned matrix below.
- The Linux x64 AOT probe must produce a native executable and pass its xvfb smoke run. It reports linker diagnostics and does not sign off Windows/macOS/Android AOT or consuming-app trim behavior.

## Interaction regression acceptance

Keep these scenarios in the headless suite; a generic focused-control drag or a layout-property assertion is not a substitute for the actual regression:

- `MdReportedIssuesTests.Carousel_Clicking_A_Collapsed_Card_Makes_It_The_Focal_Keyline`: the first click on a collapsed Carousel keyline focuses and expands that card.
- `MdDialogAndSnackbarHostTests.Gallery_Simple_Dialog_Scrim_Covers_The_Whole_Desktop_Window`: the SimpleDialog scrim spans the Gallery's entire desktop root, including the app bar and navigation rail.
- `MdMotionLifecycleTests.Rich_Tooltip_Does_Not_Start_Hide_Delay_While_The_Anchor_Is_Hovered`: rich-tooltip hover/focus transitions do not prematurely arm dismissal; leaving both sides starts the normal grace period.
- `MdComparisonDragTests.First_Gallery_Drag_From_An_Edge_Tracks_The_Pointer_Before_Focus_Has_Been_Established`: exercise the first drag before focus, with the Gallery ScrollViewer bringing the partly clipped control into view; desktop confirmation remains pending.
- `MdRichEditorTreeVisualRegressionTests.RichEditor_Gallery_Starts_With_Rasterized_Sample_Content` and `TreeView_Chevron_Painted_Ink_Is_Centered_In_The_Transparent_State_Target`: verify actual rendered pixels and the chevron's rasterized-ink centroid inside its transparent state area.

Picker stale-field behavior and Cascader label/dropdown clipping have headless/CI coverage, but their actual desktop appearance still requires manual confirmation. Do not infer desktop sign-off from the headless result.

## Manual platform matrix — not signed in this environment

Do not mark an item complete without recording device/OS, assistive technology version, tester and date.

- [ ] Windows x64: pointer, keyboard-only, 100/150/200% scale, Narrator.
- [ ] Windows ARM64: launch, Gallery navigation, popup/picker/dialog smoke.
- [ ] macOS Apple Silicon: keyboard, trackpad, VoiceOver, Light/Dark/System.
- [ ] Linux x64: Wayland/X11 launch, keyboard, Orca, font fallback.
- [ ] Android ARM64 physical device API 26+: touch targets, scroll, IME, password reveal, popup light-dismiss, back, rotation, resume, low-memory recreation.
- [ ] Android emulator compact/medium/expanded widths: theme switch and dynamic seed.
- [ ] High-contrast/forced-color platform settings where exposed by the OS.
- [ ] Localization review for English and Simplified Chinese, including picker culture behavior.

## Embedded official font gate

The repository contains the complete Google Material Symbols Rounded variable TTF and a real-outline Lite subset. `scripts/verify-fonts.py` pins upstream commit `737e3324305806514d7909874fa1818ae1808232`, the full-font SHA-256, the deterministic Lite SHA-256, required variable-font tables, and minimum glyph counts. CI and both packaging scripts run this check; missing, placeholder, or substituted fonts fail before compilation. Both icon packages embed the applicable font and Apache-2.0 license. No look-alike Unicode fallback is used.
