# API 参考

本文档按 `src/` 中六个可打包项目的公共类型整理。`python3 scripts/check-api-doc-coverage.py` 会检查类型目录和每个公共 C# 接口的独立页面。NuGet 包同时生成 XML API 文档，可用于 IDE 提示与 DocFX 等文档工具。

目前项目版本为 `0.4.1-preview.1`，目标框架为 `.NET 10`，Avalonia 版本为 `12.1.2`。预览版 API 可能在首个稳定版前调整。

## NuGet 包

| 包 | 用途 | 依赖说明 |
|---|---|---|
| `Md3.Avalonia` | Material 3 控件、主题、动态配色、动效及平台基础设施 | 核心包 |
| `Md3.Avalonia.Icons` | 完整 Material Symbols Rounded 字体与图标目录 | 依赖核心包 |
| `Md3.Avalonia.Icons.Lite` | 精简 Material Symbols 字体子集与图标目录 | 依赖核心包；与完整 Icons provider 二选一 |
| `Md3.Avalonia.Extra` | 生态扩展控件与通用数据适配接口 | 依赖核心包 |
| `Md3.Avalonia.DataGrid` | Avalonia 原生 `DataGrid` 的 Material 主题 | 可选；引入 `Avalonia.Controls.DataGrid` |
| `Md3.Avalonia.RichEditor` | AvaloniaRichEditor 的 Material 主题 | 可选；引入 `AvaloniaRichEditor` |

```xml
<PackageReference Include="Md3.Avalonia" Version="0.4.1-preview.1" />
<!-- 按需添加 Extra、Icons / Icons.Lite 或独立主题包。 -->
```

## 主题注册

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:themes="using:Md3.Avalonia.Themes"
             xmlns:extra="using:Md3.Avalonia.Extra.Themes">
  <Application.Styles>
    <themes:MaterialTheme />
    <!-- 使用 Md3.Avalonia.Extra 时添加： -->
    <extra:ExtraTheme />
  </Application.Styles>
</Application>
```

各组件主题限定在 `Md*` 类型，不替换 Avalonia 原生控件主题。DataGrid 与 RichEditor 主题由各自的可选包提供，并在 `MaterialTheme` 后注册。

## 命名空间

| 命名空间 | 内容 |
|---|---|
| `Md3.Avalonia.Controls` | 核心控件、窗口适配、弹层与服务 |
| `Md3.Avalonia.Themes` | `MaterialTheme` |
| `Md3.Avalonia.Themes.Dynamic` | HCT 主题生成、应用、JSON 与对比度诊断 |
| `Md3.Avalonia.Motion` | 继承式动效方案与弹簧参数 |
| `Md3.Avalonia.Localization` | 本地化文化设置 |
| `Md3.Avalonia.Extra.Controls` | 可选扩展控件及适配器 |
| `Md3.Avalonia.Extra.Infrastructure` | 分页、密度、快捷键与覆盖层契约 |
| `Md3.Avalonia.Extra.Themes` | `ExtraTheme` |
| `Md3.Avalonia.Ecosystem.Controls` | 为兼容旧命名空间保留的类型 |

## 公共接口

每个接口均有独立 API 页面，内容基于对应源码声明。页面涵盖成员签名、用途、行为边界与必要的接线示例。

| 接口 | 包 | 用途 |
|---|---|---|
| [`IMdDialogService`](api/IMdDialogService.md) | Core | 从 ViewModel 请求、替换或关闭对话框 |
| [`IMdSnackbarService`](api/IMdSnackbarService.md) | Core | 排队显示 Snackbar 并读取操作结果 |
| [`IMdWindowPlatformAdapter`](api/IMdWindowPlatformAdapter.md) | Core | 替换边框窗口的平台操作实现 |
| [`IMdPopupOwner`](api/IMdPopupOwner.md) | Core | Material 临时弹层的打开状态契约 |
| [`IMdPageProvider<T>`](api/IMdPageProvider.md) | Extra | 异步分页数据提供接口 |
| [`IMdChartDataProvider`](api/IMdChartDataProvider.md) | Extra | 向 `MdChart` 提供数据系列 |
| [`IMdRichEditorAdapter`](api/IMdRichEditorAdapter.md) | Extra | 将富文本工具栏连接到应用的文档引擎 |
| [`IMdRichEditorStateAdapter`](api/IMdRichEditorStateAdapter.md) | Extra | 报告富文本切换命令的选中状态 |

## 动态主题

主题选项可以在启动时或运行时应用。`dark` 参数应来自应用当前的主题变体。

```csharp
using Avalonia;
using Avalonia.Styling;
using Md3.Avalonia.Themes.Dynamic;

var options = new MdThemeOptions
{
    SeedColor = "#006A6A",
    SchemeVariant = MdThemeSchemeVariant.Expressive,
    ContrastLevel = MdThemeContrastLevel.Standard,
    ThemeMode = MdThemeMode.System
};

var application = Application.Current!;
var dark = application.ActualThemeVariant == ThemeVariant.Dark;
MdThemeManager.Apply(application, options, dark);

var json = MdThemeJson.Serialize(options);
```

`MdThemeGenerator.Generate` 生成颜色角色；`MdThemeManager.Apply` 将颜色与画笔资源写入应用资源字典。主题方案、对比度、字体、形状与动效选项均由 `MdThemeOptions` 配置。

## 常用控件示例

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:md="using:Md3.Avalonia.Controls">
  <StackPanel Spacing="16">
    <md:MdButton Content="保存" Variant="Filled" />
    <md:MdTextBox Label="项目名称" Text="{Binding ProjectName}" />
    <md:MdCheckBox Content="包含诊断信息"
                   IsChecked="{Binding IncludeDiagnostics}" />
    <md:MdSlider Minimum="0" Maximum="100" Value="{Binding Progress}" />
  </StackPanel>
</Window>
```

## 公共类型目录

以下目录列出源码中的公开类型。具体属性、默认值、事件与方法以相应程序集的 XML 文档及源码为准。

### `Md3.Avalonia`

`Md3.Avalonia.Controls`、`Md3.Avalonia.Themes`、`Md3.Avalonia.Themes.Dynamic`、`Md3.Avalonia.Motion`、`Md3.Avalonia.Localization`。

`IMdDialogService`、`IMdPopupOwner`、`IMdSnackbarService`、`IMdWindowPlatformAdapter`、`MaterialTheme`、
`MdAboutDialog`、`MdAdaptiveBreakpoint`、`MdAdaptiveInputMode`、`MdAdaptiveLayout`、`MdAdaptiveLayoutMode`、
`MdAdaptivePlatform`、`MdAdaptiveProgressIndicator`、`MdAdaptiveSwitch`、`MdAndroidWindowPlatformAdapter`、
`MdAssistChip`、`MdAutoCompleteBox`、`MdAutovalidateMode`、`MdAvaloniaWindowPlatformAdapter`、`MdBackNavigation`、
`MdBackScope`、`MdBadge`、`MdBadgeVariant`、`MdBadgedBox`、`MdBanner`、`MdBorderlessWindow`、
`MdBorderlessWindowOptions`、`MdBottomAppBar`、`MdButton`、`MdButtonShape`、`MdButtonSize`、`MdButtonVariant`、
`MdCalendarDay`、`MdCalendarGrid`、`MdCaptionButton`、`MdCaptionButtonKind`、`MdCard`、`MdCardVariant`、`MdCarousel`、
`MdCarouselController`、`MdCarouselItem`、`MdCarouselVariant`、`MdCheckBox`、`MdChip`、`MdChipVariant`、
`MdCircularProgressIndicator`、`MdComboBox`、`MdConnectedButtonGroup`、`MdContrastDiagnostic`、`MdDataTable`、
`MdDataTableRow`、`MdDataTableSortDirection`、`MdDataTableSortEventArgs`、`MdDatePicker`、`MdDatePickerDialog`、
`MdDatePickerMode`、`MdDateRangePicker`、`MdDialog`、`MdDialogHost`、`MdDialogService`、`MdDialogServiceExtensions`、
`MdDialogVariant`、`MdDismissDirection`、`MdDismissRequestedEventArgs`、`MdDismissible`、`MdDivider`、
`MdDraggableScrollableSheet`、`MdDropdownFormField`、`MdDropdownMenu`、`MdExpansionPanel`、`MdExpansionPanelList`、
`MdExtendedFabSize`、`MdExtendedFloatingActionButton`、`MdFabAlignment`、`MdFabColor`、`MdFabMenu`、
`MdFabMenuExpansionDirection`、`MdFabMenuItem`、`MdFabMenuPanel`、`MdFabSize`、`MdFilterChip`、
`MdFloatingActionButton`、`MdFocusRing`、`MdFocusTraversalGroup`、`MdFocusTraversalPolicy`、`MdForm`、`MdFormField`、
`MdFormSubmittedEventArgs`、`MdGridTile`、`MdGridTileBar`、`MdHero`、`MdHeroTransitionEventArgs`、`MdIcon`、
`MdIconButton`、`MdIconButtonVariant`、`MdIconButtonWidth`、`MdInputChip`、`MdKeyboardAvoidingHost`、
`MdLicenseEntry`、`MdLicensePage`、`MdLinearProgressIndicator`、`MdLinuxWindowPlatformAdapter`、`MdList`、
`MdListItem`、`MdListVariant`、`MdLoadingIndicator`、`MdLocalization`、`MdMacOsWindowPlatformAdapter`、`MdMenu`、
`MdMenuAnchor`、`MdMenuItem`、`MdMotion`、`MdMotionKind`、`MdMotionScheme`、`MdMotionSpec`、`MdMotionSpeed`、
`MdMotionTokens`、`MdMotionTransitions`、`MdNavigationBar`、`MdNavigationBarItem`、`MdNavigationBarLayout`、
`MdNavigationBarVariant`、`MdNavigationDrawer`、`MdNavigationDrawerPlacement`、`MdNavigationRail`、
`MdNavigationRailItem`、`MdNavigationSuite`、`MdNavigationSuiteMode`、`MdNumericBox`、`MdOutlinedFieldBorder`、
`MdPageChangedEventArgs`、`MdPaginatedDataTable`、`MdPickerRestorationStore`、`MdPlaybackRequest`、
`MdPlaybackRequestEventArgs`、`MdPresenceController`、`MdProgressShape`、`MdRadioButton`、`MdRangeSlider`、
`MdRefreshIndicator`、`MdReorderEventArgs`、`MdReorderableList`、`MdRipplePresenter`、`MdSafeArea`、
`MdSafeAreaEdges`、`MdScaffold`、`MdScrollBar`、`MdScrollViewer`、`MdScrollbarThumbVisibility`、`MdSearchBar`、
`MdSearchResultCommittedEventArgs`、`MdSearchView`、`MdSegmentedButton`、`MdSegmentedButtonGroup`、
`MdSettingsCard`、`MdSettingsCardVariant`、`MdSettingsExpander`、`MdSettingsGroup`、`MdSheetHost`、
`MdSheetPlacement`、`MdShortcutScope`、`MdSimpleDialog`、`MdSlider`、`MdSliderThumb`、`MdSnackbar`、`MdSnackbarHost`、
`MdSnackbarMessage`、`MdSnackbarResult`、`MdSnackbarService`、`MdSpatialSpringRunner`、`MdSplitButton`、`MdSpring`、
`MdSpringEasing`、`MdStandardButtonGroup`、`MdStateLayer`、`MdStep`、`MdStepState`、`MdStepper`、`MdSubMenuItem`、
`MdSuggestionChip`、`MdSurface`、`MdSurfaceVariant`、`MdSwitch`、`MdSymbolPresenter`、`MdTabItem`、`MdTabVariant`、
`MdTabView`、`MdTabViewItem`、`MdTabs`、`MdTabsView`、`MdText`、`MdTextBox`、`MdTextBoxVariant`、`MdTextStyle`、
`MdThemeContrastLevel`、`MdThemeFontProfile`、`MdThemeGenerator`、`MdThemeJson`、`MdThemeManager`、`MdThemeMode`、
`MdThemeOptions`、`MdThemeSchemeVariant`、`MdThemeShapeScale`、`MdTimeDial`、`MdTimeDialPart`、`MdTimePicker`、
`MdTimePickerDialog`、`MdTimePickerMode`、`MdToggleButton`、`MdToggleButtonVariant`、`MdToggleIconButton`、
`MdToolbar`、`MdToolbarDensity`、`MdToolbarMode`、`MdToolbarVariant`、`MdTooltip`、`MdTooltipHost`、
`MdTooltipVariant`、`MdTopAppBar`、`MdTopAppBarVariant`、`MdWindow`、`MdWindowCapabilities`、
`MdWindowCaptionButton`、`MdWindowDragRegion`、`MdWindowPlatform`、`MdWindowPlatformAdapterResolver`、
`MdWindowResizeGrip`、`MdWindowTemplateSettings`、`MdWindowTitleBar`、`MdWindowsWindowPlatformAdapter`。

### `Md3.Avalonia.Extra`

`Md3.Avalonia.Extra.Controls`、`Md3.Avalonia.Extra.Infrastructure`、`Md3.Avalonia.Extra.Themes`。

`EcosystemTheme`、`ExtraCatalog`、`ExtraTheme`、`IMdChartDataProvider`、`IMdPageProvider<T>`、
`IMdRichEditorAdapter`、`IMdRichEditorStateAdapter`、`MdAnimatedText`、`MdAnimatedTextEffect`、
`MdAnimatedVisibility`、`MdAnimationSequence`、`MdAsyncRequestState`、`MdAsyncSelect`、`MdAvatar`、`MdAvatarGroup`、
`MdBeforeAfter`、`MdBreadcrumb`、`MdBreadcrumbItem`、`MdBreadcrumbOverflowBehavior`、`MdBreadcrumbVariant`、
`MdCalendar`、`MdCalendarDay`、`MdCalendarDayPresenter`、`MdCalendarSelectionMode`、`MdCascader`、`MdCascaderItem`、`MdChart`、
`MdChartKind`、`MdChartPoint`、`MdChartSeries`、`MdChatMessage`、`MdChatMessagePresenter`、`MdChatMessageRole`、
`MdChatView`、`MdColorConverters`、`MdColorPicker`、`MdColorPickerButton`、`MdColorPickerMode`、`MdCommandItem`、
`MdCommandPalette`、`MdComparisonOrientation`、`MdContainerTransform`、`MdDataGrid`、`MdDataGridCellEditEventArgs`、
`MdDataGridColumn`、`MdDataGridHeaderPresenter`、`MdDensity`、`MdEcosystemDensity`、`MdExperimentalAttribute`、
`MdFadeThrough`、`MdFocusReturnScope`、`MdHoverCard`、`MdMasonryLayoutStrategy`、`MdMasonryPanel`、`MdMorphPanel`、
`MdOverlayAlignment`、`MdOverlayPlacement`、`MdPageRequest`、`MdPageResult<T>`、`MdPagedItemsView`、`MdPinCell`、
`MdPinCellPresenter`、`MdPinInput`、`MdPopover`、`MdRating`、`MdResultKind`、`MdResultView`、`MdRevealAxis`、
`MdRevealHost`、`MdRichEditor`、`MdRichEditorCommand`、`MdRichEditorCommandConverters`、
`MdRichEditorCommandDescriptor`、`MdRichTextEditor`、`MdSharedAxis`、`MdSharedAxisKind`、`MdShortcutBinding`、
`MdSkeleton`、`MdSkeletonGroup`、`MdSkeletonShape`、`MdSlidableItem`、`MdSpinKit`、`MdSpinKitKind`、
`MdStaggeredPanel`、`MdTagChangedEventArgs`、`MdTagEntry`、`MdTagInput`、`MdTextBoxRichEditorAdapter`、`MdTimeline`、
`MdTimelineItem`、`MdTimelineItemPresenter`、`MdTimelineItemState`、`MdTransfer`、`MdTransferLayoutMode`、
`MdTransferLayoutPanel`、`MdTreeNode`、`MdTreeRow`、`MdTreeView`、`MdVisibilityTransition`。

### 可选包

- `Md3.Avalonia.Icons`：`MdExternalMaterialSymbols`、`MdSymbols`。完整 glyph 目录由 `MdSymbols` 提供。
- `Md3.Avalonia.Icons.Lite`：`MdExternalMaterialSymbolsLite`、`MdSymbolsLite`。精简 glyph 目录由 `MdSymbolsLite` 提供。
- `Md3.Avalonia.DataGrid`：`MaterialDataGridTheme`。
- `Md3.Avalonia.RichEditor`：`MaterialRichEditorTheme`。
- 为兼容旧命名空间，`Md3.Avalonia.Extra` 还公开 `Md3.Avalonia.Ecosystem.Controls.MdTimelineItemState`。

各包的公共接口与控件由源码生成 XML 文档；完整枚举成员可用 `python3 scripts/check-api-doc-coverage.py --enums` 输出。