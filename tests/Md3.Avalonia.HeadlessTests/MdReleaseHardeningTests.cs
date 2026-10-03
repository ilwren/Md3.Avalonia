using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Gallery;
using Md3.Avalonia.Themes.Dynamic;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdReleaseHardeningTests
{
    [AvaloniaFact]
    public void Accessibility_Metadata_And_Native_Automation_Peers_Are_Preserved()
    {
        var button = new MdButton { Content = "Save" };
        var field = new MdTextBox { Label = "Name" };
        var loading = new MdLoadingIndicator();
        var snackbar = new MdSnackbar { Content = "Saved" };
        var stateLayer = new MdStateLayer();
        var ripple = new MdRipplePresenter();
        var focusRing = new MdFocusRing();

        Assert.NotNull(ControlAutomationPeer.CreatePeerForElement(button));
        Assert.NotNull(ControlAutomationPeer.CreatePeerForElement(field));
        Assert.NotNull(ControlAutomationPeer.CreatePeerForElement(loading));
        Assert.Equal(AutomationLiveSetting.Polite, AutomationProperties.GetLiveSetting(snackbar));
        Assert.Equal(AccessibilityView.Raw, AutomationProperties.GetAccessibilityView(stateLayer));
        Assert.Equal(AccessibilityView.Raw, AutomationProperties.GetAccessibilityView(ripple));
        Assert.Equal(AccessibilityView.Raw, AutomationProperties.GetAccessibilityView(focusRing));
        Assert.Equal("Loading", AutomationProperties.GetName(loading));
    }

    [AvaloniaFact]
    public void CommunityToolkit_ViewModel_Binds_TwoWay_And_Executes_RelayCommand()
    {
        var viewModel = new ToolkitCompatibilityViewModel();
        var toggle = new MdSwitch { DataContext = viewModel };
        toggle.Bind(ToggleButton.IsCheckedProperty, new Binding(nameof(ToolkitCompatibilityViewModel.Enabled))
        {
            Mode = BindingMode.TwoWay
        });
        var action = new MdButton { DataContext = viewModel, Command = viewModel.ToggleCommand };
        var window = new Window { Content = new StackPanel { Children = { toggle, action } } };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.False(toggle.IsChecked);
            viewModel.Enabled = true;
            Dispatcher.UIThread.RunJobs();
            Assert.True(toggle.IsChecked);
            Assert.Same(viewModel.ToggleCommand, action.Command);
            action.Command.Execute(action.CommandParameter);
            Assert.False(viewModel.Enabled);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void List_And_Carousel_Use_Bounded_Realization_For_Large_Data()
    {
        var list = new MdList { Height = 320, ItemsSource = Enumerable.Range(0, 5000).Select(i => $"Item {i}").ToArray() };
        var carousel = new MdCarousel
        {
            Height = 240,
            ItemsSource = Enumerable.Range(0, 2000).Select(i => $"Card {i}").ToArray()
        };
        var window = new Window
        {
            Width = 800,
            Height = 700,
            Content = new StackPanel { Spacing = 16, Children = { list, carousel } }
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var listRealized = list.GetVisualDescendants().OfType<ListBoxItem>().Count();
            var carouselRealized = carousel.GetVisualDescendants().OfType<ListBoxItem>().Count();
            Assert.InRange(listRealized, 1, 100);
            Assert.InRange(carouselRealized, 1, 100);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Visual_Matrix_Renders_All_Breakpoints_Themes_Contrasts_And_Directions()
    {
        var application = Assert.IsAssignableFrom<Application>(Application.Current);
        var view = new AndroidGalleryView();
        var window = new Window { Content = view, Height = 760 };
        window.Show();
        try
        {
            var widths = new[] { 360d, 412d, 480d, 720d, 1000d, 1360d, 1680d };
            foreach (var width in widths)
            foreach (var dark in new[] { false, true })
            foreach (var contrast in Enum.GetValues<MdThemeContrastLevel>())
            foreach (var direction in new[] { FlowDirection.LeftToRight, FlowDirection.RightToLeft })
            {
                window.Width = width;
                view.FlowDirection = direction;
                application.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
                MdThemeManager.Apply(application, new MdThemeOptions
                {
                    SeedColor = "#6750A4",
                    ThemeMode = dark ? MdThemeMode.Dark : MdThemeMode.Light,
                    ContrastLevel = contrast
                }, dark);
                Dispatcher.UIThread.RunJobs();
                using var frame = window.CaptureRenderedFrame();
                Assert.NotNull(frame);
                Assert.True(view.Bounds.Width > 0);
                Assert.True(view.Bounds.Height > 0);
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Repeated_Theme_And_Visual_Lifecycle_Does_Not_Grow_Resource_Set()
    {
        var application = Assert.IsAssignableFrom<Application>(Application.Current);
        MdThemeManager.Apply(application, new MdThemeOptions(), false);
        var count = application.Resources.Count;
        for (var index = 0; index < 100; index++)
        {
            MdThemeManager.Apply(application, new MdThemeOptions
            {
                SeedColor = index % 2 == 0 ? "#6750A4" : "#006A6A",
                SchemeVariant = index % 3 == 0 ? MdThemeSchemeVariant.Expressive : MdThemeSchemeVariant.TonalSpot,
                ContrastLevel = (MdThemeContrastLevel)(index % 3)
            }, index % 2 == 0);
        }
        Assert.Equal(count, application.Resources.Count);

        var indicator = new MdLoadingIndicator();
        var window = new Window { Content = indicator };
        window.Show();
        Assert.True(indicator.IsAttachedToVisualTree());
        window.Close();
        Assert.False(indicator.IsAttachedToVisualTree());
    }
}

internal sealed class ToolkitCompatibilityViewModel : ObservableObject
{
    private bool _enabled;

    public ToolkitCompatibilityViewModel() => ToggleCommand = new RelayCommand(() => Enabled = !Enabled);

    public bool Enabled
    {
        get => _enabled;
        set => SetProperty(ref _enabled, value);
    }

    public IRelayCommand ToggleCommand { get; }
}
