# Md3.Avalonia public API overview

This document is the public entry point for the `0.5.0-preview.1` API. Preview APIs may still receive compatibility-preserving refinements before the first stable release. The NuGet package also emits `Md3.Avalonia.xml` from the source XML comments for IDE IntelliSense and API documentation generation.

## Install and register

```xml
<PackageReference Include="Md3.Avalonia" Version="0.5.0-preview.1" />
<!-- Optional symbol providers (choose one): -->
<PackageReference Include="Md3.Avalonia.Icons" Version="0.5.0-preview.1" />
<PackageReference Include="Md3.Avalonia.Icons.Lite" Version="0.5.0-preview.1" />
<!-- Optional clean-room third-party Flutter patterns (depends on core): -->
<PackageReference Include="Md3.Avalonia.Extra" Version="0.5.0-preview.1" />
<!-- Optional Material theme for Avalonia's own DataGrid; the only package that pulls in
     Avalonia.Controls.DataGrid: -->
<PackageReference Include="Md3.Avalonia.DataGrid" Version="0.5.0-preview.1" />
<!-- Optional Material theme for the AvaloniaEdit text editor; likewise the only package that
     pulls in Avalonia.AvaloniaEdit: -->
<PackageReference Include="Md3.Avalonia.AvaloniaEdit" Version="0.5.0-preview.1" />
<!-- Optional Material theme for the AvaloniaHex hex editor; likewise the only package that
     pulls in AvaloniaHex: -->
<PackageReference Include="Md3.Avalonia.AvaloniaHex" Version="0.5.0-preview.1" />
<!-- Optional Material theme for the Dock docking layout system; likewise the only package
     that pulls in Dock.Avalonia and Dock.Avalonia.Themes.Fluent: -->
<PackageReference Include="Md3.Avalonia.Dock" Version="0.5.0-preview.1" />
<!-- Optional Material theme for Tabalonia draggable tabs; likewise the only package that
     pulls in Tabalonia: -->
<PackageReference Include="Md3.Avalonia.Tabalonia" Version="0.5.0-preview.1" />
<!-- Optional Material style collection for Markdown.Avalonia documents; likewise the only package
     that pulls in Markdown.Avalonia.Tight. Unlike every other theme package it is not registered
     in Application.Styles - see the Markdown section below: -->
<PackageReference Include="Md3.Avalonia.Markdown" Version="0.5.0-preview.1" />
<!-- Optional Material theme for LiveCharts2 charts; likewise the only package that pulls in
     LiveChartsCore.SkiaSharpView.Avalonia. Like the Markdown package it is not registered in
     Application.Styles - it is configured once through LiveCharts.Configure; see the LiveCharts2
     section below: -->
<PackageReference Include="Md3.Avalonia.LiveCharts2" Version="0.5.0-preview.1" />
<!-- Optional Material theme for ScottPlot charts; likewise the only package that pulls in
     ScottPlot.Avalonia. It is a plot-level entry point rather than a style collection, so it is not
     registered in Application.Styles either; see the ScottPlot section below: -->
<PackageReference Include="Md3.Avalonia.ScottPlot" Version="0.5.0-preview.1" />
<!-- Optional Material theme for the Avalonia.PropertyGrid property grid; likewise the only package
     that pulls in bodong.Avalonia.PropertyGrid. It is a normal style collection - register it after
     MaterialTheme; see the Property grid section below: -->
<PackageReference Include="Md3.Avalonia.PropertyGrid" Version="0.5.0-preview.1" />
<!-- Optional Material theme for the NodifyM node editor; likewise the only package that
     pulls in NodifyM.Avalonia: -->
<PackageReference Include="Md3.Avalonia.NodifyM" Version="0.5.0-preview.1" />
<!-- Optional Material theme for the Mapsui map control; likewise the only package that pulls
     in Mapsui.Avalonia12. Mapsui draws its own canvas, so the theme is a map-level entry point
     rather than a style collection; see the Mapsui section below: -->
<PackageReference Include="Md3.Avalonia.Mapsui" Version="0.5.0-preview.1" />
<!-- Optional Material theme for the DotNetCampus ink canvas plus MdSignaturePad; likewise the
     only package that pulls in DotNetCampus.AvaloniaInkCanvas. The canvas also draws itself, so
     this is a control-level entry point; see the InkCanvas section below: -->
<PackageReference Include="Md3.Avalonia.InkCanvas" Version="0.5.0-preview.1" />
<!-- Optional Material theme for the maintained AvaloniaRichEditor fork; likewise the only
     package that pulls in OlivierJC.AvaloniaRichEditor: -->
<PackageReference Include="Md3.Avalonia.RichEditor" Version="0.5.0-preview.1" />
<!-- Optional native system notifications for Windows, macOS, Linux and Android. Independent of
     Avalonia themes, so there is nothing to register in Application.Styles: -->
<PackageReference Include="Md3.Avalonia.Notifications" Version="0.5.0-preview.1" />
<!-- Optional real desktop video playback. The host separately deploys native LibVLC: -->
<PackageReference Include="Md3.Avalonia.LibVLC" Version="0.5.0-preview.1" />
```

All nineteen packages are versioned together. The five core-family packages (`Md3.Avalonia`, `Md3.Avalonia.Icons`, `Md3.Avalonia.Icons.Lite`, `Md3.Avalonia.Extra`, `Md3.Avalonia.DataGrid`) provide `net8.0` and `net10.0` assets, and the twelve opt-in third-party theme packages provide `net10.0` only. `Md3.Avalonia.Notifications` adds `net8.0-windows10.0.19041.0`, `net10.0-windows10.0.19041.0` and, when the pack scripts opt in, `net10.0-android`; the optional desktop-only `Md3.Avalonia.LibVLC` adapter provides `net8.0` and `net10.0`. The Gallery host itself targets .NET 10 only. Add only the optional packages your application uses; choose **one** icon provider:

| NuGet package | Purpose | Theme/resource registration |
|---|---|---|
| `Md3.Avalonia` | Core Material 3 controls and tokens | `MaterialTheme` |
| `Md3.Avalonia.Icons` | Full Material Symbols Rounded font and glyph catalog | Discovered automatically; no extra theme |
| `Md3.Avalonia.Icons.Lite` | Reduced Material Symbols Rounded subset | Discovered automatically; use instead of `Icons` |
| `Md3.Avalonia.Extra` | Optional ecosystem controls | `ExtraTheme` after `MaterialTheme` |
| `Md3.Avalonia.DataGrid` | Material theme for Avalonia's native `DataGrid` | `MaterialDataGridTheme` |
| `Md3.Avalonia.RichEditor` | Material theme for the API-compatible MIT-maintained `AvaloniaRichEditor` fork | `MaterialRichEditorTheme` |
| `Md3.Avalonia.AvaloniaEdit` | Material theme for the AvaloniaEdit text editor | `MaterialAvaloniaEditTheme` |
| `Md3.Avalonia.AvaloniaHex` | Material theme for the AvaloniaHex hex editor | `MaterialAvaloniaHexTheme` |
| `Md3.Avalonia.Dock` | Material theme for the Dock docking layout system | `MaterialDockTheme` |
| `Md3.Avalonia.Tabalonia` | Material theme for Tabaloria draggable and tearable tabs | `MaterialTabaloniaTheme` |
| `Md3.Avalonia.Markdown` | Material 3 styles for Markdown.Avalonia documents | `MaterialMarkdownTheme` |
| `Md3.Avalonia.LiveCharts2` | Material 3 theme for LiveCharts2 charts | `MaterialLiveChartsTheme.AddMaterialTheme` via `LiveCharts.Configure` |
| `Md3.Avalonia.ScottPlot` | Material 3 theme for ScottPlot charts | Plot-level `MaterialScottPlotTheme.Follow` / `.Apply` |
| `Md3.Avalonia.PropertyGrid` | Material theme for the Avalonia.PropertyGrid grid | `MaterialPropertyGridTheme` |
| `Md3.Avalonia.NodifyM` | Material theme for the NodifyM node editor | `MaterialNodifyMTheme` |
| `Md3.Avalonia.Mapsui` | Material theme for the Mapsui map control | Map-level `MaterialMapsuiTheme.Follow` |
| `Md3.Avalonia.InkCanvas` | Material theme for the DotNetCampus ink canvas, plus `MdSignaturePad` | `MaterialInkCanvasTheme` |
| `Md3.Avalonia.Notifications` | Unified Windows, macOS, Linux and Android system notifications; independent of Avalonia themes | `IMdSystemNotificationService` / `MdSystemNotificationService` |
| `Md3.Avalonia.LibVLC` | Optional actual desktop video playback for `MdVideoPlayer`; native LibVLC is host-deployed | `MdLibVlcVideoPlayer` (reuses core `MaterialTheme`) |

Register the core theme; register optional themes only when their packages are referenced:

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:themes="using:Md3.Avalonia.Themes"
             xmlns:extra="using:Md3.Avalonia.Extra.Themes">
  <Application.Styles>
    <themes:MaterialTheme />
    <!-- Optional: -->
    <extra:ExtraTheme />
    <themes:MaterialDataGridTheme />
    <themes:MaterialRichEditorTheme />
  </Application.Styles>
</Application>
```

All component themes are scoped to `Md*` types. Registering `MaterialTheme` does not replace Avalonia's built-in `Button`, `TextBox`, `ComboBox`, `ScrollViewer`, or other native themes. The DataGrid and rich-editor themes are opt-in and do not add those third-party controls to the core package.

## System notifications

`Md3.Avalonia.Notifications` is a standalone package (no Avalonia dependency) that exposes `IMdSystemNotificationService`, `MdSystemNotificationService`, `MdSystemNotification`, and `MdSystemNotificationOptions`:

```csharp
using Md3.Avalonia.Notifications;

var notifications = MdSystemNotificationService.CreateDefault(new MdSystemNotificationOptions
{
    ApplicationName = "My App",
    WindowsAppUserModelId = "com.example.myapp",
    AndroidChannelId = "updates",
    AndroidChannelName = "Updates",
});

await notifications.ShowAsync(new MdSystemNotification("Ready", "The export has finished."));
```

`ShowAsync(title, body, cancellationToken)` is also available. For native Windows notifications, target the Windows host at `net8.0-windows10.0.19041.0` or `net10.0-windows10.0.19041.0`; classic desktop toast still requires the supplied AppUserModelID to be registered by the host app. Linux calls `libnotify` directly and requires its shared library plus a running D-Bus notification service (it does not launch `notify-send`). Android 13+ requires the host to declare `POST_NOTIFICATIONS` and request runtime permission before calling the service. Missing Android permission throws `MdSystemNotificationPermissionException`. Platform failures are reported as `MdSystemNotificationException`. The initial API supports title and body only (no click callback or actions); see the [package README](../src/Md3.Avalonia.Notifications/README.md) for all platform requirements.

## Namespaces

| Namespace | Purpose |
|---|---|
| `Md3.Avalonia.Controls` | Component classes, variants, item containers and dialog wrappers |
| `Md3.Avalonia.Themes` | `MaterialTheme` resource provider |
| `Md3.Avalonia.Themes.Dynamic` | HCT theme generation, application, JSON and diagnostics |
| `Md3.Avalonia.Motion` | Inherited motion scheme and spring definitions |
| `Md3.Avalonia.Localization` | Inherited UI culture selection |
| `Md3.Avalonia.Icons` / `Md3.Avalonia.Controls.MdSymbols` | Optional verified Material Symbols loader and strongly typed catalog |
| `Md3.Avalonia.Extra.Controls` | Optional clean-room ecosystem controls |
| `Md3.Avalonia.Extra.Infrastructure` | Density, paging, async-state, shortcut, overlay and focus-return contracts |
| `Md3.Avalonia.Extra.Themes` | Opt-in `ExtraTheme` resource provider |
| `Md3.Avalonia.Notifications` | Standalone native system-notification service and models |
| `Md3.Avalonia.LibVLC` | Optional desktop LibVLCSharp backend for `MdVideoPlayer` |

## Components

- Actions: `MdButton`, `MdToggleButton`, `MdIconButton`, `MdToggleIconButton`, `MdSplitButton`, `MdStandardButtonGroup`, `MdConnectedButtonGroup`, `MdFloatingActionButton`, `MdExtendedFloatingActionButton`, `MdFabMenu`, `MdFabMenuItem`.
- Selection and input: `MdTextBox`, `MdMaskedTextBox`, `MdCheckBox`, `MdRadioButton`, `MdComboBox`, `MdAutoCompleteBox`, `MdNumericBox`, `MdSwitch`, `MdSlider`, `MdRangeSlider`, `MdSegmentedButtonGroup`, `MdSegmentedButton`.
- Content: `MdCard`, `MdCarousel`, `MdCarouselItem`, `MdChip`, `MdAssistChip`, `MdFilterChip`, `MdInputChip`, `MdSuggestionChip`, `MdList`, `MdListItem`, `MdDivider`, `MdCommitGraph`, `MdCommitNode`, `MdContributionGrid`, `MdContributionDay`.
- Pickers: `MdDatePicker`, `MdDatePickerDialog`, `MdDateRangePicker`, `MdTimePicker`, `MdTimePickerDialog`, the interactive `MdTimeDial`/`MdTimeDialPart` clock-face API, and the direct-path `MdFileSystemPicker` with `MdFileSystemPathSegment` breadcrumb models.
- Media: backend-neutral `MdVideoPlayer`, `MdPlaybackRequest`, and `MdPlaybackRequestEventArgs`; optional desktop `MdLibVlcVideoPlayer` playback.
- Navigation: `MdTopAppBar`, `MdBottomAppBar`, `MdNavigationBar`, `MdNavigationBarItem`, `MdNavigationDrawer`, `MdNavigationRail`, `MdTabs`, `MdTabItem`, `MdTabsView`, `MdTabView`, `MdTabViewItem`, `MdPagination`.
- Tabs come in two shapes. `MdTabView` carries its own tab strip and content, like a classic `TabControl`. `MdTabs` is only the bar, for the common Material case where it sits in an app bar or header away from the pages; pair it with `MdTabsView`, whose children are the pages matched to the bar's tabs by position, and whose `Tabs` property keeps selection in step both ways. This mirrors Flutter's `TabBar` and `TabBarView`.
- Transient surfaces: `MdDialog`, `MdDialogHost`, `MdDialogService`, `IMdDialogService`, `MdDialogServiceExtensions`, `MdDropdownMenu`, `MdMenuAnchor`, `MdMenu`, `MdMenuItem`, `MdSheetHost`, `MdSnackbar`, `MdSnackbarHost`, `MdSnackbarService`, `IMdSnackbarService`, `MdTooltip`, `MdTooltipHost`.
- Feedback: `MdLoadingIndicator`, `MdLinearProgressIndicator`, `MdCircularProgressIndicator`, `MdBadge`, `MdBadgedBox`.
- Flutter-inspired Avalonia APIs: `MdBanner`, `MdExpansionPanelList`, `MdDataTable`, `MdStepper`, `MdRefreshIndicator`, `MdPaginatedDataTable`, `MdReorderableList`, `MdGridTile`, `MdDismissible`, `MdForm`, `MdFormField`, `MdDropdownFormField`, `MdSimpleDialog`, `MdAboutDialog`, `MdLicensePage`, `MdDraggableScrollableSheet`, `MdAdaptiveSwitch`, `MdAdaptiveProgressIndicator`, `MdHero`, `MdFocusTraversalGroup`, `MdShortcutScope`. These names do not imply complete Flutter parity; see [Flutter parity status](FLUTTER_PARITY_STATUS.md).
- `MdAboutDialog` fills itself in from the entry assembly when `ApplicationName`, `ApplicationVersion` or `Legalese` are left unset, matching Flutter's `showAboutDialog`. The resolved text is readable through `EffectiveApplicationName`, `EffectiveApplicationVersion` and `EffectiveLegalese`; explicit values always win, and a blank string counts as unset. The icon, version and copyright lines collapse when empty.
- Foundations and desktop adapters: `MdScrollViewer`, `MdScrollBar`, five-breakpoint/input-aware `MdAdaptiveLayout`, `MdSurface`, `MdText`, `MdStateLayer`, `MdFocusRing`, `MdWindow`, `MdIcon`, `MdSymbolPresenter`.
- Borderless/chromeless windows: `MdBorderlessWindow`, `MdWindowTitleBar`, `MdCaptionButton`/`MdWindowCaptionButton`, `MdWindowDragRegion`, `MdWindowResizeGrip`, `IMdWindowPlatformAdapter`, named platform adapters and `MdWindowPlatformAdapterResolver`. The caption shows the window icon by default (`ShowIcon`) and has no separator line; visibility and enabled state are independently configurable through `ShowMinimizeButton`/`ShowMaximizeButton`/`ShowCloseButton` and `IsMinimizeButtonEnabled`/`IsMaximizeButtonEnabled`/`IsCloseButtonEnabled`.

- Optional Icons package: `MdExternalMaterialSymbols`, `MdSymbols` and the `Md.Icon.*` resource injection contract.
- Optional Ecosystem foundations: `MdDensity`, `MdAsyncRequestState`, `MdPageRequest`, `MdPageResult<T>`, `IMdPageProvider<T>`, `MdShortcutBinding`, `MdOverlayPlacement`, `MdFocusReturnScope`.
- Optional Ecosystem Wave A: `MdPopover`, `MdHoverCard`, `MdCommandPalette`, `MdCommandItem`.
- Optional Ecosystem Wave B: `MdSlidableItem`, `MdPagedItemsView`, `MdMasonryPanel`, `MdDataGrid`, `MdDataGridColumn`.
- Optional Ecosystem Wave C: `MdAsyncSelect`, `MdCalendar`, `MdTimeline`, `MdResultView`, `MdCascader`, `MdTransfer`.
- Optional Ecosystem Wave D: `MdChart`, `IMdChartDataProvider`, `MdRichEditor`/`MdRichTextEditor`, `IMdRichEditorAdapter`, `MdChatView`.
- Optional Ecosystem Wave E: `MdSkeleton`, `MdSkeletonGroup`, `MdAnimationSequence`.
- Optional Ecosystem Wave F: `MdPinInput`, `MdPinCell`, `MdTreeView`, `MdTreeNode`, `MdTreeRow`, `MdTagInput`, `MdTagEntry`, `MdTagChangedEventArgs`.
- Existing optional Ecosystem controls: `MdAvatar`, `MdAvatarGroup`, `MdRating`, `MdBreadcrumb`, `MdBeforeAfter`, `MdAnimatedText`, `MdSpinKit`, and `MdStaggeredPanel`.
- `MdBeforeAfter` provides draggable horizontal/vertical content comparison; `MdAnimatedText` provides typewriter/fade/pop-friendly text reveal; `MdSpinKit` contains optional non-Material loading recipes; `MdStaggeredPanel` provides cancelable staggered entrance motion. `MdWaveform` draws peak-driven audio waveforms with a scrubbable Material playhead and no bundled decoder. `MdWaterfallPanel` and `MdWaterfall` provide masonry placement. `MdFloatingWindow` is an in-app floating window: draggable within its host, resizable, and collapsible to its title bar with the hosted content left mounted. These are Extra controls; core `MdChart` remains provider-neutral and does not bundle a chart engine.

## Complete component catalog

The following catalog covers the public control surface shipped by the core package. Types ending in `*Item`, `*Part`, `*Presenter`, `*Thumb`, `*Panel`, or `*Grid` are supporting visual types that can also be composed directly when their API is public. For the exact dependency version and XML documentation, use the generated `Md3.Avalonia.xml` file in the NuGet package.

### Core controls (`Md3.Avalonia`)

| Area | Controls and supporting types |
|---|---|
| **Buttons and actions** | `MdButton` (`Filled`, `Tonal`, `Outlined`, `Text` variants), `MdIconButton`, `MdToggleButton`, `MdToggleIconButton`, `MdSplitButton`, `MdStandardButtonGroup`, `MdConnectedButtonGroup`, `MdFloatingActionButton`, `MdExtendedFloatingActionButton`, `MdFabMenu`, `MdFabMenuItem`, `MdFabMenuPanel` |
| **Text and form input** | `MdTextBox`, `MdMaskedTextBox`, `MdNumericBox`, `MdAutoCompleteBox`, `MdSearchBar`, `MdSearchView`, `MdCheckBox`, `MdRadioButton`, `MdSwitch`, `MdSlider`, `MdRangeSlider`, `MdComboBox`, `MdForm`, `MdFormField`, `MdDropdownFormField`, `MdOutlinedFieldBorder` |
| **Chips and selection** | `MdChip`, `MdAssistChip`, `MdFilterChip`, `MdInputChip`, `MdSuggestionChip`, `MdSegmentedButton`, `MdSegmentedButtonGroup` |
| **Surfaces and content** | `MdSurface`, `MdCard`, `MdList`, `MdListItem`, `MdDivider`, `MdCarousel`, `MdCarouselItem`, `MdGridTile`, `MdGridTileBar`, `MdBanner`, `MdExpansionPanel`, `MdExpansionPanelList`, `MdDataTable`, `MdPaginatedDataTable`, `MdCommitGraph`, `MdCommitNode`, `MdContributionGrid`, `MdContributionDay` |
| **Menus and dialogs** | `MdMenu`, `MdMenuItem`, `MdSubMenuItem`, `MdMenuAnchor`, `MdDropdownMenu`, `MdDialog`, `MdDialogHost`, `MdSimpleDialog`, `MdAboutDialog`, `MdLicensePage`, `MdSheetHost`, `MdTooltip`, `MdTooltipHost` |
| **Navigation and layout** | `MdScaffold`, `MdTopAppBar`, `MdBottomAppBar`, `MdToolbar`, `MdNavigationBar`, `MdNavigationBarItem`, `MdNavigationDrawer`, `MdNavigationRail`, `MdNavigationRailItem`, `MdNavigationSuite`, `MdTabs`, `MdTabItem`, `MdTabView`, `MdTabViewItem`, `MdPagination`, `MdGridSplitter`, `MdAdaptiveLayout`, `MdAdaptiveSwitch`, `MdKeyboardAvoidingHost`, `MdDraggableScrollableSheet` |
| **Mobile platform integration** | `MdSafeArea`, `MdBackNavigation`, `MdBackScope`, `MdKeyboardAvoidingHost` |
| **Settings and preference surfaces** | `MdSettingsGroup`, `MdSettingsCard`, `MdSettingsExpander` |
| **Feedback and status** | `MdLoadingIndicator`, `MdLinearProgressIndicator`, `MdCircularProgressIndicator`, `MdAdaptiveProgressIndicator`, `MdBadge`, `MdBadgedBox`, `MdRefreshIndicator`, `MdSnackbar`, `MdSnackbarHost`, `MdSnackbarService` |
| **Pickers and calendars** | `MdDatePicker`, `MdDatePickerDialog`, `MdDateRangePicker`, `MdCalendarGrid`, `MdCalendarDay`, `MdTimePicker`, `MdTimePickerDialog`, `MdTimeDial`, `MdTimeDialPart`, `MdPickerRestorationStore`, `MdFileSystemPicker`, `MdFileSystemPickerMode`, `MdFileSystemBreadcrumbStyle`, `MdFileSystemEntry`, `MdFileSystemPathSegment`, `MdFileSystemSelectionEventArgs`, `MdFileSystemNavigationFailedEventArgs` |
| **Media** | `MdVideoPlayer`, `MdPlaybackRequest`, `MdPlaybackRequestEventArgs` |
| **Lists and interaction patterns** | `MdReorderableList`, `MdDismissible`, `MdStepper`, `MdStep`, `MdHero`, `MdFocusTraversalGroup`, `MdShortcutScope` |
| **Foundations and icons** | `MdText`, `MdIcon`, `MdSymbolPresenter`, `MdStateLayer`, `MdRipplePresenter`, `MdFocusRing`, `MdScrollViewer`, `MdScrollBar`, `MdSliderThumb`, `MdWindow` |
| **Window chrome** | `MdBorderlessWindow`, `MdWindowTitleBar`, `MdCaptionButton`, `MdWindowCaptionButton`, `MdWindowDragRegion`, `MdWindowResizeGrip`, `IMdWindowPlatformAdapter`, `MdWindowPlatformAdapterResolver`, `MdWindowsWindowPlatformAdapter`, `MdMacOsWindowPlatformAdapter`, `MdLinuxWindowPlatformAdapter`, `MdAvaloniaWindowPlatformAdapter`, `MdAndroidWindowPlatformAdapter` |

Most core controls expose styled properties, bindable `Items`/`ItemsSource` where applicable, routed events, commands, and native Avalonia automation peers. Variant and option enums are part of the public API and should be preferred over string values. All 84 public enums (63 core and 21 Extra) are listed with their members under [Enumerations](#enumerations).

### Core control quick reference

| Type | Primary use | Important API surface |
|---|---|---|
| `MdButton`, `MdIconButton`, `MdToggleButton` | Actions and two-state actions | `Command`, `CommandParameter`, `IsEnabled`, `IsChecked`, content/icon properties, variant and size properties |
| `MdTextBox`, `MdNumericBox`, `MdAutoCompleteBox` | Text, numeric, and suggestion input | `Text`/`Value`, `Label`, `SupportingText`, `IsError`, `PlaceholderText`, `IsReadOnly`, `ItemsSource`/suggestion provider; `MdNumericBox` adds native `Minimum`/`Maximum`/`Increment` and `ShowButtonSpinner`. Its wheel contract is focused-only stepping; an unfocused wheel should reach the outer page. Desktop event-routing acceptance remains pending. |
| `MdSearchBar`, `MdSearchView` | Search entry and result presentation | `Text` (inherited from `TextBox`), `SearchCommand`, `SearchSubmitted`; the view adds `Header`, `IsOpen`, `SelectedResult`, `ResultDisplayMemberPath`, `CloseOnResultCommit` and `ResultCommitted` |
| `MdCheckBox`, `MdRadioButton`, `MdSwitch` | Boolean and mutually exclusive choices | `IsChecked`, `GroupName`/selection binding, `Command` and native input events |
| `MdSlider`, `MdRangeSlider` | Single and interval values | `Minimum`, `Maximum`; `MdSlider` uses the native `Value` and tick properties, `MdRangeSlider` uses `LowerValue`/`UpperValue` plus `Step` and `ShowValueIndicators` |
| `MdCard`, `MdSurface`, `MdList`, `MdListItem` | Material containers and lists | variant/elevation/surface properties, content, `ItemsSource`, item templates, selection/invocation events |
| `MdDialogHost`, `MdSheetHost` | Modal dialogs and bottom/side sheets | `ShowAsync`, `Close`, `Dialog`, `Service`, placement, dismissal and result APIs |
| `MdMenu`, `MdDropdownMenu`, `MdMenuAnchor` | Contextual and anchored actions | items, submenu support, placement, `Show`/`Dismiss`, keyboard navigation |
| `MdNavigationBar`, `MdNavigationRail`, `MdNavigationDrawer` | Primary application navigation | item collections, selected index/item, placement/layout and `ItemInvoked` |
| `MdTabs`, `MdTabView` | Peer navigation and document views | tab collections, selected item/index, closable/reorderable view items |
| `MdDatePicker`, `MdDateRangePicker`, `MdTimePicker` | Date, date range, and time selection | selected value(s), mode, dialogs, validation, restoration and `ShowAsync` |
| `MdFileSystemPicker` | Direct file-system file/folder selection | `Mode`, `BreadcrumbStyle`, localized/customizable labels, `InitialDirectory`, `CurrentDirectory`, `FilePatterns`, `ShowHiddenItems`, `AllowMultiple`, `Entries`, `PathSegments`, `SelectedPaths`, `NavigateAsync`/`RefreshAsync`/`GoUpAsync`/`GoHomeAsync`/`GoToRootAsync`, `SelectionConfirmed`, `CancelRequested`, `NavigationFailed` |
| `MdVideoPlayer` | Backend-neutral video transport | `MediaContent`, `PosterContent`, `Position`, `Duration`, `Volume`, `PlaybackRate`, buffering/live/fullscreen state, keyboard transport, `Play`/`Pause`/`Seek`/`SeekBy`, and `PlaybackRequested`; use optional `MdLibVlcVideoPlayer` for desktop playback |
| `MdGridSplitter` | Resize adjacent `Grid` rows or columns | Avalonia `ResizeDirection`, `ResizeBehavior`, `ShowsPreview`, `DragIncrement` and `KeyboardIncrement`; Material `ShowHandle`, `ResetOnDoubleTapped`, `ResetToEqual()` and `SplitReset`. The 12 dp interaction track and 2 dp divider are independently tokenized. |
| `MdSnackbarHost`, `MdSnackbarService` | Queued transient messages | `Show`, `ShowAsync`, `Dismiss`, action/result, queue and shared service injection |
| `MdCarousel`, `MdRefreshIndicator`, `MdReorderableList` | Touch-friendly content interaction | controller/provider APIs, navigation, refresh callbacks, reorder callbacks |
| `MdMaskedTextBox` | Mask-constrained text input | `Mask` (literals are filled in while typing), `PromptChar`, `HidePromptOnLeave`, `MaskCompleted`/`MaskFull`; field state through `Label`, `PrefixText`, `SupportingText`, `ErrorText`, `IsError` and `MdMaskedTextBoxVariant` |
| `MdCommitGraph`, `MdCommitNode` | GitHub-style commit history | items are `MdCommitNode` (`Id`, `Message`, `Author`, `Date`, `ParentIds`, `Tag`, `Color`); `HeadId` marks the head, `LaneWidth`/`MaxLanes`/`NodeSize` size the drawing, and `LaneCount` reports the lanes in use. Branch and merge lane/curve connectors remain continuous: first-parent links are prominent and the parts a merge absorbs are muted. Clicking a commit selects it - `SelectedId` plus the `CommitClicked` event - and highlights the row. `MdCommitGraphDateTextConverter` formats a node `Date` for the item template (`MMM d` in-year, `MMM d, yyyy` otherwise) |
| `MdContributionGrid`, `MdContributionDay` | GitHub-style contribution heatmap | `Days` is a list of `MdContributionDay` (`Date`, `Value`); week columns start on Sunday and days missing from the data read as zero. Levels 1-4 come from the 25/50/75 percentiles of the non-zero values, so sparse and dense histories use the same five colors (`Md.ContributionGrid.Level.Brush.0..4`). `CellSize`/`CellSpacing` size both the rendered cells and their per-day pointer targets, and can be increased for touch layouts; `LessLabel`/`MoreLabel` rename the legend; `SelectedDate` identifies the cell-local selection, `DayClicked` reports the clicked day, and hovering shows a tooltip |
| `MdPagination` | Paging through a large page range | `NumberOfPages`, two-way `SelectedPageIndex`, `DisplayMode` (`MdPaginationDisplayMode.Numbers`/`Dots`), previous/next buttons; the visible window stays centered on the current page |
| `MdStepper`, `MdStep` | Step-based flows | `SelectedIndex`, step `Header`/`Subtitle`/`State`; `Orientation` vertical (step list, default) or horizontal (connector header row with the active step's body below it) |
| `MdScaffold`, `MdAdaptiveLayout`, `MdToolbar` | Application shell and responsive layout | slots/regions, breakpoint mode, density, toolbar mode and platform input mode |

### Optional package catalog (`Md3.Avalonia.Extra`)

`ExtraTheme` must be registered in addition to `MaterialTheme`. These controls are maintained as optional ecosystem patterns, not as claims of official Material 3 parity.

| Area | Controls |
|---|---|
| **Overlays and commands** | `MdPopover`, `MdHoverCard`, `MdCommandPalette`, `MdCommandItem` |
| **Collections and layout** | `MdPagedItemsView`, `MdMasonryPanel`, `MdDataGrid`, `MdDataGridColumn`, `MdSlidableItem`, `MdTreeView`, `MdTreeNode`, `MdTreeRow` |
| **Selection and transfer** | `MdAsyncSelect`, `MdCalendar`, `MdCascader`, `MdCascaderItem`, `MdTransfer`, `MdColorPicker`, `MdColorPickerButton` |
| **Feedback and identity** | `MdAvatar`, `MdAvatarGroup`, `MdRating`, `MdResultView`, `MdSkeleton`, `MdSkeletonGroup`, `MdPinInput`, `MdPinCell`, `MdTagInput`, `MdTagEntry` |
| **Data visualization and editing** | `MdChart`, `MdChartSeries`, `MdChartPoint`, `MdRichEditor`, `MdRichTextEditor`, `MdChatView`, `MdChatMessage`, `MdTimeline`, `MdTimelineItem` |
| **Motion and visual extras** | `MdBeforeAfter`, `MdAnimatedText`, `MdSpinKit`, `MdStaggeredPanel`, `MdWaveform`, `MdWaterfallPanel`, `MdWaterfall`, `MdFloatingWindow`, `MdAnimationSequence`, `MdAnimatedVisibility`, `MdContainerTransform`, `MdFadeThrough`, `MdSharedAxis`, `MdRevealHost`, `MdMorphPanel` |
| **Navigation/content** | `MdBreadcrumb`, `MdBreadcrumbItem`, `MdGridTile`-style content helpers |

`MdPagedItemsView.ItemTemplate` sets the presentation of each loaded record; `MdDataGrid` is a narrower surface than Avalonia `DataGrid` and is not a drop-in replacement for it.

The Extra support contracts are also public: `IMdPageProvider<T>`, `MdPageRequest`, `MdPageResult<T>`, `MdAsyncRequestState`, `MdDensity`, `MdEcosystemDensity`, `MdOverlayPlacement`, `MdOverlayAlignment`, `MdShortcutBinding`, and `MdFocusReturnScope`. Data and adapter contracts include `IMdChartDataProvider`, `IMdRichEditorAdapter`, and `IMdRichEditorStateAdapter`. Supporting enums and records configure those controls.

### Rich-text editor theme package

`Md3.Avalonia.RichEditor` themes `AvaloniaRichEditor.Controls.RichEditor` without replacing its engine or supplying a document. It sets the default run foreground to the active M3 `OnSurface` token (including live light/dark changes), alongside the selection, caret and type scale. The host supplies the surface/chrome, and the application can load or export HTML through the editor API:

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:rte="using:AvaloniaRichEditor.Controls">
  <Border Padding="16" CornerRadius="12"
          Background="{DynamicResource Md.Sys.Color.SurfaceContainerLow.Brush}"
          BorderBrush="{DynamicResource Md.Sys.Color.OutlineVariant.Brush}" BorderThickness="1">
    <rte:RichEditor x:Name="Editor" Height="240" />
  </Border>
</UserControl>
```

```csharp
Editor.LoadHtml("<p><b>Welcome</b> to the editor.</p>");
string html = Editor.ToHtml();
```

Register `MaterialRichEditorTheme` after `MaterialTheme`. This is separate from `MdRichEditor` in `Md3.Avalonia.Extra`, which provides an engine-neutral command surface and expects an `IMdRichEditorAdapter`.

### Common usage patterns

**Register both themes when using Extra:**

```xml
<Application.Styles>
  <themes:MaterialTheme />
  <extra:ExtraTheme />
</Application.Styles>
```

**Bind an input and a selection control:**

```xml
<StackPanel Spacing="16">
  <md:MdTextBox Text="{Binding Name, Mode=TwoWay}" Watermark="Name" />
  <md:MdSegmentedButtonGroup SelectedItem="{Binding Filter, Mode=TwoWay}">
    <md:MdSegmentedButton Content="All" Tag="all" />
    <md:MdSegmentedButton Content="Unread" Tag="unread" />
  </md:MdSegmentedButtonGroup>
</StackPanel>
```

**Compose an application shell:**

```xml
<md:MdScaffold>
  <md:MdScaffold.TopBar>
    <md:MdTopAppBar Title="Inbox" />
  </md:MdScaffold.TopBar>
  <md:MdScaffold.Content>
    <md:MdList ItemsSource="{Binding Messages}" />
  </md:MdScaffold.Content>
  <md:MdScaffold.BottomBar>
    <md:MdNavigationBar ItemsSource="{Binding Destinations}" />
  </md:MdScaffold.BottomBar>
</md:MdScaffold>
```

**Use an Extra provider-backed control:**

```xml
<extra:MdPagedItemsView x:Name="Records" PageSize="20" AutoLoad="True"
                        EmptyContent="No records">
  <extra:MdPagedItemsView.ItemTemplate>
    <DataTemplate x:DataType="vm:Record">
      <TextBlock Text="{Binding Title}" />
    </DataTemplate>
  </extra:MdPagedItemsView.ItemTemplate>
</extra:MdPagedItemsView>

<extra:MdResultView Kind="Empty" Title="Nothing here yet"
                    ActionContent="Reload" ActionCommand="{Binding ReloadCommand}" />
```

`MdPagedItemsView` has no `ItemsSource`: pages arrive from a provider and the control owns the
accumulated `Items`. Assign the provider in code, because it is a delegate rather than a bindable
property, and drive it with `LoadNextPageAsync()`, `RefreshAsync()` and `RetryAsync()`:

```csharp
Records.PageProvider = async request =>
{
    var page = await api.GetRecordsAsync(request.PageKey, request.PageSize, request.CancellationToken);
    return new MdPageResult<object?>(page.Items, page.NextKey, page.IsLast);
};
await Records.LoadNextPageAsync();
```

`State` reports `Idle`, `Loading`, `Data`, `Empty`, `Completed` or `Error`, and the template shows
the matching progress, empty, error or load-more affordance. Supply `ItemTemplate`, otherwise each
record renders as its `ToString()`.

Provider-backed controls are intentionally adapter-neutral. The application owns HTTP, caching, error policy, and cancellation; the control owns loading, empty, error, and retry presentation.

## Dynamic theme API

```csharp
var options = new MdThemeOptions
{
    SeedColor = "#006A6A",
    SchemeVariant = MdThemeSchemeVariant.Expressive,
    ContrastLevel = MdThemeContrastLevel.High,
    ThemeMode = MdThemeMode.System,
    MotionScheme = MdMotionScheme.Expressive,
    FontProfile = MdThemeFontProfile.Brand,
    ShapeScale = MdThemeShapeScale.Standard
};

bool dark = Application.Current!.ActualThemeVariant == ThemeVariant.Dark;
var roles = MdThemeManager.Apply(Application.Current, options, dark);
var checks = MdThemeManager.Diagnose(roles);
string json = MdThemeJson.Serialize(options);
```

`MdThemeGenerator.Generate` returns 49 named color roles, including surface-container and fixed roles. `MdThemeManager.Apply` creates both `Md.Sys.Color.{Role}` color resources and `Md.Sys.Color.{Role}.Brush` brush resources, plus action, disabled, shape and typeface compatibility resources.

## MVVM and direct operation

Properties that represent user state use Avalonia styled/direct properties and appropriate two-way defaults. Commands use `ICommand`, so CommunityToolkit.MVVM `RelayCommand` and `AsyncRelayCommand` work without an adapter. Native base classes retain selection, keyboard, validation and automation behavior.

`MdDialogHost.Dialog` accepts either a control or a view model. Add multiple type-specific templates to the host's inherited `DataTemplates` collection, then call `ShowAsync(model)`; Avalonia selects the matching dialog template while the host keeps one modal dialog active.

The host can be driven three ways, and they interoperate — pick per call site rather than per application:

| From | How | Result |
|------|-----|--------|
| View code-behind | `await DialogHost.ShowAsync(model)` / `DialogHost.Close(result)` | awaited return value |
| View model | inject `IMdDialogService`; `await _dialogs.ShowAsync(model)` | awaited return value |
| Bindings only | two-way `IsOpen` with `Dialog` | view model state, no await |

For the view-model route, place one `MdDialogHost` in the application shell, assign a shared `MdDialogService` to its `Service` property, and inject that same instance as `IMdDialogService` (step-by-step walkthrough: [Dialog service](DIALOG_SERVICE.md)) — the same arrangement `MdSnackbarService` uses. `MdDialogServiceExtensions.ShowAsync<TResult>` returns a typed result, or `default` when the dialog was dismissed without one:

```csharp
// App composition
var dialogs = new MdDialogService();
services.AddSingleton<IMdDialogService>(dialogs);
shell.DialogHost.Service = dialogs;

// View model — no control reference
if (await _dialogs.ShowAsync<bool>(new ConfirmDeleteDialogModel(item)))
    Items.Remove(item);
```

Material allows one dialog at a time, so a request made while another is displayed, or before the host is attached, waits its turn instead of replacing it. Dismissal by the scrim, Escape, the Android back gesture, a cancelled token, or a view model clearing a bound `IsOpen` completes the pending `ShowAsync` with `null`.

An `MdSnackbar` is a visual control, so calling `Show()` on an instance that was never attached to a window cannot render it. For code-behind or ViewModels, place one `MdSnackbarHost` in the application shell, assign a shared `MdSnackbarService`, and inject that same instance as `IMdSnackbarService`. The service queues consecutive messages and the attached host displays them one at a time.

Direct APIs include `Show`/`Dismiss` on transient components, `ShowAsync`/`Show`/`Close` on `MdDialogHost` and `IMdDialogService`, `Show`/`ShowAsync`/`Dismiss` on `MdSnackbarHost` and `IMdSnackbarService`, `NavigateAsync`/`RefreshAsync`/`GoUpAsync`/`GoHomeAsync`/`GoToRootAsync`/`ConfirmSelection` on `MdFileSystemPicker`, direct collection APIs inherited from Avalonia item controls, and standard routed events. Ecosystem controls additionally expose provider delegates and direct state-machine methods such as `LoadNextPageAsync`, `RefreshAsync`, `SearchAsync`, `SelectDate`, `MoveToTarget`, `Execute`, `Submit`, `PlayAsync`, `NotifyOwnerScrolled`, `MdPinInput.SetCode`/`Clear`, `MdTreeView.ExpandAll`/`CollapseAll`/`SelectById`, `MdTagInput.AddTag`/`RemoveTag`/`ClearTags`, and `MdRating.SetValueFromPosition`.

`MdFileSystemPicker` is an in-process browser built from the library's `MdTextBox`, `MdList`, `MdButton`, `MdIconButton`, `MdCheckBox`, and progress controls (the ordered Avalonia/redesign/Flutter source study is recorded in [FILE_SYSTEM_PICKER_RESEARCH.md](FILE_SYSTEM_PICKER_RESEARCH.md)). It never invokes Avalonia `StorageProvider`, a native file dialog, or Android SAF: `MdFileSystemSelectionEventArgs.Paths` therefore contains normalized absolute paths from `System.IO`. `Mode="File"` keeps directories visible for navigation and applies `FilePatterns` only to files; `Mode="Folder"` lists directories and can confirm the current directory. `PathSegments` exposes normalized root-to-current `MdFileSystemPathSegment` entries used by the breadcrumb-first path surface; the raw path editor appears on demand. `BreadcrumbStyle` selects the standard chevron or connected powerline treatment. All visible labels and automation actions follow inherited `MdLocalization.Culture` (English/Chinese), while locally assigned `Title`, `PathLabel`, action and state-text properties are preserved as host overrides. Expanded layouts add Home/root quick access while compact layouts move Home into the app bar. Entry rows use centered 24-DIP icon viewports inside 48-DIP tonal containers, supporting metadata, a selected container/check, and a fixed confirmation surface. Single and multiple modes both support toggling an already selected row off. This is not a sandbox escape: Android scoped storage, macOS sandbox entitlements, Unix permissions, and Windows ACLs still determine which paths can be enumerated. Start in an app-accessible directory and let the host own any platform permission flow.

`MdVideoPlayer` is the backend-neutral Material transport documented in [VIDEO_PLAYER_RESEARCH.md](VIDEO_PLAYER_RESEARCH.md). Its responsive in-picture shell places a stable rewind/play/forward cluster over `MediaContent` and groups seek, time, volume, rate and fullscreen in one floating bottom dock. `MediaContent` hosts a composited or native renderer; state properties drive the UI, and typed `MdPlaybackRequestEventArgs` carry play/pause/stop, absolute seek/skip targets, mute, volume, rate and fullscreen requests to the backend. The core stays Android- and NativeAOT-safe because it contains no decoder. `Md3.Avalonia.LibVLC` adds `MdLibVlcVideoPlayer`, which adapts LibVLCSharp 3.10.2 for real desktop file/network playback, explicitly maps pause/resume through `SetPause(true/false)`, and restarts ended media for replay; the application must deploy native LibVLC separately. The regular `Md3.Avalonia.Gallery.Desktop` project directly references that adapter and opens its embedded, repository-generated eight-second MP4. The shared Gallery page receives the player through a factory so the Android, headless and NativeAOT builds of that same page do not acquire a LibVLC dependency. Repository developers can use `scripts/run-gallery-libvlc.sh` or `scripts/run-gallery-libvlc.ps1` to discover an installed VLC, configure process-local loader/plugin paths and launch the managed desktop Gallery.

`MdGridSplitter` retains Avalonia's mature resize engine and places the Material visual and interaction shell around it. Set `ShowsPreview="False"` for live pane geometry that follows every drag delta, or `True` for Avalonia's deferred preview guide. Divider state motion never eases the pane geometry behind the pointer and follows the inherited `MdMotion` scheme. Its implementation research, Flutter comparison, hit-target rationale and equalization contract are documented in [GRID_SPLITTER_RESEARCH.md](GRID_SPLITTER_RESEARCH.md).

`MdCarousel` supports a bindable `MdCarouselController`, `ScrollTo`, autoplay, pointer-hover pause and finite or wrapping navigation. Weighted variants use viewport-fitted scroll/controller keylines and constrain `SmallItemWidth` to Material's 40–56 DIP range; pointer selection does not resize an item. `MdRefreshIndicator.RequestRefreshAsync` accepts a cancelable provider-neutral handler.

`MdBreadcrumb.OverflowBehavior` selects `Wrap`, one-line native `Scroll`, or explicit `Collapse` behavior. `ShowTrailingSeparator` is opt-in, and `SeparatorTemplate` can render a custom separator from the `Separator` value. Invoking an ancestor raises `ItemInvoked`/`ItemInvokedCommand` without retaining list selection; the current/last item and disabled items are non-invokable. `MdBreadcrumbPathConverter` turns a slash-separated route into `MdBreadcrumbItem` entries whose `CommandParameter` is the cumulative path prefix; `ExtraTheme` registers it as the `MdBreadcrumbPathConverter` resource for XAML bindings. Ancestors use the theme's `Primary` text color by default (`MdBreadcrumbItem.UseAccentColor` can opt out); the current item uses `OnSurface`, and separators use `OutlineVariant`.

`MdScrollViewer` keeps wheel, trackpad, touch, pen and scrollbar behavior native. Desktop primary-button panning is opt-in through `AllowMouseDrag`; when enabled, focusable controls retain direct manipulation, and custom content takes precedence by handling the press or capturing the pointer. Set `md:MdScrollViewer.SuppressMouseDragScrolling="True"` on any precision-interaction subtree that does neither.

`MdBorderlessWindow.PlatformAdapter` is replaceable. `PreserveNativeBorder` defaults to `true`: Windows uses `WindowDecorations.Full` plus an extended client area so the native `WS_CAPTION`/minimize/maximize styles and DWM state animations remain available, while macOS/Linux retain the portable border-only path. The Material template suppresses its own outer outline whenever a native frame is present, and an empty Avalonia 12 `WindowDrawnDecorations` theme prevents Fluent/Simple title and caption visuals from being layered over the Material title bar. The caption uses the window surface background without a separator rule, and shows `Window.Icon` unless `ShowIcon="False"`. `ShowMinimizeButton`/`ShowMaximizeButton`/`ShowCloseButton` control visibility, while `IsMinimizeButtonEnabled`/`IsMaximizeButtonEnabled`/`IsCloseButtonEnabled` control each action independently. View models use Avalonia `WindowState`; Android resolves to a safe no-op adapter.

## Extra visual controls

The optional `Md3.Avalonia.Extra` package contains visual and ecosystem controls that are intentionally not counted as official Material 3 components:

```xml
<extra:MdBeforeAfter Before="{Binding Original}" After="{Binding Revised}" Position="0.5" />
<extra:MdAnimatedText Text="Loading complete" Effect="Typewriter" AutoPlay="True" />
<extra:MdSpinKit Kind="Wave" Size="40" IsActive="{Binding IsLoading}" />
<extra:MdStaggeredPanel AutoPlay="True" Stagger="0:0:0.06">
  <Border /><Border /><Border />
</extra:MdStaggeredPanel>
<extra:MdWaveform Peaks="{Binding Peaks}" PlayheadPosition="{Binding Position, Mode=TwoWay}" />
```

`MdBeforeAfter.Position` is a two-way value in the `0..1` range and supports pointer dragging plus keyboard adjustment. `MdAnimatedText` exposes `Start()`, `Stop()`, `IsPlaying`, `DisplayText` and `Completed`. `MdSpinKit` is an optional collection of non-Material loading recipes; use `MdLoadingIndicator` for the Material 3 indicator. `MdStaggeredPanel.PlayAsync()` is cancelable and snaps to the final state under Reduced/None motion.

`MdWaveform` draws an audio waveform from peaks and nothing else: `Peaks` is one normalized magnitude per bar, `PlayheadPosition` is two-way in the `0..1` range and moves by pointer drag or by Left, Right, Home and End, `IsMirrored` chooses a centre-mirrored waveform over bottom-anchored bars, and `IsInteractive`, `ShowPlayhead`, `BarGap`, `BarCornerRadius` and `PlayheadThickness` shape the rest. The theme fills `PlayedBrush`, `UnplayedBrush`, `PlayheadBrush` and `BaselineBrush` from `Md.Sys.Color` roles - Primary over SurfaceContainerHighest, the split a Material 3 slider already makes - and `BarCornerRadius` from the shape scale. It deliberately owns no decoder: a waveform that decoded audio would also own a file format, a threading model and a buffer lifetime, none of which is presentation. `MdWaveform.PeaksFromSamples(samples, barCount)` reduces a sample buffer to bars for hosts that have one; with no peaks set the control draws only its baseline, so a page can reserve the space before anything is playable.

Two of its defaults follow Flutter's `audio_waveforms`: `BarCornerRadius` is the full-radius shape token, which the control clamps to half a bar so the bars end in the round cap that package gets from `StrokeCap.round`, and the playhead is a 2dp line - the thickness its `seekLineThickness` uses - carrying a knob that grows while it is held, the way a Material slider's handle does. `ShowPlayheadKnob=false` drops back to a hairline. `IsPulsing` makes the bars pulse in place so a static peak buffer reads as a track that is playing; the pulse travels along the bars and is scaled by each bar's own peak, with `AnimationAmplitude` and `AnimationSpeed` shaping it. It is driven by `TopLevel.RequestAnimationFrame` rather than a timer, so a hidden or switched-off waveform costs nothing, and it holds still - identically, not merely more slowly - under the Reduced and None motion schemes.

`MdChart` is deliberately provider-neutral. It exposes `Series`, `Provider`, `BuildAccessibleTable()` and pointer selection, but does not bundle a chart engine. Applications may wrap a third-party chart control with Material surfaces and tokens instead of adopting a second chart data model.

## Extra layout controls

The same optional package carries a masonry layout, in the two forms a host is likely to want:

```xml
<extra:MdWaterfall MinColumnWidth="220" ItemsSource="{Binding Cards}" />

<extra:MdWaterfallPanel Columns="3" ColumnSpacing="8" ItemSpacing="20">
  <md:MdCard Height="120" /><md:MdCard Height="180" />
</extra:MdWaterfallPanel>
```

`MdWaterfallPanel` is a masonry layout: each child is measured at the column width and then placed into whichever column is currently shortest, so items of differing heights pack without the ragged gaps a grid leaves behind. `Columns` pins the count; left at its default of `0` the count is derived from `MinColumnWidth`, capped at the number of children so a short feed never reserves empty columns. `ColumnSpacing` and `ItemSpacing` are independent. Placement is greedy and never reorders children, which is the right trade for a feed, where the order is the content. Right-to-left flow mirrors the columns.

`AnimateLayout`, on by default, carries children to their new places when the column count changes - the same first-last-invert-play shape an implicit animation uses, timed by the `Md.Sys.Motion` tokens. Only a change of count is animated: a resize that keeps the count has the children tracking it frame by frame already, and starting a transition on every one of those frames would restart it before it finished and leave each child permanently lagging the pointer. Gliding writes a child's `RenderTransform` and `Transitions`, so a host that animates its own children will want it off.

`MdWaterfall` is that panel behind an items control, for a feed bound to a collection rather than one written out by hand. It re-exposes the same four layout numbers and the theme's panel reads them back off the control, so a host never writes an `ItemsPanelTemplate`; replacing `ItemsPanel` is still supported and simply leaves them unused. The theme fills `Background` from `Md.Sys.Color.SurfaceContainerLowest`.

## Floating windows

```xml
<Border Height="440">
  <Grid>
    <!-- the page, still interactive -->
    <extra:MdFloatingWindow Title="Now playing" X="330" Y="36" Width="330"
                            HeaderIcon="{DynamicResource Md.Icon.MusicNote}"
                            Closed="OnPlayerClosed">
      <extra:MdWaveform Height="56" IsPulsing="True" />
    </extra:MdFloatingWindow>
  </Grid>
</Border>
```

`MdFloatingWindow` is a window inside the application rather than above it. It is a child of whatever panel it sits in, so it inherits that page's theme, motion scheme and accessibility tree, and it is clipped by the page the way any child is. The trade-off for not being an operating-system window is that it is bounded by its host, and the control leans into that: dragging is clamped to the visual parent on every move, so the window can be parked anywhere in the panel but never lost off it.

The title bar drags, the lower-right grip resizes, and the minimize button collapses the window to its title bar. Minimizing hides the content without unmounting it, which is the whole point for a hosted player or editor: bindings, scroll position, media and undo history all survive the collapse, and an explicit `Height` is remembered and restored rather than leaving a tall empty box. `X` and `Y` hold the position and the control owns its `Margin` to realise them, so position it through those two properties rather than through the margin. `CanDrag`, `CanResize`, `CanMinimize` and `CanClose` each remove the affordance they name, for a host that wants a fixed panel rather than a window; the close button sets `IsOpen` to false and raises `Closed`.

Collapsing and restoring both travel rather than snap. The height is a spatial motion property and
the content host's opacity is an effect, so the window slides down to its title bar while what is
inside it fades - the surface itself stays opaque, because fading the whole window reads as blinking
rather than as collapsing. Three details of that are worth knowing. The collapsed height is derived
as the window minus its content row rather than named, so it stays correct if the title bar's own
minimum changes, and it is clamped to `MinHeight`. The content is unmounted only once the fade has
landed, and a second click inside that window cancels the pending hide instead of fighting it. And a
window that was measuring itself gets its `Height` back to `NaN` after the travel completes, so it
goes on fitting its content; a window with an explicit height keeps it. Under `MdMotionScheme.Reduced`
the travel is dropped and only the short fade remains, and under `MdMotionScheme.None` both are
instant.

Closing animates out and then unmounts, and reopening inside that window cancels the removal, so a quick close/reopen does not flicker. The surface takes `Md.Sys.Shape.Corner.ExtraLarge` and `Md.Sys.Elevation.Level3`, and steps up to `Level5` while the pointer is holding it - the same raise a dragged card gets. Like everything else in Extra it is not the operating-system Dynamic Island and does not attempt to be one: on iOS and Android that surface is drawn by the system above every application, so no toolkit can host it.

## Borderless/chromeless windows

```xml
<md:MdBorderlessWindow Title="My app"
                       ShowMinimizeButton="True"
                       ShowMaximizeButton="True"
                       ShowCloseButton="True"
                       IsMinimizeButtonEnabled="False"
                       IsMaximizeButtonEnabled="True"
                       IsCloseButtonEnabled="True">
  <views:Shell />
</md:MdBorderlessWindow>
```

`Show*Button` properties remove a caption button from the visual tree. `Is*ButtonEnabled` keeps the button visible but disables its command and applies the disabled state layer/opacity. Programmatic `Minimize()`, `ToggleMaximizeRestore()` and `RequestClose()` enforce the same state as the visual buttons. The two `ShowIcon` properties have different defaults and the window drives the title bar. `MdBorderlessWindow.ShowIcon` defaults to `true`, and the window template passes it down, so a borderless window shows `Window.Icon` in its caption unless `ShowIcon="False"` is set. A bare `MdWindowTitleBar` composed by hand defaults to `false`, because it has no window to inherit an icon from. Use `LeadingContent` for a mark other than the window icon.

The Windows adapter preserves native caption style bits when `PreserveNativeBorder=true`, while the Material template owns the title bar surface. Android uses a safe no-op adapter; desktop-only Gallery pages are not registered in `AndroidGalleryView`.

## Mobile platform integration

### System back gesture

Android has no Escape key. The system back button, and the predictive back gesture from API 33,
arrive as `TopLevel.BackRequested`. `MdBackNavigation` routes that request to the top-most open
Material surface so a dialog, sheet or drawer closes instead of the activity finishing.

`MdDialogHost`, `MdSheetHost`, `MdNavigationDrawer`, `MdSearchView`, `MdMenuAnchor`, `MdFabMenu`,
`MdDatePicker` and `MdTimePicker` register themselves while they are open. Nothing is needed to
opt in, and each surface dismisses on back exactly where it dismisses on Escape — a non-modal
sheet or drawer stays put and lets the request fall through to the page behind it.

Register your own surfaces with `MdBackScope`, which keeps a handler registered only while the
surface is open and only while it is attached to a window:

```csharp
public sealed class MyOverlay : ContentControl
{
    private readonly MdBackScope _backScope;

    public MyOverlay() => _backScope = new MdBackScope(this, OnBackRequested);

    private void UpdateState() => _backScope.Update(IsOpen);

    private bool OnBackRequested()
    {
        if (!IsOpen) return false;
        IsOpen = false;
        return true;   // consumed: the activity is not popped
    }
}
```

Handlers run most recently registered first, so nested surfaces unwind in the order the user
opened them. Returning `false` passes the request on. When no handler consumes it the routed
event stays unhandled and the platform performs its default back action.

`MdBackNavigation.RequestBack(visual)` raises the same chain directly, which is how the behaviour
is tested without a device, and `MdBackNavigation.GetHandlerCount(topLevel)` reports how many
surfaces are currently listening.

### Safe area insets

`MdSafeArea` insets its content past the status bar, a display cutout, the navigation bar and the
gesture handle — the Material counterpart of Flutter's `SafeArea`.

Avalonia already pads the whole root view to the safe area. That is correct but blunt: with it on
a top app bar can never paint its surface behind the status bar, which is what Material asks for.
For an edge-to-edge layout, turn the global padding off and inset only the parts that must stay
clear:

```xml
<Window xmlns:md="https://github.com/ilwren/Md3.Avalonia"
        TopLevel.AutoSafeAreaPadding="False">
  <md:MdScaffold>
    <md:MdScaffold.TopBar>
      <!-- the bar surface runs under the status bar; only its content is inset -->
      <md:MdTopAppBar Headline="Inbox">
        <md:MdTopAppBar.Styles>
          <Style Selector="md|MdTopAppBar">
            <Setter Property="Padding" Value="0" />
          </Style>
        </md:MdTopAppBar.Styles>
      </md:MdTopAppBar>
    </md:MdScaffold.TopBar>
    <md:MdSafeArea Edges="Horizontal,Bottom">
      <views:InboxList />
    </md:MdSafeArea>
  </md:MdScaffold>
</Window>
```

| Member | Purpose |
|---|---|
| `Edges` | Which edges to inset; defaults to `All`. Drop `Top` to let a bar paint behind the status bar. |
| `MinimumPadding` | A per-edge floor, so content keeps breathing room where the inset is zero. |
| `SafeAreaPadding` | The platform's current inset, written from the insets manager as it changes. Settable, so a layout can be previewed and tested without a device. |
| `EffectivePadding` | What is actually applied, after edge filtering and the minimum. |
| `IsSafeAreaActive` | True when the platform reports a non-zero inset on a selected edge. |
| `MdSafeArea.GetPlatformInsets(visual)` | The raw platform inset, or zero where there is no insets manager. |

Insets are reported on Android, iOS and browser. Desktop has no insets manager, where
`SafeAreaPadding` stays zero and `MdSafeArea` collapses to `MinimumPadding` — so the same layout
is safe to ship on every target. `MdKeyboardAvoidingHost` handles the separate case of the soft
keyboard covering a focused field.

## Avalonia DataGrid

`MdDataGrid` in `Md3.Avalonia.Extra` is a Material data table: columns, sorting, filtering,
sticky headers and inline edit. It is not a replacement for Avalonia's `DataGrid` and does not
try to be — no column resizing or reordering, no frozen columns, no grouping, no
auto-generated columns.

When an application needs the real thing, the opt-in `Md3.Avalonia.DataGrid` package themes
Avalonia's own control to Material, so the choice is about capability rather than appearance:

```xml
<Application.Styles>
  <themes:MaterialTheme />
  <themes:MaterialDataGridTheme />
</Application.Styles>
```

It is the only package that depends on `Avalonia.Controls.DataGrid`, so nothing is dragged in
unless it is asked for, and it replaces the control's Fluent theme — do not include both. Like
`MaterialTheme` it is standalone and does not need Avalonia's `FluentTheme`.

The theme is derived from the DataGrid package's own Fluent theme (MIT, see
THIRD-PARTY-NOTICES.md), keeping its control templates and `PART_` names so the control finds
everything it looks up. Changed: M3 tokens throughout, a 56 DIP header and 52 DIP rows with
16 DIP gutters, a divider between rows, `secondaryContainer` for the selected row, Material
scrollbars, and a cell editor that no longer derives from Fluent's `TextBox` theme.

## AvaloniaEdit

`MdRichEditor` in `Md3.Avalonia.Extra` is a chrome-only editor surface: it exposes a Material
command bar and delegates the document to an `IMdRichEditorAdapter`, and it never ships an
editing engine.

For a real code/text editor, use [AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit)
with the opt-in `Md3.Avalonia.AvaloniaEdit` package. `MaterialAvaloniaEditTheme` themes the
editor chrome to Material 3:

```xml
<Application.Styles>
  <themes:MaterialTheme />
  <themes:MaterialAvaloniaEditTheme />
</Application.Styles>
```

It is the only package that depends on `Avalonia.AvaloniaEdit`, and it replaces the editor's
own Fluent/Simple style sheets, so do not include both. Like `MaterialTheme` it is standalone
and does not need Avalonia's `FluentTheme`.

The theme is derived from the AvaloniaEdit package's own theme files (MIT, see
THIRD-PARTY-NOTICES.md), keeping their templates, bindings and `PART_` names — `TextEditor`,
`TextArea`, the search and replace panel, the completion list and the completion tooltip.
Changed: M3 tokens throughout, `MdScrollViewer` scrollbars, `primary` selection and caret ink,
`onSurfaceVariant` line numbers, and `tertiaryContainer` search-result highlights.

Everything the editor instantiates but does not expose — the compact search fields, the search
action buttons, the match-case/whole-word/regex option toggles, the expander and the completion
rows — is styled by keyed `Md.Comp.AvaloniaEdit.*` control themes that only the editor's own
templates reference. Registering this theme therefore never replaces Avalonia's native
`Button`, `ToggleButton`, `TextBox` or `ListBox` themes for the rest of the application.


## AvaloniaHex

For binary and hex inspection, use [AvaloniaHex](https://github.com/Washi1337/AvaloniaHex) with
the opt-in `Md3.Avalonia.AvaloniaHex` package:

```xml
<Application.Styles>
  <themes:MaterialTheme />
  <themes:MaterialAvaloniaHexTheme />
</Application.Styles>
```

It is the only package that depends on `AvaloniaHex`, and it replaces the library's own
Fluent/Simple style sheets, so do not include both. Like `MaterialTheme` it is standalone and
does not need Avalonia's `FluentTheme`.

The theme fills the resource keys AvaloniaHex publishes - `HexForegroundBase`, `OffsetForeground`,
the header, current-line, caret and selection brushes and pens - from `Md.Sys` tokens, and gives
`HexEditor` its own control theme: a `SurfaceContainerLow` surface with an `OutlineVariant` outline,
an ExtraSmall corner and `MdScrollViewer` scrollbars. `onSurface` is the text, `onSurfaceVariant`
the offsets and column headers, `surfaceContainerHigh`/`surfaceContainerHighest` the header strip
and the current line, and the caret and selection use the same primary wash the rest of the library
uses for selection.

## Dock

For docking layouts - tool panes, document tabs and floating windows - use
[Dock](https://github.com/wieslawsoltes/Dock) with the opt-in `Md3.Avalonia.Dock` package:

```xml
<Application.Styles>
  <themes:MaterialTheme />
  <themes:MaterialDockTheme />
</Application.Styles>
```

It is the only package that depends on `Dock.Avalonia` and `Dock.Avalonia.Themes.Fluent`, and it
replaces `DockFluentTheme`, so do not include both. Like `MaterialTheme` it is standalone and does
not need Avalonia's `FluentTheme`.

Being standalone is why it also carries the one implicit native theme it cannot avoid: Dock's
keyed chrome themes (`ButtonCreateDocument`, `ChromeButton`, `DockDrawnCaptionButton`) are
`BasedOn="{StaticResource {x:Type Button}}"`, and neither `MaterialTheme` (which leaves Avalonia's
native controls alone) nor an absent `FluentTheme` supplies that key, so
`Themes/Controls/MaterialButton.axaml` adds a Material text-button theme for `Button`.

`Button` is the only native type that theme touches, so in the documented application-wide setup the
application's own plain `Button`s pick up that Material text-button look too - that is the price of
Dock chrome working without Avalonia's `FluentTheme`. Nothing else changes: `TextBox`,
`ScrollViewer`, `ListBox` and the other native controls are never touched, and
`Registering_The_Dock_Theme_Only_Adds_The_Base_Button_Theme_Docks_Chrome_Needs` in the headless
suite pins both halves of that contract (which is also why the isolation tests of the other opt-in
packages check their own types rather than `Button`). Keep Dock's chrome in a scope of its own - the
gallery page registers the theme in `UserControl.Styles` rather than in `Application.Styles` - if
you would rather have the application's own buttons keep the host theme. Three Dock window types (`DockAdornerWindow`,
`DragPreviewWindow`, `PinnedDockWindow`) base themselves on the `{x:Type Window}` theme, which
comes from the core `MaterialTheme`.

The theme maps Dock's own accents onto `Md.Sys` tokens: the `DockApplicationAccent*` brushes and
every `DockTab*`, `DockSurface*`, `DockBorder*`, `DockSplitter*`, `DockChrome*` and `DockTarget*`
key follow the Material scheme, hover and pressed states are `onSurface` state layers, the
selected document tab is a `Primary` container with `OnPrimary` ink, and the window-close hover is
`ErrorContainer`. Structurally it differs from `DockFluentTheme` on purpose: instead of forking
Dock's templates it includes the pinned Fluent package's control dictionaries by `avares://` URI
and adds `Accents/Material.axaml`, so Dock keeps owning its templates and part names. Dock's
absence of a density/preset API means light and dark, contrast and density all come from the core
`MaterialTheme`.

## Tabalonia

For draggable tabs that can be reordered, torn out into their own window and attached to another
strip, use [Tabalonia](https://github.com/egorozh/Tabalonia) with the opt-in `Md3.Avalonia.Tabalonia`
package:

```xml
<Application.Styles>
  <themes:MaterialTheme />
  <themes:MaterialTabaloniaTheme />
</Application.Styles>
```

It is the only package that depends on `Tabalonia`, and it replaces both of Tabalonia's own themes
(`Tabalonia.Themes.Custom.CustomTheme` and `Tabalonia.Themes.Fluent.FluentTheme`), so do not include
either. Like `MaterialTheme` it is standalone and does not need Avalonia's `FluentTheme`. Keep using
`TabsControl` as usual - `ItemsSource` must implement `IList`, because Tabalonia reorders and removes
items through it.

The theme retokenizes the 19 brushes of Tabalonia's `Themes/Custom/Brushes.axaml`: the strip and the
tab containers sit on `Surface`, an idle tab is an `OnSurfaceVariant` label on a transparent
container, hover is an `OnSurface` state layer (0.08, or 0.04 in an inactive window), the selected tab
is a `Primary` container with `OnPrimary` ink, and the separators, the close glyph and the add-a-tab
button use `OutlineVariant`, `OnSurfaceVariant` and `Primary`. It also fills the six keys Tabalonia's
Custom templates read but its own brush dictionary never declares - `TabItemHeaderForeground{Unselected,
UnselectedPointerOver,Selected,SelectedPointerOver}`, `TabItemHeaderMargin` and
`TabItemHeaderThemeFontWeight` come from Avalonia's `FluentTheme` in upstream's setup - plus the idle
foreground of the add-a-tab button, which upstream leaves to the host, and `OnPrimary` for the close
glyph of the selected (filled) tab. Structurally it differs from `CustomTheme` on purpose: instead of
forking templates it merges `Themes/Custom/TabsControl.axaml` and style-includes
`Themes/Custom/Buttons.axaml` and `Themes/Custom/DragTabItem.axaml` by `avares://` URI and adds
`Themes/Accents/Material.axaml`, so Tabalonia keeps owning its templates and part names. Because the
accents file holds no theme dictionaries of its own, light and dark both come from the core
`MaterialTheme` variant.

## Markdown

For Markdown documents rendered with
[Markdown.Avalonia](https://github.com/whistyun/Markdown.Avalonia), use the opt-in
`Md3.Avalonia.Markdown` package. It is the only package that depends on `Markdown.Avalonia.Tight`
(the core-only package; its assembly is `Markdown.Avalonia`) and, unlike every other theme package,
it is *not* registered in `Application.Styles`:

```xml
<md:MarkdownScrollViewer Markdown="{Binding Document}">
  <md:MarkdownScrollViewer.MarkdownStyle>
    <themes:MaterialMarkdownTheme />
  </md:MarkdownScrollViewer.MarkdownStyle>
</md:MarkdownScrollViewer>
```

That shape comes from upstream, not from this package. `MarkdownScrollViewer` picks its own style
collection and inserts it into its *own* `Styles` collection, so the collection closest to the
control wins and an application-level `StyleInclude` can never restyle a viewer; the five built-in
collections are selected by reflection over `MarkdownStyle`'s static properties through
`MarkdownStyleName`, and the `INamedStyle` marker they implement is internal, so a third party
cannot add a name to that list. The only seam is the `MarkdownStyle` property itself, and it takes
one instance per viewer - the built-in collections also hand out a fresh object per access.

Read the collection as a retokenization of the package's own `MarkdownStyleFluentTheme` (the
collection of the Avalonia 12 line, which is what the pinned package ships): every selector it
styles is styled here with the same shape. Prose is `OnSurfaceVariant` on `Surface`, headings and
code are `OnSurface`, links are `Primary` underlined and switch to `Tertiary` on hover (upstream
asks for the complementary accent), the blockquote bar is `Primary`, cell and divider rules are
`OutlineVariant`, code blocks sit on `SurfaceContainerLow`, notes, table headers and inline code on
`SurfaceContainerHighest`, and the red marker upstream paints on an unhandled `:::` container uses
the `Error` role. Sizes and weights come from `Md.Sys.TypeScale`: `HeadlineLarge` for h1 through
`TitleSmall` for h6, `BodyLarge` for prose, `BodyMedium` for code blocks and table cells,
`LabelLarge` for table headers. Headings follow the weights M3 gives those roles - Regular down to
`TitleLarge`, Medium for `TitleMedium`/`TitleSmall` and table headers, the same split `MdText`'s own
ControlTheme uses - rather than Fluent's Light, and the document is typeset in
`Md.Sys.Typeface.Plain` rather than inheriting the host application's font. Layout follows upstream
except where Material has a shape of its own: the code block is a `SurfaceContainerLow` card with an
8 DIP radius and 12/10 DIP padding instead of Fluent's two hairlines, table cells get 10/6 DIP
padding instead of 2, and the `Error` marker on an unhandled `:::` container is a 3 DIP frame. The
code block's mono font is a family list of the collection's own - Material has no mono typeface
token - and it has to be a list. An inline code span takes its family from
`ColorTextBlock.Avalonia.CCode.MonospaceFontFamily`, whose default is whichever family
`FontFamilyCollector.TryGetMonospace()` picked: the first entry of the system font collection whose
name contains one of eight substrings, in enumeration order rather than preference order, and
therefore a different family on a different machine. A family taken from that collection carries the
`fonts:SystemFonts` key, and `FontManager` gives a keyed single family no fallback - one the machine
advertises but cannot shape throws at measure time, which is the intermittent
`Could not create glyphTypeface. Font family: Consolas` this page used to produce in CI. A list is a
composite key, and a composite key falls back to the default font, so code cannot fail to shape. The
list also has to be set on `MonospaceFontFamily` rather than on `FontFamily`: `CCode`'s constructor
binds `FontFamily` to that property, and a binding outranks a style setter, so upstream's own stack -
lowercase, and matched ordinally against family names that are not - never reached a span.

Three gaps the built-in collections leave are filled here, and each is asserted by the headless
suite:

- Selection is `Md.Sys.Color.PrimarySelection.Brush`. Upstream's collections define a
  `MarkdownScrollViewer.SelectionBrush` resource because they have no token to point at; the
  collection sets the viewer's and every block's `SelectionBrush` property from the Material token
  instead, which keeps `ComputedSelectionBrush` off its cyan fallback and still leaves the property
  overridable per viewer.
- h5 and h6 have classes and are emitted by the parser, but no built-in collection styles them, so
  they complete the type-scale ladder here.
- `mdc|Rule` draws its line with `Foreground`, which is otherwise inherited straight from the
  window; the M3 divider role is `OutlineVariant`.

Two things to know when the document is not the only thing on the page. `Markdown.Avalonia`'s
`SyntaxHigh` plugin is not referenced by this package: code blocks are the Tight package's plain
`TextBlock`, not an AvaloniaEdit editor. An application that adds the plugin - which is what the
`Markdown.Avalonia` metapackage does - should also add that plugin's own appendix styles for
`avedit|TextEditor`, `Label.LangInfo` and `Button.CopyButton`, because the plugin only injects them
for upstream's named collections, not for a third-party one. And because the collection replaces
the viewer's own, scope it per page (`<UserControl.Styles>`, as the gallery's Markdown page does)
when the rest of the application should keep a different document look; nothing else in the
application is affected either way, since the selectors are all scoped to the viewer's document
tree.

## LiveCharts2

For charts drawn with [LiveCharts2](https://github.com/Live-Charts/LiveCharts2), use the opt-in
`Md3.Avalonia.LiveCharts2` package. It is the only package that depends on
`LiveChartsCore.SkiaSharpView.Avalonia`, and the only Material theme in this repository that is not
an Avalonia style collection at all:

```csharp
// Once, before the first chart view is created.
LiveCharts.Configure(settings => settings.AddMaterialTheme());
```

That shape comes from upstream, not from this package. LiveCharts has no per-control style seam: a
chart resolves its look from the single `Theme` held by `LiveCharts.DefaultSettings`, so there is
nothing to register in `Application.Styles` and nothing that could leak into an application that
did not configure it. `MaterialLiveChartsTheme.AddMaterialTheme` is `LiveChartsSettings
.AddDefaultTheme` with `ApplyMaterialRules` appended - the rules run after LiveCharts' own, so the
tooltip and legend factories, the animation speed and easing, and every geometry upstream already
decides stay exactly as they ship. Appending is only possible for planes and series: a theme keeps
one rule per visual-element type in a dictionary that rejects a second rule for the same type, which
LiveCharts' default theme already registers for labels, needles and angular ticks, so those three
take the slot over. An application that builds its own theme calls
`ApplyMaterialRules` on it instead, and `SeriesPalette(bool dark)` hands out the same eight colours
for a provider that takes one colour per series.

The palette is eight `Md.Sys.Color` roles in the order LiveCharts walks them - `Primary`,
`Tertiary`, `Secondary`, `Error`, `PrimaryContainer`, `TertiaryContainer`, `InversePrimary`,
`Outline`. The first four give a four-hue palette the way LiveCharts' own palette does; the rest are
further tones of the same accent hues, so a chart with more than four series still reads as Material
instead of inventing colours. Every colour is read for the variant the chart is drawing in, which the
Avalonia views take from the application's theme variant, so a Material chart follows the light/dark
switch. An application that does not register `MaterialTheme` gets the M3 baseline values the package
carries instead.

What the rules write, and the token each value comes from:

- Axes: `LabelMedium` for tick labels and `LabelLarge` for axis names, both in `OnSurfaceVariant`;
  separators `OutlineVariant` with upstream's horizontal-only geometry and 12 DIP padding. A polar
  plane gets the full grid and a `Surface` label background.
- Line and step-line series: a 3 DIP stroke in the series colour, upstream's area fill cleared, and
  8 DIP markers filled with `Surface` inside a ring of the series colour. A stacked line keeps its
  fill - a stacked area is read as one shape - and drops its markers.
- Bars and columns: the fill in the series colour, a 4 DIP rounded end (`Rx`/`Ry`) and 6 DIP between
  bars; stacked bars and columns keep square corners so the segments read as one bar. Data labels
  drawn inside the fill take `OnPrimary`, labels outside it `OnSurfaceVariant`, as do error bars.
- Pie and donut: a 2 DIP `Surface` stroke, which is the surface showing through between slices
  rather than a border. A gauge is a `SurfaceContainerHighest` track with a `Primary` indicator -
  the shape Material's own progress ring uses.
- Candlesticks: `Tertiary` rising and `Error` falling, replacing upstream's hardcoded green and red.
- Visual elements: gauge labels and tick labels are painted in `OnSurfaceVariant`, and so is the
  needle of a gauge, which keeps upstream's 20 DIP width - it is the one shape Material has no
  counterpart for. These are the rules that take over LiveCharts' own slots rather than appending.
- Tooltips and legends: an `InverseSurface` card with `InverseOnSurface` text and a `DropShadow`,
  the legend without a background, both typeset at `LabelLarge`. The chart's canvas is cleared to
  `Surface` so a chart inside a card blends with the card.

A chart view's own overrides still win: `ChartTheme` on a view replaces the configured theme
entirely, and the view's tooltip and legend paints are preferred over the theme's. So does any paint
a series sets itself, because theme rules never overwrite a value that is already there where
LiveCharts documents user values as taking precedence.

The pinned version is **2.1.0-dev-798**: LiveCharts2's Avalonia 12 line is currently published only
as `2.1.0-dev` prereleases (`net8.0`/`net10.0`, declaring `Avalonia >= 12.0.0`, which restores
against this repository's 12.1.2 baseline). The stable `2.0.x` line is the Avalonia 11 build - it
declares `Avalonia >= 11.0.0`, targets `netstandard2.0`/`net6.0`/`net8.0` and does not load against
Avalonia 12 - so it is not an option here even though it sorts higher. Upgrading the package means
re-reading `Themes/Theme.cs`, `Themes/LiveChartsStylerExtensions.cs` and the SkiaSharp
`ThemesExtensions.cs` of the new build; `MdLiveChartsThemeTests` pins the palette, the rules and the
dark-variant reads that would move.

`MdChart` in the core package stays provider neutral: it keeps drawing through whichever chart
engine an application picks, and this package only styles LiveCharts2.

## ScottPlot

For plots drawn with [ScottPlot](https://github.com/ScottPlot/ScottPlot), use the opt-in
`Md3.Avalonia.ScottPlot` package. It is the only package that depends on `ScottPlot.Avalonia`, and -
like the LiveCharts2 package - it is not an Avalonia style collection at all, because ScottPlot has
no style seam either: a `Plot` carries its own colours and its own type, so there is nothing to
register in `Application.Styles` and nothing that could leak into a chart this package was not asked
to style.

```csharp
// Once, in the page or window that owns the control. The theme follows the application's variant
// from there, because ScottPlot bakes colours into a plot when they are set.
MaterialScottPlotTheme.Follow(avaPlot);

// A plot that never reaches the screen takes the rules directly.
MaterialScottPlotTheme.Apply(plot, dark: false);
```

`MaterialScottPlotTheme.Follow` applies the rules for the variant the control actually has and
re-applies them from the control's own `ActualThemeVariantChanged` event, so the subscription lives
exactly as long as the control does. The instance `Apply` methods and `Apply(Plot, bool)` are for an
application that knows the variant itself, including plots that are drawn off screen.

What the theme writes, and the token each value comes from:

- Surfaces: the figure and the data area are both `Surface`, so a chart inside a card blends with the
  card instead of sitting on ScottPlot's white default.
- Ink: `Plot.Axes.Color` paints the frame, the tick marks, the tick labels, the axis names and the
  title in `OnSurfaceVariant` in one call.
- Grid: major lines are `OutlineVariant` and minor lines are switched off, so the grid reads as one
  divider weight.
- Legend: a `SurfaceContainerHigh` panel with an `OutlineVariant` outline and `OnSurfaceVariant`
  text - a surface container floating over the data rather than upstream's white box.
- Type scale: `TitleMedium` for the chart title, `LabelLarge` for axis names and legend entries, and
  `LabelMedium` for tick labels, all in the family the `Md.Sys.Typeface.Plain` token names.
- Series: the same eight-role palette the LiveCharts2 theme walks - `Primary`, `Tertiary`,
  `Secondary`, `Error`, `PrimaryContainer`, `TertiaryContainer`, `InversePrimary`, `Outline` - set
  through the plot's own plottable adder, so every plottable added afterwards takes it.

Everything upstream decides stays as it ships - axis frames, tick generation, layout, interaction -
and a plot this package was not handed keeps every ScottPlot default. Nothing is process-wide: the
theme never assigns `Fonts.Default`, so it cannot change the type of plots it was not asked to style.

The pinned version is **5.1.59**: it targets `net8.0`/`net10.0` and declares `Avalonia >= 12.0.0` and
`ScottPlot >= 5.1.59`, which restores against this repository's 12.1.2 baseline. Upgrading the
package means re-reading `Plot.cs`, `Primitives/PlotStyle.cs`, `Primitives/LabelStyle.cs` and the
`ScottPlot.Avalonia` `AvaPlot` control; `MdScottPlotThemeTests` pins the palette, the rules, the
variant tracking, the baselines and the fact that a plot that was not asked for keeps its defaults.

`MdChart` in the core package stays provider neutral: it keeps drawing through whichever chart
engine an application picks, and this package only styles ScottPlot.

One deployment note, because it is invisible until it breaks: `ScottPlot` depends on
`SkiaSharp.NativeAssets.Linux.NoDependencies`, a `libSkiaSharp.so` built without fontconfig for
minimal containers. It installs over the same `runtimes/linux-x64/native/libSkiaSharp.so` path as
the fontconfig build `Avalonia.Skia` needs, and if that copy wins the host loses system font
enumeration entirely - text in any family the app does not embed then fails with "Could not create
glyphTypeface". A Linux head that wants system fonts should exclude it:

```xml
<PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" ExcludeAssets="all" />
```

The gallery's desktop head and the headless suite both do.

## Property grid

For an object inspector built with
[bodong.Avalonia.PropertyGrid](https://github.com/bodong1987/Avalonia.PropertyGrid), use the opt-in
`Md3.Avalonia.PropertyGrid` package. It is the only package that depends on
`bodong.Avalonia.PropertyGrid`, and unlike the chart packages it is an ordinary style collection:

```xml
<Application.Styles>
  <themes:MaterialTheme />
  <themes:MaterialPropertyGridTheme />
</Application.Styles>
```

It can also be registered on the `Styles` of the window or page that owns the grid, which is what
the gallery does.

The reason the package exists is easy to miss. `PropertyGrid` sets no colours of its own: its 604
lines of embedded styles only lay out templates, and every cell editor is a plain Avalonia control
the grid builds at runtime - `TextBox`, `CheckBox`, `ComboBox`, `ToggleButton`, `RadioButton`,
`NumericUpDown`, `Slider`, `ProgressBar`, `DatePicker`, `TimePicker`, `Button`, `Expander`,
`GridSplitter`, `ScrollViewer`, `ContextMenu`, `MenuItem`, `Separator`, `ToolTip`, `ItemsControl`,
`ContentControl` and `DataValidationErrors`. All of those are `TemplatedControl`s, so in an
application that has no native control theme - which is the state this repository's own themes
leave an app in, by design - the grid draws its chrome and none of its cells.

What the package supplies is 32 keyed `Md.Comp.PropertyGrid.*` control themes, one per role: the
pass-through containers, Material filled fields (40 DIP, `SurfaceContainerHighest` fill, `Outline`
hairline that becomes a 2 DIP `Primary` rule on focus, `Error` when the cell fails validation),
buttons, filter chips, checkboxes, radio buttons, drop-downs and their items, the numeric field and
its spinners, sliders and their thumbs, progress bars, the date and time fields with their wheel
presenters, category expanders, the name/value splitter, the scroller, the context menu with its
items and separators, and tooltips. Every colour is a `Md.Sys.Color.*` token, every size a
`Md.Sys.TypeScale.*` value, every corner a `Md.Sys.Shape.Corner.*` one, and the property list
scrolls on the core package's `MdScrollBar`.

It stays local the same way `Md3.Avalonia.AvaloniaEdit` does: none of those themes is registered for
its native type. One style per native type hands the theme over, and every one of those selectors
requires a `PropertyGrid` ancestor, so registering this theme never replaces Avalonia's native
`TextBox`, `ComboBox` or `DatePicker` theme for the rest of the application - a control outside a
grid keeps having no theme at all. `MdPropertyGridThemeTests` pins that, including the fact that the
package defines no implicit control theme.

Three things stay upstream's, and two of them cannot be changed from a style:

- The 0.5 DIP `Gray` hairline the grid's own templates draw around the list, radio-button and
  toggle-group editors, and the same hairline its expandable-object factory sets from C#. Both are
  local values, so no style reaches them; forking the grid's eleven embedded control styles to
  repaint them would drift from the part names the grid looks up.
- Avalonia's `ColorPicker` in the colour cells, whose Fluent and Simple sheets the grid already
  style-includes for its own subtree. This package therefore has no ColorPicker dependency.
- The date and time flyouts keep the control's wheel mechanics inside a Material surface card. A
  Material date picker is a calendar, and re-implementing `DatePickerPresenter` as one is a
  different project.

The pinned version is **12.1.1.1**: it targets `net8.0`/`net9.0`/`net10.0` and declares
`Avalonia >= 12.1.1`, `Avalonia.Controls.ColorPicker >= 12.1.1` and `bodong.PropertyModels >= 12.0.0`,
so it restores against this repository's 12.1.2 baseline. The grid iterates quickly, so upgrading it
means re-reading `Controls/*.axaml` and the built-in cell-edit factories for new controls and new
part names; `MdPropertyGridThemeTests` pins the cell editors, the chrome, the date flyout's parts
and the editing behaviour that would move.

## Node editor

For a graph or node editor built with
[NodifyM.Avalonia](https://github.com/Maklith/NodifyM.Avalonia), use the opt-in
`Md3.Avalonia.NodifyM` package. It is the only package that depends on `NodifyM.Avalonia`, and like
the property grid it is an ordinary style collection:

```xml
<Application.Styles>
  <themes:MaterialTheme />
  <themes:MaterialNodifyMTheme />
</Application.Styles>
```

It can also be registered on the `Styles` of the window or page that owns the editor, which is what
the gallery does. Registering it replaces `NodifyM.Avalonia`'s own `Styles/ControlStyles.axaml`, so
do not include that as well.

Upstream's single style file carries both the brushes its templates read and the eleven sheets that
style its controls. This theme takes the two halves apart: it style-includes the eleven per-control
sheets as upstream lists them, and its own resources merge upstream's data templates and brush
dictionary followed by `Themes/Accents/Material.axaml`, which redeclares 43 of the 44 brushes against
`Md.Sys.Color.*`. Dictionaries are searched last-merged first, so the accent answers in upstream's
place and the templates keep reading the keys they were written against. Nothing here is a control
theme, and NodifyM's controls are all its own types, so no native control theme is replaced anywhere
in the application.

The values upstream sets as literals rather than brush keys are set again by additive styles: a
node's ink, the borders it paints on hover and when selected, the background behind its items, a node
group's four inline colours, the grid rule's brush and the six brushes the minimap builds from raw
colour keys. The node's 3 DIP corner radius stays upstream's - it is baked into the template.

The pinned version is **12.0.0**: it targets `net8.0` and declares `Avalonia >= 12.0.2`,
`CommunityToolkit.Mvvm >= 8.2.2` and `System.Reactive >= 6.0.1`, so it restores against this
repository's 12.1.2 baseline. Upgrading it means re-reading `ResourceDictionaries/Brushes.axaml` for
new keys and `Controls/*.axaml` for new templates and part names; `MdNodifyMThemeTests` pins the
brushes the realized controls actually use, the additive styles and the variant switch that would
move.

## Map

For maps drawn with [Mapsui](https://github.com/Mapsui/Mapsui), use the opt-in `Md3.Avalonia.Mapsui`
package. It is the only package that depends on `Mapsui.Avalonia12`, and - like the two chart
providers - it is not an Avalonia style collection at all: `MapControl` draws its own surface from
the colours a `Map` and its layers carry, and asks no resource host for anything, so there is nothing
to register in `Application.Styles` and nothing that could leak into a map this package was not asked
to style.

```csharp
var theme = new MaterialMapsuiTheme(dark: false);

// A layer takes the styles it is drawn with. Both in one collection, so a feature is drawn as a
// marker and labelled.
map.Layers.Add(new MemoryLayer
{
    Name = "Sensors",
    Features = features,
    Style = new StyleCollection { Styles = { theme.VectorStyle(), theme.LabelStyle() } },
});

// Once, in the page or window that owns the control.
MaterialMapsuiTheme.Follow(mapControl);
```

`MaterialMapsuiTheme.Follow` applies the rules for the variant the control actually has and
re-applies them from the control's own `ActualThemeVariantChanged` event, so the subscription lives
exactly as long as the control does. The instance `Apply` methods and `Apply(Map, bool)` are for an
application that knows the variant itself, including a map that is rendered off screen. `Apply` on a
`Map` sets the canvas and styles the layers nobody has styled. That is not the same as a null style -
`BaseLayer`'s constructor gives every layer a grey outline, a black line and a white fill - so a layer
counts as unstyled while its style is still that default, and a layer whose style was chosen keeps it:
a layer's style is its content's business and a thematic map picks per feature. Only the colours are
replaced; what a layer shows, and at which resolutions, travels across.

What the theme writes, and the token each value comes from:

- Canvas: `Map.BackColor` is `Surface`, so a map inside a card blends with the card instead of
  sitting on Mapsui's white default.
- Boundary: a polygon's outline is a 1 DIP hairline in `Outline`, the weight a Material divider
  carries.
- Line: a line feature is a 1.5 DIP stroke in `OnSurfaceVariant`, the medium-emphasis ink.
- Region: a polygon's fill is `Primary` at twelve percent - the tint a Material container lays over a
  surface, mixed into the colour because Mapsui's brush takes a colour rather than an opacity.
- Label: a `SurfaceContainer` chip with an `OutlineVariant` border, the `Md.Sys.Shape.Corner.ExtraSmall`
  radius and `OnSurface` ink, typeset at `LabelMedium` in the family the `Md.Sys.Typeface.Plain` token
  names.
- Classes: the same eight-role palette the chart providers walk - `Primary`, `Tertiary`, `Secondary`,
  `Error`, `PrimaryContainer`, `TertiaryContainer`, `InversePrimary`, `Outline` - as
  `SeriesPalette(dark)`, for a thematic map that needs more colours than the vector style carries.

Everything upstream decides stays as it ships - projection, tile fetching, navigation, hit testing,
animation - and a map this package was not handed keeps every Mapsui default. Core Mapsui draws lines
and polygons through `Mapsui.Nts`, which this package does not depend on; the gallery demo is points
and labels for that reason, and it holds its features in memory so the page never fetches a tile.

The pinned version is **`Mapsui.Avalonia12` 5.1.0**: the Avalonia 12 package (`net8.0`, declaring
`Avalonia >= 12.0.2`, `Avalonia.Skia >= 12.0.2` and `SkiaSharp >= 3.119.4`, all of which restore
against this repository's 12.1.2 and 3.119.4 pins). `Mapsui.Avalonia` is the Avalonia 11 build with
identical XAML and C#, so it is not an option here. Upgrading the package means re-reading
`Mapsui.UI.Shared/MapControl.cs`, `Map.cs` and `Mapsui/Styles`; `MdMapsuiThemeTests` pins the
palette, the canvas, both styles and the dark-variant reads that would move.

## Signature pad

For handwriting - a signature, an annotation, a sketch - use the opt-in `Md3.Avalonia.InkCanvas`
package. It is the only package that depends on `DotNetCampus.AvaloniaInkCanvas`, and the control it
wraps is like the map and the two chart providers rather than like a text editor: `InkCanvas` draws
its own surface, rendering every stroke with Skia, and registers no styles, so there is nothing to
put in `Application.Styles` for a bare canvas and nothing that could leak into an ink surface this
package was not asked to style.

Upstream's defaults are red ink ten units thick, and that is the whole reason a theme is needed. The
entry point writes the two settings that decide what ink looks like and leaves the rest - eraser
sizing, stylus pressure, the bitmap cache - to the host and to upstream:

```csharp
// Once, after the canvas is in the tree so the tokens resolve.
MaterialInkCanvasTheme.Follow(inkCanvas);

// Or for a host that knows the variant itself, including a canvas rendered off screen.
MaterialInkCanvasTheme.Apply(inkCanvas, dark: false);
```

`Follow` applies the rules for the variant the control actually has and re-applies them from the
control's own `ActualThemeVariantChanged` event, so the subscription lives exactly as long as the
control does. The ink is `Md.Sys.Color.OnSurface`, which is the only colour that reads as "written
on this surface" in both variants. A brush that is not solid has no Skia colour, so it falls back to
black instead of throwing.

`MdSignaturePad` is the same ink with the chrome a signature needs around it, and it is what the
registered `MaterialInkCanvasTheme` style collection styles:

```xml
<themes:MaterialTheme />
<themes:MaterialInkCanvasTheme />   <!-- opt-in package, after MaterialTheme -->

<md:MdSignaturePad Height="260"
                   Placeholder="Sign above the line"
                   StrokeCompleted="OnStrokeCompleted"
                   Cleared="OnCleared" />
```

| Member | Default | Notes |
| --- | --- | --- |
| `InkBrush` | `Md.Sys.Color.OnSurface.Brush` | Pushed into the canvas's own `InkColor`. |
| `InkThickness` | 3 | Coerced to a positive number, so a bound zero cannot make the pen vanish. |
| `IsErasing` | `false` | Switches the canvas to `InkCanvasEditingMode.EraseByPoint`. |
| `ShowBaseline` / `ShowClearButton` | `true` / `true` | The line to sign on, and the clear affordance. |
| `Placeholder` | `null` | Shown only while the pad is empty. |
| `StrokeCount` / `IsEmpty` | read-only | Read off the canvas, so they are always true to it. |
| `Clear()` | | Removes every stroke and raises `Cleared`. |

Two pseudo-classes carry the state a template or an application wants to key off: `:empty` before the
first stroke, `:erasing` while the eraser is selected. `StrokeCompleted` fires when a stroke is
committed, which is the moment a signature form becomes valid; it carries no stroke data, because the
host usually needs only that one finished, and reading the strokes themselves is upstream's API.

Three things about this control are forced by upstream and are worth knowing before you ship a
signature flow:

- **There is no clear upstream.** `AvaloniaSkiaInkCanvas` exposes add and remove but no clear, so
  `Clear()` snapshots the stroke list - removing mutates the very list being walked - removes and
  disposes each stroke, and then drops the bitmap cache, because removing a stroke does not
  invalidate it.
- **`IsErasing` is only safe between strokes.** Upstream throws `InvalidOperationException` if
  `EditingMode` changes while a stroke is in progress and keeps that state private, so the pad cannot
  check it. The exception propagates rather than being swallowed, because silently ignoring a mode
  change is worse than reporting one. A toolbar button cannot land mid-stroke, which is why the
  gallery's eraser affordance is a button and not a hold.
- **It is not a whiteboard.** Upstream states plainly that it is not built for high-performance
  large-canvas use; a signature pad is the workload it is designed for.

The package declares `IsTrimmable`, `IsAotCompatible` and `EnableTrimAnalyzer`, and adds no
reflection. `MdSignaturePadThemeTests` pins the ink colour and thickness both variants read, that the
settings upstream owns survive an `Apply`, that a gradient brush falls back instead of throwing, that
real pointer input commits a stroke and hides the placeholder, that `Clear` empties the canvas and
raises its event, and that erasing reaches the canvas mode - so upgrading the package means
re-reading `InkCanvas.cs`, `AvaloniaSkiaInkCanvas.cs` and `AvaloniaSkiaInkCanvasSettings.cs`, which
is where every member this section names lives.

## Trimming

The eighteen non-LibVLC packages declare `IsTrimmable` and `IsAotCompatible` and build with the IL2xxx/IL3xxx analyzers enabled. The optional `Md3.Avalonia.LibVLC` native desktop adapter explicitly declares neither compatibility and is excluded from the core/Android dependency graph. These declarations describe the library assemblies; application-owned reflection, dynamic XAML, and third-party code may still require preservation or selectors in the consuming app.

The Linux x64 AOT probe publishes the desktop Gallery with `PublishAot=true`, checks that the output is a native executable without a managed Gallery assembly beside it, and smoke-runs that binary under xvfb. Its `file-system-picker` scenario opens the actual Gallery page inside the native process, materializes the control template and compiled bindings, enumerates and filters a temporary real directory, selects an entry, confirms its normalized path, and must write a success marker before the workflow passes. The probe reports linker diagnostics but does not claim zero diagnostics for every downstream app or validate Windows, macOS, or Android AOT.

Two things in a UI library genuinely need reflection, and both are handled:

- **Theme JSON** (`MdThemeJson`) uses a source-generated serializer context. The contract is
  closed and owned by the library, so nothing is discovered at runtime.
- **String property paths** are the library's only remaining reflective step, because the model
  type belongs to the application. Every control that accepts one also accepts a delegate that
  reaches the same data directly:

| Control | Path property | Reflection-free alternative |
|---|---|---|
| `MdDataGrid` / `MdDataGridColumn` | `PropertyName` | `ValueSelector` to read, `ValueParser` + `ValueSetter` to edit |
| `MdAsyncSelect` | `DisplayMemberPath` | `DisplaySelector` |
| `MdSearchView` | `ResultDisplayMemberPath` | `ResultDisplaySelector` |

```csharp
// Reflective: needs Invoice.Number and Invoice.Total to survive trimming.
grid.Columns.Add(new MdDataGridColumn { Header = "Number", PropertyName = nameof(Invoice.Number) });

// Reflection-free: nothing to preserve, and faster on large grids.
grid.Columns.Add(new MdDataGridColumn
{
    Header = "Total",
    PropertyName = nameof(Invoice.Total),          // still the sort key
    ValueSelector = item => ((Invoice)item!).Total,
    ValueParser = text => decimal.TryParse(text, out var value) ? value : null,
    ValueSetter = (item, value) => ((Invoice)item).Total = (decimal)value!
});
```

Keeping `PropertyName` on its own is fine as long as the application preserves its own models —
with `[DynamicallyAccessedMembers]`, a `TrimmerRootDescriptor`, or simply by binding to the same
properties elsewhere in XAML. The selector exists so that is a choice rather than a trap.

`MdCommandItem` is bound by name from a control theme, which no analyzer can see;
`MdCommandPalette` roots its properties, so no action is needed. The optional icon packages are
discovered by assembly probing, which is annotated and degrades to the zero-width fallback glyphs
when neither package is present.



## Enumerations

Every public enum and its members are listed below. To print the current member lists from source, run `python3 scripts/check-api-doc-coverage.py --enums`. The default coverage check fails when a public type name is missing from this document; it does not compare enum member lists, so review this table when the source enums change.

### Core (`Md3.Avalonia`) — 62 enumerations

| Enum | Members |
|---|---|
| `MdAdaptiveBreakpoint` | `Compact` · `Medium` · `Expanded` · `Large` · `ExtraLarge` |
| `MdAdaptiveInputMode` | `Automatic` · `Touch` · `Pointer` · `Keyboard` |
| `MdAdaptiveLayoutMode` | `Compact` · `Medium` · `Expanded` |
| `MdAdaptivePlatform` | `Automatic` · `Material` · `Cupertino` |
| `MdAutovalidateMode` | `Disabled` · `OnUserInteraction` · `Always` |
| `MdBadgeVariant` | `Dot` · `Label` |
| `MdButtonShape` | `Round` · `Square` |
| `MdButtonSize` | `ExtraSmall` · `Small` · `Medium` · `Large` · `ExtraLarge` |
| `MdButtonVariant` | `Filled` · `Tonal` · `Outlined` · `Text` · `Elevated` |
| `MdCaptionButtonKind` | `Minimize` · `MaximizeRestore` · `Close` |
| `MdCardVariant` | `Elevated` · `Filled` · `Outlined` |
| `MdCarouselVariant` | `MultiBrowse` · `Hero` · `CenterAligned` · `Uncontained` |
| `MdChipVariant` | `Assist` · `Filter` · `Input` · `Suggestion` |
| `MdDataTableSortDirection` | `None` · `Ascending` · `Descending` |
| `MdDatePickerMode` | `Docked` · `Modal` |
| `MdDialogVariant` | `Basic` · `FullScreen` |
| `MdDismissDirection` | `Horizontal` · `StartToEnd` · `EndToStart` |
| `MdExtendedFabSize` | `Small` · `Medium` · `Large` |
| `MdFabAlignment` | `End` · `Start` · `Right` · `Left` |
| `MdFabColor` | `PrimaryContainer` · `SecondaryContainer` · `TertiaryContainer` · `Primary` · `Secondary` · `Tertiary` |
| `MdFabMenuExpansionDirection` | `Up` · `Down` |
| `MdFabSize` | `Small` · `Regular` · `Medium` · `Large` |
| `MdFileSystemPickerMode` | `File` · `Folder` |
| `MdFileSystemBreadcrumbStyle` | `Standard` · `Powerline` |
| `MdFocusTraversalPolicy` | `VisualOrder` · `TabIndex` · `ReadingOrder` |
| `MdIconButtonVariant` | `Standard` · `Filled` · `Tonal` · `Outlined` |
| `MdIconButtonWidth` | `Narrow` · `Default` · `Wide` |
| `MdListVariant` | `Standard` · `Segmented` |
| `MdMaskedTextBoxVariant` | `Filled` · `Outlined` |
| `MdMotionKind` | `Spatial` · `Effects` |
| `MdMotionScheme` | `Expressive` · `Standard` · `Reduced` · `None` |
| `MdMotionSpeed` | `Fast` · `Default` · `Slow` |
| `MdNavigationBarLayout` | `Stacked` · `Horizontal` |
| `MdNavigationBarVariant` | `Flexible` · `Baseline` |
| `MdNavigationDrawerPlacement` | `Left` · `Right` |
| `MdNavigationSuiteMode` | `Auto` · `NavigationBar` · `NavigationRail` · `NavigationDrawer` |
| `MdPaginationDisplayMode` | `Numbers` · `Dots` |
| `MdPlaybackRequest` | `Play` · `Pause` · `Stop` · `Previous` · `Next` · `Rewind` · `Forward` · `Seek` · `Mute` · `Unmute` · `SetVolume` · `SetPlaybackRate` · `EnterFullScreen` · `ExitFullScreen` |
| `MdProgressShape` | `Flat` · `Wavy` |
| `MdSafeAreaEdges` | `None` · `Left` · `Top` · `Right` · `Bottom` · `Horizontal` · `Vertical` · `All` |
| `MdScrollbarThumbVisibility` | `Auto` · `Always` · `Hidden` |
| `MdSettingsCardVariant` | `Flat` · `Filled` · `Elevated` · `Outlined` |
| `MdSheetPlacement` | `Bottom` · `Start` · `End` · `Left` · `Right` |
| `MdSnackbarResult` | `Dismissed` · `ActionInvoked` |
| `MdStepState` | `Indexed` · `Editing` · `Complete` · `Error` · `Disabled` |
| `MdSurfaceVariant` | `Surface` · `ContainerLowest` · `ContainerLow` · `Container` · `ContainerHigh` · `ContainerHighest` · `Inverse` |
| `MdTabVariant` | `Primary` · `Secondary` |
| `MdTextBoxVariant` | `Filled` · `Outlined` |
| `MdTextStyle` | `DisplayLarge` · `DisplayMedium` · `DisplaySmall` · `HeadlineLarge` · `HeadlineMedium` · `HeadlineSmall` · `TitleLarge` · `TitleMedium` · `TitleSmall` · `BodyLarge` · `BodyMedium` · `BodySmall` · `LabelLarge` · `LabelMedium` · `LabelSmall` |
| `MdThemeContrastLevel` | `Standard` · `Medium` · `High` |
| `MdThemeFontProfile` | `Brand` · `Plain` |
| `MdThemeMode` | `System` · `Light` · `Dark` |
| `MdThemeSchemeVariant` | `TonalSpot` · `Neutral` · `Vibrant` · `Expressive` · `Monochrome` · `Fidelity` |
| `MdThemeShapeScale` | `Compact` · `Standard` · `Expressive` |
| `MdTimeDialPart` | `Hour` · `Minute` |
| `MdTimePickerMode` | `Dial` · `Input` |
| `MdToggleButtonVariant` | `Elevated` · `Filled` · `Tonal` · `Outlined` |
| `MdToolbarDensity` | `Standard` · `Compact` |
| `MdToolbarMode` | `Docked` · `Floating` |
| `MdToolbarVariant` | `Standard` · `Vibrant` |
| `MdTooltipVariant` | `Plain` · `Rich` |
| `MdTopAppBarVariant` | `Small` · `MediumFlexible` · `LargeFlexible` |
| `MdWindowCapabilities` | `None` · `Move` · `Resize` · `Minimize` · `Maximize` · `Close` · `SystemMenu` · `All` |
| `MdWindowPlatform` | `Unknown` · `Windows` · `MacOS` · `Linux` · `Android` |

### Optional (`Md3.Avalonia.Extra`) — 21 enumerations

| Enum | Members |
|---|---|
| `MdAnimatedTextEffect` | `Typewriter` · `Fade` · `Pop` · `None` |
| `MdAsyncRequestState` | `Idle` · `Loading` · `Data` · `Empty` · `Error` · `Completed` |
| `MdBreadcrumbVariant` | `Standard` · `Powerline` |
| `MdBreadcrumbOverflowBehavior` | `Wrap` · `Scroll` · `Collapse` |
| `MdCalendarSelectionMode` | `Single` · `Range` · `Multiple` |
| `MdChartKind` | `Line` · `Area` · `Bar` |
| `MdChatMessageRole` | `User` · `Assistant` · `System` |
| `MdColorPickerMode` | `MaterialPalette` · `SpectrumSliders` · `Presets` |
| `MdComparisonOrientation` | `Horizontal` · `Vertical` |
| `MdEcosystemDensity` | `Comfortable` · `Compact` · `Touch` |
| `MdMasonryLayoutStrategy` | `Masonry` · `Quilted` · `Woven` |
| `MdOverlayAlignment` | `Start` · `Center` · `End` |
| `MdResultKind` | `Information` · `Success` · `Warning` · `Error` · `Empty` |
| `MdRichEditorCommand` | `Bold` · `Italic` · `Underline` · `StrikeThrough` · `Heading` · `Quote` · `Code` · `Link` · `BulletedList` · `NumberedList` · `Undo` · `Redo` · `HorizontalRule` · `ClearFormatting` |
| `MdRevealAxis` | `Vertical` · `Horizontal` · `Both` |
| `MdSharedAxisKind` | `X` · `Y` · `Z` |
| `MdSkeletonShape` | `Rectangle` · `RoundedRectangle` · `Circle` · `Text` |
| `MdSpinKitKind` | `RotatingPlain` · `ThreeBounce` · `Wave` · `FadingCircle` · `ChasingDots` |
| `MdTimelineItemState` | `Neutral` · `Active` · `Completed` · `Error` |
| `MdTransferLayoutMode` | `Auto` · `Standard` · `Compact` |
| `MdVisibilityTransition` | `Fade` · `ExpandVertical` · `ExpandHorizontal` · `Scale` · `SlideAndFade` |

## Events and event arguments

Components raise ordinary Avalonia routed or CLR events. These argument types are public so
handlers can be written without casting:

| Event argument | Raised by |
|---|---|
| `MdSearchResultCommittedEventArgs` | `MdSearchView.ResultCommitted` — carries the committed result |
| `MdDataTableSortEventArgs` | `MdDataTable` / `MdPaginatedDataTable` column sorting |
| `MdPageChangedEventArgs` | `MdPaginatedDataTable` page navigation |
| `MdReorderEventArgs` | `MdReorderableList` drag completion |
| `MdDismissRequestedEventArgs` | `MdDismissible` swipe-to-dismiss |
| `MdFormSubmittedEventArgs` | `MdForm.Submitted` |
| `MdHeroTransitionEventArgs` | `MdHero.TransitionRequested` — source and destination rectangles |
| `MdPlaybackRequestEventArgs` | media/playback affordances, paired with `MdPlaybackRequest` |
| `MdFileSystemSelectionEventArgs` | `MdFileSystemPicker.SelectionConfirmed` — normalized selected paths |
| `MdFileSystemNavigationFailedEventArgs` | `MdFileSystemPicker.NavigationFailed` — requested path and file-system exception |
| `MdDataGridCellEditEventArgs` | `MdDataGrid` cell commit (Extra) |

`MdSearchBar.SearchSubmitted` passes the submitted text as a plain `string`.

`MdSnackbarMessage` is the queue item accepted by `MdSnackbarHost` and `IMdSnackbarService`:

```csharp
await snackbarService.ShowAsync(new MdSnackbarMessage("Draft archived")
{
    ActionContent = "Undo",
    ActionCommand = undoCommand,
    ActionCommandParameter = draftId
});
```

`Content` is set through the constructor; `ActionContent`, `ActionCommand` and
`ActionCommandParameter` are init-only. The result of a shown message is an `MdSnackbarResult`.

## Motion

`MdMotion` is the inherited motion entry point. `MdMotion.SetScheme(element, scheme)` and
`GetScheme` attach an `MdMotionScheme` that flows down the visual tree, and
`MdMotion.Resolve(element, MdMotionKind, MdMotionSpeed)` returns the `MdMotionSpec` a control
should animate with:

```csharp
MdMotion.SetScheme(page, MdMotionScheme.Expressive);
var spec = MdMotion.Resolve(page, MdMotionKind.Spatial, MdMotionSpeed.Default);
```

`MdMotionKind` separates `Spatial` motion (position, size, rotation, corner shape) from `Effects`
(opacity, colour, elevation). `MdMotionSpec` is a readonly record struct describing one resolved
animation, and `MdSpring(DampingRatio, Stiffness)` describes the physics. `MdMotionTokens` holds
the compiled spring constants, `MdSpringEasing` is the `Easing` implementation that plays them,
`MdMotionTransitions` builds `Transitions` collections from a spec, and `MdSpatialSpringRunner`
and `MdPresenceController` drive spring-based and enter/exit animations.

Motion springs are compiled constants rather than resources, so `Md.Sys.Motion.*` entries
declared in XAML are descriptive and do not override `MdMotionTokens`.

## Localization

`MdLocalization` resolves the culture used by built-in strings such as picker weekday names and
caption-button automation names:

```csharp
MdLocalization.SetCulture(window, new CultureInfo("zh-CN"));
var culture = MdLocalization.ResolveCulture(element);
var text = MdLocalization.GetString("MoveAllToSource", element);
```

`Culture` is an attached property, so a subtree can override the application culture. `GetString`
and `Format` accept either an element or an explicit `CultureInfo`.

## Supporting and infrastructure types

| Type | Role |
|---|---|
| `MdLicenseEntry(Package, License, Text, ProjectUrl?)` | One row for `MdAboutDialog` and `MdLicensePage` |
| `MdContrastDiagnostic(ForegroundRole, BackgroundRole, Purpose, Ratio, RequiredRatio)` | One result from `MdThemeManager.Diagnose` |
| `MdBorderlessWindowOptions(CanResize, ExtendIntoTitleBar, TitleBarHeight, PreserveNativeBorder)` | Values an `IMdWindowPlatformAdapter` receives |
| `MdWindowTemplateSettings` | Template-facing window metrics |
| `MdWindowCapabilities`, `MdWindowPlatform` | What an adapter supports, and which platform resolved |
| `IMdPopupOwner` | Implemented by transient surfaces so the shared popup coordinator can close the previous one |
| `MdDataTableRow` | Row model for `MdDataTable` |
| `MdContributionDay(Date, Value)` | One day in `MdContributionGrid` |
| `MdContributionMonthLabel(Text, WeekIndex)` | A month mark computed from the grid's data |
| `MdRichEditorCommandDescriptor`, `MdRichEditorCommandConverters` | Describe and bind `MdRichEditorCommand` toolbar actions (Extra) |
| `MdTextBoxRichEditorAdapter` | The default `IMdRichEditorAdapter`, backed by a plain text box (Extra) |
| `MdColorConverters` | Value converters used by `MdColorPicker` (Extra) |
| `MdExperimentalAttribute` | Marks an Extra type whose shape may still change; carries a `Reason` |
| `ExtraCatalog` | Static listing of the Extra control surface |
| `ExtraTheme` | Optional styles entry for the `Md3.Avalonia.Extra` controls |
| `MdCalendarDayPresenter`, `MdChatMessagePresenter`, `MdPinCellPresenter`, `MdTimelineItemPresenter`, `MdDataGridHeaderPresenter`, `MdTransferLayoutPanel` | Template-internal presenters, public so templates can be replaced |

## Icon packages

Two interchangeable symbol providers are offered; reference at most one.

| Package | Contents | Catalog type |
|---|---|---|
| `Md3.Avalonia.Icons` | The complete Material Symbols Rounded variable TTF | `MdSymbols`, `MdExternalMaterialSymbols` |
| `Md3.Avalonia.Icons.Lite` | A real outline subset cut from the same official file | `MdSymbolsLite`, `MdExternalMaterialSymbolsLite` |

Both providers expose the same shape: `FontFamily`, `IsAvailable`, `EnsureConfigured()` and
`ApplyTo(Application)`. Referencing a provider alongside the core package is enough — the core
`MdSymbolPresenter` discovers it, registers the embedded font, verifies the family, typeface and a
known glyph, then injects the `Md.Icon.*` resources. If verification fails the glyph stays blank
rather than falling back to a look-alike character.

## Accessibility

Interactive components derive from native Avalonia button, toggle, selector, range or text-input classes wherever possible, retaining their AutomationPeers. `MdLoadingIndicator` exposes progress semantics; `MdSnackbar` is a polite live region. State, ripple and focus visuals are excluded from the automation control/content views.

Applications should provide `AutomationProperties.Name` for icon-only actions and domain-specific controls, and should test with the screen readers on every target operating system.
