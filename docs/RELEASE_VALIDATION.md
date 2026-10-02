# Release validation record

Version: `0.2.0`
Automated baseline date: 2026-09-26

## Automated gates

| Gate | Command or evidence | Expected |
|---|---|---|
| Restore | `dotnet restore Md3.Avalonia.sln -p:MaterialSymbolsRoundedFontFile=/offline/font.ttf` | Success |
| Release compile | `dotnet build Md3.Avalonia.sln -c Release --no-restore -p:MaterialSymbolsRoundedFontFile=/offline/font.ttf` | 0 warnings, 0 errors for `net8.0;net10.0` release libraries |
| Tests | run xUnit v3 test DLL directly | All pass |
| Package | independently pack `Md3.Avalonia`, `Md3.Avalonia.Icons`, and `Md3.Avalonia.Ecosystem` | `.nupkg` and `.snupkg`; metadata/readme/notices/XML docs and one-way dependencies present |
| Immediate artifact cleanup | after every build/test/pack, scan and remove `bin`, `obj`, `TestResults`, DLL/PDB and packages before continuing | No generated workspace artifacts |
| Dynamic color | golden seed vectors, arbitrary seed/variant, contrast pairs, JSON round-trip | Pass |
| Responsive Gallery | 599/600/839/840/1199/1200/1599/1600 boundary behavior | Pass |
| Visual directions/themes | LTR/RTL, Light/Dark, Standard/Medium/High | Render without exception |
| Virtualization | large `MdList` and `MdCarousel` data sets | Bounded realized containers |
| Lifecycle | repeated theme replacement and attach/detach | No resource-count/timer growth |

## Current automated result

- Release solution build with `MaterialSymbolsRoundedFontFile` supplied: **0 warnings / 0 errors**; release libraries compile for both `net8.0` and `net10.0`.
- xUnit v3 headless suite: **196 total / 196 passed / 0 failed / 0 skipped**.
- Earlier official Material Symbols font targeted acceptance + issue regressions + Gallery suite: **22 total / 22 passed / 0 failed / 0 skipped**.
- Independent package verification: Core, Icons and Ecosystem `.nupkg`/`.snupkg` contain both `lib/net8.0` and `lib/net10.0` assemblies, XML docs, README and notices. Icons embeds the build-supplied font resource; Ecosystem depends only on Core plus Avalonia. Temporary validation packages are deleted after inspection.
- Android workload: **not installed in this environment**; Android source host is therefore not claimed as compiled or device-validated.

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
