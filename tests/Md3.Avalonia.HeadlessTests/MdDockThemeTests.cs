using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Md3.Avalonia.Themes;
using Xunit;
using DockDocument = Dock.Model.Avalonia.Controls.Document;
using DockRootDock = Dock.Model.Avalonia.Controls.RootDock;
using DockTool = Dock.Model.Avalonia.Controls.Tool;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Covers the Material theme for Dock, which ships in the opt-in Md3.Avalonia.Dock package.
/// </summary>
/// <remarks>
/// Every test here scopes the theme to its own window instead of registering it in
/// <see cref="TestApplication"/>, exactly as the gallery's Docking page scopes it to its own page.
/// The reason is that the Dock theme also has to carry the implicit Button theme Dock's chrome
/// derives from (see <see cref="Only_The_Scoped_Dock_Theme_Adds_The_Base_Button_Theme_Docks_Chrome_Needs"/>),
/// and this headless application is shared with the core and the other opt-in suites, whose
/// contracts are that adding <em>their</em> package does not restyle Avalonia's native controls.
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
        using var window = Show(new TextBlock());
        var variant = Application.Current!.ActualThemeVariant;
        Assert.DoesNotContain(window.Styles, style => style.GetType().Name == "DockFluentTheme");
        Assert.DoesNotContain(Application.Current.Styles, style => style.GetType().Name == "DockFluentTheme");

        foreach (var type in new[]
                 {
                     typeof(DockControl), typeof(DocumentControl), typeof(DocumentTabStripItem),
                     typeof(ToolControl), typeof(ToolTabStripItem), typeof(DockTarget),
                     typeof(RootDockControl), typeof(HostWindow)
                 })
        {
            Assert.True(window.TryGetResource(type, variant, out var value), type.Name);
            Assert.IsType<ControlTheme>(value);
        }
    }

    [AvaloniaFact]
    public void The_Material_Theme_Retokenizes_Docks_Accents()
    {
        // Dock's five DockApplicationAccent* brushes used to be #007ACC and friends, and every
        // tab, surface, border and chrome key was an alias of one of them. The package rewrites
        // those values into Md.Sys.Color tokens, so light and dark come from the Material theme.
        using var window = Show(new TextBlock());
        Assert.NotEqual(Color.Parse("#007ACC"), ColorOf(Brush(window, "DockApplicationAccentBrushLow")));

        Assert.Equal(ColorOf(Brush(window, "Md.Sys.Color.Primary.Brush")),
            ColorOf(Brush(window, "DockApplicationAccentBrushLow")));
        Assert.Equal(ColorOf(Brush(window, "Md.Sys.Color.Primary.Brush")),
            ColorOf(Brush(window, "DockTabActiveBackgroundBrush")));
        Assert.Equal(ColorOf(Brush(window, "Md.Sys.Color.Primary.Brush")),
            ColorOf(Brush(window, "DockTabSelectedForegroundBrush")));
        Assert.Equal(ColorOf(Brush(window, "Md.Sys.Color.Primary.Brush")),
            ColorOf(Brush(window, "DockTargetIndicatorBrush")));
        Assert.Equal(ColorOf(Brush(window, "Md.Sys.Color.OnPrimary.Brush")),
            ColorOf(Brush(window, "DockTabActiveForegroundBrush")));
        Assert.Equal(ColorOf(Brush(window, "Md.Sys.Color.OnPrimary.Brush")),
            ColorOf(Brush(window, "DockDocumentTabSelectedForegroundBrush")));
        Assert.Equal(ColorOf(Brush(window, "Md.Sys.Color.OnSurfaceVariant.Brush")),
            ColorOf(Brush(window, "DockTabForegroundBrush")));
        Assert.Equal(ColorOf(Brush(window, "Md.Sys.Color.OutlineVariant.Brush")),
            ColorOf(Brush(window, "DockBorderSubtleBrush")));
        Assert.Equal(ColorOf(Brush(window, "Md.Sys.Color.Outline.Brush")),
            ColorOf(Brush(window, "DockBorderStrongBrush")));
        Assert.Equal(ColorOf(Brush(window, "Md.Sys.Color.SurfaceContainerLowest.Brush")),
            ColorOf(Brush(window, "DockSurfaceEditorBrush")));
        Assert.Equal(ColorOf(Brush(window, "Md.Sys.Color.SurfaceContainerHigh.Brush")),
            ColorOf(Brush(window, "DockSurfaceHeaderBrush")));
        Assert.Equal(ColorOf(Brush(window, "Md.Sys.Color.ErrorContainer.Brush")),
            ColorOf(Brush(window, "DockChromeButtonDangerHoverBrush")));

        // Hover is a Material state layer rather than a second accent colour.
        var hover = Assert.IsAssignableFrom<ISolidColorBrush>(Brush(window, "DockTabHoverBackgroundBrush"));
        Assert.Equal(ColorOf(Brush(window, "Md.Sys.Color.OnSurface.Brush")), hover.Color);
        Assert.Equal(0.08, hover.Opacity, 3);

        // Dock's metrics survive the retoken; only the corner shape became M3.
        Assert.True(window.TryGetResource("DockHeaderHeight", null, out var headerHeight));
        Assert.Equal(26d, Assert.IsType<double>(headerHeight));
        Assert.True(window.TryGetResource("DockDocumentTabItemCornerRadius", null, out var corner));
        Assert.Equal(new CornerRadius(4), Assert.IsType<CornerRadius>(corner));
    }

    [AvaloniaFact]
    public void The_Dock_Control_Builds_A_Layout_With_The_Material_Theme()
    {
        var dock = NewDock();
        using var window = Show(dock);

        Assert.NotNull(dock.Layout);
        var center = dock.Layout!.VisibleDockables!.OfType<ProportionalDock>().Single();
        var documentDock = center.VisibleDockables!.OfType<DocumentDock>().Single();
        Assert.Equal(2, documentDock.VisibleDockables!.Count);
        Assert.Equal("Documents", documentDock.ActiveDockable!.Title);

        // The dock chrome is built from Dock's own templates, which the theme reaches through the
        // implicit ControlThemes asserted above.
        Assert.NotEmpty(dock.GetVisualDescendants().OfType<DocumentTabStripItem>());
        Assert.NotEmpty(dock.GetVisualDescendants().OfType<ToolTabStripItem>());
    }

    [AvaloniaFact]
    public void Only_The_Scoped_Dock_Theme_Adds_The_Base_Button_Theme_Docks_Chrome_Needs()
    {
        // Dock's keyed chrome themes (ButtonCreateDocument, ChromeButton and DockDrawnCaptionButton)
        // are BasedOn="{StaticResource {x:Type Button}}". Core MaterialTheme leaves Avalonia's native
        // controls alone and this package replaces DockFluentTheme, so wherever the Dock theme is
        // registered it brings that one implicit theme with it - and only that one.
        var variant = Application.Current!.ActualThemeVariant;
        foreach (var native in new[]
                 {
                     typeof(Button), typeof(TextBox), typeof(ScrollViewer), typeof(ListBox), typeof(ToggleButton)
                 })
        {
            Assert.False(Application.Current.TryGetResource(native, variant, out _), native.Name);
        }

        var untouched = new Button { Content = "plain" };
        using (Show(untouched, dockTheme: false))
        {
            Assert.Null(untouched.Background);
        }

        var button = new Button { Content = "plain" };
        using var window = Show(button);
        foreach (var native in new[] { typeof(TextBox), typeof(ScrollViewer), typeof(ListBox), typeof(ToggleButton) })
        {
            Assert.False(window.TryGetResource(native, variant, out _), native.Name);
        }

        // The Button theme is this package's own Material flat button, not Avalonia FluentTheme's:
        // the marker state-layer brushes only exist in Controls/MaterialButton.axaml. Avalonia applies
        // an implicit ControlTheme without setting Control.Theme, so the observable proof is the
        // style it installs - the transparent container the theme gives every Button.
        Assert.True(window.TryGetResource("Md.Comp.Dock.ButtonHover.Brush", variant, out _));
        Assert.True(window.TryGetResource("Md.Comp.Dock.ButtonPressed.Brush", variant, out _));
        Assert.True(window.TryGetResource(typeof(Button), variant, out var value));
        Assert.Equal(typeof(Button), Assert.IsType<ControlTheme>(value).TargetType);
        Assert.Equal(Colors.Transparent, Assert.IsType<SolidColorBrush>(button.Background).Color);
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

        var dock = new DockControl
        {
            Width = 720,
            Height = 420,
            Factory = factory,
            Layout = root
        };

        factory.InitLayout(root);
        return dock;
    }

    /// <summary>
    /// Shows <paramref name="content"/> in a window that carries the Dock theme in its own styles,
    /// the same scoping the gallery's Docking page uses.
    /// </summary>
    private static Window Show(Control content, bool dockTheme = true)
    {
        var window = new Window { Content = content };
        if (dockTheme) window.Styles.Add(new MaterialDockTheme());
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static IBrush Brush(IResourceHost host, string key)
    {
        Assert.True(host.TryGetResource(key, Application.Current!.ActualThemeVariant, out var value), key);
        return Assert.IsAssignableFrom<IBrush>(value);
    }

    private static Color ColorOf(IBrush brush) => Assert.IsType<SolidColorBrush>(brush).Color;
}
