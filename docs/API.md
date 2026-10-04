# Md3.Avalonia public API overview

This document is the public entry point for the `3.1.0-preview.1` API. Preview APIs may still receive compatibility-preserving refinements before 0.3.0 stable. The NuGet package also emits `Md3.Avalonia.xml` from the source XML comments for IDE IntelliSense and API documentation generation.

## Install and register

```xml
<PackageReference Include="Md3.Avalonia" Version="3.1.0-preview.1" />
<!-- Optional symbol providers (choose one): -->
<PackageReference Include="Md3.Avalonia.Icons" Version="3.1.0-preview.1" />
<PackageReference Include="Md3.Avalonia.Icons.Lite" Version="3.1.0-preview.1" />
<!-- Optional clean-room third-party Flutter patterns (depends on core): -->
<PackageReference Include="Md3.Avalonia.Extra" Version="3.1.0-preview.1" />
```

```xml
<Application.Styles>
  <themes:MaterialTheme />
</Application.Styles>
```

All component themes are scoped to `Md*` types. Registering `MaterialTheme` does not replace Avalonia's built-in `Button`, `TextBox`, `ComboBox`, `ScrollViewer`, or other native themes.

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

## Components

- Actions: `MdButton`, `MdToggleButton`, `MdIconButton`, `MdToggleIconButton`, `MdSplitButton`, `MdStandardButtonGroup`, `MdConnectedButtonGroup`, `MdFloatingActionButton`, `MdExtendedFloatingActionButton`, `MdFabMenu`, `MdFabMenuItem`.
- Selection and input: `MdTextBox`, `MdCheckBox`, `MdRadioButton`, `MdComboBox`, `MdAutoCompleteBox`, `MdNumericBox`, `MdSwitch`, `MdSlider`, `MdRangeSlider`, `MdSegmentedButtonGroup`, `MdSegmentedButton`.
- Content: `MdCard`, `MdCarousel`, `MdCarouselItem`, `MdChip`, `MdAssistChip`, `MdFilterChip`, `MdInputChip`, `MdSuggestionChip`, `MdList`, `MdListItem`, `MdDivider`.
- Pickers: `MdDatePicker`, `MdDatePickerDialog`, `MdDateRangePicker`, `MdTimePicker`, `MdTimePickerDialog`, and the interactive `MdTimeDial`/`MdTimeDialPart` clock-face API.
- Navigation: `MdTopAppBar`, `MdBottomAppBar`, `MdNavigationBar`, `MdNavigationBarItem`, `MdNavigationDrawer`, `MdNavigationRail`, `MdTabs`, `MdTabItem`, `MdTabView`, `MdTabViewItem`.
- Transient surfaces: `MdDialog`, `MdDialogHost`, `MdDropdownMenu`, `MdMenuAnchor`, `MdMenu`, `MdMenuItem`, `MdSheetHost`, `MdSnackbar`, `MdSnackbarHost`, `MdSnackbarService`, `IMdSnackbarService`, `MdTooltip`, `MdTooltipHost`.
- Feedback: `MdLoadingIndicator`, `MdLinearProgressIndicator`, `MdCircularProgressIndicator`, `MdBadge`, `MdBadgedBox`.
- Flutter-inspired Avalonia APIs: `MdBanner`, `MdExpansionPanelList`, `MdDataTable`, `MdStepper`, `MdRefreshIndicator`, `MdPaginatedDataTable`, `MdReorderableList`, `MdGridTile`, `MdDismissible`, `MdForm`, `MdFormField`, `MdDropdownFormField`, `MdSimpleDialog`, `MdAboutDialog`, `MdLicensePage`, `MdDraggableScrollableSheet`, `MdAdaptiveSwitch`, `MdAdaptiveProgressIndicator`, `MdHero`, `MdFocusTraversalGroup`, `MdShortcutScope`. These names do not imply complete Flutter parity; see [Flutter parity status](FLUTTER_PARITY_STATUS.md).
- Foundations and desktop adapters: `MdScrollViewer`, `MdScrollBar`, five-breakpoint/input-aware `MdAdaptiveLayout`, `MdSurface`, `MdText`, `MdStateLayer`, `MdFocusRing`, `MdWindow`, `MdIcon`, `MdSymbolPresenter`.
- Borderless/chromeless windows: `MdBorderlessWindow`, `MdWindowTitleBar`, `MdCaptionButton`/`MdWindowCaptionButton`, `MdWindowDragRegion`, `MdWindowResizeGrip`, `IMdWindowPlatformAdapter`, named platform adapters and `MdWindowPlatformAdapterResolver`. The default caption is icon-free and has no separator line; visibility and enabled state are independently configurable through `ShowMinimizeButton`/`ShowMaximizeButton`/`ShowCloseButton` and `IsMinimizeButtonEnabled`/`IsMaximizeButtonEnabled`/`IsCloseButtonEnabled`.

- Optional Icons package: `MdExternalMaterialSymbols`, `MdSymbols` and the `Md.Icon.*` resource injection contract.
- Optional Ecosystem foundations: `MdDensity`, `MdAsyncRequestState`, `MdPageRequest`, `MdPageResult<T>`, `IMdPageProvider<T>`, `MdShortcutBinding`, `MdOverlayPlacement`, `MdFocusReturnScope`.
- Optional Ecosystem Wave A: `MdPopover`, `MdHoverCard`, `MdCommandPalette`, `MdCommandItem`.
- Optional Ecosystem Wave B: `MdSlidableItem`, `MdPagedItemsView`, `MdMasonryPanel`, `MdDataGrid`, `MdDataGridColumn`.
- Optional Ecosystem Wave C: `MdAsyncSelect`, `MdCalendar`, `MdTimeline`, `MdResultView`, `MdCascader`, `MdTransfer`.
- Optional Ecosystem Wave D: `MdChart`, `IMdChartDataProvider`, `MdRichEditor`/`MdRichTextEditor`, `IMdRichEditorAdapter`, `MdChatView`.
- Optional Ecosystem Wave E: `MdSkeleton`, `MdSkeletonGroup`, `MdAnimationSequence`.
- Optional Ecosystem Wave F: `MdPinInput`, `MdPinCell`, `MdTreeView`, `MdTreeNode`, `MdTreeRow`, `MdTagInput`, `MdTagEntry`, `MdTagChangedEventArgs`.
- Existing optional Ecosystem controls: `MdAvatar`, `MdAvatarGroup`, `MdRating`, `MdBreadcrumb`, `MdBeforeAfter`, `MdAnimatedText`, `MdSpinKit`, and `MdStaggeredPanel`.
- `MdBeforeAfter` provides draggable horizontal/vertical content comparison; `MdAnimatedText` provides typewriter/fade/pop-friendly text reveal; `MdSpinKit` contains optional non-Material loading recipes; `MdStaggeredPanel` provides cancelable staggered entrance motion. These are Extra controls; core `MdChart` remains provider-neutral and does not bundle a chart engine.

## Complete component catalog

The following catalog covers the public control surface shipped by the core package. Types ending in `*Item`, `*Part`, `*Presenter`, `*Thumb`, `*Panel`, or `*Grid` are supporting visual types that can also be composed directly when their API is public. For the exact dependency version and XML documentation, use the generated `Md3.Avalonia.xml` file in the NuGet package.

### Core controls (`Md3.Avalonia`)

| Area | Controls and supporting types |
|---|---|
| **Buttons and actions** | `MdButton` (`Filled`, `Tonal`, `Outlined`, `Text` variants), `MdIconButton`, `MdToggleButton`, `MdToggleIconButton`, `MdSplitButton`, `MdStandardButtonGroup`, `MdConnectedButtonGroup`, `MdFloatingActionButton`, `MdExtendedFloatingActionButton`, `MdFabMenu`, `MdFabMenuItem`, `MdFabMenuPanel` |
| **Text and form input** | `MdTextBox`, `MdNumericBox`, `MdAutoCompleteBox`, `MdSearchBar`, `MdSearchView`, `MdCheckBox`, `MdRadioButton`, `MdSwitch`, `MdSlider`, `MdRangeSlider`, `MdComboBox`, `MdForm`, `MdFormField`, `MdDropdownFormField`, `MdOutlinedFieldBorder` |
| **Chips and selection** | `MdChip`, `MdAssistChip`, `MdFilterChip`, `MdInputChip`, `MdSuggestionChip`, `MdSegmentedButton`, `MdSegmentedButtonGroup` |
| **Surfaces and content** | `MdSurface`, `MdCard`, `MdList`, `MdListItem`, `MdDivider`, `MdCarousel`, `MdCarouselItem`, `MdGridTile`, `MdGridTileBar`, `MdBanner`, `MdExpansionPanel`, `MdExpansionPanelList`, `MdDataTable`, `MdPaginatedDataTable` |
| **Menus and dialogs** | `MdMenu`, `MdMenuItem`, `MdSubMenuItem`, `MdMenuAnchor`, `MdDropdownMenu`, `MdDialog`, `MdDialogHost`, `MdSimpleDialog`, `MdAboutDialog`, `MdLicensePage`, `MdSheetHost`, `MdTooltip`, `MdTooltipHost` |
| **Navigation and layout** | `MdScaffold`, `MdTopAppBar`, `MdBottomAppBar`, `MdToolbar`, `MdNavigationBar`, `MdNavigationBarItem`, `MdNavigationDrawer`, `MdNavigationRail`, `MdNavigationRailItem`, `MdNavigationSuite`, `MdTabs`, `MdTabItem`, `MdTabView`, `MdTabViewItem`, `MdAdaptiveLayout`, `MdAdaptiveSwitch`, `MdKeyboardAvoidingHost`, `MdDraggableScrollableSheet` |
| **Settings and preference surfaces** | `MdSettingsGroup`, `MdSettingsCard`, `MdSettingsExpander` |
| **Feedback and status** | `MdLoadingIndicator`, `MdLinearProgressIndicator`, `MdCircularProgressIndicator`, `MdAdaptiveProgressIndicator`, `MdBadge`, `MdBadgedBox`, `MdRefreshIndicator`, `MdSnackbar`, `MdSnackbarHost`, `MdSnackbarService` |
| **Pickers and calendars** | `MdDatePicker`, `MdDatePickerDialog`, `MdDateRangePicker`, `MdCalendarGrid`, `MdCalendarDay`, `MdTimePicker`, `MdTimePickerDialog`, `MdTimeDial`, `MdTimeDialPart`, `MdPickerRestorationStore` |
| **Lists and interaction patterns** | `MdReorderableList`, `MdDismissible`, `MdStepper`, `MdStep`, `MdHero`, `MdFocusTraversalGroup`, `MdShortcutScope` |
| **Foundations and icons** | `MdText`, `MdIcon`, `MdSymbolPresenter`, `MdStateLayer`, `MdRipplePresenter`, `MdFocusRing`, `MdScrollViewer`, `MdScrollBar`, `MdSliderThumb`, `MdWindow` |
| **Window chrome** | `MdBorderlessWindow`, `MdWindowTitleBar`, `MdCaptionButton`, `MdWindowCaptionButton`, `MdWindowDragRegion`, `MdWindowResizeGrip`, `IMdWindowPlatformAdapter`, `MdWindowPlatformAdapterResolver`, `MdWindowsWindowPlatformAdapter`, `MdMacOsWindowPlatformAdapter`, `MdLinuxWindowPlatformAdapter`, `MdAvaloniaWindowPlatformAdapter`, `MdAndroidWindowPlatformAdapter` |

Most core controls expose styled properties, bindable `Items`/`ItemsSource` where applicable, routed events, commands, and native Avalonia automation peers. The following variant/support enums are part of the public API and should be preferred over string values: `MdButtonVariant`, `MdButtonSize`, `MdButtonShape`, `MdCardVariant`, `MdChipVariant`, `MdDialogVariant`, `MdListVariant`, `MdNavigationBarVariant`, `MdNavigationBarLayout`, `MdNavigationDrawerPlacement`, `MdProgressShape`, `MdTabVariant`, `MdTextBoxVariant`, `MdTimePickerMode`, `MdToolbarDensity`, `MdToolbarMode`, `MdToolbarVariant`, `MdTooltipVariant`, and `MdTopAppBarVariant`.

### Core control quick reference

| Type | Primary use | Important API surface |
|---|---|---|
| `MdButton`, `MdIconButton`, `MdToggleButton` | Actions and two-state actions | `Command`, `CommandParameter`, `IsEnabled`, `IsChecked`, content/icon properties, variant and size properties |
| `MdTextBox`, `MdNumericBox`, `MdAutoCompleteBox` | Text, numeric, and suggestion input | `Text`/`Value`, validation properties, `ItemsSource`/suggestion provider, `Watermark`, `IsReadOnly` |
| `MdSearchBar`, `MdSearchView` | Search entry and result presentation | `Query`, `SearchCommand`, result templates, `SearchResultCommitted` |
| `MdCheckBox`, `MdRadioButton`, `MdSwitch` | Boolean and mutually exclusive choices | `IsChecked`, `GroupName`/selection binding, `Command` and native input events |
| `MdSlider`, `MdRangeSlider` | Single and interval values | `Minimum`, `Maximum`, `Value` or `StartValue`/`EndValue`, `Step`, `Orientation` |
| `MdCard`, `MdSurface`, `MdList`, `MdListItem` | Material containers and lists | variant/elevation/surface properties, content, `ItemsSource`, item templates, selection/invocation events |
| `MdDialogHost`, `MdSheetHost` | Modal dialogs and bottom/side sheets | `ShowAsync`, `Close`, `Dialog`, placement, dismissal and result APIs |
| `MdMenu`, `MdDropdownMenu`, `MdMenuAnchor` | Contextual and anchored actions | items, submenu support, placement, `Show`/`Dismiss`, keyboard navigation |
| `MdNavigationBar`, `MdNavigationRail`, `MdNavigationDrawer` | Primary application navigation | item collections, selected index/item, placement/layout and `ItemInvoked` |
| `MdTabs`, `MdTabView` | Peer navigation and document views | tab collections, selected item/index, closable/reorderable view items |
| `MdDatePicker`, `MdDateRangePicker`, `MdTimePicker` | Date, date range, and time selection | selected value(s), mode, dialogs, validation, restoration and `ShowAsync` |
| `MdSnackbarHost`, `MdSnackbarService` | Queued transient messages | `Show`, `ShowAsync`, `Dismiss`, action/result, queue and shared service injection |
| `MdCarousel`, `MdRefreshIndicator`, `MdReorderableList` | Touch-friendly content interaction | controller/provider APIs, navigation, refresh callbacks, reorder callbacks |
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
| **Motion and visual extras** | `MdBeforeAfter`, `MdAnimatedText`, `MdSpinKit`, `MdStaggeredPanel`, `MdAnimationSequence`, `MdAnimatedVisibility`, `MdContainerTransform`, `MdFadeThrough`, `MdSharedAxis` |
| **Navigation/content** | `MdBreadcrumb`, `MdBreadcrumbItem`, `MdGridTile`-style content helpers |

The Extra support contracts are also public: `IMdPageProvider<T>`, `MdPageRequest`, `MdPageResult<T>`, `MdAsyncRequestState`, `MdDensity`, `MdEcosystemDensity`, `MdOverlayPlacement`, `MdOverlayAlignment`, `MdShortcutBinding`, and `MdFocusReturnScope`. Data and adapter contracts include `IMdChartDataProvider`, `IMdRichEditorAdapter`, and `IMdRichEditorStateAdapter`. Supporting enums and records such as `MdCalendarSelectionMode`, `MdTransferLayoutMode`, `MdMasonryLayoutStrategy`, `MdResultKind`, `MdSkeletonShape`, `MdSpinKitKind`, `MdAnimatedTextEffect`, and `MdTimelineItemState` configure those controls.

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
<extra:MdPagedItemsView ItemsSource="{Binding Items}"
                        LoadPageAsync="{Binding LoadPageAsync}" />
<extra:MdResultView Result="{Binding RequestResult}" />
```

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

An `MdSnackbar` is a visual control, so calling `Show()` on an instance that was never attached to a window cannot render it. For code-behind or ViewModels, place one `MdSnackbarHost` in the application shell, assign a shared `MdSnackbarService`, and inject that same instance as `IMdSnackbarService`. The service queues consecutive messages and the attached host displays them one at a time.

Direct APIs include `Show`/`Dismiss` on transient components, `ShowAsync`/`Close` on `MdDialogHost`, `Show`/`ShowAsync`/`Dismiss` on `MdSnackbarHost` and `IMdSnackbarService`, direct collection APIs inherited from Avalonia item controls, and standard routed events. Ecosystem controls additionally expose provider delegates and direct state-machine methods such as `LoadNextPageAsync`, `RefreshAsync`, `SearchAsync`, `SelectDate`, `MoveToTarget`, `Execute`, `Submit`, `PlayAsync`, `NotifyOwnerScrolled`, `MdPinInput.SetCode`/`Clear`, `MdTreeView.ExpandAll`/`CollapseAll`/`SelectById`, `MdTagInput.AddTag`/`RemoveTag`/`ClearTags`, and `MdRating.SetValueFromPosition`.

`MdCarousel` supports a bindable `MdCarouselController`, `ScrollTo`, autoplay, pointer-hover pause and finite or wrapping navigation. Weighted variants use viewport-fitted scroll/controller keylines and constrain `SmallItemWidth` to Material's 40–56 DIP range; pointer selection does not resize an item. `MdRefreshIndicator.RequestRefreshAsync` accepts a cancelable provider-neutral handler.

`MdBreadcrumb.OverflowBehavior` selects `Wrap`, one-line native `Scroll`, or explicit `Collapse` behavior. `ShowTrailingSeparator` is opt-in, and `SeparatorTemplate` can render a custom separator from the `Separator` value. Invoking an ancestor raises `ItemInvoked`/`ItemInvokedCommand` without retaining list selection; the current/last item and disabled items are non-invokable.

`MdScrollViewer` keeps wheel, trackpad, touch, pen and scrollbar behavior native. Desktop primary-button panning is opt-in through `AllowMouseDrag`; when enabled, focusable controls retain direct manipulation, and custom content takes precedence by handling the press or capturing the pointer. Set `md:MdScrollViewer.SuppressMouseDragScrolling="True"` on any precision-interaction subtree that does neither.

`MdBorderlessWindow.PlatformAdapter` is replaceable. `PreserveNativeBorder` defaults to `true`: Windows uses `WindowDecorations.Full` plus an extended client area so the native `WS_CAPTION`/minimize/maximize styles and DWM state animations remain available, while macOS/Linux retain the portable border-only path. The Material template suppresses its own outer outline whenever a native frame is present, and an empty Avalonia 12 `WindowDrawnDecorations` theme prevents Fluent/Simple title and caption visuals from being layered over the Material title bar. The default caption is icon-free and uses the window surface background without a separator rule. `ShowMinimizeButton`/`ShowMaximizeButton`/`ShowCloseButton` control visibility, while `IsMinimizeButtonEnabled`/`IsMaximizeButtonEnabled`/`IsCloseButtonEnabled` control each action independently. View models use Avalonia `WindowState`; Android resolves to a safe no-op adapter.

## Extra visual controls

The optional `Md3.Avalonia.Extra` package contains visual and ecosystem controls that are intentionally not counted as official Material 3 components:

```xml
<extra:MdBeforeAfter Before="{Binding Original}" After="{Binding Revised}" Position="0.5" />
<extra:MdAnimatedText Text="Loading complete" Effect="Typewriter" AutoPlay="True" />
<extra:MdSpinKit Kind="Wave" Size="40" IsActive="{Binding IsLoading}" />
<extra:MdStaggeredPanel AutoPlay="True" Stagger="0:0:0.06">
  <Border /><Border /><Border />
</extra:MdStaggeredPanel>
```

`MdBeforeAfter.Position` is a two-way value in the `0..1` range and supports pointer dragging plus keyboard adjustment. `MdAnimatedText` exposes `Start()`, `Stop()`, `IsPlaying`, `DisplayText` and `Completed`. `MdSpinKit` is an optional collection of non-Material loading recipes; use `MdLoadingIndicator` for the Material 3 indicator. `MdStaggeredPanel.PlayAsync()` is cancelable and snaps to the final state under Reduced/None motion.

`MdChart` is deliberately provider-neutral. It exposes `Series`, `Provider`, `BuildAccessibleTable()` and pointer selection, but does not bundle a chart engine. Applications may wrap a third-party chart control with Material surfaces and tokens instead of adopting a second chart data model.

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

`Show*Button` properties remove a caption button from the visual tree. `Is*ButtonEnabled` keeps the button visible but disables its command and applies the disabled state layer/opacity. Programmatic `Minimize()`, `ToggleMaximizeRestore()` and `RequestClose()` enforce the same state as the visual buttons. `MdWindowTitleBar.ShowIcon` defaults to `false`; set it to `true` and provide `LeadingContent` only when an application wants a leading mark.

The Windows adapter preserves native caption style bits when `PreserveNativeBorder=true`, while the Material template owns the title bar surface. Android uses a safe no-op adapter; desktop-only Gallery pages are not registered in `AndroidGalleryView`.

## Accessibility

Interactive components derive from native Avalonia button, toggle, selector, range or text-input classes wherever possible, retaining their AutomationPeers. `MdLoadingIndicator` exposes progress semantics; `MdSnackbar` is a polite live region. State, ripple and focus visuals are excluded from the automation control/content views.

Applications should provide `AutomationProperties.Name` for icon-only actions and domain-specific controls, and should test with the screen readers on every target operating system.
