using System.Globalization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Gallery.Pages;
using Md3.Avalonia.Localization;
using Md3.Avalonia.Themes.Dynamic;

namespace Md3.Avalonia.Gallery;

public partial class MainWindow : Window
{
    private readonly MdButton[] _navigationButtons;
    private readonly MdButton[] _topNavigationButtons;
    private readonly GalleryIndexEntry[] _galleryIndex;
    private readonly Dictionary<Control, ResponsiveSizeState> _responsiveSizeStates = [];
    private CultureInfo _culture = CultureInfo.GetCultureInfo("en-US");
    private Type? _currentPageType;
    private MdButton? _currentNavigationButton;
    private MdButton? _currentTopNavigationButton;
    private bool _usesModalNavigation;
    private bool _navigationOpen;

    internal GalleryBreakpoint CurrentBreakpoint { get; private set; }
    public MdDialogHost DialogHost => GlobalDialogHost;

    public MainWindow()
    {
        InitializeComponent();
        AutomationProperties.SetLandmarkType(NavigationPane, AutomationLandmarkType.Navigation);
        AutomationProperties.SetLandmarkType(PageScroll, AutomationLandmarkType.Main);
        AutomationProperties.SetLandmarkType(TableOfContentsPane, AutomationLandmarkType.Complementary);
        AutomationProperties.SetLandmarkType(GallerySearch, AutomationLandmarkType.Search);
        AutomationProperties.SetName(NavigationMenuButton, "Open component navigation");
        _navigationButtons =
        [
            ComponentsOverviewNav, AndroidSettingsNav, ClockNav, TasksNav,
            ButtonNav, IconButtonNav, FabNav, AppBarNav, BadgeNav, BreadcrumbNav, TextFieldNav,
            CheckBoxNav, RadioButtonNav, ComboBoxNav, CarouselNav, CardNav, ChipNav, PickerNav, ColorPickerNav,
            DialogNav, DividerNav, ListNav, LoadingNav, ProgressNav, MenuNav, NavigationBarNav,
            NavigationDrawerNav, AdaptiveNav, SearchNav, SettingsCardNav, SheetNav, SliderNav, AdvancedSelectionNav,
            SnackbarNav, SwitchNav, TabNav, ToolbarNav, TooltipNav, FlutterParityNav, EcosystemNav, BorderlessWindowNav,
            DesktopAdaptersNav, ThemeResourcesNav, SymbolsNav, MotionNav
        ];
        _topNavigationButtons = [GetStartedTopNav, DevelopTopNav, FoundationsTopNav, StylesTopNav, ComponentsTopNav];
        _galleryIndex =
        [
            Entry("Android settings", "android settings preference card wifi bluetooth sound display battery", AndroidSettingsNav, () => new AndroidSettingsSamplePage()),
            Entry("Clock app", "clock alarm timer stopwatch world cities timezone", ClockNav, () => new ClockSamplePage()),
            Entry("Tasks & todo", "tasks todo slidable list swipe check due date priority", TasksNav, () => new TasksSamplePage()),
            Entry("Buttons", "button toggle split connected group", ButtonNav, () => new ButtonGalleryPage()),
            Entry("Icon buttons", "icon action round square", IconButtonNav, () => new IconButtonGalleryPage()),
            Entry("FABs", "floating action button menu extended", FabNav, () => new FabGalleryPage()),
            Entry("App bars", "top bottom app bar toolbar", AppBarNav, () => new AppBarGalleryPage()),
            Entry("Badges", "badge notification dot", BadgeNav, () => new BadgeGalleryPage()),
            Entry("Breadcrumbs", "breadcrumb path navigation separator hierarchy", BreadcrumbNav, () => new BreadcrumbGalleryPage()),
            Entry("Text fields", "textbox input password clear context menu", TextFieldNav, () => new TextBoxGalleryPage()),
            Entry("Checkbox", "check selection", CheckBoxNav, () => new CheckBoxGalleryPage()),
            Entry("Radio buttons", "radio selection", RadioButtonNav, () => new RadioButtonGalleryPage()),
            Entry("Combo box", "combobox dropdown popup select", ComboBoxNav, () => new ComboBoxGalleryPage()),
            Entry("Carousel", "carousel browse item", CarouselNav, () => new CarouselGalleryPage()),
            Entry("Cards", "card elevated filled outlined", CardNav, () => new CardGalleryPage()),
            Entry("Chips", "chip assist filter input suggestion", ChipNav, () => new ChipGalleryPage()),
            Entry("Date and time pickers", "calendar clock picker localized minute", PickerNav, () => new PickerGalleryPage()),
            Entry("Color picker", "color picker palette hsv hex rgb alpha flutter swatch", ColorPickerNav, () => new ColorPickerGalleryPage()),
            Entry("Dialogs", "dialog modal alert", DialogNav, () => new DialogGalleryPage()),
            Entry("Divider", "separator inset", DividerNav, () => new DividerGalleryPage()),
            Entry("Lists", "list item virtualized", ListNav, () => new ListGalleryPage()),
            Entry("Loading", "loading indicator contained", LoadingNav, () => new LoadingGalleryPage()),
            Entry("Progress", "progress linear circular", ProgressNav, () => new ProgressGalleryPage()),
            Entry("Menus", "menu anchor popup dropdown", MenuNav, () => new MenuGalleryPage()),
            Entry("Navigation bar", "navigation bar destination flexible", NavigationBarNav, () => new NavigationBarGalleryPage()),
            Entry("Navigation drawer", "navigation drawer modal standard", NavigationDrawerNav, () => new NavigationDrawerGalleryPage()),
            Entry("Adaptive layout", "navigation rail adaptive responsive breakpoint", AdaptiveNav, () => new AdaptiveGalleryPage()),
            Entry("Search", "search bar view suggestions", SearchNav, () => new SearchGalleryPage()),
            Entry("Settings cards", "settings card preference group expander android tile", SettingsCardNav, () => new SettingsCardGalleryPage()),
            Entry("Sheets", "bottom side sheet", SheetNav, () => new SheetGalleryPage()),
            Entry("Slider", "slider range value", SliderNav, () => new SliderGalleryPage()),
            Entry("Segmented and range", "segmented button range slider", AdvancedSelectionNav, () => new AdvancedSelectionGalleryPage()),
            Entry("Snackbar", "snackbar message action", SnackbarNav, () => new SnackbarGalleryPage()),
            Entry("Switch", "switch toggle", SwitchNav, () => new SwitchGalleryPage()),
            Entry("Tabs", "tabs tab view", TabNav, () => new TabGalleryPage()),
            Entry("Toolbars", "toolbar dock floating", ToolbarNav, () => new ToolbarGalleryPage()),
            Entry("Tooltips", "tooltip rich plain", TooltipNav, () => new TooltipGalleryPage()),
            Entry("Flutter parity", "banner expansion panel paginated table reorder form dialog adaptive hero focus shortcut sheet refresh", FlutterParityNav, () => new FlutterParityGalleryPage()),
            Entry("Flutter ecosystem", "third party avatar rating breadcrumb clean room chart chat calendar transfer masonry data grid skeleton command palette", EcosystemNav, () => new EcosystemGalleryPage()),
            Entry("Borderless windows", "custom chrome title bar caption drag resize platform adapter", BorderlessWindowNav, () => new BorderlessWindowGalleryPage()),
            Entry("Desktop adapters", "scrollbar numeric autocomplete window surface text focus", DesktopAdaptersNav, () => new DesktopAdaptersGalleryPage()),
            Entry("Theme Lab", "theme color seed hct contrast json shape font", ThemeResourcesNav, () => new ThemeResourcesGalleryPage()),
            Entry("Material Symbols", "icons glyph copy symbols", SymbolsNav, () => new SymbolGalleryPage()),
            Entry("Motion", "animation ripple spring easing", MotionNav, () => new MotionGalleryPage())
        ];

        ThemeSelector.ItemsSource = new[] { "Light", "Dark", "System" };
        ThemeSelector.SelectedIndex = 2;
        LanguageSelector.ItemsSource = new[] { "English", "简体中文" };
        LanguageSelector.SelectedIndex = 0;
        MdLocalization.SetCulture(this, _culture);
        PageScroll.AddHandler(InputElement.PointerWheelChangedEvent, OnPagePointerWheel, RoutingStrategies.Tunnel);
        Navigate(new ComponentsOverviewGalleryPage(), ComponentsOverviewNav, ComponentsTopNav);
        Dispatcher.UIThread.Post(() =>
        {
            GalleryLocalization.Apply(this, _culture);
            ApplyResponsiveLayout(Bounds.Width > 0 ? Bounds.Width : Width);
        });
    }

    private static GalleryIndexEntry Entry(string title, string keywords, MdButton button, Func<Control> factory) =>
        new(title, keywords, button, factory);

    internal bool NavigateToIndexedPage(string title)
    {
        var entry = _galleryIndex.FirstOrDefault(candidate => string.Equals(candidate.Title, title, StringComparison.Ordinal));
        if (entry is null) return false;
        Navigate(entry.Factory(), entry.Button, ComponentsTopNav);
        return true;
    }

    private void ShowGetStarted(object? s, RoutedEventArgs e) => Navigate(new GettingStartedGalleryPage(), null, GetStartedTopNav);
    private void ShowDevelop(object? s, RoutedEventArgs e) => Navigate(new DeveloperGalleryPage(), null, DevelopTopNav);
    private void ShowFoundations(object? s, RoutedEventArgs e) => Navigate(new ThemeResourcesGalleryPage(), ThemeResourcesNav, FoundationsTopNav);
    private void ShowStyles(object? s, RoutedEventArgs e) => Navigate(new StylesGalleryPage(), null, StylesTopNav);
    private void ShowComponents(object? s, RoutedEventArgs e) => Navigate(new ComponentsOverviewGalleryPage(), ComponentsOverviewNav, ComponentsTopNav);

    private void ShowAndroidSettings(object? s, RoutedEventArgs e) => Navigate(new AndroidSettingsSamplePage(), AndroidSettingsNav);
    private void ShowClock(object? s, RoutedEventArgs e) => Navigate(new ClockSamplePage(), ClockNav);
    private void ShowTasks(object? s, RoutedEventArgs e) => Navigate(new TasksSamplePage(), TasksNav);

    private void ShowButtons(object? s, RoutedEventArgs e) => Navigate(new ButtonGalleryPage(), ButtonNav);
    private void ShowIconButtons(object? s, RoutedEventArgs e) => Navigate(new IconButtonGalleryPage(), IconButtonNav);
    private void ShowFabs(object? s, RoutedEventArgs e) => Navigate(new FabGalleryPage(), FabNav);
    private void ShowAppBars(object? s, RoutedEventArgs e) => Navigate(new AppBarGalleryPage(), AppBarNav);
    private void ShowBadges(object? s, RoutedEventArgs e) => Navigate(new BadgeGalleryPage(), BadgeNav);
    private void ShowBreadcrumbs(object? s, RoutedEventArgs e) => Navigate(new BreadcrumbGalleryPage(), BreadcrumbNav);
    private void ShowTextFields(object? s, RoutedEventArgs e) => Navigate(new TextBoxGalleryPage(), TextFieldNav);
    private void ShowCheckBoxes(object? s, RoutedEventArgs e) => Navigate(new CheckBoxGalleryPage(), CheckBoxNav);
    private void ShowRadioButtons(object? s, RoutedEventArgs e) => Navigate(new RadioButtonGalleryPage(), RadioButtonNav);
    private void ShowComboBoxes(object? s, RoutedEventArgs e) => Navigate(new ComboBoxGalleryPage(), ComboBoxNav);
    private void ShowCarousel(object? s, RoutedEventArgs e) => Navigate(new CarouselGalleryPage(), CarouselNav);
    private void ShowCards(object? s, RoutedEventArgs e) => Navigate(new CardGalleryPage(), CardNav);
    private void ShowChips(object? s, RoutedEventArgs e) => Navigate(new ChipGalleryPage(), ChipNav);
    private void ShowPickers(object? s, RoutedEventArgs e) => Navigate(new PickerGalleryPage(), PickerNav);
    private void ShowColorPicker(object? s, RoutedEventArgs e) => Navigate(new ColorPickerGalleryPage(), ColorPickerNav);
    private void ShowDialogs(object? s, RoutedEventArgs e) => Navigate(new DialogGalleryPage(), DialogNav);
    private void ShowDivider(object? s, RoutedEventArgs e) => Navigate(new DividerGalleryPage(), DividerNav);
    private void ShowLists(object? s, RoutedEventArgs e) => Navigate(new ListGalleryPage(), ListNav);
    private void ShowLoading(object? s, RoutedEventArgs e) => Navigate(new LoadingGalleryPage(), LoadingNav);
    private void ShowProgress(object? s, RoutedEventArgs e) => Navigate(new ProgressGalleryPage(), ProgressNav);
    private void ShowMenus(object? s, RoutedEventArgs e) => Navigate(new MenuGalleryPage(), MenuNav);
    private void ShowNavigationBar(object? s, RoutedEventArgs e) => Navigate(new NavigationBarGalleryPage(), NavigationBarNav);
    private void ShowNavigationDrawer(object? s, RoutedEventArgs e) => Navigate(new NavigationDrawerGalleryPage(), NavigationDrawerNav);
    private void ShowAdaptive(object? s, RoutedEventArgs e) => Navigate(new AdaptiveGalleryPage(), AdaptiveNav);
    private void ShowSearch(object? s, RoutedEventArgs e) => Navigate(new SearchGalleryPage(), SearchNav);
    private void ShowSettingsCard(object? s, RoutedEventArgs e) => Navigate(new SettingsCardGalleryPage(), SettingsCardNav);
    private void ShowSheets(object? s, RoutedEventArgs e) => Navigate(new SheetGalleryPage(), SheetNav);
    private void ShowSlider(object? s, RoutedEventArgs e) => Navigate(new SliderGalleryPage(), SliderNav);
    private void ShowAdvancedSelection(object? s, RoutedEventArgs e) => Navigate(new AdvancedSelectionGalleryPage(), AdvancedSelectionNav);
    private void ShowSnackbar(object? s, RoutedEventArgs e) => Navigate(new SnackbarGalleryPage(), SnackbarNav);
    private void ShowSwitch(object? s, RoutedEventArgs e) => Navigate(new SwitchGalleryPage(), SwitchNav);
    private void ShowTabs(object? s, RoutedEventArgs e) => Navigate(new TabGalleryPage(), TabNav);
    private void ShowToolbars(object? s, RoutedEventArgs e) => Navigate(new ToolbarGalleryPage(), ToolbarNav);
    private void ShowTooltips(object? s, RoutedEventArgs e) => Navigate(new TooltipGalleryPage(), TooltipNav);
    private void ShowFlutterParity(object? s, RoutedEventArgs e) => Navigate(new FlutterParityGalleryPage(), FlutterParityNav);
    private void ShowEcosystem(object? s, RoutedEventArgs e) => Navigate(new EcosystemGalleryPage(), EcosystemNav);
    private void ShowBorderlessWindows(object? s, RoutedEventArgs e) => Navigate(new BorderlessWindowGalleryPage(), BorderlessWindowNav);
    private void ShowDesktopAdapters(object? s, RoutedEventArgs e) => Navigate(new DesktopAdaptersGalleryPage(), DesktopAdaptersNav);
    private void ShowThemeResources(object? s, RoutedEventArgs e) => Navigate(new ThemeResourcesGalleryPage(), ThemeResourcesNav);
    private void ShowSymbols(object? s, RoutedEventArgs e) => Navigate(new SymbolGalleryPage(), SymbolsNav);
    private void ShowMotion(object? s, RoutedEventArgs e) => Navigate(new MotionGalleryPage(), MotionNav);

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        ApplyResponsiveLayout(e.NewSize.Width);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && _usesModalNavigation && _navigationOpen)
        {
            SetNavigationOpen(false);
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    private void ApplyResponsiveLayout(double width)
    {
        CurrentBreakpoint = width switch
        {
            < 600 => GalleryBreakpoint.Compact,
            < 840 => GalleryBreakpoint.Medium,
            < 1200 => GalleryBreakpoint.Expanded,
            < 1600 => GalleryBreakpoint.Large,
            _ => GalleryBreakpoint.ExtraLarge
        };

        var usesPrimaryRail = width >= 600;
        _usesModalNavigation = width < 1200;
        RootLayout.ColumnDefinitions = usesPrimaryRail
            ? new ColumnDefinitions("96,*")
            : new ColumnDefinitions("0,*");
        PrimaryNavigationRail.IsVisible = usesPrimaryRail;
        CompactBrandMark.IsVisible = !usesPrimaryRail;
        NavigationMenuButton.IsVisible = _usesModalNavigation;
        TopNavigation.IsVisible = true;
        DevelopTopNav.IsVisible = true;
        StylesTopNav.IsVisible = true;
        GallerySearch.IsVisible = width >= 540;
        LanguageSelector.IsVisible = width >= 1040;
        ThemeSelector.IsVisible = width >= 420;
        BrandTitle.IsVisible = width >= 640;
        TableOfContentsPane.IsVisible = width >= 1200;

        ShellGrid.ColumnDefinitions = CurrentBreakpoint switch
        {
            GalleryBreakpoint.Compact or GalleryBreakpoint.Medium or GalleryBreakpoint.Expanded => new ColumnDefinitions("0,*,0"),
            GalleryBreakpoint.Large => new ColumnDefinitions("260,*,210"),
            _ => new ColumnDefinitions("280,*,240")
        };
        PageHost.Margin = CurrentBreakpoint switch
        {
            GalleryBreakpoint.Compact => new Thickness(16, 20, 16, 32),
            GalleryBreakpoint.Medium => new Thickness(28, 28, 24, 36),
            GalleryBreakpoint.Expanded => new Thickness(36, 36, 32, 44),
            _ => new Thickness(64, 56, 56, 72)
        };
        PageHost.MaxWidth = width >= 1200 ? 960 : double.PositiveInfinity;

        if (_usesModalNavigation)
        {
            Grid.SetColumn(NavigationPane, 0);
            Grid.SetColumnSpan(NavigationPane, 3);
            NavigationPane.IsModal = true;
            NavigationPane.DrawerWidth = Math.Min(320, Math.Max(260, width * 0.85));
            NavigationPane.Width = NavigationPane.DrawerWidth;
            NavigationPane.HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Left;
            NavigationPane.IsOpen = _navigationOpen;
            NavigationPane.IsVisible = true;
            NavigationScrim.IsVisible = _navigationOpen;
        }
        else
        {
            _navigationOpen = false;
            Grid.SetColumn(NavigationPane, 0);
            Grid.SetColumnSpan(NavigationPane, 1);
            NavigationPane.IsModal = false;
            NavigationPane.DrawerWidth = CurrentBreakpoint == GalleryBreakpoint.Expanded ? 240 : CurrentBreakpoint == GalleryBreakpoint.Large ? 260 : 280;
            NavigationPane.Width = double.NaN;
            NavigationPane.HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch;
            NavigationPane.IsOpen = true;
            NavigationPane.IsVisible = true;
            NavigationScrim.IsVisible = false;
        }

        Dispatcher.UIThread.Post(() => ApplyResponsivePageSizing(width), DispatcherPriority.Loaded);
    }

    private void ApplyResponsivePageSizing(double shellWidth)
    {
        if (PageHost.Content is not Control page) return;
        if (CurrentBreakpoint is GalleryBreakpoint.Compact or GalleryBreakpoint.Medium)
        {
            var available = Math.Max(260, shellWidth - PageHost.Margin.Left - PageHost.Margin.Right);
            foreach (var control in page.GetVisualDescendants().OfType<Control>().Prepend(page))
            {
                if (double.IsNaN(control.Width) || control.Width <= available || control is Window) continue;
                if (!_responsiveSizeStates.ContainsKey(control))
                    _responsiveSizeStates[control] = new ResponsiveSizeState(control.Width, control.MaxWidth, control.HorizontalAlignment);
                control.MaxWidth = available;
                control.Width = double.NaN;
                control.HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch;
            }
        }
        else
        {
            foreach (var pair in _responsiveSizeStates.Where(pair => ReferenceEquals(pair.Key, page) || pair.Key.IsVisualAncestorOf(page) || page.IsVisualAncestorOf(pair.Key)).ToArray())
            {
                pair.Key.Width = pair.Value.Width;
                pair.Key.MaxWidth = pair.Value.MaxWidth;
                pair.Key.HorizontalAlignment = pair.Value.HorizontalAlignment;
                _responsiveSizeStates.Remove(pair.Key);
            }
        }
    }

    private void FocusGallerySearch(object? sender, RoutedEventArgs e)
    {
        GallerySearch.IsVisible = true;
        GallerySearch.Focus();
    }

    private void ToggleNavigation(object? sender, RoutedEventArgs e) => SetNavigationOpen(!_navigationOpen);
    private void DismissNavigation(object? sender, PointerPressedEventArgs e) => SetNavigationOpen(false);

    private void SetNavigationOpen(bool open)
    {
        if (!_usesModalNavigation) return;
        _navigationOpen = open;
        NavigationPane.IsOpen = open;
        NavigationScrim.IsVisible = open;
        if (open) _navigationButtons.FirstOrDefault(button => button.IsVisible)?.Focus();
        else NavigationMenuButton.Focus();
    }

    private void Navigate(Control page, MdButton? selected, MdButton? selectedTopNavigation = null)
    {
        foreach (var pair in _responsiveSizeStates)
        {
            pair.Key.Width = pair.Value.Width;
            pair.Key.MaxWidth = pair.Value.MaxWidth;
            pair.Key.HorizontalAlignment = pair.Value.HorizontalAlignment;
        }
        _responsiveSizeStates.Clear();
        if (page is UserControl userPage && userPage.Content is ScrollViewer localScroll)
        {
            // Pages keep a standalone ScrollViewer for direct preview, but the Gallery shell owns
            // the real finite viewport. Unwrapping prevents the inner viewer from being measured
            // with infinity and guarantees that PageScroll receives a non-zero extent.
            var body = localScroll.Content;
            localScroll.Content = null;
            userPage.Content = body;
        }
        else if (page is UserControl dialogPage && dialogPage.Content is Grid standaloneRoot &&
                 standaloneRoot.Children.OfType<MdScrollViewer>().FirstOrDefault(viewer => viewer.Name == "StandalonePageScroll") is { } standaloneScroll)
        {
            // Dialog-heavy pages keep a local modal host for standalone previews. In the shell,
            // use GlobalDialogHost and unwrap only the scrolling body into PageScroll.
            var body = standaloneScroll.Content;
            standaloneScroll.Content = null;
            dialogPage.Content = body;
        }
        PageScroll.ScrollToHome();
        PageHost.Content = page;
        PageScroll.ScrollToHome();
        foreach (var button in _navigationButtons)
            button.Variant = ReferenceEquals(button, selected) ? MdButtonVariant.Tonal : MdButtonVariant.Text;
        selectedTopNavigation ??= selected is null
            ? ComponentsTopNav
            : ReferenceEquals(selected, ThemeResourcesNav) || ReferenceEquals(selected, SymbolsNav) || ReferenceEquals(selected, MotionNav)
                ? FoundationsTopNav
                : ReferenceEquals(selected, DesktopAdaptersNav) ? DevelopTopNav : ComponentsTopNav;
        _currentPageType = page.GetType();
        _currentNavigationButton = selected;
        // The M3 site has hierarchical primary and contextual navigation, but only the actual
        // destination receives an active indicator. A contextual leaf therefore clears the
        // category-rail highlight instead of leaving two destinations looking selected.
        _currentTopNavigationButton = selected is null ? selectedTopNavigation : null;
        foreach (var button in _topNavigationButtons)
            button.Variant = ReferenceEquals(button, _currentTopNavigationButton) ? MdButtonVariant.Tonal : MdButtonVariant.Text;
        if (_usesModalNavigation) SetNavigationOpen(false);
        Dispatcher.UIThread.Post(() =>
        {
            GalleryLocalization.Apply(page, _culture);
            BuildTableOfContents(page);
            ApplyResponsivePageSizing(Bounds.Width > 0 ? Bounds.Width : Width);
            PageScroll.ScrollToHome();
            // Scroll anchoring runs during layout after content replacement. Reset once more at a
            // lower priority so every page opens at its title rather than retaining the old anchor.
            Dispatcher.UIThread.Post(PageScroll.ScrollToHome, DispatcherPriority.Background);
        }, DispatcherPriority.Loaded);
    }

    private void GallerySearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        var query = GallerySearch.Text?.Trim() ?? string.Empty;
        foreach (var entry in _galleryIndex)
            entry.Button.IsVisible = string.IsNullOrEmpty(query) || entry.Matches(query);
    }

    private void GallerySearchSubmitted(object? sender, string query)
    {
        var match = _galleryIndex.FirstOrDefault(entry => entry.Matches(query));
        if (match is null) return;
        Navigate(match.Factory(), match.Button);
        GallerySearch.Text = match.Title;
    }

    private void BuildTableOfContents(Control page)
    {
        TableOfContentsItems.Children.Clear();
        var allHeadings = page.GetVisualDescendants()
            .OfType<TextBlock>()
            .Where(text => text.Classes.Contains("h1") || text.Classes.Contains("h2"))
            .Where(text => !string.IsNullOrWhiteSpace(text.Text))
            .ToArray();
        foreach (var heading in allHeadings)
            AutomationProperties.SetHeadingLevel(heading, heading.Classes.Contains("h1") ? 1 : 2);

        // Material's page-level table of contents lists document sections, not the page title.
        foreach (var heading in allHeadings.Where(text => text.Classes.Contains("h2")))
        {
            var link = new MdButton
            {
                Content = heading.Text,
                Variant = MdButtonVariant.Text,
                HorizontalContentAlignment = global::Avalonia.Layout.HorizontalAlignment.Left,
                FontWeight = global::Avalonia.Media.FontWeight.Normal
            };
            link.Click += (_, _) => heading.BringIntoView();
            TableOfContentsItems.Children.Add(link);
        }
        OnThisPageTitle.Text = GalleryLocalization.Choose("On this page", "本页内容");
    }

    private void OnPagePointerWheel(object? sender, PointerWheelEventArgs e)
    {
        // Let an embedded material scrolling surface consume the wheel while it still has room.
        // The shell takes over only at that surface's boundary, preserving normal scroll chaining.
        var nested = (e.Source as Visual)?.GetVisualAncestors()
            .OfType<ScrollViewer>()
            .FirstOrDefault(viewer => !ReferenceEquals(viewer, PageScroll));
        if (nested is not null)
        {
            var nestedMaximum = Math.Max(0, nested.Extent.Height - nested.Viewport.Height);
            var canConsume = e.Delta.Y > 0 && nested.Offset.Y > 0 ||
                             e.Delta.Y < 0 && nested.Offset.Y < nestedMaximum;
            if (canConsume) return;
        }

        var scroller = GetPageScroller();
        if (scroller is null || scroller.Extent.Height <= scroller.Viewport.Height) return;
        var next = Math.Clamp(scroller.Offset.Y - e.Delta.Y * 64, 0, Math.Max(0, scroller.Extent.Height - scroller.Viewport.Height));
        scroller.Offset = new Vector(scroller.Offset.X, next);
        e.Handled = true;
    }

    private ScrollViewer GetPageScroller() => PageScroll;

    private void LanguageSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _culture = CultureInfo.GetCultureInfo(LanguageSelector.SelectedIndex == 1 ? "zh-CN" : "en-US");
        CultureInfo.CurrentCulture = _culture;
        CultureInfo.CurrentUICulture = _culture;
        MdLocalization.SetCulture(this, _culture);
        var themeIndex = Math.Max(0, ThemeSelector.SelectedIndex);
        ThemeSelector.ItemsSource = _culture.TwoLetterISOLanguageName == "zh"
            ? new[] { "浅色", "深色", "跟随系统" }
            : new[] { "Light", "Dark", "System" };
        ThemeSelector.SelectedIndex = Math.Min(themeIndex, 2);
        GalleryLocalization.Apply(this, _culture);
        // Recreate the active page after changing CurrentUICulture. This updates data-bound
        // option collections and runtime-created content that are not present in the visual tree
        // when the localization walker runs (closed popups, dialog pages, and virtualized rows).
        if (_currentPageType is { } pageType && Activator.CreateInstance(pageType) is Control localizedPage)
            Navigate(localizedPage, _currentNavigationButton, _currentTopNavigationButton);
        else if (PageHost.Content is Control page)
            GalleryLocalization.Apply(page, _culture);
    }

    private void ThemeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var variant = ThemeSelector.SelectedIndex switch { 0 => ThemeVariant.Light, 1 => ThemeVariant.Dark, _ => ThemeVariant.Default };
        if (Application.Current is not { } application) return;
        application.RequestedThemeVariant = variant;
        var mode = ThemeSelector.SelectedIndex switch
        {
            0 => MdThemeMode.Light,
            1 => MdThemeMode.Dark,
            _ => MdThemeMode.System
        };
        var options = MdThemeManager.Current with { ThemeMode = mode };
        var dark = mode == MdThemeMode.Dark || mode == MdThemeMode.System && application.ActualThemeVariant == ThemeVariant.Dark;
        MdThemeManager.Apply(application, options, dark);
    }
}

internal enum GalleryBreakpoint
{
    Compact,
    Medium,
    Expanded,
    Large,
    ExtraLarge
}

internal readonly record struct ResponsiveSizeState(double Width, double MaxWidth, global::Avalonia.Layout.HorizontalAlignment HorizontalAlignment);

internal sealed record GalleryIndexEntry(string Title, string Keywords, MdButton Button, Func<Control> Factory)
{
    public bool Matches(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return true;
        return query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .All(term => Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                         Keywords.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                         Button.Content?.ToString()?.Contains(term, StringComparison.OrdinalIgnoreCase) == true);
    }
}
