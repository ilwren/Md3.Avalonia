# Md3.Avalonia

[English](README.md) | [简体中文](README.zh-CN.md)

A Material Design 3 control library and component gallery for Avalonia 11.

https://github.com/user-attachments/assets/53c05bc6-f1b5-4076-8b62-c4a7a2b819bf

[![CI](https://github.com/ilwren/Md3.Avalonia/actions/workflows/ci.yml/badge.svg)](https://github.com/ilwren/Md3.Avalonia/actions/workflows/ci.yml)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg)](LICENSE)

- 352 public API types for buttons, inputs, navigation, dialogs, charts, and motion; indexed in the API reference and checked by a script gate.

- HCT dynamic color: any seed color, 6 schemes, 3 contrast levels, and 49 color roles.

- Bundles the official Material Symbols Rounded font; checksum and glyph count are verified at build time.

- 17 independently installable NuGet packages; third-party integration themes are opt-in.

- Each control has its own CLR type and scoped `ControlTheme`, without replacing native Avalonia controls.

- The core package does not depend on `Avalonia.Themes.Fluent` or `Avalonia.Themes.Simple`.

- Desktop and Android targets; every package declares `IsTrimmable` and `IsAotCompatible`.

> **Current preview: [`0.5.0-preview.2`](https://www.nuget.org/packages/Md3.Avalonia11/0.5.0-preview.2).** The install examples below use this version. Preview APIs may still receive compatibility changes. See [Known limitations](#known-limitations) for manual screen-reader and Android-device validation status.
>

## Contents

- [Requirements](#requirements)
- [Install](#install)
- [Quick start](#quick-start)
- [NuGet packages](#nuget-packages)
- [Theming](#theming)
- [Controls overview](#controls-overview)
- [Repository layout](#repository-layout)
- [Build and test](#build-and-test)
- [Documentation](#documentation)
- [Known limitations](#known-limitations)
- [License](#license)

## Requirements

| Item | Requirement |
|---|---|
| Avalonia | `11.3.13` (minimum version is determined by the dependency floor of third-party integrations; see [`docs/COMPATIBILITY.md`](docs/COMPATIBILITY.md)) |
| Library target frameworks | Core family: `net8.0;net10.0`; third-party theme packages: `net10.0` (see [NuGet packages](#nuget-packages)) |
| Gallery and tests | .NET 10 |
| Android | Minimum API 23; host target `net10.0-android` |
| Packaging scripts | .NET 10 SDK, Java 17, and `dotnet workload install android` |

## Install

Install the core package

```bash
dotnet add package Md3.Avalonia11 --version 0.5.0-preview.2
```

Add optional packages as needed; all packages use the same version

```bash
dotnet add package Md3.Avalonia11.Icons --version 0.5.0-preview.2
```

## Quick start

Register the theme

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:themes="using:Md3.Avalonia.Themes"
             xmlns:extra="using:Md3.Avalonia.Extra.Themes">
  <Application.Styles>
    <themes:MaterialTheme />
    <!-- Add when using Md3.Avalonia11.Extra: -->
    <extra:ExtraTheme />
  </Application.Styles>
</Application>
```

Use the controls

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:md="using:Md3.Avalonia.Controls">
  <StackPanel Spacing="16" Margin="24">
    <md:MdButton Content="Continue" Variant="Filled" Command="{Binding ContinueCommand}" />
    <md:MdTextBox Label="Project name" Text="{Binding ProjectName}" MaxLength="40" />
    <md:MdComboBox Label="Platform" Variant="Outlined"
                   ItemsSource="{Binding Platforms}" SelectedItem="{Binding Platform}" />
  </StackPanel>
</Window>
```

All bindable APIs are Avalonia properties and work directly with CommunityToolkit.MVVM; controls can also be constructed and used from code.

| Material 3 actions | Material 3 components |
|---|---|
| ![Action-button preview](docs/MdActionButtonsPreview.png) | ![Component preview](docs/MdNewComponentsPreview.png) |

## NuGet packages

All 17 packages share one version and can be installed independently; unused packages are not pulled in.

| Package | Capability | Theme registration | Target frameworks |
|---|---|---|---|
| `Md3.Avalonia11` | Core Material 3 controls and design tokens | `MaterialTheme` | `net8.0;net10.0` |
| `Md3.Avalonia11.Icons` | Complete Material Symbols Rounded font and icon catalog | Auto-discovered | `net8.0;net10.0` |
| `Md3.Avalonia11.Icons.Lite` | Reduced icon subset; choose this or the full Icons package | Auto-discovered | `net8.0;net10.0` |
| `Md3.Avalonia11.Extra` | Flutter-inspired extension controls | `ExtraTheme` | `net8.0;net10.0` |
| `Md3.Avalonia11.DataGrid` | Theme for Avalonia's native `DataGrid` | `MaterialDataGridTheme` | `net8.0;net10.0` |
| `Md3.Avalonia11.Notifications` | Native system notifications for Windows, macOS, Linux, and Android | None (independent of UI themes) | `net8.0;net10.0` + Windows-specific |
| `Md3.Avalonia11.AvaloniaEdit` | AvaloniaEdit editor theme | `MaterialAvaloniaEditTheme` | `net10.0` |
| `Md3.Avalonia11.AvaloniaHex` | AvaloniaHex hex-editor theme | `MaterialAvaloniaHexTheme` | `net10.0` |
| `Md3.Avalonia11.Dock` | Dock layout theme | `MaterialDockTheme` | `net10.0` |
| `Md3.Avalonia11.Tabalonia` | Draggable Tabalonia tab theme | `MaterialTabaloniaTheme` | `net10.0` |
| `Md3.Avalonia11.Markdown` | Markdown.Avalonia document styling | Assign to `MarkdownScrollViewer.MarkdownStyle` | `net10.0` |
| `Md3.Avalonia11.LiveCharts2` | LiveCharts2 chart theme | Configure once with `LiveCharts.Configure` | `net10.0` |
| `Md3.Avalonia11.ScottPlot` | ScottPlot chart theme | Plot-level `MaterialScottPlotTheme` | `net10.0` |
| `Md3.Avalonia11.PropertyGrid` | Avalonia.PropertyGrid property-grid theme | `MaterialPropertyGridTheme` | `net10.0` |
| `Md3.Avalonia11.NodifyM` | NodifyM node-editor theme | `MaterialNodifyMTheme` | `net10.0` |
| `Md3.Avalonia11.Mapsui` | Mapsui map theme | Map-level `MaterialMapsuiTheme` | `net10.0` |
| `Md3.Avalonia11.InkCanvas` | Handwriting-canvas theme and `MdSignaturePad` | Control-level `MaterialInkCanvasTheme` | `net10.0` |

Each third-party theme package is the **only** package that brings in its upstream dependency, keeping those dependencies out of the core package. The `Markdown`, `LiveCharts2`, `ScottPlot`, `Mapsui`, and `InkCanvas` packages do not register in `Application.Styles`: Markdown styles are assigned to the document control, while the others expose object-level entry points.

Design principles

- Each interactive control has its own `Md*` CLR type and scoped `ControlTheme`, while reusing Avalonia's native behavior, binding, keyboard, and selection models where possible.

- Consume color, typography, shape, state layers, elevation, and motion through tokens and `DynamicResource`.

- If a font or icon provider is missing or fails validation, keep the icon hidden instead of substituting a similar-looking glyph.

## Theming

Light, Dark, and System modes all consume theme tokens through `DynamicResource`. Apply any seed color at startup or at runtime:

```csharp
var options = new MdThemeOptions
{
    SeedColor = "#006A6A",
    SchemeVariant = MdThemeSchemeVariant.Expressive,
    ContrastLevel = MdThemeContrastLevel.High,
    ThemeMode = MdThemeMode.System
};
var dark = Application.Current!.ActualThemeVariant == ThemeVariant.Dark;
MdThemeManager.Apply(Application.Current, options, dark);
var json = MdThemeJson.Serialize(options);
```

The `MaterialColorUtilities` HCT generator supports six schemes (TonalSpot, Neutral, Vibrant, Expressive, Monochrome, Fidelity), three contrast levels (Standard, Medium, High), and 49 color roles. It also provides System and Component token layers, contrast diagnostics, and theme JSON round-tripping.

### Embedded official Material Symbols font

`Md3.Avalonia11.Icons` ships the complete Google **Material Symbols Rounded** variable TTF. `Md3.Avalonia11.Icons.Lite` ships a genuine outline subset generated from the same file; consumers do not need to download or register a font manually.

The source is pinned to commit `737e3324305806514d7909874fa1818ae1808232` of `google/material-design-icons`. The complete TTF has SHA-256 `95b24392bb49efd1bc3e92cff4e2452ad094461bab7c97e7d8723fab97e330ca`. Both local builds and CI run:

```bash
python3 scripts/verify-fonts.py
```

The verifier checks checksums, variable-font tables, and glyph counts for the full font and Lite subset. It rejects missing, placeholder, or substituted binaries. After referencing Core and Icons, there is no need to call `ConfigureFonts` manually: the provider is discovered and registered automatically, and symbol glyphs remain hidden if validation fails.

## Controls overview

The online [DocFX API reference](https://ilwren.github.io/Md3.Avalonia/) is generated from the C# XML comments in all 17 packages. [`docs/API.md`](docs/API.md) describes package coverage and documentation sources. `scripts/check-api-doc-coverage.py` scans all 352 public types; the table lists representative types in each group.

| Group | Representative types |
|---|---|
| Buttons | `MdButton`, `MdToggleButton`, `MdIconButton`, `MdToggleIconButton`, `MdSplitButton`, `MdStandardButtonGroup`, `MdConnectedButtonGroup`, `MdFloatingActionButton`, `MdExtendedFloatingActionButton`, `MdFabMenu` |
| Inputs | `MdTextBox`, `MdCheckBox`, `MdRadioButton`, `MdComboBox`, `MdAutoCompleteBox`, `MdSlider`, `MdSwitch`, `MdDatePicker`, `MdTimePicker`, `MdPinInput`, `MdTagInput`, `MdForm` |
| Navigation | `MdNavigationDrawer`, `MdNavigationBar`, `MdTabs`, `MdToolbar`, `MdTopAppBar`, `MdBottomAppBar`, `MdSearchBar`, `MdSearchView`, `MdCommandPalette`, `MdBreadcrumb` |
| Containers and overlays | `MdCard`, `MdDialog`, `MdDialogHost`, `MdMenu`, `MdPopover`, `MdTooltip`, `MdSnackbar`, `MdBanner`, `MdSheetHost`, `MdExpansionPanelList`, `MdFloatingWindow`, `MdBorderlessWindow` |
| Data display | `MdList`, `MdDataTable`, `MdDataGrid`, `MdCarousel`, `MdCalendar`, `MdTreeView`, `MdAvatar`, `MdBadge`, `MdChip`, `MdTimeline`, `MdWaveform`, `MdChart` |
| Feedback and progress | `MdCircularProgressIndicator`, `MdLinearProgressIndicator`, `MdLoadingIndicator`, `MdSkeleton`, `MdRefreshIndicator`, `MdSpinKit`, `MdStateLayer`, `MdFocusRing` |
| Layout and motion | `MdSurface`, `MdAdaptiveLayout`, `MdScrollViewer`, `MdWaterfall`, `MdMasonryPanel`, `MdStaggeredPanel`, `MdAnimationSequence`, `MdAnimatedText`, `MdRevealHost`, `MdMorphPanel`, `MdHero` |

All Button, Icon Button, and FAB templates use `MdRipplePresenter`: ripples originate at the press point, clip to the full container shape, and support Expressive, Standard, Reduced, and None motion schemes. The Elevated `MdButton` variant is retained for compatibility but is deprecated in M3; prefer another variant for new interfaces.

`Md3.Avalonia11.Extra` is an optional clean-room extension package and does not claim to reproduce the full API of corresponding Flutter packages. For precise parity details, see [`docs/FLUTTER_PARITY_STATUS.md`](docs/FLUTTER_PARITY_STATUS.md); similar `Md*` type names alone do not imply full compatibility.

## Repository layout

```text
src/Md3.Avalonia/                  #  Core controls and design tokens; no specific icon-font dependency
├─ Controls/                       #  Md* controls, enums, and property APIs
├─ Motion/                         #  Motion schemes, spring tokens, and transitions
└─ Themes/
   ├─ MaterialTheme.axaml
   ├─ Tokens/                      #  11 token dictionaries
   └─ Controls/                    #  Scoped ControlTheme for each control
src/Md3.Avalonia.Icons/            #  Icon provider with the complete official TTF embedded
src/Md3.Avalonia.Extra/            #  Flutter-inspired extension controls
src/Md3.Avalonia.<Vendor>/         #  11 opt-in third-party theme packages, each the sole upstream dependency entry point
src/Md3.Avalonia.Notifications/    #  Native system notifications, independent of Avalonia UI

gallery/Md3.Avalonia.Gallery/          #  Desktop Gallery: 108 component pages + Theme Lab
gallery/Md3.Avalonia.Gallery.Android/  #  net10.0-android single-view host
tests/Md3.Avalonia.HeadlessTests/      #  API, input, theme, regression, and rendering tests
tests/.../Spec/                        # Layered spec checks (L1 tokens / L2 geometry / L3 golden images / L5 motion)
docs/                                  #  API, compatibility, publishing, and validation docs
spec-snapshot/                         #  External-source expected-value oracle and ratchet policy
```

See [`docs/SPEC_VERIFICATION.md`](docs/SPEC_VERIFICATION.md) for the responsibilities of each spec-verification layer, the ratchet process, and known gaps.

## Build and test

```bash
python3 scripts/verify-fonts.py     #  Verify embedded fonts
bash scripts/build-nuget.sh         # Build all 17 .nupkg / .snupkg packages

dotnet build gallery/Md3.Avalonia.Gallery.Desktop/Md3.Avalonia.Gallery.Desktop.csproj -c Release
dotnet test tests/Md3.Avalonia.HeadlessTests/Md3.Avalonia.HeadlessTests.csproj -c Release
```

Install the Android workload before building the full solution:

```bash
dotnet workload install android
dotnet build Md3.Avalonia.sln -c Release
```

The following static checks do not require the .NET SDK:

```bash
python3 scripts/lint-design-tokens.py       #  Lint design tokens
python3 scripts/check-api-doc-coverage.py   #  Check DocFX projects, XML output, and bilingual interface docs
python3 scripts/check-gallery-localization.py --strict
```

Publish the desktop Gallery with native AOT (Linux requires clang/zlib; `--smoke` also requires xvfb):

```bash
bash scripts/publish-gallery-aot.sh --rid linux-x64 --smoke   # Other supported RIDs: win-x64 / osx-x64 / osx-arm64
```

The script verifies that the output is a native executable with no managed DLL beside it. `--smoke` runs the binary under xvfb to check that trimming has not removed runtime reflection metadata. `.github/workflows/aot-probe.yml` runs the same script on the main and arena branches.

The release gate builds all 17 packages and verifies 17 `.nupkg` files, 17 `.snupkg` files, a unified version, and the embedded official font. Releases use NuGet Trusted Publishing; see [`docs/PUBLISHING.md`](docs/PUBLISHING.md) for setup.

Clean build artifacts immediately after every build, test, or pack operation. Do not ship `bin/`, `obj/`, `TestResults/`, DLLs, PDBs, NuGet packages, APKs, or AABs in the repository; `docs/*.png` are retained as documentation reference images.

## Documentation

| Document | Description |
|---|---|
| [`CHANGELOG.md`](CHANGELOG.md) | Release history |
| [Online DocFX API reference](https://ilwren.github.io/Md3.Avalonia/) · [`docs/API.md`](docs/API.md) | Public API pages generated from XML comments and package entry points |
| [`docs/AI_DISCLOSURE.md`](docs/AI_DISCLOSURE.md) | AI-generated and AI-assisted content disclosure and disclaimer |
| [`docs/GETTING_STARTED.md`](docs/GETTING_STARTED.md) | Quick start, theme registration, and first page |
| [`docs/USAGE_EXAMPLES.md`](docs/USAGE_EXAMPLES.md) | Form, icon, dynamic-color, waterfall, and dialog examples with images |
| [`docs/NUGET_PACKAGES.md`](docs/NUGET_PACKAGES.md) | 17 NuGet packages, target frameworks, and theme entry points |
| [`docs/PROJECT_STRUCTURE.md`](docs/PROJECT_STRUCTURE.md) | Repository layout, project responsibilities, and API-doc build flow |
| [`docs/COMPATIBILITY.md`](docs/COMPATIBILITY.md) | Target frameworks, platforms, and dependency compatibility policy |
| [`docs/RELEASE_NOTES_0.5.0-preview.1.md`](docs/RELEASE_NOTES_0.5.0-preview.1.md) | Changes in the published release |
| [`docs/RELEASE_NOTES_0.5.0-preview.2.md`](docs/RELEASE_NOTES_0.5.0-preview.2.md) | Changes in preview 0.5.0-preview.2 |
| [`docs/RELEASE_VALIDATION.md`](docs/RELEASE_VALIDATION.md) | Manual validation checklist and sign-off status |
| [`docs/PUBLISHING.md`](docs/PUBLISHING.md) | Release process and NuGet Trusted Publishing setup |
| [`docs/SPEC_VERIFICATION.md`](docs/SPEC_VERIFICATION.md) | Layered spec verification and known gaps |
| [`docs/FLUTTER_PARITY_STATUS.md`](docs/FLUTTER_PARITY_STATUS.md) | Flutter parity status for corresponding Extra controls |
| [`src/Md3.Avalonia.Notifications/README.md`](src/Md3.Avalonia.Notifications/README.md) | System-notification usage and platform prerequisites |

## Known limitations

- **Manual validation is not signed off.** Narrator, VoiceOver, Orca, and TalkBack must be tested by people with the corresponding OS and assistive technology, following the release checklist. A successful Android APK build is not device validation; ARM64, rotation, lifecycle, and TalkBack still require on-device sign-off. Results are tracked in [`docs/RELEASE_VALIDATION.md`](docs/RELEASE_VALIDATION.md).
- **Motion.** Ripple, spring tokens, and inherited motion schemes are in place. The four M3 transitions—shared-axis, fade-through, container transform, and animated visibility—are implemented (`MdRevealHost` yields layout space and clips; `MdMorphPanel` interpolates endpoint sizes), but `Control` content does not cross-fade on exit.
- **AOT.** All 17 packages declare `IsTrimmable` and `IsAotCompatible` and enable the corresponding analyzers; theme JSON uses source-generated serialization. The Linux x64 native Gallery has been published and smoke-run under xvfb. The AOT workflow reports diagnostics, but zero diagnostics do not mean AOT validation has passed for Windows, macOS, Android, or a consuming application.
- **Android back gestures and safe areas.** `MdBackNavigation` covers dialogs, sheets, drawers, search, menus, FAB menus, and both pickers. `MdSafeArea` provides per-edge insets. Headless behavior has regression tests; on-device gesture animation and cutout geometry still need manual validation.
- **RTL and localization.** Related adaptation code remains in place, but full RTL/multilingual layout and manual screen-reader validation are not currently preview release blockers.
- **Font provenance.** The complete Material Symbols Rounded variable TTF and genuine Lite subset are checked in and packaged automatically. CI pins the upstream commit and verifies SHA-256, variable tables, and glyph counts to prevent placeholder or substituted fonts from entering a release.

See [`spec-snapshot/manifest.json`](spec-snapshot/manifest.json) for the official pages, pinned AndroidX commit `e2171b56112f43bf962f5d608413f74033b580b6`, and token version.

## License

Apache-2.0. See [`LICENSE`](LICENSE) and [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md) for the license terms of third-party components. The embedded Material Symbols font is also Apache-2.0; its license is distributed with both Icons packages.
