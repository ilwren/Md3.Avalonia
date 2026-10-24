# Release validation record

Latest published package release: [`0.5.0-preview.1`](https://www.nuget.org/packages/Md3.Avalonia/0.5.0-preview.1) (18 packages; NuGet.org publication succeeded on 2026-10-19)
Current source package candidate: `0.5.0-preview.2` (not yet published; release validation is pending)
Historical automated baseline: `0.4.5-preview.1` (2026-10-12)

> The `0.4.5-preview.1` evidence below is historical. The published `0.5.0-preview.1` release workflow [passed](https://github.com/ilwren/Md3.Avalonia/actions/runs/39198020957), including package build/publish, headless tests, and Gallery builds for Linux, macOS, Windows, and Android. That run does not validate the `.2` source candidate or sign off the manual device, desktop, or assistive-technology checks listed below.

## Automated gates

| Gate | Command or evidence | Expected |
|---|---|---|
| Font provenance | `python3 scripts/verify-fonts.py` | Pinned complete Google variable TTF and real Lite subset pass SHA/table/glyph gates |
| Restore | `dotnet restore Md3.Avalonia.sln` | Success |
| Release compile | CI `Build Gallery Desktop` plus multi-platform Gallery workflow | 0 warnings, 0 errors for .NET 10 release projects |
| Tests | `dotnet test tests/Md3.Avalonia.HeadlessTests/Md3.Avalonia.HeadlessTests.csproj -c Release` | 0 failed; no unexplained skipped tests |
| Package | `bash scripts/build-nuget.sh` or `scripts/build-nuget.ps1` (all 18 packable projects; requires the Android workload) | 18 `.nupkg` and 18 `.snupkg`; consistent `0.5.0-preview.1` metadata, readmes, notices, XML docs, and one-way dependencies |
| Immediate artifact cleanup | after every build/test/pack, scan and remove `bin`, `obj`, `TestResults`, DLL/PDB and packages before continuing | No generated workspace artifacts |
| Dynamic color | golden seed vectors, arbitrary seed/variant, contrast pairs, JSON round-trip | Pass |
| Trim analysis | CI/package build with `EnableTrimAnalyzer` on all 18 `0.5.0-preview.1` packages | 0 IL2xxx warnings; every suppression carries a written justification |
| Trim escape hatches | `MdTrimmingTests` | Selector delegates on `MdDataGrid`, `MdAsyncSelect` and `MdSearchView` return without reflection; `IsTrimmable` present on shipped assemblies |
| Responsive Gallery | 599/600/839/840/1199/1200/1599/1600 boundary behavior | Pass |
| Visual directions/themes | LTR/RTL, Light/Dark, Standard/Medium/High | Render without exception |
| Virtualization | large `MdList` and `MdCarousel` data sets | Bounded realized containers |
| Lifecycle | repeated theme replacement and attach/detach | No resource-count/timer growth |

## Automated result for the 0.4.5-preview.1 baseline (historical)

Automated evidence for code commit [`258abb6`](https://github.com/ilwren/Md3.Avalonia/commit/258abb666c63df7cac051f6119bc53dc0dbd84e6):

- [CI run `38224349059`](https://github.com/ilwren/Md3.Avalonia/actions/runs/38224349059) passed static checks, Gallery/test builds and the full headless suite, including the interaction regressions below.
- [NuGet run `38224348930`](https://github.com/ilwren/Md3.Avalonia/actions/runs/38224348930) passed the six-package build, pack and artifact upload. It also verified embedded fonts.
- [Multi-platform Gallery run `38224348901`](https://github.com/ilwren/Md3.Avalonia/actions/runs/38224348901) passed Linux, Windows and macOS desktop publish plus Android APK build/upload. APK compilation is not physical-device sign-off.
- [Linux x64 AOT probe `38224348903`](https://github.com/ilwren/Md3.Avalonia/actions/runs/38224348903) passed native executable verification and xvfb smoke test. It reported `Binding(String)` trim/dynamic-code warnings in `gallery/Md3.Avalonia.Gallery/Pages/DataGridGalleryPage.axaml.cs:23–25`; this is not a zero-diagnostic result and does not sign off other platforms or consuming-app behavior.

## Automated validation for the published `0.5.0-preview.1` release

The release workflow completed on 2026-10-19. Its successful jobs include headless tests, packing and pushing the NuGet packages, and publishing Gallery artifacts for Linux, macOS, Windows, and Android:

- [x] All 18 NuGet packages built and were pushed to NuGet.org.
- [x] The release's headless test job passed.
- [x] Desktop Gallery publishing succeeded for Linux x64, macOS x64, and Windows x64; Android Gallery publishing also succeeded.

The successful package build validates the declared release targets. A separate consumer-level check remains useful before future target-framework changes:

- [ ] Inspect each `.nupkg` asset group and compile a minimal .NET 8 consumer against the seven packages that provide `net8.0` assets. The 11 opt-in third-party theme packages target `net10.0`; verify those against a .NET 10 consumer.

## Pending release validation for `0.5.0-preview.2`

The repository projects now target `.2`, but no `0.5.0-preview.2` release run exists yet. After these changes are reviewed and integrated into `main`, run the release workflow and confirm the 18 package outputs, NuGet push, headless tests, and platform Gallery builds for `.2`. The successful `.1` run above is not evidence for the new package version.

## Interaction regression acceptance

Keep these scenarios in the headless suite; a generic focused-control drag or a layout-property assertion is not a substitute for the actual regression:

- `MdReportedIssuesTests.Carousel_Clicking_A_Collapsed_Card_Makes_It_The_Focal_Keyline`: the first click on a collapsed Carousel keyline focuses and expands that card.
- `MdDialogAndSnackbarHostTests.Gallery_Simple_Dialog_Scrim_Covers_The_Whole_Desktop_Window`: headless assertion that the SimpleDialog scrim spans the Gallery root, including the app bar and navigation rail. This does not prove that the black mask covers the full native desktop window.
- `MdMotionLifecycleTests.Rich_Tooltip_Uses_Owner_Overlay_And_Remains_Open_While_Attended`: verify the tooltip popup actually uses the owner's overlay layer and shares its `TopLevel`, move onto the interactive action, then leave both trigger and tooltip to check the grace period. The root-cause fix targets native-popup owner deactivation; physical desktop verification remains pending.
- `MdDesktopPopupAndComparisonTests.Comparison_First_Arrange_Uses_Final_Size_For_Initial_Position`: before any pointer input or Position change, check that the default 0.5 handle is centered after the first arrange in both orientations.
- `MdComparisonDragTests.First_Gallery_Drag_From_Left_And_Top_Edges_Does_Not_Focus_Or_Scroll_During_Drag`: exercise the first inward-and-return drag from clipped left/top handles in both orientations, while checking the Gallery ScrollViewer stays fixed and restores its focus behavior. The focus/scroll guard in `22488b5` still failed on the reported Windows 11 / 150% setup; the initial-geometry correction now has headless coverage, but physical desktop confirmation remains pending.
- `MdAcceptanceRegressionTests.Numeric_Box_Mouse_Wheel_Spins_Only_When_Focused_And_Unfocused_Wheel_Scrolls_Page`: a focused numeric field may step; when it is not focused, the wheel must reach the outer page ScrollViewer. This is headless coverage only until the desktop event-routing behavior is confirmed.
- `MdRichEditorTreeVisualRegressionTests.RichEditor_Gallery_Starts_With_Rasterized_Sample_Content` and `TreeView_Chevron_Painted_Ink_Is_Centered_In_The_Transparent_State_Target`: verify actual rendered pixels and the chevron's rasterized-ink centroid inside its transparent state area.

Picker stale-field behavior and Cascader label/dropdown clipping have headless/CI coverage, but their actual desktop appearance still requires manual confirmation. The SimpleDialog black scrim must still be checked against the full native window, and the NumericBox wheel contract must still be checked with real desktop event routing. Do not infer desktop sign-off from any of these headless results.

## Manual platform matrix — not signed in this environment

Do not mark an item complete without recording device/OS, assistive technology version, tester and date. The System notifications item is a manual follow-up for the published `0.5.0-preview.1` Notifications package; successful package publication does not constitute platform-delivery sign-off.

- [ ] Windows x64: pointer, keyboard-only, 100/150/200% scale, Narrator. Recheck BorderlessWindow maximize/restore and `WindowDecorations.Full` state changes on Avalonia 12.1.2. Specifically recheck `MdBeforeAfter` at 150%: the latest report on `22488b5` showed Position 0.5 with the horizontal/vertical handle at the left/top and a first-drag jump; no desktop sign-off yet. Also verify the SimpleDialog black scrim covers the entire native window, and that `MdNumericBox` steps only while focused while an unfocused wheel scrolls the outer page.
- [ ] System notifications: verify the Windows 11 24H2 Gallery's first-run per-user shortcut/AUMID registration, toast delivery, and notification settings; macOS delivery/attribution; Linux X11 and Wayland using the direct `libnotify` backend; Android API 32 vs 33+ with permission denied/granted, channel settings, and app-specific small icon. This package still needs platform sign-off.
- [ ] Windows ARM64: launch, Gallery navigation, popup/picker/dialog smoke.
- [ ] macOS Apple Silicon: keyboard, trackpad, VoiceOver, Light/Dark/System.
- [ ] Linux x64: Wayland/X11 launch, keyboard, Orca, font fallback.
- [ ] Android ARM64 physical device on minimum API 23 and the current target API: touch targets, scroll, IME, password reveal, popup light-dismiss, back, rotation, resume, low-memory recreation.
- [ ] Android emulator compact/medium/expanded widths: theme switch and dynamic seed.
- [ ] High-contrast/forced-color platform settings where exposed by the OS.
- [ ] Localization review for English and Simplified Chinese, including picker culture behavior.

## Embedded official font gate

The repository contains the complete Google Material Symbols Rounded variable TTF and a real-outline Lite subset. `scripts/verify-fonts.py` pins upstream commit `737e3324305806514d7909874fa1818ae1808232`, the full-font SHA-256, the deterministic Lite SHA-256, required variable-font tables, and minimum glyph counts. CI and both packaging scripts run this check; missing, placeholder, or substituted fonts fail before compilation. Both icon packages embed the applicable font and Apache-2.0 license. No look-alike Unicode fallback is used.
