using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Gallery.Pages;
using Md3.Avalonia.Themes;
using Tabalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Covers the Material theme for Tabalonia, which ships in the opt-in Md3.Avalonia.Tabalonia package.
/// </summary>
/// <remarks>
/// Unlike the Dock theme this one brings no implicit native control theme at all - Tabalonia's own
/// templates already give its add and close buttons a template - so it is registered application-wide
/// in <see cref="TestApplication"/> without changing what a plain <c>Button</c> looks like, and the
/// isolation test below can check that directly.
/// </remarks>
public class MdTabaloniaThemeTests
{
    [AvaloniaFact]
    public void The_Material_Theme_Loads_Tabalonias_Theme()
    {
        // The package does not fork Tabalonia's templates: it includes the pinned package's Custom
        // theme dictionaries by avares URI and adds its own accents, so the implicit ControlTheme
        // for TabsControl and the DragTabItem styles have to come from this theme alone, with neither
        // of Tabalonia's own themes in the application.
        var variant = Application.Current!.ActualThemeVariant;
        Assert.Contains(Application.Current.Styles, style => style is MaterialTabaloniaTheme);
        Assert.DoesNotContain(Application.Current.Styles, style => style.GetType().Name is "CustomTheme" or "FluentTheme");

        Assert.True(Application.Current.TryGetResource(typeof(TabsControl), variant, out var tabsTheme));
        Assert.Equal(typeof(TabsControl), Assert.IsType<ControlTheme>(tabsTheme).TargetType);

        // Upstream keeps the window-drag thumb in a keyed theme inside the same dictionary.
        Assert.True(Application.Current.TryGetResource("WindowThumbStyle", variant, out var thumbTheme));
        Assert.Equal(typeof(Thumb), Assert.IsType<ControlTheme>(thumbTheme).TargetType);
    }

    [AvaloniaFact]
    public void The_Material_Theme_Retokenizes_Tabalonias_Brushes()
    {
        // Tabalonia's Custom theme ships its own #D0D0D0/#202020 light/dark brush dictionary. Every
        // value here has to be a Material role instead, including the six keys upstream inherits from
        // Avalonia's FluentTheme (which a standalone Material theme cannot rely on).
        Assert.Equal(ColorOf(Token("Md.Sys.Color.Surface.Brush")), ColorOf(Brush("TabControlWindowActiveBackgroundBrush")));
        Assert.Equal(ColorOf(Token("Md.Sys.Color.Primary.Brush")), ColorOf(Brush("SelectedTabItemBackgroundBrush")));
        Assert.Equal(ColorOf(Token("Md.Sys.Color.OutlineVariant.Brush")), ColorOf(Brush("TabItemRightSeparatorBackgroundBrush")));
        Assert.Equal(ColorOf(Token("Md.Sys.Color.OnSurfaceVariant.Brush")), ColorOf(Brush("TabItemHeaderForegroundUnselected")));
        Assert.Equal(ColorOf(Token("Md.Sys.Color.OnSurface.Brush")), ColorOf(Brush("TabItemHeaderForegroundUnselectedPointerOver")));
        Assert.Equal(ColorOf(Token("Md.Sys.Color.OnPrimary.Brush")), ColorOf(Brush("TabItemHeaderForegroundSelected")));
        Assert.Equal(ColorOf(Token("Md.Sys.Color.OnPrimary.Brush")), ColorOf(Brush("TabItemHeaderForegroundSelectedPointerOver")));
        Assert.Equal(ColorOf(Token("Md.Sys.Color.OnSurfaceVariant.Brush")), ColorOf(Brush("CloseItemButtonForeground")));
        Assert.Equal(ColorOf(Token("Md.Sys.Color.Primary.Brush")), ColorOf(Brush("AddItemCommandButtonForegroundPointerOverBrush")));

        // The idle tab is transparent, so the strip's surface shows through; hover is a Material state
        // layer rather than a second accent colour.
        Assert.Equal(Colors.Transparent, ColorOf(Brush("TabItemBackgroundBrush")));

        var hover = Assert.IsAssignableFrom<ISolidColorBrush>(Brush("TabItemHeaderBackgroundUnselectedPointerOver"));
        Assert.Equal(ColorOf(Token("Md.Sys.Color.OnSurface.Brush")), hover.Color);
        Assert.Equal(0.08, hover.Opacity, 3);

        // The two keys that are not brushes: upstream reads them for the label's weight and padding,
        // and these are the values Avalonia's FluentTheme happens to declare for them.
        Assert.Equal(FontWeight.Medium, Assert.IsType<FontWeight>(NonNull("TabItemHeaderThemeFontWeight")));
        Assert.Equal(new Thickness(12, 0, 12, 0), Assert.IsType<Thickness>(NonNull("TabItemHeaderMargin")));
    }

    [AvaloniaFact]
    public void The_Tab_Strip_Renders_Material_Tab_Containers()
    {
        var tabs = Tabs("Overview", "Tokens");
        var strip = new TabsControl { Width = 420, Height = 200, ItemsSource = tabs, TabItemWidth = 160, ItemTemplate = TabLabel };
        using var scope = Show(strip);
        Layout(scope.Window, strip);

        var containers = new[] { Container(strip, 0), Container(strip, 1) };

        // These values come from this package's accents. Tabalonia's own Custom theme would leave the
        // containers on whatever TabsControl has (no Material role and no state layer) instead.
        Assert.All(containers, item =>
        {
            Assert.Equal(32d, item.Height);
            Assert.Equal(new CornerRadius(4, 4, 0, 0), item.CornerRadius);
            Assert.Equal(ColorOf(Token("Md.Sys.Color.OnSurfaceVariant.Brush")), ColorOf(item.Foreground!));
            Assert.Equal(Colors.Transparent, ColorOf(item.Background!));
        });

        // Selecting the second tab turns it into a Primary container and swaps the label ink to
        // OnPrimary, which the template applies as an inherited TextBlock.Foreground on the container.
        strip.SelectedIndex = 1;
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(ColorOf(Token("Md.Sys.Color.Primary.Brush")), ColorOf(containers[1].Background!));
        Assert.Equal(ColorOf(Token("Md.Sys.Color.OnPrimary.Brush")), ColorOf(Label(containers[1], "Tokens").Foreground!));
        Assert.Equal(ColorOf(Token("Md.Sys.Color.OnSurfaceVariant.Brush")), ColorOf(Label(containers[0], "Overview").Foreground!));
    }

    [AvaloniaFact]
    public void The_Tab_Strips_Add_And_Close_Buttons_Wear_Material_Ink()
    {
        var tabs = Tabs("Overview", "Tokens");
        var strip = new TabsControl
        {
            Width = 420,
            Height = 200,
            ItemsSource = tabs,
            TabItemWidth = 160,
            ItemTemplate = TabLabel,
            NewItemFactory = () => new DemoTab($"Tab {tabs.Count + 1}", DemoTab.DefaultBody)
        };
        using var scope = Show(strip);
        Layout(scope.Window, strip);

        var containers = new[] { Container(strip, 0), Container(strip, 1) };

        // The add button's idle ink is the key this package adds on top of upstream's Custom theme,
        // which never sets one.
        var addButton = strip.GetVisualDescendants().OfType<Button>()
            .Single(button => button.Classes.Contains("AddItemCommandButtonStyle"));
        Assert.Equal(ColorOf(Token("Md.Sys.Color.OnSurfaceVariant.Brush")), ColorOf(addButton.Foreground!));

        // The close glyph follows the container it sits on: idle ink while unselected, and the same
        // OnPrimary the label uses once the container is a filled Primary.
        Assert.Equal(ColorOf(Token("Md.Sys.Color.OnPrimary.Brush")), ColorOf(CloseButton(containers[0]).Foreground!));
        Assert.Equal(ColorOf(Token("Md.Sys.Color.OnSurfaceVariant.Brush")), ColorOf(CloseButton(containers[1]).Foreground!));

        // The theme changes ink only: the strip still adds and closes tabs exactly as upstream does.
        strip.AddItemCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(3, tabs.Count);
        Assert.Equal("Tab 3", tabs[^1].Header);

        strip.CloseItemCommand.Execute(Container(strip, 0));
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(new[] { "Tokens", "Tab 3" }, tabs.Select(tab => tab.Header).ToArray());
    }

    [AvaloniaFact]
    public void Registering_The_Tab_Theme_Does_Not_Replace_Native_Control_Themes()
    {
        // Every selector this theme brings targets TabsControl, DragTabItem or a Button carrying one of
        // Tabalonia's classes, so the native controls keep whatever the host application gave them.
        // (Button is deliberately absent from the list below: the Dock package has to carry the
        // implicit Button theme Dock's chrome derives from, and MdDockThemeTests owns that contract.)
        Assert.DoesNotContain(Application.Current!.Styles,
            style => style.GetType().Namespace?.StartsWith("Tabalonia") == true);

        var variant = Application.Current.ActualThemeVariant;
        foreach (var native in new[] { typeof(TextBox), typeof(ScrollViewer), typeof(ListBox), typeof(ToggleButton) })
        {
            Assert.False(Application.Current.TryGetResource(native, variant, out _), native.Name);
        }

        var plainButton = new Button { Content = "plain" };
        using var scope = Show(plainButton);
        Assert.False(plainButton.Classes.Contains("CloseItemButtonStyle"));
        Assert.False(plainButton.Classes.Contains("AddItemCommandButtonStyle"));
    }

    /// <summary>The strip mutates ItemsSource, so the collection has to implement IList.</summary>
    private static ObservableCollection<DemoTab> Tabs(params string[] headers) => [.. DemoTab.Create(headers)];

    /// <summary>Tabalonia renders the header through the item template, exactly like a TabControl.</summary>
    private static IDataTemplate TabLabel { get; } = new FuncDataTemplate<DemoTab>((tab, _) => new TextBlock { Text = tab?.Header });

    private static DragTabItem Container(TabsControl strip, int index) =>
        Assert.IsType<DragTabItem>(strip.ContainerFromIndex(index));

    private static Button CloseButton(DragTabItem container) => container.GetVisualDescendants().OfType<Button>()
        .Single(button => button.Classes.Contains("CloseItemButtonStyle"));

    private static TextBlock Label(DragTabItem container, string header) => container.GetVisualDescendants().OfType<TextBlock>()
        .First(text => text.Text == header);

    private static Scope Show(Control content)
    {
        var window = new Window { Width = 520, Height = 300, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new Scope(window);
    }

    private static void Layout(Window window, Control control)
    {
        control.ApplyTemplate();
        window.UpdateLayout();
        control.UpdateLayout();
    }

    private static object NonNull(string key)
    {
        Assert.True(Application.Current!.TryGetResource(key, Application.Current.ActualThemeVariant, out var value), key);
        return value!;
    }

    private static IBrush Brush(string key) => Assert.IsAssignableFrom<IBrush>(NonNull(key));

    // Md.Sys.* tokens live in the core theme's dictionaries and are read through the same resource
    // host, so they need no separate lookup.
    private static IBrush Token(string key) => Brush(key);

    private static Color ColorOf(IBrush brush) => Assert.IsType<SolidColorBrush>(brush).Color;

    private sealed class Scope(Window window) : IDisposable
    {
        public Window Window { get; } = window;

        public void Dispose() => Window.Close();
    }
}
