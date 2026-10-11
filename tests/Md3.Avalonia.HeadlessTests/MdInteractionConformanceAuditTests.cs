using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Interaction conformance audit: the class of defects that compile and render but violate
/// Material 3 state/touch/contrast rules or feel wrong under real input. Everything here is
/// asserted through real simulated input and realized visual trees, not source text.
/// </summary>
public sealed class MdInteractionConformanceAuditTests
{
    [AvaloniaFact]
    public void Tab_Item_State_Layers_Track_Hover_And_Press()
    {
        using var host = ShowTabView();
        var state = FirstItemState(host);
        var item = (Control)state.GetVisualAncestors().OfType<MdTabViewItem>().First();
        var center = ToWindow(host.Window, item);

        host.Window.MouseMove(center, RawInputModifiers.None);
        Assert.Equal(0.08, state.Opacity, precision: 3);

        host.Window.MouseDown(center, MouseButton.Left, RawInputModifiers.None);
        Assert.Equal(0.10, state.Opacity, precision: 3);

        host.Window.MouseUp(center, MouseButton.Left, RawInputModifiers.None);
        Assert.Equal(0.08, state.Opacity, precision: 3);
    }

    [AvaloniaFact]
    public void Tab_Item_Template_Carries_Ripple_And_A_Hit_Test_Transparent_State_Layer()
    {
        using var host = ShowTabView();
        var item = host.TabView.GetVisualDescendants().OfType<MdTabViewItem>().First();
        var state = FirstItemState(host);

        Assert.NotEmpty(item.GetVisualDescendants().OfType<MdRipplePresenter>());
        Assert.False(state.IsHitTestVisible, "the state layer must never swallow content hits");
        Assert.True(state.Bounds.Width >= item.Bounds.Width - 1 && state.Bounds.Height >= item.Bounds.Height - 1,
            "the state layer must cover the whole item");
    }

    [AvaloniaFact]
    public void Disabled_Tab_Item_Fades_To_38_Percent()
    {
        using var host = ShowTabView();
        var item = host.TabView.GetVisualDescendants().OfType<MdTabViewItem>().First();
        item.IsEnabled = false;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(0.38, item.Opacity, precision: 3);
    }

    [AvaloniaFact]
    public void Realized_Tab_Items_Meet_The_Minimum_Touch_Target()
    {
        using var host = ShowTabView();
        foreach (var item in host.TabView.GetVisualDescendants().OfType<MdTabViewItem>())
        {
            Assert.True(item.Bounds.Height >= 48, $"tab item height {item.Bounds.Height} is below the 48 dip touch target");
            Assert.True(item.Bounds.Width >= 90, $"tab item width {item.Bounds.Width} is below the 90 dip primary-tab minimum");
        }
    }

    [AvaloniaFact]
    public void Arrow_Keys_Move_The_Tab_Selection()
    {
        using var host = ShowTabView();
        var items = host.TabView.GetVisualDescendants().OfType<MdTabViewItem>().ToList();
        Assert.True(items[0].Focus(), "the tab item must be focusable for keyboard users");
        Dispatcher.UIThread.RunJobs();

        host.Window.KeyPress(Key.Right, RawInputModifiers.None, PhysicalKey.Right, null);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(1, host.TabView.SelectedIndex);

        host.Window.KeyPress(Key.Left, RawInputModifiers.None, PhysicalKey.Left, null);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(0, host.TabView.SelectedIndex);
    }

    [AvaloniaFact]
    public void Core_Text_Tokens_Meet_Wcag_Aa_Contrast()
    {
        using var host = ShowTabView();
        var surface = ColorOf(host.TabView, "Md.Sys.Color.Surface.Brush");
        var onSurface = ColorOf(host.TabView, "Md.Sys.Color.OnSurface.Brush");
        var onSurfaceVariant = ColorOf(host.TabView, "Md.Sys.Color.OnSurfaceVariant.Brush");

        Assert.True(Contrast(onSurface, surface) >= 4.5, "OnSurface on Surface must meet 4.5:1");
        Assert.True(Contrast(onSurfaceVariant, surface) >= 4.5, "OnSurfaceVariant on Surface must meet 4.5:1 for body-size text");
    }

    private static Scope ShowTabView()
    {
        var tabView = new MdTabView();
        tabView.Items.Add(new MdTabViewItem { Header = "Alpha", Content = new TextBlock { Text = "a" } });
        tabView.Items.Add(new MdTabViewItem { Header = "Beta", Content = new TextBlock { Text = "b" } });
        var window = new Window { Width = 600, Height = 400, Content = tabView };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new Scope(tabView, window);
    }

    private sealed class Scope(MdTabView tabView, Window window) : IDisposable
    {
        public MdTabView TabView { get; } = tabView;
        public Window Window { get; } = window;
        public void Dispose() => Window.Close();
    }

    private static Border FirstItemState(Scope host)
    {
        var item = host.TabView.GetVisualDescendants().OfType<MdTabViewItem>().First();
        return item.GetVisualDescendants().OfType<Border>().Single(border => border.Name == "PART_State");
    }

    private static Point ToWindow(Window window, Visual visual) =>
        visual.TranslatePoint(new Point(visual.Bounds.Width / 2, visual.Bounds.Height / 2), window)!.Value;

    private static Color ColorOf(Control anchor, string key)
    {
        var brush = Assert.IsAssignableFrom<ISolidColorBrush>(
            global::Avalonia.Controls.ResourceNodeExtensions.FindResource(anchor, key));
        return brush.Color;
    }

    private static double Contrast(Color a, Color b)
    {
        var la = Luminance(a);
        var lb = Luminance(b);
        var hi = Math.Max(la, lb);
        var lo = Math.Min(la, lb);
        return (hi + 0.05) / (lo + 0.05);
    }

    private static double Luminance(Color c)
    {
        return 0.2126 * Channel(c.R) + 0.7152 * Channel(c.G) + 0.0722 * Channel(c.B);

        static double Channel(byte value)
        {
            var v = value / 255.0;
            return v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
        }
    }
}
