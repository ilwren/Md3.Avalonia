using System.Globalization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Avalonia.Threading;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Gallery.Pages;
using Md3.Avalonia.Localization;

namespace Md3.Avalonia.Gallery;

/// <summary>
/// Compact, single-view Gallery shell used on Android. The modal drawer intentionally mirrors the
/// desktop Gallery index so every sample remains reachable on a phone-sized viewport.
/// </summary>
public partial class AndroidGalleryView : UserControl
{
    private readonly List<MobileGalleryEntry> _galleryIndex = [];
    private object? _overviewPage;
    private string? _currentPageTitle;
    private Control? _currentPage;
    private readonly Dictionary<Control, MobileControlSizeState> _mobileSizeStates = [];
    private readonly Dictionary<StackPanel, Orientation> _mobilePanelOrientations = [];

    internal int NavigationPageCount => _galleryIndex.Count;
    internal string? CurrentPageTitle => _currentPageTitle;

    public AndroidGalleryView()
    {
        InitializeComponent();
        _overviewPage = MobilePageHost.Content;
        AutomationProperties.SetLandmarkType(MobileDrawer, AutomationLandmarkType.Navigation);
        AutomationProperties.SetLandmarkType(MobilePageHost, AutomationLandmarkType.Main);
        BuildCompleteNavigation();
        MdLocalization.SetCulture(this, CultureInfo.GetCultureInfo("en-US"));
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        ApplyMobileResponsiveLayout(e.NewSize.Width);
    }

    private void BuildCompleteNavigation()
    {
        AddSection("Start");
        AddPage("Overview", () => _overviewPage);
        AddPage("Get started", () => new GettingStartedGalleryPage());
        AddPage("Develop", () => new DeveloperGalleryPage());
        AddPage("Foundations", () => new ThemeResourcesGalleryPage());
        AddPage("Styles", () => new StylesGalleryPage());

        AddSection("Samples");
        AddPage("Android settings", () => new AndroidSettingsSamplePage());
        AddPage("Clock app", () => new ClockSamplePage());
        AddPage("Tasks & todo", () => new TasksSamplePage());

        AddSection("Components");
        AddPage("Buttons", () => new ButtonGalleryPage());
        AddPage("Icon buttons", () => new IconButtonGalleryPage());
        AddPage("FABs", () => new FabGalleryPage());
        AddPage("App bars", () => new AppBarGalleryPage());
        AddPage("Badges", () => new BadgeGalleryPage());
        AddPage("Banner", () => new BannerGalleryPage());
        AddPage("Breadcrumbs", () => new BreadcrumbGalleryPage());
        AddPage("Text fields", () => new TextBoxGalleryPage());
        AddPage("Numeric input", () => new NumericGalleryPage());
        AddPage("Keyboard avoidance", () => new KeyboardAvoidanceGalleryPage());
        AddPage("Checkbox", () => new CheckBoxGalleryPage());
        AddPage("Radio buttons", () => new RadioButtonGalleryPage());
        AddPage("Combo box", () => new ComboBoxGalleryPage());
        AddPage("Carousel", () => new CarouselGalleryPage());
        AddPage("Cards", () => new CardGalleryPage());
        AddPage("Chips", () => new ChipGalleryPage());
        AddPage("Date and time pickers", () => new PickerGalleryPage());
        AddPage("Picker restoration", () => new PickerRestorationGalleryPage());
        AddPage("Color picker", () => new ColorPickerGalleryPage());
        AddPage("Data table", () => new DataTableGalleryPage());
        AddPage("Paginated table", () => new PaginatedDataTableGalleryPage());
        AddPage("Dialogs", () => new DialogGalleryPage());
        AddPage("Simple dialog", () => new SimpleDialogGalleryPage());
        AddPage("About and licenses", () => new AboutDialogGalleryPage());
        AddPage("Dismissible", () => new DismissibleGalleryPage());
        AddPage("Divider", () => new DividerGalleryPage());
        AddPage("Expansion panels", () => new ExpansionPanelGalleryPage());
        AddPage("Form validation", () => new FormValidationGalleryPage());
        AddPage("Grid tiles", () => new GridTileGalleryPage());
        AddPage("Lists", () => new ListGalleryPage());
        AddPage("Loading", () => new LoadingGalleryPage());
        AddPage("Progress", () => new ProgressGalleryPage());
        AddPage("Pull to refresh", () => new RefreshIndicatorGalleryPage());
        AddPage("Reorderable list", () => new ReorderableListGalleryPage());
        AddPage("Menus", () => new MenuGalleryPage());
        AddPage("Navigation bar", () => new NavigationBarGalleryPage());
        AddPage("Navigation drawer", () => new NavigationDrawerGalleryPage());
        AddPage("Adaptive layout", () => new AdaptiveGalleryPage());
        AddPage("Adaptive controls", () => new AdaptiveControlsGalleryPage());
        AddPage("Search", () => new SearchGalleryPage());
        AddPage("Settings cards", () => new SettingsCardGalleryPage());
        AddPage("Sheets", () => new SheetGalleryPage());
        AddPage("Draggable sheet", () => new DraggableSheetGalleryPage());
        AddPage("Slider", () => new SliderGalleryPage());
        AddPage("Stepper", () => new StepperGalleryPage());
        AddPage("Segmented buttons", () => new SegmentedButtonGalleryPage());
        AddPage("Range slider", () => new RangeSliderGalleryPage());
        AddPage("Date range picker", () => new DateRangePickerGalleryPage());
        AddPage("Autocomplete", () => new AutoCompleteGalleryPage());
        AddPage("Surfaces and type scale", () => new SurfaceGalleryPage());
        AddPage("Responsive content", () => new ResponsiveContentGalleryPage());
        AddPage("Scrolling surface", () => new ScrollViewerGalleryPage());
        AddPage("Snackbar", () => new SnackbarGalleryPage());
        AddPage("Switch", () => new SwitchGalleryPage());
        AddPage("Tabs", () => new TabGalleryPage());
        AddPage("Toolbars", () => new ToolbarGalleryPage());
        AddPage("Tooltips", () => new TooltipGalleryPage());

        AddSection("Extra controls");
        AddPage("Animated text", () => new AnimatedTextGalleryPage());
        AddPage("Animation sequence", () => new AnimationSequenceGalleryPage());
        AddPage("Async select", () => new AsyncSelectGalleryPage());
        AddPage("Avatar", () => new AvatarGalleryPage());
        AddPage("Before and after", () => new BeforeAfterGalleryPage());
        AddPage("Calendar", () => new CalendarGalleryPage());
        AddPage("Cascader", () => new CascaderGalleryPage());
        AddPage("Chart", () => new ChartGalleryPage());
        AddPage("Chat view", () => new ChatViewGalleryPage());
        AddPage("Command palette", () => new CommandPaletteGalleryPage());
        AddPage("Data grids", () => new DataGridGalleryPage());
        AddPage("Density", () => new DensityGalleryPage());
        AddPage("Hover cards", () => new HoverCardGalleryPage());
        AddPage("Masonry panel", () => new MasonryPanelGalleryPage());
        AddPage("Paged items", () => new PagedItemsGalleryPage());
        AddPage("PIN input", () => new PinInputGalleryPage());
        AddPage("Popover", () => new PopoverGalleryPage());
        AddPage("Rating", () => new RatingGalleryPage());
        AddPage("Result view", () => new ResultViewGalleryPage());
        AddPage("Rich editor", () => new RichEditorGalleryPage());
        AddPage("Skeleton", () => new SkeletonGalleryPage());
        AddPage("Slidable item", () => new SlidableItemGalleryPage());
        AddPage("Spin kit", () => new SpinKitGalleryPage());
        AddPage("Staggered panel", () => new StaggeredPanelGalleryPage());
        AddPage("Tag input", () => new TagInputGalleryPage());
        AddPage("Timeline", () => new TimelineGalleryPage());
        AddPage("Transfer", () => new TransferGalleryPage());
        AddPage("Tree view", () => new TreeViewGalleryPage());

        AddSection("Library");
        // Borderless window/chrome and desktop adapter samples are intentionally desktop-only.
        // Do not expose them from the Android single-view navigation: Android has no native
        // desktop caption, resize frame, or desktop window lifetime to demonstrate.
        AddPage("Theme Lab", () => new ThemeResourcesGalleryPage());
        AddPage("Material Symbols", () => new SymbolGalleryPage());
        AddPage("Motion", () => new MotionGalleryPage());
        AddPage("Focus, shortcut, Hero", () => new FocusShortcutHeroGalleryPage());
    }

    private void AddSection(string title)
    {
        MobileNavigationItems.Children.Add(new TextBlock
        {
            Text = title,
            Margin = new Thickness(16, 16, 16, 4),
            FontSize = 12,
            FontWeight = global::Avalonia.Media.FontWeight.SemiBold,
            Foreground = this.FindResource("Md.Sys.Color.OnSurfaceVariant.Brush") as global::Avalonia.Media.IBrush
        });
    }

    private void AddPage(string title, Func<object?> factory)
    {
        var entry = new MobileGalleryEntry(title, factory);
        _galleryIndex.Add(entry);
        var button = new MdButton
        {
            Content = title,
            Variant = MdButtonVariant.Text,
            MinHeight = 48,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Tag = entry
        };
        entry.NavigationButton = button;
        AutomationProperties.SetName(button, title);
        button.Click += NavigateFromDrawer;
        MobileNavigationItems.Children.Add(button);
    }

    private void NavigateFromDrawer(object? sender, RoutedEventArgs e)
    {
        if (sender is MdButton { Tag: MobileGalleryEntry entry }) Navigate(entry);
    }

    internal bool NavigateToPage(string title)
    {
        var entry = _galleryIndex.FirstOrDefault(candidate => string.Equals(candidate.Title, title, StringComparison.Ordinal));
        if (entry is null) return false;
        Navigate(entry);
        return true;
    }

    private void Navigate(MobileGalleryEntry entry)
    {
        if (MobilePageHost is null || MobileDrawer is null) return;
        RestoreMobileResponsiveState();
        var content = entry.Factory();
        _currentPage = content as Control;
        MobilePageHost.Content = _currentPage is { } page ? CreateMobileViewport(page) : content;
        _currentPageTitle = entry.Title;
        if (MobileAppBar is not null) MobileAppBar.Title = entry.Title;
        foreach (var candidate in _galleryIndex)
            if (candidate.NavigationButton is { } button)
                button.Variant = ReferenceEquals(candidate, entry) ? MdButtonVariant.Tonal : MdButtonVariant.Text;
        MobileDrawer.IsOpen = false;
        Dispatcher.UIThread.Post(() => ApplyMobileResponsiveLayout(Bounds.Width), DispatcherPriority.Loaded);
    }

    private Control CreateMobileViewport(Control page)
    {
        page.Margin = new Thickness(16, 14, 16, 24);
        if (page is UserControl userPage &&
            (userPage.Content is ScrollViewer ||
             userPage.Content is Grid root && root.Children.OfType<ScrollViewer>().Any()))
        {
            return page;
        }

        return new MdScrollViewer
        {
            VerticalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            Content = page
        };
    }

    private void ApplyMobileResponsiveLayout(double shellWidth)
    {
        if (_currentPage is not { } page || shellWidth <= 0) return;

        // Re-evaluate from the page's authored values on every breakpoint/rotation change. Without
        // this reset, a control constrained at 360 DIP would stay constrained after rotating to a
        // wide viewport, and a row converted to a column could never become a row again.
        RestoreMobileResponsiveState();
        var available = Math.Max(240, shellWidth - 32);
        foreach (var control in page.GetLogicalDescendants().OfType<Control>().Prepend(page))
        {
            var explicitWidthOverflows = !double.IsNaN(control.Width) && control.Width > available;
            var minimumOverflows = control.MinWidth > available;
            if (!explicitWidthOverflows && !minimumOverflows) continue;
            if (!_mobileSizeStates.ContainsKey(control))
                _mobileSizeStates[control] = new MobileControlSizeState(
                    control.Width, control.MinWidth, control.MaxWidth, control.HorizontalAlignment);
            control.Width = double.NaN;
            control.MinWidth = 0;
            control.MaxWidth = Math.Min(control.MaxWidth, available);
            control.HorizontalAlignment = HorizontalAlignment.Stretch;
        }

        foreach (var panel in page.GetLogicalDescendants().OfType<StackPanel>()
                     .Where(panel => panel.Orientation == Orientation.Horizontal && panel.Children.Count > 1))
        {
            var requiredWidth = panel.Children.Sum(child =>
            {
                if (child is not Control control) return child.DesiredSize.Width;
                var authoredWidth = _mobileSizeStates.TryGetValue(control, out var state)
                    ? state.Width
                    : control.Width;
                var width = !double.IsNaN(authoredWidth) ? authoredWidth : child.DesiredSize.Width;
                return width + control.Margin.Left + control.Margin.Right;
            }) + panel.Spacing * (panel.Children.Count - 1);
            if (requiredWidth <= available) continue;
            _mobilePanelOrientations.TryAdd(panel, panel.Orientation);
            panel.Orientation = Orientation.Vertical;
        }
    }

    private void RestoreMobileResponsiveState()
    {
        foreach (var (control, state) in _mobileSizeStates)
        {
            control.Width = state.Width;
            control.MinWidth = state.MinWidth;
            control.MaxWidth = state.MaxWidth;
            control.HorizontalAlignment = state.HorizontalAlignment;
        }
        foreach (var (panel, orientation) in _mobilePanelOrientations)
            panel.Orientation = orientation;
        _mobileSizeStates.Clear();
        _mobilePanelOrientations.Clear();
    }

    private void OpenNavigation(object? sender, RoutedEventArgs e) => MobileDrawer.IsOpen = true;
    private void CloseNavigation(object? sender, RoutedEventArgs e) => MobileDrawer.IsOpen = false;

    private void MobileNavigationChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (MobilePageHost is null || MobileNavigation is null) return;
        var entry = MobileNavigation.SelectedIndex switch
        {
            1 => _galleryIndex.FirstOrDefault(candidate => candidate.Title == "Color picker"),
            2 => _galleryIndex.FirstOrDefault(candidate => candidate.Title == "Motion"),
            3 => new MobileGalleryEntry("Theme & Info", BuildThemePage),
            _ => _galleryIndex.FirstOrDefault(candidate => candidate.Title == "Overview")
        };
        if (entry is not null) Navigate(entry);
    }

    private Control BuildThemePage()
    {
        var light = new MdButton { Content = "Light", Variant = MdButtonVariant.Tonal };
        var dark = new MdButton { Content = "Dark", Variant = MdButtonVariant.Tonal };
        var system = new MdButton { Content = "System", Variant = MdButtonVariant.Outlined };
        light.Click += (_, _) => ApplyTheme(ThemeVariant.Light);
        dark.Click += (_, _) => ApplyTheme(ThemeVariant.Dark);
        system.Click += (_, _) => ApplyTheme(ThemeVariant.Default);
        return BuildPage("Theme & Info", new TextBlock
        {
            Text = "Material 3 design tokens and theme resources are shared between desktop and mobile. Switch theme mode below:",
            TextWrapping = global::Avalonia.Media.TextWrapping.Wrap
        }, new WrapPanel { Children = { light, dark, system } }, new MdCard
        {
            Padding = new Thickness(16),
            Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new TextBlock { Text = "Md3.Avalonia v3.1.0-preview.1", FontSize = 18, FontWeight = global::Avalonia.Media.FontWeight.SemiBold },
                    new TextBlock { Text = "Material Design 3 and Flutter ecosystem components for Avalonia UI.", TextWrapping = global::Avalonia.Media.TextWrapping.Wrap },
                    new MdLinearProgressIndicator { Value = 100 }
                }
            }
        });
    }

    private static MdScrollViewer BuildPage(string title, params Control[] controls)
    {
        var panel = new StackPanel { Margin = new Thickness(16, 14, 16, 24), Spacing = 16 };
        panel.Children.Add(new TextBlock { Text = title, FontSize = 26, FontWeight = global::Avalonia.Media.FontWeight.SemiBold });
        foreach (var control in controls) panel.Children.Add(control);
        return new MdScrollViewer
        {
            VerticalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            Content = panel
        };
    }

    private static void ApplyTheme(ThemeVariant variant)
    {
        if (Application.Current is { } application) application.RequestedThemeVariant = variant;
    }

    private sealed record MobileGalleryEntry(string Title, Func<object?> Factory)
    {
        public MdButton? NavigationButton { get; set; }
    }

    private readonly record struct MobileControlSizeState(
        double Width,
        double MinWidth,
        double MaxWidth,
        HorizontalAlignment HorizontalAlignment);
}
