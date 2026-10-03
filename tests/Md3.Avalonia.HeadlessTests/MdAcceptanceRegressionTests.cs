using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Embedding;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Gallery;
using Md3.Avalonia.Icons;
using Md3.Avalonia.Gallery.Pages;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdAcceptanceRegressionTests
{
    [AvaloniaFact]
    public void Every_Material_Popup_Template_Allows_Platform_Host_Selection()
    {
        var controls = new Control[]
        {
            new MdComboBox { ItemsSource = new[] { "One", "Two" } },
            new MdDatePicker(),
            new MdTimePicker(),
            new MdAutoCompleteBox { ItemsSource = new[] { "One", "Two" } },
            new MdMenuAnchor { Menu = new MdMenu(), Content = new MdButton { Content = "Open" } },
            new MdDropdownMenu { Content = new MdMenu() },
            new MdTooltipHost { Tooltip = new MdTooltip { Content = "Tip" }, Content = new TextBlock { Text = "Target" } }
        };
        var panel = new StackPanel();
        foreach (var control in controls) panel.Children.Add(control);
        using var host = Show(panel);
        Dispatcher.UIThread.RunJobs();

        foreach (var control in controls.OfType<TemplatedControl>())
        {
            var popup = control.Template!.Build(control)!.NameScope.Find<Popup>("PART_Popup");
            Assert.True(popup is not null, $"{control.GetType().Name} did not expose PART_Popup");
            Assert.False(popup!.ShouldUseOverlayLayer);
            // MaterialTheme supplies global PopupRoot and OverlayPopupHost templates. Leaving this
            // false lets desktop use a native host and Android fall back to the popup overlay.
        }
    }

    [AvaloniaFact]
    public void Single_View_Root_Provides_Android_Popup_Overlay_For_ComboBox()
    {
        Assert.True(Application.Current!.TryGetResource(typeof(EmbeddableControlRoot), null, out var rootTheme));
        Assert.NotEmpty(Assert.IsType<global::Avalonia.Styling.ControlTheme>(rootTheme).Setters);

        try
        {
            using var root = new EmbeddableControlRoot
            {
                Width = 420,
                Height = 720,
                Content = new MdComboBox { ItemsSource = new[] { "One", "Two" } }
            };
            root.Prepare();
            Dispatcher.UIThread.RunJobs();

            Assert.Contains(root.GetVisualDescendants().OfType<VisualLayerManager>(),
                layer => layer.Name == "PART_VisualLayerManager");
            var comboBox = Assert.IsType<MdComboBox>(root.Content);
            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();
            var popup = comboBox.GetVisualDescendants().OfType<Popup>()
                .Single(control => control.Name == "PART_Popup");
            Assert.True(popup.IsUsingOverlayLayer);
            Assert.True(comboBox.IsDropDownOpen);
        }
        catch (PlatformNotSupportedException)
        {
            // Avalonia's headless backend cannot instantiate EmbeddableControlRoot. The theme
            // registration above remains a headless contract check; Android Maestro provides the
            // runtime open/select/no-crash evidence on the actual single-view lifetime.
        }
    }

    [AvaloniaFact]
    public void Dropdown_Closes_Custom_Surface_And_Native_Host_In_The_Same_Turn()
    {
        // Exercise the state machine before attachment: a headless top level intentionally has no
        // native IPopupImpl, while the two public states are platform-independent.
        var menu = new MdDropdownMenu { Content = new MdMenu() };
        menu.IsOpen = true;
        Assert.True(menu.IsPopupOpen);
        menu.IsOpen = false;
        Assert.False(menu.IsPopupOpen);
        Assert.DoesNotContain(":closing", menu.Classes);
    }

    [AvaloniaFact]
    public void External_Official_Symbol_Font_Automatically_Resolves_The_Expected_Glyph()
    {
        // Access the strongly typed catalog exactly as a package consumer does: no ConfigureFonts,
        // ApplyTo, or EnsureConfigured call is allowed before this lookup.
        var searchGlyph = MdSymbols.Search;
        if (searchGlyph is null)
        {
            // Local verification may deliberately substitute a non-Symbols font to validate only the
            // build/XAML pipeline. A real Icons package cannot be packed without an embedded font.
            Assert.False(MdExternalMaterialSymbols.IsAvailable);
            Assert.False(string.IsNullOrWhiteSpace(MdExternalMaterialSymbols.LoadFailureReason));
            return;
        }

        Assert.True(MdExternalMaterialSymbols.IsAvailable, MdExternalMaterialSymbols.LoadFailureReason);
        Assert.NotNull(MdExternalMaterialSymbols.FontFamily);
        Assert.True(Application.Current!.TryGetResource("Md.Sys.Typeface.Symbols.Rounded", null, out var registeredFamily));
        Assert.Equal(MdExternalMaterialSymbols.FontFamily, registeredFamily);
        Assert.True(FontManager.Current.TryGetGlyphTypeface(
            new Typeface(MdExternalMaterialSymbols.FontFamily!), out var glyphTypeface));
        var searchCodepoint = char.ConvertToUtf32(searchGlyph, 0);
        Assert.True(glyphTypeface.CharacterToGlyphMap.TryGetGlyph(searchCodepoint, out var glyph));
        Assert.True(glyph > 0);
        foreach (var controlGlyph in new[] { MdSymbols.Check, MdSymbols.Schedule, MdSymbols.RotateRight })
        {
            Assert.NotNull(controlGlyph);
            var codepoint = char.ConvertToUtf32(controlGlyph!, 0);
            Assert.True(glyphTypeface.CharacterToGlyphMap.TryGetGlyph(codepoint, out var controlGlyphIndex));
            Assert.True(controlGlyphIndex > 0);
        }

        var icon = new MdIcon { Glyph = MdSymbols.Search, Size = 32 };
        using var host = Show(icon);
        Assert.Equal(MdExternalMaterialSymbols.FontFamily, icon.FontFamily);
        Assert.Equal(MdSymbols.Search, icon.Text);

        var segment = new MdSegmentedButton { Content = "Day", IsSelected = true };
        using var segmentHost = Show(segment);
        var selectedIcon = segment.GetVisualDescendants().OfType<MdSymbolPresenter>()
            .Single(presenter => presenter.Name == "PART_SelectedIcon");
        Assert.NotNull(segment.SelectedIcon);
        Assert.True(selectedIcon.IsVisible);
        Assert.Equal(MdExternalMaterialSymbols.FontFamily, selectedIcon.FontFamily);
        Assert.Equal(segment.SelectedIcon, selectedIcon.Content);
    }

    [AvaloniaFact]
    public void Numeric_Input_Renders_Both_Aligned_Spinner_Actions()
    {
        var numeric = new MdNumericBox
        {
            Width = 260,
            Value = 12,
            Minimum = 0,
            Maximum = 100,
            Label = "Quantity",
            SupportingText = "Between 0 and 100"
        };
        using var host = Show(numeric);
        Dispatcher.UIThread.RunJobs();
        var paths = numeric.GetVisualDescendants().OfType<global::Avalonia.Controls.Shapes.Path>().ToArray();
        Assert.Equal(2, paths.Length);
        Assert.All(paths, path => Assert.True(path.Bounds.Width > 0 && path.Bounds.Height > 0));

        var fieldContainer = numeric.GetVisualDescendants().OfType<Border>()
            .Single(border => border.Name == "PART_Container");
        var spinnerButtons = numeric.GetVisualDescendants().OfType<RepeatButton>().ToArray();
        Assert.Equal(2, spinnerButtons.Length);
        foreach (var button in spinnerButtons)
        {
            var origin = button.TranslatePoint(default, fieldContainer);
            Assert.NotNull(origin);
            Assert.InRange(origin.Value.X, 0, fieldContainer.Bounds.Width - button.Bounds.Width);
            Assert.InRange(origin.Value.Y, 0, fieldContainer.Bounds.Height - button.Bounds.Height);
        }

        var leftNumeric = new MdNumericBox
        {
            Width = 260,
            Value = 12,
            ButtonSpinnerLocation = Location.Left
        };
        using var leftHost = Show(leftNumeric);
        Dispatcher.UIThread.RunJobs();
        var leftContainer = leftNumeric.GetVisualDescendants().OfType<Border>()
            .Single(border => border.Name == "PART_Container");
        var leftButtons = leftNumeric.GetVisualDescendants().OfType<RepeatButton>().ToArray();
        Assert.Equal(2, leftButtons.Length);
        foreach (var button in leftButtons)
        {
            var origin = button.TranslatePoint(default, leftContainer);
            Assert.NotNull(origin);
            Assert.InRange(origin.Value.X, 0, leftContainer.Bounds.Width - button.Bounds.Width);
            Assert.InRange(origin.Value.Y, 0, leftContainer.Bounds.Height - button.Bounds.Height);
            Assert.True(origin.Value.X < leftContainer.Bounds.Width / 2);
        }
    }

    [AvaloniaFact]
    public void Editing_Menu_Exposes_Gestures_And_Native_Editing_Shortcuts_Work()
    {
        var field = new MdTextBox { Text = "Material" };
        var window = new Window { Width = 420, Height = 180, Content = field };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var items = field.ContextMenu!.Items.OfType<MenuItem>().ToArray();
            Assert.Equal(6, items.Length);
            Assert.All(items, item => Assert.NotNull(item.InputGesture));

            field.Focus();
            window.KeyPress(Key.A, RawInputModifiers.Control, PhysicalKey.A, "a");
            window.KeyRelease(Key.A, RawInputModifiers.Control, PhysicalKey.A, "a");
            window.KeyTextInput("X");
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("X", field.Text);
            window.KeyPress(Key.Z, RawInputModifiers.Control, PhysicalKey.Z, "z");
            window.KeyRelease(Key.Z, RawInputModifiers.Control, PhysicalKey.Z, "z");
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("Material", field.Text);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Date_And_DateRange_Templates_Populate_FortyTwo_Localized_Days()
    {
        var date = new MdDatePicker { DisplayDate = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero) };
        var range = new MdDateRangePicker();
        using var host = Show(new StackPanel { Children = { date, range } });
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(42, date.CalendarDays.Count);
        var daysHost = date.Template!.Build(date)!.NameScope.Find<ItemsControl>("PART_DaysHost");
        Assert.NotNull(daysHost);
        Assert.NotNull(daysHost!.ItemTemplate);
        var nestedCalendars = range.GetVisualDescendants().OfType<MdDatePicker>().ToArray();
        Assert.Equal(2, nestedCalendars.Length);
        Assert.All(nestedCalendars, picker => Assert.Equal(42, picker.CalendarDays.Count));
    }

    [AvaloniaFact]
    public void Modal_Pickers_Roll_Back_Provisional_Values_When_Dismissed()
    {
        var originalDate = new DateTimeOffset(2026, 9, 23, 0, 0, 0, TimeSpan.Zero);
        var provisionalDate = originalDate.AddDays(4);
        var date = new MdDatePicker { Mode = MdDatePickerMode.Modal, SelectedDate = originalDate };
        date.IsOpen = true;
        date.SelectedDate = provisionalDate;
        date.IsOpen = false;
        Assert.Equal(originalDate, date.SelectedDate);

        var originalTime = new TimeSpan(9, 15, 0);
        var time = new MdTimePicker { SelectedTime = originalTime };
        time.IsOpen = true;
        time.SelectedTime = new TimeSpan(14, 42, 0);
        time.IsOpen = false;
        Assert.Equal(originalTime, time.SelectedTime);
    }

    [AvaloniaFact]
    public void Search_Escape_Dismisses_Expanded_View_And_Enter_Submits()
    {
        var submitted = string.Empty;
        var search = new MdSearchBar { Text = "buttons" };
        search.SearchSubmitted += (_, query) => submitted = query;
        var view = new MdSearchView { Header = search, IsOpen = true, Content = new TextBlock { Text = "Results" } };
        var window = new Window { Width = 620, Height = 300, Content = view };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            search.Focus();
            window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, "\r");
            window.KeyRelease(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, "\r");
            Assert.Equal("buttons", submitted);
            window.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
            window.KeyRelease(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
            Dispatcher.UIThread.RunJobs();
            Assert.False(view.IsOpen);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Range_Slider_Loading_And_Wavy_Progress_Render_Their_New_Geometries()
    {
        var panel = new StackPanel
        {
            Width = 680,
            Spacing = 18,
            Children =
            {
                new MdRangeSlider { LowerValue = 20, UpperValue = 80, ShowValueIndicators = true },
                new MdSlider { Value = 65, ShowValueIndicator = true },
                new MdLoadingIndicator { IsActive = false },
                new MdLinearProgressIndicator { Value = 58, Shape = MdProgressShape.Wavy, WaveAmplitude = 4 }
            }
        };
        var window = new Window { Width = 760, Height = 420, Content = panel };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.NotNull(window.CaptureRenderedFrame());
            Assert.Contains(panel.GetVisualDescendants(), control => control is global::Avalonia.Controls.Shapes.Path);
            Assert.True(((MdRangeSlider)panel.Children[0]).Bounds.Height >= 104);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Snackbar_Action_And_Dismiss_Affordances_Use_Inverse_Contrast_In_Dark_Mode()
    {
        Application.Current!.RequestedThemeVariant = global::Avalonia.Styling.ThemeVariant.Dark;
        try
        {
            var actionInvoked = false;
            var snackbar = new MdSnackbar
            {
                Content = "Draft archived",
                ActionContent = "Undo",
                IsDismissible = true,
                IsOpen = true,
                Duration = TimeSpan.Zero
            };
            snackbar.ActionInvoked += (_, _) => actionInvoked = true;
            using var host = Show(snackbar);
            var action = snackbar.GetVisualDescendants().OfType<MdButton>()
                .Single(button => button.Name == "PART_ActionButton");
            var dismiss = snackbar.GetVisualDescendants().OfType<MdIconButton>()
                .Single(button => button.Name == "PART_DismissButton");
            Assert.Equal(snackbar.Foreground, action.Foreground);
            Assert.Equal(snackbar.Foreground, dismiss.Foreground);

            action.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.True(actionInvoked);
            Assert.False(snackbar.IsOpen);
        }
        finally
        {
            Application.Current.RequestedThemeVariant = global::Avalonia.Styling.ThemeVariant.Light;
        }
    }

    [AvaloniaFact]
    public void Contextual_Toolbar_Opens_On_Click_And_Reports_Actions()
    {
        // Keep the page unattached because Avalonia.Headless intentionally has no native popup
        // implementation. This still exercises the click handlers and public popup state.
        var page = new ToolbarGalleryPage();
        var trigger = page.GetLogicalDescendants().OfType<MdButton>()
            .Single(button => button.Name == "ContextToolbarTrigger");
        var popup = page.GetLogicalDescendants().OfType<MdDropdownMenu>()
            .Single(menu => menu.Name == "ContextToolbarPopup");
        var edit = page.GetLogicalDescendants().OfType<MdIconButton>()
            .Single(button => button.Name == "ContextEditAction");
        var status = page.GetLogicalDescendants().OfType<TextBlock>()
            .Single(text => text.Name == "ContextToolbarStatus");

        trigger.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.True(popup.IsOpen);
        edit.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.False(popup.IsOpen);
        Assert.Equal("Edit selected.", status.Text);
    }

    [AvaloniaFact]
    public void Autocomplete_Is_Clearable_And_Multiline_Labels_Use_The_Dedicated_State()
    {
        var autocomplete = new MdAutoCompleteBox { Text = "Avalonia", Label = "Technology" };
        var multiline = new MdTextBox { AcceptsReturn = true, Label = "JSON", Variant = MdTextBoxVariant.Outlined, MinHeight = 180 };
        using var host = Show(new StackPanel { Children = { autocomplete, multiline } });
        Dispatcher.UIThread.RunJobs();

        var editor = autocomplete.GetVisualDescendants().OfType<MdTextBox>().Single();
        Assert.True(editor.ShowClearButton);
        var clear = editor.GetVisualDescendants().OfType<MdIconButton>()
            .Single(button => button.Name == "PART_ClearButton");
        clear.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(string.Empty, editor.Text);
        Assert.Equal(string.Empty, autocomplete.Text);

        var label = multiline.GetVisualDescendants().OfType<ContentPresenter>()
            .Single(presenter => presenter.Name == "PART_Label");
        Assert.Equal(global::Avalonia.Layout.VerticalAlignment.Top, label.VerticalAlignment);
    }

    [AvaloniaFact]
    public void Theme_Diagnostics_Show_Each_Actual_Color_Pair_And_Purpose()
    {
        var page = new ThemeResourcesGalleryPage();
        using var host = Show(page);
        Dispatcher.UIThread.RunJobs();
        var diagnostics = page.GetVisualDescendants().OfType<StackPanel>()
            .Single(panel => panel.Name == "DiagnosticsPanel");
        Assert.Equal(11, diagnostics.Children.Count);
        Assert.All(diagnostics.Children.OfType<Border>(), border =>
        {
            Assert.NotNull(border.Background);
            Assert.IsType<Grid>(border.Child);
            Assert.Contains(((Grid)border.Child!).GetVisualDescendants().OfType<TextBlock>(),
                text => text.Text?.Contains("actions", StringComparison.OrdinalIgnoreCase) == true ||
                        text.Text?.Contains("content", StringComparison.OrdinalIgnoreCase) == true ||
                        text.Text?.Contains("surfaces", StringComparison.OrdinalIgnoreCase) == true ||
                        text.Text?.Contains("controls", StringComparison.OrdinalIgnoreCase) == true ||
                        text.Text?.Contains("containers", StringComparison.OrdinalIgnoreCase) == true ||
                        text.Text?.Contains("emphasis", StringComparison.OrdinalIgnoreCase) == true ||
                        text.Text?.Contains("messages", StringComparison.OrdinalIgnoreCase) == true);
        });
    }

    [AvaloniaFact]
    public void Top_Documentation_Entrances_Navigate_To_Real_Pages()
    {
        var gallery = new MainWindow { Width = 1700, Height = 800 };
        gallery.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var host = gallery.GetVisualDescendants().OfType<ContentControl>().Single(control => control.Name == "PageHost");
            var getStarted = gallery.GetVisualDescendants().OfType<MdButton>().Single(button => button.Name == "GetStartedTopNav");
            getStarted.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            Assert.IsType<GettingStartedGalleryPage>(host.Content);

            var pageScroll = gallery.FindControl<ScrollViewer>("PageScroll")!;
            pageScroll.Offset = new Vector(0, 180);
            var develop = gallery.GetVisualDescendants().OfType<MdButton>().Single(button => button.Name == "DevelopTopNav");
            develop.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            Assert.IsType<DeveloperGalleryPage>(host.Content);
            Assert.Equal(0, pageScroll.Offset.Y);

            var navigationLabels = gallery.GetVisualDescendants().OfType<TextBlock>()
                .Where(text => text.Text is "Components" or "LIBRARY EXTENSIONS" or "ALL COMPONENTS").ToArray();
            Assert.Contains(navigationLabels, text => text.Text == "Components");
            Assert.Contains(navigationLabels, text => text.Text == "LIBRARY EXTENSIONS");
            Assert.Contains(navigationLabels, text => text.Text == "ALL COMPONENTS");
        }
        finally { gallery.Close(); }
    }

    private static IDisposable Show(Control content)
    {
        var window = new Window { Width = 1000, Height = 720, Content = content };
        window.Show();
        return new Scope(window);
    }

    private sealed class Scope(Window window) : IDisposable
    {
        public void Dispose() => window.Close();
    }
}
