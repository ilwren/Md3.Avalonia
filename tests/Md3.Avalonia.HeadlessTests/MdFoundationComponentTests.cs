using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Motion;
using Xunit;
using Layoutable = global::Avalonia.Layout.Layoutable;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdFoundationComponentTests
{
    [AvaloniaFact]
    public void Dark_Buttons_Use_On_Color_Roles_And_Explicit_Presenter_Foreground()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
        var filled = new MdButton { Content = "Filled" };
        var tonal = new MdButton { Content = "Tonal", Variant = MdButtonVariant.Tonal };
        var outlined = new MdButton { Content = "Outlined", Variant = MdButtonVariant.Outlined };
        var row = new StackPanel { Orientation = global::Avalonia.Layout.Orientation.Horizontal, Children = { filled, tonal, outlined } };
        using var host = Show(row);

        Assert.Equal(Colors.White, Assert.IsAssignableFrom<ISolidColorBrush>(filled.Foreground).Color);
        Assert.Equal(Color.Parse("#E8DEF8"), Assert.IsAssignableFrom<ISolidColorBrush>(tonal.Foreground).Color);
        Assert.Equal(Color.Parse("#CAC4D0"), Assert.IsAssignableFrom<ISolidColorBrush>(outlined.Foreground).Color);
        foreach (var button in new[] { filled, tonal, outlined })
        {
            var presenter = button.GetVisualDescendants().OfType<ContentPresenter>()
                .Single(control => control.Name == "PART_ContentPresenter");
            Assert.Same(button.Foreground, presenter.Foreground);
        }

        Application.Current.RequestedThemeVariant = ThemeVariant.Light;
    }

    [AvaloniaFact]
    public void Split_Button_Has_Exact_Two_Dip_Visual_Gap_And_Standalone_Menu()
    {
        var split = new MdSplitButton { Width = 260, Content = "Create" };
        using var host = Show(split);
        var primary = split.GetVisualDescendants().OfType<MdButton>()
            .Single(control => control.Name == "PART_PrimaryButton");
        var trailing = split.GetVisualDescendants().OfType<MdToggleIconButton>()
            .Single(control => control.Name == "PART_TrailingButton");
        var primaryContainer = primary.GetVisualDescendants().OfType<Border>()
            .Single(control => control.Name == "PART_Container");
        var trailingContainer = trailing.GetVisualDescendants().OfType<Border>()
            .Single(control => control.Name == "PART_Container");
        var primaryRight = primaryContainer.TranslatePoint(new Point(primaryContainer.Bounds.Width, 0), split)!.Value.X;
        var trailingLeft = trailingContainer.TranslatePoint(new Point(0, 0), split)!.Value.X;

        Assert.InRange(trailingLeft - primaryRight, 1.9, 2.1);
        Assert.Equal(48, trailing.ContainerWidth);
        Assert.Equal(22, trailing.IconSize);
        Assert.False(trailing.EnableSelectedShapeMorph);
        Assert.Equal(new CornerRadius(4, 20, 20, 4), trailing.ContainerCornerRadius);
        Assert.Single(split.GetVisualDescendants().OfType<MdDropdownMenu>());
    }

    [AvaloniaFact]
    public void Extended_Fab_State_Layer_Covers_Container_And_Content_Is_Centered()
    {
        var fab = new MdExtendedFloatingActionButton
        {
            Width = 220,
            Content = "Compose",
            Icon = MdSymbols.Edit
        };
        using var host = Show(fab);
        var container = fab.GetVisualDescendants().OfType<Border>()
            .Single(control => control.Name == "PART_Container");
        var stateLayer = fab.GetVisualDescendants().OfType<Border>()
            .Single(control => control.Name == "PART_StateLayer");
        var content = fab.GetVisualDescendants().OfType<ContentPresenter>()
            .Last(control => control.Content?.ToString() == "Compose");

        Assert.Equal(container.Bounds.Size, stateLayer.Bounds.Size);
        var center = content.TranslatePoint(new Point(content.Bounds.Width / 2, content.Bounds.Height / 2), container)!.Value.Y;
        Assert.InRange(center, container.Bounds.Height / 2 - 1, container.Bounds.Height / 2 + 1);
    }

    [AvaloniaFact]
    public void Fab_Menu_Uses_Full_Pill_Items_And_Reversible_Transitions()
    {
        var item = new MdExtendedFloatingActionButton { Content = "Photo", Icon = MdSymbols.Photo };
        var menu = new MdFabMenu { Items = { item } };
        using var host = Show(menu);
        menu.IsOpen = true;
        Dispatcher.UIThread.RunJobs();
        var presenter = menu.GetVisualDescendants().OfType<ItemsPresenter>()
            .Single(control => control.Name == "PART_MenuItems");

        Assert.True(menu.AreItemsVisible);
        Assert.Equal(520, presenter.GetBaseValue(Layoutable.MaxHeightProperty));
        Assert.Equal(new CornerRadius(28), item.ContainerCornerRadius);
        Assert.Contains(presenter.Transitions!, transition => transition is TransformOperationsTransition);
        Assert.Contains(presenter.Transitions!, transition =>
            transition is DoubleTransition doubleTransition && doubleTransition.Property == Layoutable.MaxHeightProperty);

        menu.IsOpen = false;
        Assert.True(menu.AreItemsVisible);
        Assert.Equal(0, presenter.GetBaseValue(Layoutable.MaxHeightProperty));
    }

    [AvaloniaFact]
    public void Ripple_And_Motion_System_Are_Available_To_Buttons()
    {
        var button = new MdButton { Content = "Ripple" };
        MdMotion.SetScheme(button, MdMotionScheme.Standard);
        using var host = Show(button);
        var ripple = Assert.Single(button.GetVisualDescendants().OfType<MdRipplePresenter>());

        Assert.False(ripple.IsHitTestVisible);
        Assert.Equal(TimeSpan.FromMilliseconds(450), ripple.Duration);
        Assert.Equal(MdMotionScheme.Standard, MdMotion.GetScheme(ripple));
        Assert.True(MdMotionTokens.ExpressiveFastSpatial.Sample(0.1) > 0);
        Assert.Equal(1, MdMotionTokens.DefaultEffects.DampingRatio);
    }

    [AvaloniaFact]
    public void Symbols_Dictionary_Contains_All_4284_Glyphs_And_Resolves_Names()
    {
        Assert.Equal(4284, MdSymbols.Count);
        Assert.True(MdSymbols.AllNames.Count >= 4284);
        Assert.True(MdSymbols.TryGetByName("settings", out var settingsGlyph));
        Assert.True(MdSymbols.TryGetByName("home", out var homeGlyph));
        Assert.True(MdSymbols.TryGetByName("search", out var searchGlyph));
        Assert.Equal(settingsGlyph, MdSymbols.Settings);
        Assert.Equal(homeGlyph, MdSymbols.Home);
        Assert.Equal(searchGlyph, MdSymbols.Search);
    }

    [AvaloniaFact]
    public void Symbols_Are_Hidden_When_External_Official_Font_Is_Not_Loaded()
    {
        if (MdSymbols.Settings is not null) return;
        var icon = new MdIcon { Glyph = MdSymbols.Settings, Size = 32 };
        using var host = Show(icon);

        Assert.Null(MdSymbols.Settings);
        Assert.Null(icon.Text);
        Assert.Equal(32, icon.FontSize);
        Assert.DoesNotContain("Material Symbols Rounded", icon.FontFamily.Name);
    }

    [AvaloniaFact]
    public void Radio_Button_Preserves_Native_Mutual_Exclusion()
    {
        var first = new MdRadioButton { GroupName = "choice", Content = "First", IsChecked = true };
        var second = new MdRadioButton { GroupName = "choice", Content = "Second" };
        var column = new StackPanel { Children = { first, second } };
        using var host = Show(column);

        second.IsChecked = true;
        Assert.False(first.IsChecked);
        Assert.True(second.IsChecked);
        Assert.Equal(48, second.MinHeight);
    }

    [AvaloniaFact]
    public void Radio_Button_Responds_To_Space_Key()
    {
        var radio = new MdRadioButton { Content = "Keyboard choice" };
        var window = new Window { Width = 300, Height = 120, Content = radio };
        window.Show();
        try
        {
            radio.Focus();
            window.KeyPress(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
            window.KeyRelease(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
            Assert.True(radio.IsChecked);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Badge_And_AppBar_Use_Frozen_Measurements()
    {
        var dot = new MdBadge();
        var label = new MdBadge { Variant = MdBadgeVariant.Label, Content = "24" };
        var appBar = new MdTopAppBar { Title = "Library", Variant = MdTopAppBarVariant.MediumFlexible };
        var column = new StackPanel { Children = { dot, label, appBar } };
        using var host = Show(column);

        Assert.Equal(6, dot.Bounds.Width);
        Assert.Equal(6, dot.Bounds.Height);
        Assert.True(label.Bounds.Width >= 16);
        Assert.Equal(16, label.Bounds.Height);
        Assert.Equal(112, appBar.Height);
    }

    [AvaloniaFact]
    public void New_Component_Matrix_Can_Render_In_Dark_Theme()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
        var stack = new StackPanel { Spacing = 18 };
        stack.Children.Add(new StackPanel
        {
            Orientation = global::Avalonia.Layout.Orientation.Horizontal,
            Spacing = 12,
            Children =
            {
                new MdButton { Content = "Filled" },
                new MdButton { Content = "Tonal", Variant = MdButtonVariant.Tonal },
                new MdButton { Content = "Outlined", Variant = MdButtonVariant.Outlined },
                new MdSplitButton { Content = "Create" },
                new MdRadioButton { Content = "Selected", IsChecked = true },
                new MdBadgedBox { Badge = "3", BadgeVariant = MdBadgeVariant.Label, Content = new MdIconButton { Icon = MdSymbols.Notifications, Variant = MdIconButtonVariant.Tonal } }
            }
        });
        stack.Children.Add(new MdTopAppBar
        {
            Title = "Material components",
            LeadingContent = new MdIconButton { Icon = MdSymbols.Menu },
            TrailingContent = new MdIconButton { Icon = MdSymbols.MoreVert }
        });
        var surface = new Border
        {
            Padding = new Thickness(28),
            Background = new SolidColorBrush(Color.Parse("#141218")),
            Child = stack
        };
        var window = new Window { Width = 980, Height = 260, Content = surface };
        window.Show();
        try
        {
            var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);
            var path = Path.Combine(AppContext.BaseDirectory, "MdFoundationComponentsDarkPreview.png");
            using var stream = File.Create(path);
            frame.Save(stream, new PngBitmapEncoderOptions());
        }
        finally
        {
            window.Close();
            Application.Current.RequestedThemeVariant = ThemeVariant.Light;
        }
    }

    private static IDisposable Show(Control control)
    {
        var window = new Window { Width = 900, Height = 360, Content = control };
        window.Show();
        return new Scope(window);
    }

    private sealed class Scope(Window window) : IDisposable
    {
        public void Dispose() => window.Close();
    }
}
