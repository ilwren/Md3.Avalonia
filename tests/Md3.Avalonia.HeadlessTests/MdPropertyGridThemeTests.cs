using System;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.PropertyGrid.Controls;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Themes;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Covers the Material theme for bodong.Avalonia.PropertyGrid, which ships in the opt-in
/// Md3.Avalonia.PropertyGrid package.
/// </summary>
public class MdPropertyGridThemeTests
{
    [AvaloniaFact]
    public void The_Material_Theme_Templates_The_Grids_Cell_Editors()
    {
        var model = new PropertyGridTestModel();
        var grid = NewGrid(model);
        using var host = Show(grid);

        // The grid builds every cell itself, out of plain Avalonia controls: those controls are
        // TemplatedControls, and this test application has no native control theme at all, so the
        // only reason any of them has a template is the keyed theme the package hands them.
        var field = grid.GetVisualDescendants().OfType<TextBox>().Single(box => box.Name != "SearchTextBox");
        Assert.Same(Keyed(grid, "Md.Comp.PropertyGrid.FieldTheme"), field.Theme);

        var check = grid.GetVisualDescendants().OfType<CheckBox>().Single();
        Assert.Same(Keyed(grid, "Md.Comp.PropertyGrid.CheckTheme"), check.Theme);

        var combo = grid.GetVisualDescendants().OfType<ComboBox>().Single();
        Assert.Same(Keyed(grid, "Md.Comp.PropertyGrid.DropDownTheme"), combo.Theme);

        var picker = grid.GetVisualDescendants().OfType<DatePicker>().Single();
        Assert.Same(Keyed(grid, "Md.Comp.PropertyGrid.DateFieldTheme"), picker.Theme);

        // The enum cell's items are generated, so they take the item theme from the drop-down.
        combo.IsDropDownOpen = true;
        Dispatcher.UIThread.RunJobs();
        var items = combo.GetLogicalDescendants().OfType<ComboBoxItem>().ToList();
        Assert.NotEmpty(items);
        Assert.All(items, item => Assert.Same(Keyed(grid, "Md.Comp.PropertyGrid.DropDownItemTheme"), item.Theme));
        combo.IsDropDownOpen = false;
    }

    [AvaloniaFact]
    public void The_Material_Theme_Reaches_The_Grid_Chrome()
    {
        var grid = NewGrid(new PropertyGridTestModel());
        using var host = Show(grid);

        var search = grid.GetVisualDescendants().OfType<TextBox>().Single(box => box.Name == "SearchTextBox");
        Assert.Same(Keyed(grid, "Md.Comp.PropertyGrid.FieldTheme"), search.Theme);

        // One expander per category, and the grid hands every one of them the same theme.
        var categories = grid.GetVisualDescendants().OfType<Expander>().ToList();
        Assert.Equal(3, categories.Count);
        Assert.All(categories, category =>
            Assert.Same(Keyed(grid, "Md.Comp.PropertyGrid.CategoryTheme"), category.Theme));

        var splitter = grid.GetVisualDescendants().OfType<GridSplitter>().Single();
        Assert.Same(Keyed(grid, "Md.Comp.PropertyGrid.SplitterTheme"), splitter.Theme);

        // The property list scrolls in a native ScrollViewer, whose scrollbars the core package
        // deliberately does not theme - the package's scroller hands them to MdScrollBar instead.
        Assert.NotEmpty(grid.GetVisualDescendants().OfType<MdScrollBar>());

        Assert.DoesNotContain(Application.Current!.Styles, style => style.GetType().Name == "FluentTheme");
    }

    [AvaloniaFact]
    public void Every_Control_Theme_The_Package_Ships_Is_Keyed()
    {
        var theme = new MaterialPropertyGridTheme();

        // The whole isolation claim lives here: an implicit ControlTheme is the one that would leak
        // into the rest of an application, and this package does not define a single one.
        var keys = theme.Resources.Keys.ToList();
        Assert.NotEmpty(keys);
        Assert.All(keys, key => Assert.StartsWith("Md.Comp.PropertyGrid.", Assert.IsType<string>(key)));
        Assert.NotEmpty(theme.Resources.Values.OfType<ControlTheme>());
    }

    [AvaloniaFact]
    public void Registering_The_Grid_Theme_Does_Not_Replace_Native_Control_Themes()
    {
        // Button and ItemsControl are left out: Md3.Avalonia.Dock already installs an implicit
        // Button theme for its own chrome, and the exact contract for that one is pinned by
        // MdDockThemeTests. Nothing else in this test application themes a native control.
        var variant = Application.Current!.ActualThemeVariant;
        foreach (var native in new[]
                 {
                     typeof(TextBox), typeof(ComboBox), typeof(ComboBoxItem), typeof(CheckBox),
                     typeof(RadioButton), typeof(ToggleButton), typeof(Expander), typeof(Slider),
                     typeof(ProgressBar), typeof(NumericUpDown), typeof(DatePicker), typeof(TimePicker),
                     typeof(GridSplitter), typeof(ScrollViewer), typeof(ContextMenu), typeof(MenuItem),
                     typeof(ToolTip)
                 })
        {
            Assert.False(Application.Current.TryGetResource(native, variant, out _), native.Name);
        }

        // And a control that is not inside a grid stays unthemed even in a window that carries the
        // theme, which is what makes the package safe to register application-wide.
        var box = new TextBox { Text = "plain" };
        var combo = new ComboBox();
        var check = new CheckBox();
        var window = new Window
        {
            Width = 240,
            Height = 160,
            Content = new StackPanel { Children = { box, combo, check } }
        };
        window.Styles.Add(new MaterialPropertyGridTheme());
        window.Show();
        Dispatcher.UIThread.RunJobs();

        Assert.Null(box.Theme);
        Assert.Null(combo.Theme);
        Assert.Null(check.Theme);
        window.Close();
    }

    [AvaloniaFact]
    public void The_Date_Cell_Opens_The_Material_Wheel_Flyout_And_Confirms()
    {
        var model = new PropertyGridTestModel();
        var grid = NewGrid(model);
        using var host = Show(grid);

        var picker = grid.GetVisualDescendants().OfType<DatePicker>().Single();
        var popup = picker.GetVisualDescendants().OfType<Popup>().Single(control => control.Name == "PART_Popup");
        var flyout = picker.GetVisualDescendants().OfType<Button>().Single(button => button.Name == "PART_FlyoutButton");
        Assert.Same(Keyed(grid, "Md.Comp.PropertyGrid.PickerFieldTheme"), flyout.Theme);

        flyout.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, flyout));
        Dispatcher.UIThread.RunJobs();
        Assert.True(popup.IsOpen);

        // Once the flyout is open the presenter hangs off the popup host, which is parented to the
        // popup by pointer only - so it is no longer reachable by walking down from the picker, and
        // the popup's own child is the handle to it.
        var presenter = Assert.IsType<DatePickerPresenter>(popup.Child);
        Assert.Same(Keyed(grid, "Md.Comp.PropertyGrid.DatePanelTheme"), presenter.Theme);

        // Applying the template is the point of the test: the presenter resolves its container, its
        // three panels and its accept button with NameScope.Get, so a template missing one of them
        // throws here rather than degrading. Headless only lays the new popup host out on a later
        // frame, so the template is applied directly rather than waiting for it.
        presenter.ApplyTemplate();
        using var frame = host.Window.CaptureRenderedFrame();

        Assert.NotNull(presenter.GetVisualDescendants().OfType<Grid>().SingleOrDefault(panel => panel.Name == "PART_PickerContainer"));
        Assert.Equal(3, presenter.GetVisualDescendants().OfType<DateTimePickerPanel>().Count());

        var accept = presenter.GetVisualDescendants().OfType<Button>().Single(button => button.Name == "PART_AcceptButton");
        accept.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, accept));
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(new DateTime(2026, 10, 12), model.Published.Date);
        Assert.False(popup.IsOpen);
    }

    [AvaloniaFact]
    public void The_Options_Menu_Renders_Its_Items()
    {
        var grid = NewGrid(new PropertyGridTestModel());
        using var host = Show(grid);

        // The grid declares its options menu as a style setter value and opens it from the click
        // handler, so it is not on Button.ContextMenu - and once it is open it hangs off the popup
        // host rather than off the grid, which is why the grid's own accessor is the handle to it.
        var options = grid.GetVisualDescendants().OfType<Button>().Single(button => button.Name == "OptionsButton");
        var menu = grid.DefaultOptionsContextMenu;
        menu.Open(options);
        Dispatcher.UIThread.RunJobs();
        using var frame = host.Window.CaptureRenderedFrame();

        // The menu is in no tree until it is opened, so this is also the proof that it attached:
        // the scoped rule only reaches it through the popup the open creates.
        Assert.Same(Keyed(grid, "Md.Comp.PropertyGrid.MenuTheme"), menu.Theme);

        var items = menu.GetLogicalDescendants().OfType<MenuItem>().ToList();
        Assert.NotEmpty(items);
        Assert.All(items, item => Assert.Same(Keyed(grid, "Md.Comp.PropertyGrid.MenuItemTheme"), item.Theme));
        Assert.Equal(Keyed(grid, "Md.Comp.PropertyGrid.MenuSeparatorTheme"),
                     menu.GetLogicalDescendants().OfType<Separator>().First().Theme);

        menu.Close();
    }

    [AvaloniaFact]
    public void The_Grid_Keeps_Its_Editing_Behaviour()
    {
        var model = new PropertyGridTestModel();
        var grid = NewGrid(model);
        using var host = Show(grid);

        // The themed templates have to keep the wiring the grid's factories attach: the boolean
        // cell writes through IsCheckedChanged, the enum cell through SelectionChanged.
        var check = grid.GetVisualDescendants().OfType<CheckBox>().Single();
        check.IsChecked = true;
        Dispatcher.UIThread.RunJobs();
        Assert.True(model.Enabled);

        var combo = grid.GetVisualDescendants().OfType<ComboBox>().Single();
        combo.SelectedIndex = 2;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(PropertyGridTestAccent.Tertiary, model.Accent);

        // And the grid's own filtering still narrows the rows, which is what the search field is for.
        var field = grid.GetVisualDescendants().OfType<TextBox>().Single(box => box.Name != "SearchTextBox");
        var search = grid.GetVisualDescendants().OfType<TextBox>().Single(box => box.Name == "SearchTextBox");
        search.Text = "title";
        Dispatcher.UIThread.RunJobs();
        Assert.True(field.IsEffectivelyVisible);
        Assert.False(check.IsEffectivelyVisible);
    }

    [AvaloniaFact]
    public void The_Package_Exposes_One_Public_Type()
    {
        // Avalonia's XAML compiler adds two public helper types under CompiledAvaloniaXaml to every
        // assembly that embeds XAML, so the surface is counted over the package's own namespace.
        var exported = typeof(MaterialPropertyGridTheme).Assembly.GetTypes()
            .Where(type => type.IsPublic && type.Namespace != "CompiledAvaloniaXaml")
            .ToArray();

        Assert.Equal([typeof(MaterialPropertyGridTheme)], exported);
        Assert.IsAssignableFrom<Styles>(new MaterialPropertyGridTheme());
    }

    private static PropertyGrid NewGrid(PropertyGridTestModel model) => new()
    {
        Width = 460,
        Height = 520,
        DataContext = model,
        IsCategoryVisible = true,
        AllCategoriesExpanded = true
    };

    // The theme is handed to the window that owns the grid, the way an application that does not
    // want it application-wide would register it; the gallery page registers it the same way.
    private static Scope Show(Control content)
    {
        var window = new Window { Width = 560, Height = 640, Content = content };
        window.Styles.Add(new MaterialPropertyGridTheme());
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new Scope(window);
    }

    private static ControlTheme Keyed(StyledElement scope, string key)
    {
        Assert.True(scope.TryFindResource(key, scope.ActualThemeVariant, out var value), key);
        return Assert.IsType<ControlTheme>(value);
    }

    private sealed class Scope : IDisposable
    {
        public Scope(Window window) => Window = window;

        public Window Window { get; }

        public void Dispose() => Window.Close();
    }
}

/// <summary>One property per cell-edit factory the theme has to cover.</summary>
internal sealed class PropertyGridTestModel
{
    [Category("Text")]
    public string Title { get; set; } = "Material";

    [Category("Choices")]
    public bool Enabled { get; set; }

    [Category("Choices")]
    public PropertyGridTestAccent Accent { get; set; } = PropertyGridTestAccent.Primary;

    [Category("When")]
    public DateTime Published { get; set; } = new(2026, 10, 12);
}

internal enum PropertyGridTestAccent
{
    Primary,
    Secondary,
    Tertiary
}
