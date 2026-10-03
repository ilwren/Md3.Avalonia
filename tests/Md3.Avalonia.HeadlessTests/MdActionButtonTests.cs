using Avalonia;
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

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdActionButtonTests
{
    [AvaloniaFact]
    public void Toggle_Button_Uses_Selected_Color_And_Shape_Morph()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var button = new MdToggleButton { Content = "Toggle", IsChecked = true };
        using var host = Show(button);

        Assert.Equal(40, button.ContainerHeight);
        Assert.Equal(new CornerRadius(12), button.ContainerCornerRadius);
        Assert.Equal(Color.Parse("#6750A4"), Assert.IsType<SolidColorBrush>(button.Background).Color);
        Assert.Equal(Color.Parse("#FFFFFF"), Assert.IsType<SolidColorBrush>(button.Foreground).Color);
    }

    [AvaloniaFact]
    public void Icon_Button_Uses_Expressive_Size_And_Width_Tokens()
    {
        var button = new MdIconButton
        {
            Icon = new TextBlock { Text = "+" },
            Variant = MdIconButtonVariant.Filled,
            Size = MdButtonSize.Medium,
            WidthMode = MdIconButtonWidth.Wide
        };
        using var host = Show(button);

        Assert.Equal(56, button.ContainerHeight);
        Assert.Equal(72, button.ContainerWidth);
        Assert.Equal(24, button.IconSize);
        Assert.Equal(new CornerRadius(28), button.ContainerCornerRadius);
    }

    [AvaloniaFact]
    public void Toggle_Icon_Button_Preserves_Native_Toggle_State()
    {
        var button = new MdToggleIconButton { Icon = MdSymbols.Favorite, SelectedIcon = MdSymbols.Check };
        using var host = Show(button);

        button.IsChecked = true;
        Assert.True(button.IsChecked);
        Assert.NotNull(button.Template);
    }

    [AvaloniaFact]
    public void Toggle_Icon_Button_Responds_To_Keyboard_Input()
    {
        var button = new MdToggleIconButton { Icon = MdSymbols.Favorite, SelectedIcon = MdSymbols.Check };
        var window = new Window { Width = 240, Height = 120, Content = button };
        window.Show();
        try
        {
            button.Focus();
            window.KeyPress(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
            window.KeyRelease(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
            Assert.True(button.IsChecked);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Fab_And_Extended_Fab_Use_Frozen_Size_Tokens()
    {
        var fab = new MdFloatingActionButton { Icon = "+", Size = MdFabSize.Medium };
        var extended = new MdExtendedFloatingActionButton
        {
            Content = "Create",
            Icon = "+",
            Size = MdExtendedFabSize.Large,
            ColorStyle = MdFabColor.TertiaryContainer
        };
        var row = new StackPanel { Orientation = global::Avalonia.Layout.Orientation.Horizontal };
        row.Children.Add(fab);
        row.Children.Add(extended);
        using var host = Show(row);

        Assert.Equal(80, fab.ContainerSize);
        Assert.Equal(28, fab.IconSize);
        Assert.Equal(96, extended.ContainerHeight);
        Assert.Equal(32, extended.IconSize);
        Assert.Equal(Color.Parse("#FFD8E4"), Assert.IsType<SolidColorBrush>(extended.Background).Color);
    }

    [AvaloniaFact]
    public void Button_Groups_Expose_Standard_And_Connected_Spacing()
    {
        var standard = new MdStandardButtonGroup { Size = MdButtonSize.ExtraSmall };
        var first = new MdToggleButton { Content = "One" };
        var middle = new MdToggleButton { Content = "Two" };
        var last = new MdToggleButton { Content = "Three" };
        var connected = new MdConnectedButtonGroup { Items = { first, middle, last } };
        var column = new StackPanel();
        column.Children.Add(standard);
        column.Children.Add(connected);
        using var host = Show(column);

        Assert.Equal(18, standard.ItemSpacing);
        Assert.Equal(2, connected.ItemSpacing);
        Assert.Equal(new CornerRadius(20, 0, 0, 20), first.ContainerCornerRadius);
        Assert.Equal(new CornerRadius(0), middle.ContainerCornerRadius);
        Assert.Equal(new CornerRadius(0, 20, 20, 0), last.ContainerCornerRadius);
    }

    [AvaloniaFact]
    public void Split_Button_Retains_Independent_Primary_And_Trailing_State()
    {
        var split = new MdSplitButton { Content = "Create", IsDropDownOpen = false };
        using var host = Show(split);

        Assert.NotNull(split.Template);
        var metadata = MdSplitButton.IsDropDownOpenProperty.GetMetadata<MdSplitButton>();
        Assert.Equal(global::Avalonia.Data.BindingMode.TwoWay, metadata.DefaultBindingMode);
    }

    [AvaloniaFact]
    public void Fab_Menu_Trigger_Two_Way_Binds_Open_State()
    {
        var menu = new MdFabMenu { OpenIcon = "+", CloseIcon = "×" };
        menu.Items.Add(new MdExtendedFloatingActionButton { Content = "Photo", Icon = "▣" });
        using var host = Show(menu);
        var trigger = menu.GetVisualDescendants().OfType<MdToggleIconButton>()
            .Single(control => control.Name == "PART_Trigger");

        trigger.IsChecked = true;
        Dispatcher.UIThread.RunJobs();
        Assert.True(menu.IsOpen);
        Assert.True(menu.GetVisualDescendants().OfType<ItemsPresenter>()
            .Single(control => control.Name == "PART_MenuItems").IsVisible);
    }

    [AvaloniaFact]
    public void Fab_Menu_Can_Open_Initially_Without_Overriding_Explicit_Live_State()
    {
        var initiallyOpen = new MdFabMenu
        {
            IsInitiallyOpen = true,
            Items =
            {
                new MdFabMenuItem { Content = "Photo" },
                new MdFabMenuItem { Content = "Document" }
            }
        };
        MdMotion.SetScheme(initiallyOpen, MdMotionScheme.None);
        using (Show(initiallyOpen))
        {
            Dispatcher.UIThread.RunJobs();
            Assert.True(initiallyOpen.IsOpen);
            Assert.True(initiallyOpen.AreItemsVisible);

            initiallyOpen.Dismiss();
            Dispatcher.UIThread.RunJobs();
            Assert.False(initiallyOpen.IsOpen);
            Assert.False(initiallyOpen.AreItemsVisible);
        }

        var explicitlyClosed = new MdFabMenu
        {
            IsInitiallyOpen = true,
            IsOpen = false,
            Items =
            {
                new MdFabMenuItem { Content = "Photo" },
                new MdFabMenuItem { Content = "Document" }
            }
        };
        using (Show(explicitlyClosed))
        {
            Dispatcher.UIThread.RunJobs();
            Assert.False(explicitlyClosed.IsOpen);
            Assert.False(explicitlyClosed.AreItemsVisible);
        }
    }

    [AvaloniaFact]
    public void Fab_Menu_Expansion_Direction_Controls_Actual_Geometry_Independent_Of_Parent_Alignment()
    {
        var menu = new MdFabMenu
        {
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
            Items =
            {
                new MdFabMenuItem { Content = "Photo", Icon = MdSymbols.Photo },
                new MdFabMenuItem { Content = "Document", Icon = MdSymbols.Description }
            }
        };
        MdMotion.SetScheme(menu, MdMotionScheme.None);
        using var host = Show(menu);
        menu.Show();
        Dispatcher.UIThread.RunJobs();

        var presenter = menu.GetVisualDescendants().OfType<ItemsPresenter>()
            .Single(control => control.Name == "PART_MenuItems");
        var trigger = menu.GetVisualDescendants().OfType<MdToggleIconButton>()
            .Single(control => control.Name == "PART_Trigger");
        var upItemsBottom = presenter.TranslatePoint(
            new Point(0, presenter.Bounds.Height), menu)!.Value.Y;
        var upTriggerTop = trigger.TranslatePoint(default, menu)!.Value.Y;

        Assert.Equal(MdFabMenuExpansionDirection.Up, menu.ExpansionDirection);
        Assert.Contains(":expand-up", menu.Classes);
        Assert.True(upItemsBottom <= upTriggerTop,
            $"Up menu bottom {upItemsBottom} must be above trigger top {upTriggerTop}.");
        Assert.Equal(global::Avalonia.Layout.VerticalAlignment.Center, menu.VerticalAlignment);

        menu.ExpansionDirection = MdFabMenuExpansionDirection.Down;
        Dispatcher.UIThread.RunJobs();

        var downItemsTop = presenter.TranslatePoint(default, menu)!.Value.Y;
        var downTriggerBottom = trigger.TranslatePoint(
            new Point(0, trigger.Bounds.Height), menu)!.Value.Y;
        Assert.Contains(":expand-down", menu.Classes);
        Assert.DoesNotContain(":expand-up", menu.Classes);
        Assert.True(downItemsTop >= downTriggerBottom,
            $"Down menu top {downItemsTop} must be below trigger bottom {downTriggerBottom}.");
        Assert.Equal(0, presenter.RenderTransformOrigin.Point.Y);
        Assert.Equal(global::Avalonia.Layout.VerticalAlignment.Center, menu.VerticalAlignment);
    }

    [AvaloniaFact]
    public void Action_Button_Matrix_Can_Render_To_Bitmap()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var content = new StackPanel { Spacing = 22 };
        content.Children.Add(new StackPanel
        {
            Orientation = global::Avalonia.Layout.Orientation.Horizontal,
            Spacing = 12,
            Children =
            {
                new MdToggleButton { Content = "Toggle" },
                new MdToggleButton { Content = "Selected", IsChecked = true },
                new MdSplitButton { Content = "Split" },
                new MdIconButton { Icon = MdSymbols.Add, Variant = MdIconButtonVariant.Filled },
                new MdToggleIconButton { Icon = MdSymbols.Favorite, SelectedIcon = MdSymbols.Check, Variant = MdIconButtonVariant.Tonal, IsChecked = true }
            }
        });
        content.Children.Add(new StackPanel
        {
            Orientation = global::Avalonia.Layout.Orientation.Horizontal,
            Spacing = 18,
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
            Children =
            {
                new MdFloatingActionButton { Icon = MdSymbols.Add },
                new MdFloatingActionButton { Icon = MdSymbols.Edit, Size = MdFabSize.Medium, ColorStyle = MdFabColor.SecondaryContainer },
                new MdExtendedFloatingActionButton { Content = "Create", Icon = MdSymbols.Add },
                new MdExtendedFloatingActionButton { Content = "Compose", Icon = MdSymbols.Edit, Size = MdExtendedFabSize.Medium, ColorStyle = MdFabColor.TertiaryContainer }
            }
        });
        var surface = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#FFFBFE")),
            Padding = new Thickness(32),
            Child = content
        };
        var window = new Window { Width = 1050, Height = 300, Content = surface };
        window.Show();
        try
        {
            var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);
            var path = Path.Combine(AppContext.BaseDirectory, "MdActionButtonsPreview.png");
            using var stream = File.Create(path);
            frame.Save(stream, new PngBitmapEncoderOptions());
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Fab_Menu_Supports_Left_And_Right_Alignment()
    {
        var leftMenu = new MdFabMenu
        {
            Alignment = MdFabAlignment.Left,
            ItemsSource = new[]
            {
                new MdFabMenuItem { Content = "Scanner", Icon = MdSymbols.QrCodeScanner },
                new MdFabMenuItem { Content = "Attachment", Icon = MdSymbols.AttachFile }
            }
        };
        var rightMenu = new MdFabMenu
        {
            Alignment = MdFabAlignment.Right,
            ItemsSource = new[]
            {
                new MdFabMenuItem { Content = "Photo", Icon = MdSymbols.Photo }
            }
        };
        var fabLeft = new MdFloatingActionButton { Alignment = MdFabAlignment.Left };
        var fabRight = new MdFloatingActionButton { Alignment = MdFabAlignment.Right };

        var panel = new StackPanel { Children = { leftMenu, rightMenu, fabLeft, fabRight } };
        using var host = Show(panel);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(MdFabAlignment.Left, leftMenu.Alignment);
        Assert.Equal(MdFabAlignment.Right, rightMenu.Alignment);
        Assert.Equal(MdFabAlignment.Left, fabLeft.Alignment);
        Assert.Equal(MdFabAlignment.Right, fabRight.Alignment);

        leftMenu.Show();
        Assert.True(leftMenu.IsOpen);
        Assert.True(leftMenu.AreItemsVisible);
        leftMenu.Dismiss();
        Assert.False(leftMenu.IsOpen);
    }

    private static IDisposable Show(Control content)
    {
        var window = new Window { Width = 900, Height = 320, Content = content };
        window.Show();
        return new WindowScope(window);
    }

    private sealed class WindowScope(Window window) : IDisposable
    {
        public void Dispose() => window.Close();
    }
}
