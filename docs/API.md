# Md3.Avalonia public API overview

This document is the public entry point for the `0.3.0-preview.1` API. Preview APIs may still receive compatibility-preserving refinements before 0.3.0 stable. The NuGet package also emits `Md3.Avalonia.xml` from the source XML comments for IDE IntelliSense and API documentation generation.

## Install and register

```xml
<PackageReference Include="Md3.Avalonia" Version="0.3.0-preview.1" />
<!-- Optional symbol providers (choose one): -->
<PackageReference Include="Md3.Avalonia.Icons" Version="0.3.0-preview.1" />
<PackageReference Include="Md3.Avalonia.Icons.Lite" Version="0.3.0-preview.1" />
<!-- Optional clean-room third-party Flutter patterns (depends on core): -->
<PackageReference Include="Md3.Avalonia.Extra" Version="0.3.0-preview.1" />
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
- Borderless windows: `MdBorderlessWindow`, `MdWindowTitleBar`, `MdCaptionButton`/`MdWindowCaptionButton`, `MdWindowDragRegion`, `MdWindowResizeGrip`, `IMdWindowPlatformAdapter`, named platform adapters and `MdWindowPlatformAdapterResolver`.
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

`MdBorderlessWindow.PlatformAdapter` is replaceable. `PreserveNativeBorder` defaults to `true`: Windows uses `WindowDecorations.Full` plus an extended client area so the native `WS_CAPTION`/minimize/maximize styles and DWM state animations remain available, while macOS/Linux retain the portable border-only path. The Material template suppresses its own outer outline whenever a native frame is present, and an empty Avalonia 12 `WindowDrawnDecorations` theme prevents Fluent/Simple title and caption visuals from being layered over the Material title bar. View models use Avalonia `WindowState`; Android resolves to a safe no-op adapter.

## Accessibility

Interactive components derive from native Avalonia button, toggle, selector, range or text-input classes wherever possible, retaining their AutomationPeers. `MdLoadingIndicator` exposes progress semantics; `MdSnackbar` is a polite live region. State, ripple and focus visuals are excluded from the automation control/content views.

Applications should provide `AutomationProperties.Name` for icon-only actions and domain-specific controls, and should test with the screen readers on every target operating system.
