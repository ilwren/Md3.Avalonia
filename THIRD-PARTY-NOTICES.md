# Third-party notices

This repository and its distributable projects use the following third-party software. Their names and trademarks remain the property of their respective owners.

| Dependency or asset | Use | License | Project |
|---|---|---|---|
| Avalonia 12 | UI framework and platform hosts | MIT | https://github.com/AvaloniaUI/Avalonia |
| MaterialColorUtilities 0.3.0 | HCT, tonal palettes and Material scheme mapping | Apache-2.0 | https://github.com/albi005/MaterialColorUtilities |
| Material Symbols Rounded | Complete official variable TTF embedded in `Md3.Avalonia.Icons`, plus a real-outline Lite subset generated from the same pinned upstream file | Apache-2.0 | https://github.com/google/material-design-icons (commit `737e3324305806514d7909874fa1818ae1808232`) |
| Material loading indicator shape geometry | Seven expressive shape paths and animation constants, ported from Material Components Android assets (Copyright Google LLC / Android Open Source Project) | Apache-2.0 | https://github.com/material-components/material-components-android |
| AvaloniaEdit | Gallery source-code editor | MIT | https://github.com/AvaloniaUI/AvaloniaEdit |
| CommunityToolkit.Mvvm | Gallery MVVM compatibility examples | MIT | https://github.com/CommunityToolkit/dotnet |
| xUnit.net | Automated tests only | Apache-2.0 | https://github.com/xunit/xunit |

Each NuGet package declares only its direct runtime dependencies. Gallery- and test-only dependencies are listed here for repository-wide compliance.

The Material Symbols license text is retained in the optional package at `src/Md3.Avalonia.Icons/Assets/Fonts/MaterialSymbolsRounded-LICENSE.txt`. The project itself is distributed under the Apache License 2.0 in `LICENSE`.

## Documentation-only ecosystem research

The following packages are **not dependencies** and none of their Dart source, assets, fonts, screenshots, package names, or visual trade dress is distributed in the assemblies. Frozen public documentation was used only to write independent behavior requirements before clean-room Avalonia implementation:

| Package snapshot | Documented behavior reviewed | License | Source |
|---|---|---|---|
| GetWidget 7.0.2 | Avatar fallback and overlapping avatar-group behavior | MIT | https://pub.dev/packages/getwidget |
| flutter_rating_bar 4.0.1 | Fractional/read-only rating behavior | MIT | https://pub.dev/packages/flutter_rating_bar/versions/4.0.1 |
| shadcn_ui 0.57.0 | Composable breadcrumb and item invocation behavior | MIT | https://pub.dev/packages/shadcn_ui |
| flutter_pinput 1.0.3 | Fixed-length PIN/OTP entry, paste, masking, visual states and completion behavior | MIT | https://pub.dev/packages/flutter_pinput/versions/1.0.3 |
| animated_tree_view 2.3.0 | Nested tree nodes, expansion, indentation utilities and RTL behavior | MIT | https://pub.dev/packages/animated_tree_view/versions/2.3.0 |
| chips_input_autocomplete 1.2.2 | Dynamic/removable tags, suggestions, limits and validation behavior | MIT | https://pub.dev/packages/chips_input_autocomplete/versions/1.2.2 |

The frozen review and clean-room dispositions are recorded in `spec-snapshot/manifest.json`.
