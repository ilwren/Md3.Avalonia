# Md3.Avalonia public API overview

This document is the public entry point for the `0.4.1` API. Preview APIs may still receive compatibility-preserving refinements before the first stable release. The NuGet package also emits `Md3.Avalonia.xml` from the source XML comments for IDE IntelliSense and API documentation generation.

## Install and register

```xml
<PackageReference Include="Md3.Avalonia" Version="0.4.1" />
<!-- Optional symbol providers (choose one): -->
<PackageReference Include="Md3.Avalonia.Icons" Version="0.4.1" />
<PackageReference Include="Md3.Avalonia.Icons.Lite" Version="0.4.1" />
<!-- Optional clean-room third-party Flutter patterns (depends on core): -->
<PackageReference Include="Md3.Avalonia.Extra" Version="0.4.1" />
<!-- Optional Material theme for Avalonia's own DataGrid; the only package that pulls in
     Avalonia.Controls.DataGrid: -->
<PackageReference Include="Md3.Avalonia.DataGrid" Version="0.4.1" />
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
- Navigation: `MdTopAppBar`, `MdBottomAppBar`, `MdNavigationBar`, `MdNavigationBarItem`, `MdNavigationDrawer`, `MdNavigationRail`, `MdTabs`, `MdTabItem`, `MdTabsView`, `MdTabView`, `MdTabViewItem`.
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
| **Mobile platform integration** | `MdSafeArea`, `MdBackNavigation`, `MdBackScope`, `MdKeyboardAvoidingHost` |
| **Settings and preference surfaces** | `MdSettingsGroup`, `MdSettingsCard`, `MdSettingsExpander` |
| **Feedback and status** | `MdLoadingIndicator`, `MdLinearProgressIndicator`, `MdCircularProgressIndicator`, `MdAdaptiveProgressIndicator`, `MdBadge`, `MdBadgedBox`, `MdRefreshIndicator`, `MdSnackbar`, `MdSnackbarHost`, `MdSnackbarService` |
| **Pickers and calendars** | `MdDatePicker`, `MdDatePickerDialog`, `MdDateRangePicker`, `MdCalendarGrid`, `MdCalendarDay`, `MdTimePicker`, `MdTimePickerDialog`, `MdTimeDial`, `MdTimeDialPart`, `MdPickerRestorationStore` |
| **Lists and interaction patterns** | `MdReorderableList`, `MdDismissible`, `MdStepper`, `MdStep`, `MdHero`, `MdFocusTraversalGroup`, `MdShortcutScope` |
| **Foundations and icons** | `MdText`, `MdIcon`, `MdSymbolPresenter`, `MdStateLayer`, `MdRipplePresenter`, `MdFocusRing`, `MdScrollViewer`, `MdScrollBar`, `MdSliderThumb`, `MdWindow` |
| **Window chrome** | `MdBorderlessWindow`, `MdWindowTitleBar`, `MdCaptionButton`, `MdWindowCaptionButton`, `MdWindowDragRegion`, `MdWindowResizeGrip`, `IMdWindowPlatformAdapter`, `MdWindowPlatformAdapterResolver`, `MdWindowsWindowPlatformAdapter`, `MdMacOsWindowPlatformAdapter`, `MdLinuxWindowPlatformAdapter`, `MdAvaloniaWindowPlatformAdapter`, `MdAndroidWindowPlatformAdapter` |

Most core controls expose styled properties, bindable `Items`/`ItemsSource` where applicable, routed events, commands, and native Avalonia automation peers. Variant and option enums are part of the public API and should be preferred over string values. All 79 of them are listed with their members under [Enumerations](#enumerations).

### Core control quick reference

| Type | Primary use | Important API surface |
|---|---|---|
| `MdButton`, `MdIconButton`, `MdToggleButton` | Actions and two-state actions | `Command`, `CommandParameter`, `IsEnabled`, `IsChecked`, content/icon properties, variant and size properties |
| `MdTextBox`, `MdNumericBox`, `MdAutoCompleteBox` | Text, numeric, and suggestion input | `Text`/`Value`, `Label`, `SupportingText`, `IsError`, `PlaceholderText`, `IsReadOnly`, `ItemsSource`/suggestion provider; `MdNumericBox` adds the native `Minimum`/`Maximum`/`Increment` and `ShowButtonSpinner` |
| `MdSearchBar`, `MdSearchView` | Search entry and result presentation | `Text` (inherited from `TextBox`), `SearchCommand`, `SearchSubmitted`; the view adds `Header`, `IsOpen`, `SelectedResult`, `ResultDisplayMemberPath`, `CloseOnResultCommit` and `ResultCommitted` |
| `MdCheckBox`, `MdRadioButton`, `MdSwitch` | Boolean and mutually exclusive choices | `IsChecked`, `GroupName`/selection binding, `Command` and native input events |
| `MdSlider`, `MdRangeSlider` | Single and interval values | `Minimum`, `Maximum`; `MdSlider` uses the native `Value` and tick properties, `MdRangeSlider` uses `LowerValue`/`UpperValue` plus `Step` and `ShowValueIndicators` |
| `MdCard`, `MdSurface`, `MdList`, `MdListItem` | Material containers and lists | variant/elevation/surface properties, content, `ItemsSource`, item templates, selection/invocation events |
| `MdDialogHost`, `MdSheetHost` | Modal dialogs and bottom/side sheets | `ShowAsync`, `Close`, `Dialog`, `Service`, placement, dismissal and result APIs |
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
| **Motion and visual extras** | `MdBeforeAfter`, `MdAnimatedText`, `MdSpinKit`, `MdStaggeredPanel`, `MdAnimationSequence`, `MdAnimatedVisibility`, `MdContainerTransform`, `MdFadeThrough`, `MdSharedAxis`, `MdRevealHost`, `MdMorphPanel` |
| **Navigation/content** | `MdBreadcrumb`, `MdBreadcrumbItem`, `MdGridTile`-style content helpers |

`MdPagedItemsView.ItemTemplate` sets the presentation of each loaded record; `MdDataGrid` is a narrower surface than Avalonia `DataGrid` and is not a drop-in replacement for it.

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

Direct APIs include `Show`/`Dismiss` on transient components, `ShowAsync`/`Show`/`Close` on `MdDialogHost` and `IMdDialogService`, `Show`/`ShowAsync`/`Dismiss` on `MdSnackbarHost` and `IMdSnackbarService`, direct collection APIs inherited from Avalonia item controls, and standard routed events. Ecosystem controls additionally expose provider delegates and direct state-machine methods such as `LoadNextPageAsync`, `RefreshAsync`, `SearchAsync`, `SelectDate`, `MoveToTarget`, `Execute`, `Submit`, `PlayAsync`, `NotifyOwnerScrolled`, `MdPinInput.SetCode`/`Clear`, `MdTreeView.ExpandAll`/`CollapseAll`/`SelectById`, `MdTagInput.AddTag`/`RemoveTag`/`ClearTags`, and `MdRating.SetValueFromPosition`.

`MdCarousel` supports a bindable `MdCarouselController`, `ScrollTo`, autoplay, pointer-hover pause and finite or wrapping navigation. Weighted variants use viewport-fitted scroll/controller keylines and constrain `SmallItemWidth` to Material's 40–56 DIP range; pointer selection does not resize an item. `MdRefreshIndicator.RequestRefreshAsync` accepts a cancelable provider-neutral handler.

`MdBreadcrumb.OverflowBehavior` selects `Wrap`, one-line native `Scroll`, or explicit `Collapse` behavior. `ShowTrailingSeparator` is opt-in, and `SeparatorTemplate` can render a custom separator from the `Separator` value. Invoking an ancestor raises `ItemInvoked`/`ItemInvokedCommand` without retaining list selection; the current/last item and disabled items are non-invokable.

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

## Trimming

All five packages set `IsTrimmable`, so an application that publishes with
`PublishTrimmed=true` can trim them, and they build with the IL2xxx analyzer on, so a new
reflective call fails the build rather than breaking a trimmed app quietly.

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

AOT is a separate question and is not claimed: these packages do not set `IsAotCompatible`.

## Enumerations

Every public enumeration, with its members in declaration order. Generated from the sources and
held in place by `python3 scripts/check-api-doc-coverage.py`, which fails when a public type is
missing from this document.

### Core (`Md3.Avalonia`) — 60 enumerations

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
| `MdFocusTraversalPolicy` | `VisualOrder` · `TabIndex` · `ReadingOrder` |
| `MdIconButtonVariant` | `Standard` · `Filled` · `Tonal` · `Outlined` |
| `MdIconButtonWidth` | `Narrow` · `Default` · `Wide` |
| `MdListVariant` | `Standard` · `Segmented` |
| `MdMotionKind` | `Spatial` · `Effects` |
| `MdMotionScheme` | `Expressive` · `Standard` · `Reduced` · `None` |
| `MdMotionSpeed` | `Fast` · `Default` · `Slow` |
| `MdNavigationBarLayout` | `Stacked` · `Horizontal` |
| `MdNavigationBarVariant` | `Flexible` · `Baseline` |
| `MdNavigationDrawerPlacement` | `Left` · `Right` |
| `MdNavigationSuiteMode` | `Auto` · `NavigationBar` · `NavigationRail` · `NavigationDrawer` |
| `MdPlaybackRequest` | `Play` · `Pause` · `Previous` · `Next` · `Rewind` · `Forward` · `Mute` · `Unmute` · `EnterFullScreen` · `ExitFullScreen` |
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

### Optional (`Md3.Avalonia.Extra`) — 19 enumerations

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
| `MdRichEditorCommandDescriptor`, `MdRichEditorCommandConverters` | Describe and bind `MdRichEditorCommand` toolbar actions (Extra) |
| `MdTextBoxRichEditorAdapter` | The default `IMdRichEditorAdapter`, backed by a plain text box (Extra) |
| `MdColorConverters` | Value converters used by `MdColorPicker` (Extra) |
| `MdExperimentalAttribute` | Marks an Extra type whose shape may still change; carries a `Reason` |
| `ExtraCatalog` | Static listing of the Extra control surface |
| `EcosystemTheme` | Styles entry registered by `ExtraTheme` |
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
