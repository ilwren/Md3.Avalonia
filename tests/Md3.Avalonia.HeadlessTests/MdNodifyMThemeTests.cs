using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
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
/// That makes "the brushes the templates actually use are Material brushes" the whole claim, so the
/// tests below read the brushes back off realized controls rather than off the dictionary, and the
/// additive styles - the values upstream sets as literals, which no accent can reach - are asserted
/// the same way.
/// </remarks>
public class MdNodifyMThemeTests
{
    [AvaloniaFact]
    public void The_Canvas_And_Its_Nodes_Are_Painted_From_Tokens()
    {
        var editor = NewEditor();
        using var scope = Show(editor);

        // Upstream's editor style sets Background to a literal Azure and its canvas to the
        // NodifyEditor brush key; the theme answers the key with Surface and sets the literal again.
        Assert.Equal(TokenColor("Md.Sys.Color.Surface.Brush"), ColorOf(editor.Background!));

        var node = editor.GetVisualDescendants().OfType<Node>().Single();
        Assert.Equal(TokenColor("Md.Sys.Color.SurfaceContainerHigh.Brush"), ColorOf(node.Background!));
        Assert.Equal(TokenColor("Md.Sys.Color.OutlineVariant.Brush"), ColorOf(node.BorderBrush!));
        Assert.Equal(new Thickness(1), node.BorderThickness);

        // The header and footer bars are filled containers, and the additive styles give each the ink
        // its container calls for instead of the literal White upstream's node style sets.
        Assert.Equal(TokenColor("Md.Sys.Color.SecondaryContainer.Brush"), ColorOf(node.HeaderBrush!));
        Assert.Equal(TokenColor("Md.Sys.Color.SurfaceContainer.Brush"), ColorOf(node.FooterBrush!));
        Assert.Equal(TokenColor("Md.Sys.Color.OnSurface.Brush"), ColorOf(node.Foreground!));

        var header = editor.GetVisualDescendants().OfType<ContentPresenter>()
            .Single(presenter => presenter.Name == "HeaderContentPresenter");
        Assert.Equal(TokenColor("Md.Sys.Color.OnSecondaryContainer.Brush"), ColorOf(header.Foreground!));
        var footer = editor.GetVisualDescendants().OfType<ContentPresenter>()
            .Single(presenter => presenter.Name == "FooterContentPresenter");
        Assert.Equal(TokenColor("Md.Sys.Color.OnSurfaceVariant.Brush"), ColorOf(footer.Foreground!));
    }

    [AvaloniaFact]
    public void The_Pins_Split_By_Direction_And_The_Wires_Follow_The_Ink()
    {
        var editor = NewEditor();
        using var scope = Show(editor);

        // The one place the palette deliberately breaks upstream's symmetry: an input pin is a
        // secondary container and an output pin a primary container, so a wire's two ends differ.
        var input = editor.GetVisualDescendants().OfType<NodeInput>().Single();
        Assert.Equal(TokenColor("Md.Sys.Color.SecondaryContainer.Brush"), ColorOf(input.Background!));
        Assert.Equal(TokenColor("Md.Sys.Color.OnSecondaryContainer.Brush"), ColorOf(input.Foreground!));

        var output = editor.GetVisualDescendants().OfType<NodeOutput>().Single();
        Assert.Equal(TokenColor("Md.Sys.Color.PrimaryContainer.Brush"), ColorOf(output.Background!));
        Assert.Equal(TokenColor("Md.Sys.Color.OnPrimaryContainer.Brush"), ColorOf(output.Foreground!));

        // A pending wire - the drag in progress - is Primary rather than the resting wire's ink.
        var pending = editor.GetVisualDescendants().OfType<PendingConnection>().Single();
        Assert.Equal(TokenColor("Md.Sys.Color.Primary.Brush"), ColorOf(pending.Stroke!));
    }

    [AvaloniaFact]
    public void The_Theme_Reaches_The_Values_Upstream_Never_Put_In_A_Brush_Key()
    {
        var editor = NewEditor();
        var minimap = new NodifyEditorMinimap { Width = 180, Height = 140, Editor = editor };
        Grid.SetColumn(minimap, 1);
        var layout = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(new GridLength(200)) },
            Children = { editor, minimap }
        };
        using var scope = new Scope(NewWindow(layout));
        scope.Window.Show();
        editor.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        // The grid rule takes its colour from the property's own default - Gainsboro - because the
        // default grid-line template never sets a brush, so the key upstream defines for grid lines
        // is read by nothing until this style points the control at it.
        var grid = editor.GetVisualDescendants().OfType<LargeGridLine>().Single();
        Assert.Equal(TokenColor("Md.Sys.Color.OutlineVariant.Brush"), ColorOf(grid.Brush));

        // The minimap is the other place upstream builds brushes from raw colour keys: all six are
        // re-pointed at the brushes the canvas itself uses.
        Assert.Equal(TokenColor("Md.Sys.Color.Surface.Brush"), ColorOf(minimap.MapBackground));
        Assert.Equal(TokenColor("Md.Sys.Color.OutlineVariant.Brush"), ColorOf(minimap.MapBorderBrush));
        Assert.Equal(TokenColor("Md.Sys.Color.SecondaryContainer.Brush"), ColorOf(minimap.NodeBrush));
        Assert.Equal(TokenColor("Md.Sys.Color.OutlineVariant.Brush"), ColorOf(minimap.NodeBorderBrush));
        Assert.Equal(TokenColor("Md.Sys.Color.Primary.Brush"), ColorOf(minimap.ViewportBorderBrush));
        var viewport = Assert.IsAssignableFrom<ISolidColorBrush>(minimap.ViewportBrush);
        Assert.Equal(TokenColor("Md.Sys.Color.Primary.Brush"), viewport.Color);
        Assert.Equal(0.12, viewport.Opacity, 3);

        // ...and so is the marquee, which upstream fills with a node group's colours.
        var marquee = editor.GetVisualDescendants().OfType<Rectangle>().Single(rect => rect.Name == "SelectionRent");
        Assert.Equal(TokenColor("Md.Sys.Color.Primary.Brush"), ColorOf(marquee.Stroke!));
        var fill = Assert.IsAssignableFrom<ISolidColorBrush>(marquee.Fill);
        Assert.Equal(TokenColor("Md.Sys.Color.Primary.Brush"), fill.Color);
        Assert.Equal(0.12, fill.Opacity, 3);
    }

    [AvaloniaFact]
    public void Selecting_A_Node_Frames_It_In_Primary()
    {
        var editor = NewEditor();
        using var scope = Show(editor);
        var node = editor.GetVisualDescendants().OfType<Node>().Single();

        // Upstream paints LightBlue on hover and Orange when selected; both literals become the same
        // Primary the item frame uses, and the resting border goes back when the node is released.
        node.IsSelected = true;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(TokenColor("Md.Sys.Color.Primary.Brush"), ColorOf(node.BorderBrush!));

        node.IsSelected = false;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(TokenColor("Md.Sys.Color.OutlineVariant.Brush"), ColorOf(node.BorderBrush!));
    }

    [AvaloniaFact]
    public void The_Canvas_Follows_The_Material_Variant()
    {
        var editor = NewEditor();
        using var scope = Show(editor);
        var node = editor.GetVisualDescendants().OfType<Node>().Single();

        // Every brush in the accent dictionary takes its colour from a Md.Sys.Color.* token by
        // DynamicResource, so the theme needs no ThemeDictionaries of its own: the variant switch
        // moves the brushes under the controls rather than swapping a dictionary.
        var light = ColorOf(node.Background!);
        Assert.Equal(TokenColor("Md.Sys.Color.SurfaceContainerHigh.Brush"), light);

        var previous = Application.Current!.RequestedThemeVariant;
        try
        {
            Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
            Dispatcher.UIThread.RunJobs();

            var dark = ColorOf(node.Background!);
            Assert.NotEqual(light, dark);
            Assert.Equal(TokenColor("Md.Sys.Color.SurfaceContainerHigh.Brush"), dark);
            Assert.Equal(TokenColor("Md.Sys.Color.Surface.Brush"), ColorOf(editor.Background!));
        }
        finally
        {
            Application.Current!.RequestedThemeVariant = previous;
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public void Registering_The_Theme_Leaves_Native_Controls_Alone()
    {
        // NodifyM's controls are all its own types, so there is no native control for this theme to
        // be careful about - which is what makes it safe to register application-wide. The claim is
        // checked where it matters: a plain control in the same window as the editor.
        var editor = NewEditor();
        var box = new TextBox { Text = "plain" };
        var button = new Button { Content = "plain" };
        var check = new CheckBox();
        var panel = new StackPanel { Children = { box, button, check } };
        Grid.SetColumn(panel, 1);
        var window = new Window
        {
            Width = 900,
            Height = 640,
            Content = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(new GridLength(220)) },
                Children = { editor, panel }
            }
        };
        window.Styles.Add(new MaterialNodifyMTheme());
        window.Show();
        Dispatcher.UIThread.RunJobs();

        // The editor still realizes its node with the theme registered, which is the other half of
        // the claim: the theme reaches NodifyM's controls and stops there.
        Assert.Equal("Filter", editor.GetVisualDescendants().OfType<Node>().Single().Header);
        Assert.Null(box.Theme);
        Assert.Null(button.Theme);
        Assert.Null(check.Theme);
        Assert.DoesNotContain(Application.Current!.Styles, style => style.GetType().Name == "FluentTheme");
        window.Close();
    }

    [AvaloniaFact]
    public void The_Package_Exposes_One_Public_Type()
    {
        // Avalonia's XAML compiler adds two public helper types under CompiledAvaloniaXaml to every
        // assembly that embeds XAML, so the surface is counted over the package's own namespace.
        var exported = typeof(MaterialNodifyMTheme).Assembly.GetTypes()
            .Where(type => type.IsPublic && type.Namespace != "CompiledAvaloniaXaml")
            .ToArray();

        Assert.Equal([typeof(MaterialNodifyMTheme)], exported);
        Assert.IsAssignableFrom<Styles>(new MaterialNodifyMTheme());
    }

    /// <summary>
    /// One node with one pin of each direction, plus a pending wire: enough to realize every control
    /// the theme restyles except the connection shapes, which need anchors two pins write back.
    /// </summary>
    /// <remarks>
    /// The items have to be view models that a template turns into nodes. The editor casts the first
    /// visual child of every item container to <see cref="BaseNode"/>, so a <see cref="Node"/> handed
    /// to it as the item - which then becomes its own container - puts the cast on the border its
    /// template starts with, and the layout pass throws instead of arranging.
    /// </remarks>
    private static NodifyEditor NewEditor()
    {
        var editor = new NodifyEditor
        {
            Width = 640,
            Height = 420,
            ItemTemplate = new FuncDataTemplate<NodeViewModelBase>((node, _) => new Node
            {
                Header = node.Title,
                Footer = node.Footer,
                Location = node.Location,
                Input = node.Input,
                Output = node.Output
            })
        };
        editor.ItemsSource = new[]
        {
            new NodeViewModelBase
            {
                Title = "Filter",
                Footer = "rows > 0",
                Location = new Point(96, 72),
                Input = new ObservableCollection<object> { Pin("Rows", ConnectorViewModelBase.ConnectorFlow.Input) },
                Output = new ObservableCollection<object> { Pin("Kept", ConnectorViewModelBase.ConnectorFlow.Output) }
            }
        };
        editor.PendingConnection = new NodifyEditorViewModelBase().PendingConnection;
        return editor;
    }

    private static ConnectorViewModelBase Pin(string title, ConnectorViewModelBase.ConnectorFlow flow) =>
        new() { Title = title, Flow = flow, IsConnected = true };

    // The theme is handed to the window that owns the editor, the way an application that does not
    // want it application-wide would register it; the gallery page registers it the same way. The
    // template is applied before the jobs run because the parts these tests read - the marquee
    // rectangle, the grid-line presenter - are only in the tree once it is.
    private static Scope Show(NodifyEditor editor)
    {
        var window = NewWindow(editor);
        window.Show();
        editor.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
        return new Scope(window);
    }

    private static Window NewWindow(Control content)
    {
        var window = new Window { Width = 900, Height = 640, Content = content };
        window.Styles.Add(new MaterialNodifyMTheme());
        return window;
    }

    private static IBrush Brush(string key)
    {
        Assert.True(Application.Current!.TryGetResource(key, Application.Current.ActualThemeVariant, out var value), key);
        return Assert.IsAssignableFrom<IBrush>(value);
    }

    private static Color TokenColor(string key) => ColorOf(Brush(key));

    private static Color ColorOf(IBrush brush) => Assert.IsType<SolidColorBrush>(brush).Color;

    private sealed class Scope : IDisposable
    {
        public Scope(Window window) => Window = window;

        public Window Window { get; }

        public void Dispose() => Window.Close();
    }
}
