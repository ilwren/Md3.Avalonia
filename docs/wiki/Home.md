# Md3.Avalonia Documentation

Welcome to the official documentation and API reference for **Md3.Avalonia** — the modern, cross-platform Material Design 3 (M3) control library for [Avalonia UI](https://avaloniaui.net/). The library packages target **.NET 8.0 and .NET 10.0**; the Gallery and the hosts in this repository target **.NET 10.0**.

---

## 📦 Packages Overview

Md3.Avalonia is structured into modular NuGet packages:

| Package | Description | Target |
| :--- | :--- | :---: |
| **`Md3.Avalonia`** | Core Material Design 3 controls (Buttons, TextBoxes, Cards, FAB, Navigation, Dialogs, Sliders, Sheets, etc.) | `.NET 8.0 / 10.0` |
| **`Md3.Avalonia.Extra`** | Extended controls inspired by Flutter ecosystem widgets (Rich Editor, Chat View, Timeline, DataGrid, Chart, Calendar, TreeView, Breadcrumb, PIN Input, etc.) | `.NET 8.0 / 10.0` |
| **`Md3.Avalonia.Icons`** | Full Material Symbols Rounded icon catalog (4,000+ glyphs) with embedded TTF font resource. | `.NET 8.0 / 10.0` |
| **`Md3.Avalonia.Icons.Lite`** | Lightweight Material Symbols Rounded icon subset with embedded compact TTF font resource (~98 KB). | `.NET 8.0 / 10.0` |
| **`Md3.Avalonia.DataGrid`** | Material Design 3 theme for Avalonia's DataGrid (opt-in: carries the DataGrid dependency). | `.NET 8.0 / 10.0` |
| **`Md3.Avalonia.RichEditor`** | Material Design 3 theme for the AvaloniaRichEditor control (opt-in: carries that dependency). | `.NET 10.0` |

---

## 📚 Table of Contents

1. [**Getting Started**](Getting-Started)
   - Installation via NuGet
   - Application initialization in `App.axaml`
   - Setting up themes and primary seed colors
2. [**Core Controls API Reference**](Core-Controls-API)
   - Action Buttons (MdButton, MdIconButton, MdFloatingActionButton, MdFabMenu with Left/Right alignment)
   - Inputs (MdTextBox, MdSearchBar, MdSearchView)
   - Selection & Controls (MdSwitch, MdCheckBox, MdRadioButton, MdSlider)
   - Cards & Surfaces (MdCard, Elevation levels)
   - Chips & Badges (MdAssistChip, MdFilterChip, MdInputChip, MdBadge)
   - Navigation (MdTopAppBar, MdNavigationBar, MdNavigationRail, MdNavigationDrawer, MdTabControl)
   - Dialogs & Sheets (MdDialogHost, MdSheetHost)
   - Settings Layouts (MdSettingsCard, MdSettingsExpander, MdSettingsGroup)
3. [**Extra Controls API Reference**](Extra-Controls-API)
   - `MdRichEditor` (Flutter Quill inspired rich-text toolbar and live Markdown adapter)
   - `MdChatView` (Flutter Chat UI inspired asymmetric message bubbles, avatars, delivery status, and composer)
   - `MdDataGrid` (Sortable, editable, virtualized enterprise grid with clipboard export)
   - `MdTimeline` (Horizontal and vertical timeline presenters)
   - `MdChart` (Bar and line charts with hover exploration)
   - `MdCalendar` (Single and range date selection)
   - `MdCascader` & `MdTransfer` (Hierarchical cascades and dual-list transfer)
   - `MdPinInput` (Segmented verification / OTP code input)
   - `MdTagInput` (Multi-tag entry with autocomplete and validation)
   - `MdTreeView` (Virtualization, guides, expansion animations, RTL support)
   - `MdBreadcrumb` (Icons, custom separators, overflow collapsing, command binding)
   - `MdPopover` & `MdHoverCard` (Transient interactive overlay cards)
   - `MdSkeleton` (Pulsing loading placeholders)
   - `MdAnimationSequence` (Staggered entrance animations)
4. [**Theming & Design Tokens**](Theming-and-Design-Tokens)
   - HCT Dynamic Color System
   - State Layer Opacities
   - Light & Dark mode switching
5. [**Icons & Typography**](Icons-and-Typography)
   - Using `MdIcon`, `MdSymbols`, and `MdSymbolsLite`
   - Using embedded TTF fonts without external runtime downloads
6. [**Multi-Platform Development**](Multi-Platform-Development)
   - Running on Desktop (Windows, macOS, Linux)
   - Running on Android (`net10.0-android`, `AvaloniaMainActivity`)
   - Adaptive responsive layouts (`MdAdaptiveLayout`)
