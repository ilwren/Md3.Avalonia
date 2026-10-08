# Compatibility and support policy

## Supported baseline

| Area | Baseline | Status |
|---|---|---|
| .NET — library packages | `net8.0` and `net10.0` | Five of the six packages ship both `lib/net8.0` and `lib/net10.0` assets; `Md3.Avalonia.RichEditor` ships `net10.0` only |
| .NET — Gallery | `net10.0` | The desktop Gallery, its tray host, the Android single-view host and the CI/AOT harnesses build for .NET 10 |
| Avalonia | 12.1.2 | Build and headless-test baseline |
| Windows | x64/ARM64 hosts supported by Avalonia 12 | Library-compatible; device sign-off external |
| macOS | x64/Apple Silicon hosts supported by Avalonia 12 | Library-compatible; VoiceOver sign-off external |
| Linux | x64/ARM64 hosts supported by Avalonia 12 | Headless Linux tested; Orca sign-off external |
| Android | `net10.0-android`, API 23 host project; release target API 26+ | Source host supplied; emulator/device sign-off external |
| iOS/browser | Not a release target | No support commitment |

The library packages target `net8.0` and `net10.0`. The Gallery application is built for `net10.0`. No other target framework is offered.

Neither framework carries a special note: the same sources, the same design tokens and the same analyzer strictness (trim + AOT analyzers with warnings as errors) apply to both, so an application does not have to pick a framework to get a supported configuration. The single packaging exception is `Md3.Avalonia.RichEditor`: the `AvaloniaRichEditor` control library it themes ships `net10.0` only, so a `net8.0` asset there could not reference it. The other five packages are declared once in `Directory.Build.props` (`Md3LibraryTargetFrameworks`) and `scripts/verify-package-assets.py` fails the packaging job if a `.nupkg` does not carry exactly that asset set.

Building the `net8.0` assets requires the .NET 8 targeting pack, which restore pulls from NuGet when the local SDK does not have it; an offline build therefore needs it pre-seeded (see `docs/LOCAL_VERIFICATION.md`).

The core runtime library references only `Avalonia` and `MaterialColorUtilities`. It does not reference Avalonia Desktop, Win32, X11, macOS, or a desktop-only theme package. Two of the six packages add one dependency each, and neither is required by the others: `Md3.Avalonia.DataGrid` references `Avalonia.Controls.DataGrid` for the theme it ships, and `Md3.Avalonia.RichEditor` references `AvaloniaRichEditor`.

## Release channel

Every published version is a preview (`0.4.1-preview.1` and earlier). The preview channel is the only caveat: no version has been declared stable, so no version carries a stability guarantee beyond the compatibility surface below.

## Versioning

- Preview packages may adjust API after a changelog entry and migration note.
- Stable releases follow Semantic Versioning.
- Avalonia minor-version upgrades are validated by Release build, headless tests, package validation and the platform matrix before the supported baseline changes.
- The public `Md*` control types, styled properties, enums, theme JSON fields and resource-key names form the compatibility surface.

## Android boundary

`gallery/Md3.Avalonia.Gallery.Android` is deliberately kept outside the desktop solution so developers without the Android workload can build the library and desktop Gallery. Validate it with an installed .NET Android workload:

```bash
dotnet workload install android
dotnet build gallery/Md3.Avalonia.Gallery.Android -c Release
```

The host uses `ISingleViewApplicationLifetime` and `AndroidGalleryView`. Popup templates do not force `OverlayLayer`, preserving Avalonia's Android fallback behavior.

## External acceptance still required

The repository cannot substitute automation for ARM64 device behavior or assistive-technology review. Before a stable release, sign the checklist in `docs/RELEASE_VALIDATION.md` on physical or representative systems for Android touch/IME/rotation/lifecycle, Windows Narrator, macOS VoiceOver, and Linux Orca.
