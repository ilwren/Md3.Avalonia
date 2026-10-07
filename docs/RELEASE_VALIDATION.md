# Release validation record

Version: `0.4.1-preview.1`
Automated baseline date: 2026-10-05

## Automated gates

| Gate | Command or evidence | Expected |
|---|---|---|
| Font provenance | `python3 scripts/verify-fonts.py` | Pinned complete Google variable TTF and real Lite subset pass SHA/table/glyph gates |
| Restore | `dotnet restore Md3.Avalonia.sln` | Success |
| Release compile | CI `Build Gallery Desktop` plus multi-platform Gallery workflow | 0 warnings, 0 errors for .NET 10 release projects |
| Tests | `dotnet test tests/Md3.Avalonia.HeadlessTests/Md3.Avalonia.HeadlessTests.csproj -c Release` | 0 failed; no unexplained skipped tests |
| Package | `bash scripts/build-nuget.sh` for Core, Icons, Icons.Lite, Extra, DataGrid and RichEditor | `.nupkg` and `.snupkg`; `0.4.1-preview.1` metadata/readme/notices/XML docs and one-way dependencies present |
| Immediate artifact cleanup | after every build/test/pack, scan and remove `bin`, `obj`, `TestResults`, DLL/PDB and packages before continuing | No generated workspace artifacts |
| Dynamic color | golden seed vectors, arbitrary seed/variant, contrast pairs, JSON round-trip | Pass |
| Trim analysis | CI `Build tests` with `EnableTrimAnalyzer` on all six packages | 0 IL2xxx warnings; every suppression carries a written justification |
| Native AOT | `.github/workflows/aot-probe.yml` publishes the desktop Gallery with `PublishAot=true` | Native ELF with no managed assembly beside it, zero ILxxxx diagnostics, and the binary still running after a 25 s Xvfb smoke test |
| Trim escape hatches | `MdTrimmingTests` | Selector delegates on `MdDataGrid`, `MdAsyncSelect` and `MdSearchView` return without reflection; `IsTrimmable` present on shipped assemblies |
| Responsive Gallery | 599/600/839/840/1199/1200/1599/1600 boundary behavior | Pass |
| Visual directions/themes | LTR/RTL, Light/Dark, Standard/Medium/High | Render without exception |
| Virtualization | large `MdList` and `MdCarousel` data sets | Bounded realized containers |
| Lifecycle | repeated theme replacement and attach/detach | No resource-count/timer growth |

## Current automated result

- GitHub CI builds the Desktop Gallery with warnings as errors, runs the complete xUnit v3 headless suite, emits named failure annotations, and verifies all NuGet packages.
- The multi-platform workflow publishes Linux, Windows and macOS Desktop Gallery outputs and builds the Android APK with the .NET Android workload.
- Independent package verification checks the six `0.4.1-preview.1` packages (Core, Icons, Icons.Lite, Extra, DataGrid, RichEditor) `.nupkg`/`.snupkg`, XML docs, README/notices and one-way dependencies. Temporary validation packages are deleted after inspection.
- The final 0.4.1 preview run URL and exact test totals are recorded by GitHub Actions; release readiness requires **0 failed**. Do not copy historical 0.1 totals into this release record.
- Android source/APK compilation is automated. Physical-device interaction is **not** inferred from compilation and remains in the unsigned matrix below.

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
