# Release validation record

Version: `0.3.0-preview.1`
Automated baseline date: 2026-10-03

## Automated gates

| Gate | Command or evidence | Expected |
|---|---|---|
| Restore | `dotnet restore Md3.Avalonia.sln -p:MaterialSymbolsRoundedFontFile=/offline/font.ttf` | Success |
| Release compile | CI `Build Gallery Desktop` plus multi-platform Gallery workflow | 0 warnings, 0 errors for .NET 10 release projects |
| Tests | `dotnet test tests/Md3.Avalonia.HeadlessTests/Md3.Avalonia.HeadlessTests.csproj -c Release` | 0 failed; no unexplained skipped tests |
| Package | `bash scripts/build-nuget.sh` for Core, Icons, Icons.Lite and Extra | `.nupkg` and `.snupkg`; `0.3.0-preview.1` metadata/readme/notices/XML docs and one-way dependencies present |
| Immediate artifact cleanup | after every build/test/pack, scan and remove `bin`, `obj`, `TestResults`, DLL/PDB and packages before continuing | No generated workspace artifacts |
| Dynamic color | golden seed vectors, arbitrary seed/variant, contrast pairs, JSON round-trip | Pass |
| Responsive Gallery | 599/600/839/840/1199/1200/1599/1600 boundary behavior | Pass |
| Visual directions/themes | LTR/RTL, Light/Dark, Standard/Medium/High | Render without exception |
| Virtualization | large `MdList` and `MdCarousel` data sets | Bounded realized containers |
| Lifecycle | repeated theme replacement and attach/detach | No resource-count/timer growth |

## Current automated result

- GitHub CI builds the Desktop Gallery with warnings as errors, runs the complete xUnit v3 headless suite, emits named failure annotations, and verifies all NuGet packages.
- The multi-platform workflow publishes Linux, Windows and macOS Desktop Gallery outputs and builds the Android APK with the .NET Android workload.
- Independent package verification checks Core, Icons, Icons.Lite and Extra `0.3.0-preview.1` `.nupkg`/`.snupkg`, XML docs, README/notices and one-way dependencies. Temporary validation packages are deleted after inspection.
- The final 0.3 preview run URL and exact test totals are recorded by GitHub Actions; release readiness requires **0 failed**. Do not copy historical 0.1 totals into this release record.
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

## Offline font gate

The Material Symbols Rounded binary is intentionally absent from the repository. `Md3.Avalonia.Icons` must be built with the official font copied to `Assets/Fonts/MaterialSymbolsRounded.ttf` or supplied through `MaterialSymbolsRoundedFontFile`; otherwise compilation fails by design. The resulting Icons assembly embeds that file as an Avalonia resource and still validates its internal family and control glyphs at runtime. The package publisher must verify the Apache-2.0 source and checksum. No look-alike Unicode fallback is used.
