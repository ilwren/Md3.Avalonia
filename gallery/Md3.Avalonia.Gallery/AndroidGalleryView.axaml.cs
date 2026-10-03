using System.Globalization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Styling;
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
        AddPage("Breadcrumbs", () => new BreadcrumbGalleryPage());
        AddPage("Text fields", () => new TextBoxGalleryPage());
        AddPage("Checkbox", () => new CheckBoxGalleryPage());
        AddPage("Radio buttons", () => new RadioButtonGalleryPage());
        AddPage("Combo box", () => new ComboBoxGalleryPage());
        AddPage("Carousel", () => new CarouselGalleryPage());
        AddPage("Cards", () => new CardGalleryPage());
        AddPage("Chips", () => new ChipGalleryPage());
        AddPage("Date and time pickers", () => new PickerGalleryPage());
        AddPage("Color picker", () => new ColorPickerGalleryPage());
        AddPage("Dialogs", () => new DialogGalleryPage());
        AddPage("Divider", () => new DividerGalleryPage());
        AddPage("Lists", () => new ListGalleryPage());
        AddPage("Loading", () => new LoadingGalleryPage());
        AddPage("Progress", () => new ProgressGalleryPage());
        AddPage("Menus", () => new MenuGalleryPage());
        AddPage("Navigation bar", () => new NavigationBarGalleryPage());
        AddPage("Navigation drawer", () => new NavigationDrawerGalleryPage());
        AddPage("Adaptive layout", () => new AdaptiveGalleryPage());
        AddPage("Search", () => new SearchGalleryPage());
        AddPage("Settings cards", () => new SettingsCardGalleryPage());
        AddPage("Sheets", () => new SheetGalleryPage());
        AddPage("Slider", () => new SliderGalleryPage());
        AddPage("Segmented and range", () => new AdvancedSelectionGalleryPage());
        AddPage("Snackbar", () => new SnackbarGalleryPage());
        AddPage("Switch", () => new SwitchGalleryPage());
        AddPage("Tabs", () => new TabGalleryPage());
        AddPage("Toolbars", () => new ToolbarGalleryPage());
        AddPage("Tooltips", () => new TooltipGalleryPage());

        AddSection("Library");
        AddPage("Flutter parity", () => new FlutterParityGalleryPage());
        AddPage("Flutter ecosystem", () => new EcosystemGalleryPage());
        AddPage("Borderless windows", () => new BorderlessWindowGalleryPage());
        AddPage("Desktop adapters", () => new DesktopAdaptersGalleryPage());
        AddPage("Theme Lab", () => new ThemeResourcesGalleryPage());
        AddPage("Material Symbols", () => new SymbolGalleryPage());
        AddPage("Motion", () => new MotionGalleryPage());
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
        MobilePageHost.Content = entry.Factory();
        _currentPageTitle = entry.Title;
        if (MobileAppBar is not null) MobileAppBar.Title = entry.Title;
        MobileDrawer.IsOpen = false;
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
                    new TextBlock { Text = "Md3.Avalonia v0.2.0", FontSize = 18, FontWeight = global::Avalonia.Media.FontWeight.SemiBold },
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

    private sealed record MobileGalleryEntry(string Title, Func<object?> Factory);
}
