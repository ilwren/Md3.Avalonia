using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Shapes;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Themes;
using NodifyM.Avalonia.Controls;
using NodifyM.Avalonia.ViewModelBase;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Covers the Material style collection for NodifyM.Avalonia, which ships in the opt-in
/// Md3.Avalonia.NodifyM package.
/// </summary>
/// <remarks>
/// The package does not port a single template: it style-includes the eleven per-control sheets the
/// pinned package ships and answers the brush keys those sheets read from its own accent dictionary.
/// That makes "the brush a control ends up using is a Material brush" the whole claim, so the tests
/// read brushes back off realized controls rather than off the dictionary.
/// <para>
/// One thing these tests deliberately do not do is pump the dispatcher with a populated editor on
/// screen. <see cref="NodifyEditor"/> rewrites its own <c>Width</c>, <c>Height</c> and <c>Zoom</c>
/// from its parent's size both in <c>OnLoaded</c> and in the <c>SizeChanged</c> handler it attaches
/// in <c>OnApplyTemplate</c>, and a headless run that realized its node containers and then pumped
/// the dispatcher stopped making progress: the job was cancelled rather than failing, so nothing
/// was reported. The tests therefore realize the individual controls the theme touches, and put the
/// editor in a window only to read back the two literals its style hardcodes - without a pump
/// afterwards. A populated editor is still covered: <see cref="MdGalleryTests"/> renders the gallery
/// page that carries one. Every test here also carries a timeout, so a stall shows up as a named
/// failure instead of a cancelled job.
/// </para>
/// </remarks>
public class MdNodifyMThemeTests
{
    [AvaloniaFact(Timeout = 60_000)]
    public void The_Node_Is_A_Material_Container_With_The_Right_Ink()
    {
        var node = NewNode();
        using var scope = Show(node);

        Assert.Equal(TokenColor("Md.Sys.Color.SurfaceContainerHigh.Brush"), ColorOf(node.Background!));
        Assert.Equal(TokenColor("Md.Sys.Color.OutlineVariant.Brush"), ColorOf(node.BorderBrush!));

        // Upstream's node style sets a 2 DIP border; an M3 card carries a hairline.
        Assert.Equal(new Thickness(1), node.BorderThickness);

        // The header and footer bars are filled containers, and the additive styles give each the
        // ink its container calls for instead of the literal White upstream's node style sets.
        Assert.Equal(TokenColor("Md.Sys.Color.SecondaryContainer.Brush"), ColorOf(node.HeaderBrush!));
        Assert.Equal(TokenColor("Md.Sys.Color.SurfaceContainer.Brush"), ColorOf(node.FooterBrush!));
        Assert.Equal(TokenColor("Md.Sys.Color.OnSurface.Brush"), ColorOf(node.Foreground!));

        var header = node.GetVisualDescendants().OfType<ContentPresenter>()
            .Single(presenter => presenter.Name == "HeaderContentPresenter");
        Assert.Equal(TokenColor("Md.Sys.Color.OnSecondaryContainer.Brush"), ColorOf(header.Foreground!));

        var footer = node.GetVisualDescendants().OfType<ContentPresenter>()
            .Single(presenter => presenter.Name == "FooterContentPresenter");
        Assert.Equal(TokenColor("Md.Sys.Color.OnSurfaceVariant.Brush"), ColorOf(footer.Foreground!));
    }

    [AvaloniaFact(Timeout = 60_000)]
    public void The_Pins_Split_By_Direction_And_The_Dragged_Wire_Is_Primary()
    {
        var node = NewNode();
        var pending = new PendingConnection();
        using var scope = Show(new StackPanel { Children = { node, pending } });

        // A connector throws "Connector must be placed inside a BaseNode" if it is realized on its
        // own, so the pins are read off the ones the node builds from its Input and Output.
        var input = node.GetVisualDescendants().OfType<NodeInput>().Single();
        var output = node.GetVisualDescendants().OfType<NodeOutput>().Single();

        // The one place the palette deliberately breaks upstream's symmetry, which leaves both pins
        // transparent with one border colour: an input pin is a secondary container and an output
        // pin a primary container, so a wire's two ends differ without reading a label.
        Assert.Equal(TokenColor("Md.Sys.Color.SecondaryContainer.Brush"), ColorOf(input.Background!));
        Assert.Equal(TokenColor("Md.Sys.Color.OnSecondaryContainer.Brush"), ColorOf(input.Foreground!));
        Assert.Equal(TokenColor("Md.Sys.Color.Outline.Brush"), ColorOf(input.BorderBrush!));

        Assert.Equal(TokenColor("Md.Sys.Color.PrimaryContainer.Brush"), ColorOf(output.Background!));
        Assert.Equal(TokenColor("Md.Sys.Color.OnPrimaryContainer.Brush"), ColorOf(output.Foreground!));

        // A pending wire - the drag in progress - is Primary rather than the resting wire's ink.
        Assert.Equal(TokenColor("Md.Sys.Color.Primary.Brush"), ColorOf(pending.Stroke!));
        Assert.Equal(TokenColor("Md.Sys.Color.SurfaceContainerHigh.Brush"), ColorOf(pending.Background!));
    }

    [AvaloniaFact(Timeout = 60_000)]
    public void The_Grid_Rule_Takes_The_Canvas_Brush()
    {
        var grid = new LargeGridLine { Width = 240, Height = 120 };
        using var scope = Show(grid);

        // The grid rule takes its colour from the property's own default - Gainsboro - because the
        // default grid-line template never sets a brush, so the key upstream defines for grid lines
        // is read by nothing until this style points the control at it.
        Assert.Equal(TokenColor("Md.Sys.Color.OutlineVariant.Brush"), ColorOf(grid.Brush));
    }

    [AvaloniaFact(Timeout = 60_000)]
    public void Selecting_A_Node_Frames_It_In_Primary()
    {
        var node = NewNode();
        using var scope = Show(node);

        // Upstream paints LightBlue on hover and Orange when selected; both literals become the same
        // Primary the item frame uses, and the resting border goes back when the node is released.
        node.IsSelected = true;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(TokenColor("Md.Sys.Color.Primary.Brush"), ColorOf(node.BorderBrush!));

        node.IsSelected = false;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(TokenColor("Md.Sys.Color.OutlineVariant.Brush"), ColorOf(node.BorderBrush!));
    }

    [AvaloniaFact(Timeout = 60_000)]
    public void The_Brushes_Follow_The_Material_Variant()
    {
        using var light = Show(NewNode());
        var lightSurface = TokenColor("Md.Sys.Color.SurfaceContainerHigh.Brush");
        Assert.Equal(lightSurface, ColorOf(light.Content<Node>().Background!));

        // Every brush in the accent dictionary takes its colour from a Md.Sys.Color.* token by
        // DynamicResource, so the theme needs no ThemeDictionaries of its own: the variant moves the
        // brushes under the controls rather than swapping a dictionary. The dark half of the
        // comparison gets its own window, because RequestedThemeVariant is a top-level property and
        // moving it on the application would re-theme every window the suite has open.
        var darkWindow = NewWindow(NewNode());
        darkWindow.RequestedThemeVariant = ThemeVariant.Dark;
        using var dark = new Scope(darkWindow);
        darkWindow.Show();
        Dispatcher.UIThread.RunJobs();

        var darkSurface = ColorOf(dark.Content<Node>().Background!);
        Assert.NotEqual(lightSurface, darkSurface);
        Assert.Equal(TokenColor("Md.Sys.Color.SurfaceContainerHigh.Brush", ThemeVariant.Dark), darkSurface);
    }

    [AvaloniaFact(Timeout = 60_000)]
    public void Registering_The_Theme_Leaves_Native_Controls_Alone()
    {
        // NodifyM's controls are all its own types, so there is no native control for this theme to
        // be careful about - which is what makes it safe to register application-wide. The claim is
        // checked where it matters: plain controls in the same window as a themed node.
        var box = new TextBox { Text = "plain" };
        var button = new Button { Content = "plain" };
        var check = new CheckBox();
        using var scope = Show(new StackPanel { Children = { NewNode(), box, button, check } });

        Assert.Equal("Filter", scope.Window.GetVisualDescendants().OfType<Node>().Single().Header);
        Assert.Null(box.Theme);
        Assert.Null(button.Theme);
        Assert.Null(check.Theme);
        Assert.DoesNotContain(Application.Current!.Styles, style => style.GetType().Name == "FluentTheme");
    }

    [AvaloniaFact(Timeout = 60_000)]
    public void The_Editor_Canvas_Takes_The_Surface_Brush()
    {
        // The editor's items host is painted with a literal Azure, so the canvas under it is
        // re-pointed at a brush key and the control gets a Background of its own to match. Both come
        // back off a real editor, templated but not laid out: the dispatcher is not pumped after it
        // is shown (see the remarks on this class).
        var editor = new NodifyEditor { Width = 640, Height = 420 };
        var window = NewWindow(editor);
        window.Show();
        editor.ApplyTemplate();
        using var scope = new Scope(window);

        Assert.Equal(TokenColor("Md.Sys.Color.Surface.Brush"), ColorOf(editor.Background!));

        // The Azure canvas, as distinct from the panel the items are hosted in.
        var canvas = editor.GetVisualDescendants().OfType<Canvas>()
            .First(child => child.Name != "PartItemsHost");
        Assert.Equal(TokenColor("Md.Sys.Color.Surface.Brush"), ColorOf(canvas.Background!));

        // The marquee is deliberately not asserted. Its stroke and fill are written on the rectangle
        // in the template rather than in a style, and a value set that way outranks every style
        // setter - a rule pointing it at a brush key measurably lost to the template's own value.
        // Reaching it would mean forking the editor's template, which this package does not do.
        var marquee = editor.GetVisualDescendants().OfType<Rectangle>()
            .Single(rectangle => rectangle.Name == "SelectionRent");
        Assert.Equal(TokenColor("Md.Sys.Color.SurfaceContainerLow.Brush"), ColorOf(marquee.Stroke!));
    }

    /// <summary>
    /// A node with one pin of each direction: enough to realize the card, its two presenter bars and
    /// the pins its template builds from <c>Input</c> and <c>Output</c>.
    /// </summary>
    private static Node NewNode() => new()
    {
        Header = "Filter",
        Footer = "rows > 0",
        MinWidth = 176,
        Location = new Point(96, 72),
        Input = new ObservableCollection<object> { Pin("Rows", ConnectorViewModelBase.ConnectorFlow.Input) },
        Output = new ObservableCollection<object> { Pin("Kept", ConnectorViewModelBase.ConnectorFlow.Output) }
    };

    private static ConnectorViewModelBase Pin(string title, ConnectorViewModelBase.ConnectorFlow flow) =>
        new() { Title = title, Flow = flow, IsConnected = true };

    // The theme is handed to the window that owns the controls, the way an application that does not
    // want it application-wide would register it; the gallery page registers it the same way.
    private static Scope Show(Control content)
    {
        var window = NewWindow(content);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new Scope(window);
    }

    private static Window NewWindow(Control content)
    {
        var window = new Window { Width = 900, Height = 640, Content = content };
        window.Styles.Add(new MaterialNodifyMTheme());
        return window;
    }

    private static IBrush Brush(string key, ThemeVariant? variant = null)
    {
        variant ??= Application.Current!.ActualThemeVariant;
        Assert.True(Application.Current!.TryGetResource(key, variant, out var value), key);
        return Assert.IsAssignableFrom<IBrush>(value);
    }

    private static Color TokenColor(string key, ThemeVariant? variant = null) => ColorOf(Brush(key, variant));

    private static Color ColorOf(IBrush brush) => Assert.IsType<SolidColorBrush>(brush).Color;

    private sealed class Scope : IDisposable
    {
        public Scope(Window window) => Window = window;

        public Window Window { get; }

        public T Content<T>() where T : Control => Assert.IsType<T>(Window.Content);

        public void Dispose() => Window.Close();
    }
}
