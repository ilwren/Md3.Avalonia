# Compatibility and support policy

## Supported baseline

| Area | Baseline | Status |
|---|---|---|
| .NET | .NET 8 and .NET 10 (`net8.0;net10.0`) | Both assets shipped by the three release packages |
| Avalonia | 12.1.2 | Build and headless-test baseline |
| Windows | x64/ARM64 hosts supported by Avalonia 12 | Library-compatible; device sign-off external |
| macOS | x64/Apple Silicon hosts supported by Avalonia 12 | Library-compatible; VoiceOver sign-off external |
| Linux | x64/ARM64 hosts supported by Avalonia 12 | Headless Linux tested; Orca sign-off external |
| Android | `net8.0-android`, API 21 host project; release target API 26+ | Source host supplied; emulator/device sign-off external |
| iOS/browser | Not a preview-release target | No support commitment |

The runtime library references only `Avalonia` and `MaterialColorUtilities`. It does not reference Avalonia Desktop, Win32, X11, macOS, or a desktop-only theme package.

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
