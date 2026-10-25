# API Reference

[English](API.md) | [简体中文](API.zh-CN.md)

The DocFX site generates API pages from all 17 packable projects under `src/`. Browse by package and namespace, or use site search to find public types and members. Each project's compiled public API and C# XML documentation comments are the source for the generated reference.

[Open the published API reference](https://ilwren.github.io/Md3.Avalonia/)

## NuGet packages

| Package | API area |
|---|---|
| `Md3.Avalonia11` | Core controls, themes, motion, and platform services. |
| `Md3.Avalonia11.Extra` | Optional controls, layout, and data-provider contracts. |
| `Md3.Avalonia11.Icons` | Full Material Symbols Rounded catalog. |
| `Md3.Avalonia11.Icons.Lite` | Reduced Material Symbols font and catalog. |
| `Md3.Avalonia11.DataGrid` | Material theme for Avalonia `DataGrid`. |
| `Md3.Avalonia11.AvaloniaEdit` | Material theme for AvaloniaEdit. |
| `Md3.Avalonia11.AvaloniaHex` | Material theme for AvaloniaHex. |
| `Md3.Avalonia11.Dock` | Material theme for Dock. |
| `Md3.Avalonia11.Tabalonia` | Material theme for Tabalonia. |
| `Md3.Avalonia11.Markdown` | Material styles for Markdown.Avalonia. |
| `Md3.Avalonia11.LiveCharts2` | Material theme for LiveCharts2. |
| `Md3.Avalonia11.ScottPlot` | Material theme for ScottPlot. |
| `Md3.Avalonia11.PropertyGrid` | Material theme for Avalonia.PropertyGrid. |
| `Md3.Avalonia11.NodifyM` | Material theme for NodifyM. |
| `Md3.Avalonia11.Mapsui` | Material theme for Mapsui. |
| `Md3.Avalonia11.InkCanvas` | Handwriting canvas theme and signature pad. |
| `Md3.Avalonia11.Notifications` | Cross-platform system notifications. |

## Source of truth

Public API summaries, contracts, parameter descriptions, and symbol-level examples are maintained in C# XML comments beside their declarations. DocFX builds the generated reference from the 17 projects' public APIs and XML documentation files. Task-oriented walkthroughs and screen-sized examples live in the guides below; the generated `MdSymbols` and `MdSymbolsLite` glyph catalogs are intentionally excluded so thousands of constants do not overwhelm API navigation and search.

## Guides

- [Quick start](GETTING_STARTED.md) — install the core package, register `MaterialTheme`, and add a first screen.
- [Usage examples](USAGE_EXAMPLES.md) — form binding, icons, dynamic color, waterfall layout, and dialogs, with screenshots.
- [NuGet packages](NUGET_PACKAGES.md) — all 17 package IDs, target frameworks, and theme entry points.
- [Project structure](PROJECT_STRUCTURE.md) — repository map and API-documentation build flow.
- [Compatibility](COMPATIBILITY.md) and the [dialog service guide](DIALOG_SERVICE.md).

Run `python3 scripts/check-api-doc-coverage.py` from the repository root to verify the DocFX project set, XML output, XML comment validity, and bilingual public-interface summaries.
