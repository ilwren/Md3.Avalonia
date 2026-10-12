using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;
using DockDocument = Dock.Model.Avalonia.Controls.Document;
using DockRootDock = Dock.Model.Avalonia.Controls.RootDock;
using DockTool = Dock.Model.Avalonia.Controls.Tool;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Covers the Material theme for Dock, which ships in the opt-in Md3.Avalonia.Dock package.
/// </summary>
/// <remarks>
/// The theme is registered in <see cref="TestApplication"/> alongside the other packages, which is
/// the documented application-wide usage: <c>Application.Styles</c> with <c>MaterialTheme</c> first.
/// The one consequence worth knowing is asserted by
/// <see cref="Registering_The_Dock_Theme_Only_Adds_The_Base_Button_Theme_Docks_Chrome_Needs"/>: Dock
/// keys its chrome themes on Avalonia's implicit Button theme, so this package supplies one, and it
/// is the only native control theme any of these suites adds.
/// </remarks>
public class MdDockThemeTests
{
    [AvaloniaFact]
    public void The_Material_Theme_Loads_Docks_Control_Themes()
    {
        // The package deliberately does not fork Dock's templates: it includes the pinned
        // Dock.Avalonia.Themes.Fluent dictionaries by avares URI and adds its own accents, so the
        // implicit themes for Dock's controls have to be reachable from this theme alone, with no
        // DockFluentTheme anywhere.
        var variant = Application.Current!.ActualThemeVariant;
        Assert.DoesNotContain(Application.Current.Styles, style => style.GetType().Name == "DockFluentTheme");

        foreach (var type in new[]
                 {
                     typeof(DockControl), typeof(DocumentControl), typeof(DocumentTabStripItem),
                     typeof(ToolControl), typeof(ToolTabStripItem), typeof(DockTarget),
                     typeof(RootDockControl), typeof(HostWindow)
                 })
        {
            Assert.True(Application.Current.TryGetResource(type, variant, out var value), type.Name);
            Assert.IsType<ControlTheme>(value);
        }
    }

    [AvaloniaFact]
    public void The_Material_Theme_Retokenizes_Docks_Accents()
    {
        // Dock's five DockApplicationAccent* brushes used to be #007ACC and friends, and every
        // tab, surface, border and chrome key was an alias of one of them. The package rewrites
        // those values into Md.Sys.Color tokens, so light and dark come from the Material theme.
        Assert.NotEqual(Color.Parse("#007ACC"), ColorOf(Brush("DockApplicationAccentBrushLow")));

        Assert.Equal(ColorOf(Brush("Md.Sys.Color.Primary.Brush")),
            ColorOf(Brush("DockApplicationAccentBrushLow")));
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.Primary.Brush")),
            ColorOf(Brush("DockTabActiveBackgroundBrush")));
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.Primary.Brush")),
            ColorOf(Brush("DockTabSelectedForegroundBrush")));
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.Primary.Brush")),
            ColorOf(Brush("DockTargetIndicatorBrush")));
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.OnPrimary.Brush")),
            ColorOf(Brush("DockTabActiveForegroundBrush")));
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.OnPrimary.Brush")),
            ColorOf(Brush("DockDocumentTabSelectedForegroundBrush")));
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.OnSurfaceVariant.Brush")),
            ColorOf(Brush("DockTabForegroundBrush")));
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.OutlineVariant.Brush")),
            ColorOf(Brush("DockBorderSubtleBrush")));
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.Outline.Brush")),
            ColorOf(Brush("DockBorderStrongBrush")));
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.SurfaceContainerLowest.Brush")),
            ColorOf(Brush("DockSurfaceEditorBrush")));
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.SurfaceContainerHigh.Brush")),
            ColorOf(Brush("DockSurfaceHeaderBrush")));
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.ErrorContainer.Brush")),
            ColorOf(Brush("DockChromeButtonDangerHoverBrush")));

        // Hover is a Material state layer rather than a second accent colour.
        var hover = Assert.IsAssignableFrom<ISolidColorBrush>(Brush("DockTabHoverBackgroundBrush"));
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.OnSurface.Brush")), hover.Color);
        Assert.Equal(0.08, hover.Opacity, 3);

        // Dock's metrics survive the retoken; only the corner shape became M3.
        Assert.True(Application.Current!.TryGetResource("DockHeaderHeight", null, out var headerHeight));
        Assert.Equal(26d, Assert.IsType<double>(headerHeight));
        Assert.True(Application.Current.TryGetResource("DockDocumentTabItemCornerRadius", null, out var corner));
        Assert.Equal(new CornerRadius(4), Assert.IsType<CornerRadius>(corner));
    }

    [AvaloniaFact]
    public void The_Dock_Control_Builds_A_Layout_With_The_Material_Theme()
    {
        var dock = NewDock();
        using var scope = Show(dock);

        Assert.NotNull(dock.Layout);
        var center = dock.Layout!.VisibleDockables!.OfType<ProportionalDock>().Single();
        var documentDock = center.VisibleDockables!.OfType<DocumentDock>().Single();
        Assert.Equal(2, documentDock.VisibleDockables!.Count);
        Assert.Equal("Documents", documentDock.ActiveDockable!.Title);

        // The dock chrome is built from Dock's own templates, which the theme reaches through the
        // implicit ControlThemes asserted above. Dock materializes the layout through a
        // DeferredContentControl, so the frame has to actually render before the tab strip items
        // exist - the same reason the gallery suite captures frames instead of just pumping jobs.
        scope.Window.CaptureRenderedFrame();
        Dispatcher.UIThread.RunJobs();

        Assert.NotEmpty(dock.GetVisualDescendants().OfType<DocumentTabStripItem>());
        Assert.NotEmpty(dock.GetVisualDescendants().OfType<ToolTabStripItem>());
    }

    [AvaloniaFact]
    public void Registering_The_Dock_Theme_Only_Adds_The_Base_Button_Theme_Docks_Chrome_Needs()
    {
        // Dock's keyed chrome themes (ButtonCreateDocument, ChromeButton and DockDrawnCaptionButton)
        // are BasedOn="{StaticResource {x:Type Button}}". Core MaterialTheme leaves Avalonia's native
        // controls alone and this package replaces DockFluentTheme, so the Dock theme brings that one
        // implicit theme with it - and only that one. This is the package's widest side effect and it
        // is deliberate: without it Dock chrome could not load at all.
        var variant = Application.Current!.ActualThemeVariant;
        foreach (var native in new[] { typeof(TextBox), typeof(ScrollViewer), typeof(ListBox), typeof(ToggleButton) })
        {
            Assert.False(Application.Current.TryGetResource(native, variant, out _), native.Name);
        }

        // The Button theme is this package's own Material flat button, not Avalonia FluentTheme's:
        // the marker state-layer brushes only exist in Controls/MaterialButton.axaml. Avalonia hangs
        // an implicit ControlTheme on the element without setting Control.Theme, so the observable
        // proof is the style it installs - the transparent container the theme gives every Button.
        Assert.True(Application.Current.TryGetResource("Md.Comp.Dock.ButtonHover.Brush", variant, out _));
        Assert.True(Application.Current.TryGetResource("Md.Comp.Dock.ButtonPressed.Brush", variant, out _));
        Assert.True(Application.Current.TryGetResource(typeof(Button), variant, out var value));
        var buttonTheme = Assert.IsType<ControlTheme>(value);
        Assert.Equal(typeof(Button), buttonTheme.TargetType);

        // Dock's own chrome then builds on it: ChromeButton is the keyed theme Dock's tool chrome
        // resolves, and it is only loadable because the key above existed when Dock's dictionaries
        // were merged.
        Assert.True(Application.Current.TryGetResource("ChromeButton", variant, out var chromeButtonValue));
        Assert.Same(buttonTheme, Assert.IsType<ControlTheme>(chromeButtonValue).BasedOn);

        var button = new Button { Content = "plain" };
        using var scope = Show(button);
        Assert.Equal(Colors.Transparent, Assert.IsAssignableFrom<ISolidColorBrush>(button.Background).Color);
    }

    private static DockControl NewDock()
    {
        var factory = new Factory();

        var explorer = new DockTool { Id = "Explorer", Title = "Explorer" };
        var output = new DockTool { Id = "Output", Title = "Output" };
        var tools = new ToolDock
        {
            Id = "Tools",
            Title = "Tools",
            Alignment = Alignment.Left,
            VisibleDockables = factory.CreateList<IDockable>(explorer),
            ActiveDockable = explorer
        };
        var bottom = new ToolDock
        {
            Id = "Bottom",
            Title = "Bottom",
            Alignment = Alignment.Bottom,
            VisibleDockables = factory.CreateList<IDockable>(output),
            ActiveDockable = output
        };

        var first = new DockDocument { Id = "Doc1", Title = "Documents" };
        var second = new DockDocument { Id = "Doc2", Title = "README.md" };
        var documents = new DocumentDock
        {
            Id = "Documents",
            CanCreateDocument = false,
            VisibleDockables = factory.CreateList<IDockable>(first, second),
            ActiveDockable = first
        };

        var center = new ProportionalDock
        {
            Id = "Center",
            Orientation = Orientation.Vertical,
            VisibleDockables = factory.CreateList<IDockable>(documents, new ProportionalDockSplitter(), bottom),
            ActiveDockable = documents
        };

        var root = new DockRootDock
        {
            Id = "Root",
            IsCollapsable = false,
            VisibleDockables = factory.CreateList<IDockable>(tools, new ProportionalDockSplitter(), center),
            ActiveDockable = center,
            DefaultDockable = center
        };

        // Deliberately the same shape as the Docking gallery page: let DockControl run
        // InitializeFactory/InitializeLayout itself, which also means it rebuilds the layout when
        // it attaches to the visual tree instead of leaving a manually built model behind.
        return new DockControl
        {
            Width = 720,
            Height = 420,
            InitializeFactory = true,
            InitializeLayout = true,
            Factory = factory,
            Layout = root
        };
    }

    /// <summary>Shows <paramref name="content"/> in a window and returns a scope that closes it.</summary>
    private static Scope Show(Control content)
    {
        var window = new Window { Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new Scope(window);
    }

    private sealed class Scope(Window window) : IDisposable
    {
        public Window Window { get; } = window;

        public void Dispose() => Window.Close();
    }

    private static IBrush Brush(string key)
    {
        Assert.True(Application.Current!.TryGetResource(key, Application.Current.ActualThemeVariant, out var value), key);
        return Assert.IsAssignableFrom<IBrush>(value);
    }

    private static Color ColorOf(IBrush brush) => Assert.IsType<SolidColorBrush>(brush).Color;
}
