using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Gallery;
using Md3.Avalonia.Gallery.Components;
using Md3.Avalonia.Localization;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdCurrentRegressionTests
{
    [AvaloniaFact]
    public void Time_Dial_Uses_A_Real_Clock_Face_With_One_Minute_Pointer_Precision()
    {
        var dial = new MdTimeDial { Width = 256, Height = 256, ActivePart = MdTimeDialPart.Minute, Minute = 0 };
        using var host = Show(dial, 300, 300);
        var window = host.Window;
        Dispatcher.UIThread.RunJobs();

        const int expectedMinute = 17;
        var angle = expectedMinute / 60d * Math.Tau;
        var local = new Point(128 + Math.Sin(angle) * 90, 128 - Math.Cos(angle) * 90);
        var point = dial.TranslatePoint(local, window)!.Value;
        window.MouseMove(point, RawInputModifiers.None);
        window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);

        Assert.Equal(expectedMinute, dial.Minute);
        Assert.NotNull(window.CaptureRenderedFrame());
    }

    [AvaloniaFact]
    public void Rating_Hit_Test_Quantizes_Inside_Each_Star_And_Ignores_Spacing()
    {
        var rating = new MdRating { ItemSize = 32, Spacing = 12, Precision = .5, Value = 0 };

        // 5 * 48 + 4 * 12 = 288. The window has to grant the row its natural width: below it the
        // control now folds the spacing in to keep every star reachable, which is the right
        // behaviour but a different arithmetic than the untouched pitch this test pins.
        using var host = Show(rating, 320, 80);
        var window = host.Window;
        Dispatcher.UIThread.RunJobs();
        var origin = rating.TranslatePoint(default, window)!.Value;

        // Every compact 32-DIP star now owns a full 48-DIP Material touch target. The left half
        // of the third target therefore resolves to 2.5 without reducing the visible glyph size.
        const double targetSize = 48;
        var leftHalf = origin + new Vector(2 * (targetSize + 12) + 12, targetSize / 2);
        window.MouseMove(leftHalf, RawInputModifiers.None);
        window.MouseDown(leftHalf, MouseButton.Left, RawInputModifiers.None);
        window.MouseUp(leftHalf, MouseButton.Left, RawInputModifiers.None);
        Assert.Equal(2.5, rating.Value);

        // The gap after the same star still belongs to its full value, never a shifted fraction.
        var gapPosition = 2 * (targetSize + 12) + targetSize + 6;
        rating.SetValueFromPosition(gapPosition);
        Assert.Equal(3, rating.Value);
    }

    [AvaloniaFact]
    public void Rating_Arranged_Narrower_Than_Its_Natural_Row_Still_Reaches_Both_Ends()
    {
        // Reported from the gallery: the row was pinned to 176 DIP while its natural width is
        // 5 * 48 + 4 * 4 = 256. The trailing stars were clipped away and every pixel the pointer
        // could still reach mapped to a lower value than the star drawn there, so the score could
        // be dragged down but never back up. The visible row must span the whole range.
        var rating = new MdRating { ItemSize = 32, Spacing = 4, Precision = .5, Value = 3.5, Width = 176 };
        using var host = Show(rating, 400, 120);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(176d, rating.Bounds.Width);
        rating.SetValueFromPosition(rating.Bounds.Width);
        Assert.Equal(5, rating.Value);
        rating.SetValueFromPosition(0);
        Assert.Equal(0.5, rating.Value);
    }

    [AvaloniaFact]
    public void Reorderable_List_Owns_A_Complete_Visible_Template_And_Moves_Real_Data()
    {
        var source = new ObservableCollection<string> { "Design", "Core", "Gallery", "Android" };
        var list = new MdReorderableList { Width = 360, Height = 240, ItemsSource = source, SelectedIndex = 0 };
        using var host = Show(list, 420, 300);
        Dispatcher.UIThread.RunJobs();

        var rows = list.GetVisualDescendants().OfType<ListBoxItem>().ToArray();
        Assert.Equal(4, rows.Length);
        Assert.All(rows, row => Assert.True(row.Bounds.Height >= 48));
        Assert.True(list.MoveItem(0, 3));
        Assert.Equal(new[] { "Core", "Gallery", "Android", "Design" }, source);
    }

    [AvaloniaFact]
    public void Ecosystem_Calendar_And_Collections_Render_Without_A_Global_Fluent_Theme()
    {
        var calendar = new MdCalendar { Width = 340 };
        MdLocalization.SetCulture(calendar, System.Globalization.CultureInfo.GetCultureInfo("zh-CN"));
        var transfer = new MdTransfer
        {
            Width = 720,
            ItemsSource = new[] { "Android", "Desktop", "Localization" },
            SelectedItems = new[] { "Desktop" }
        };
        var root = new StackPanel { Spacing = 12, Children = { calendar, transfer } };
        using var host = Show(root, 800, 700);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(42, calendar.VisibleDays.Count);
        Assert.Equal(7, calendar.WeekdayLabels.Count);
        Assert.Contains("年", calendar.DisplayMonthText);
        Assert.Equal(42, calendar.GetVisualDescendants().OfType<MdCalendarDayPresenter>().Count());
        Assert.Equal(2, transfer.GetVisualDescendants().OfType<ListBox>().Count());
        Assert.All(transfer.GetVisualDescendants().OfType<ListBox>(), list => Assert.True(list.Bounds.Height > 0));
    }

    [AvaloniaFact]
    public void Popover_Dismisses_When_The_Owning_Page_Scrolls()
    {
        // Native popups are intentionally unavailable in the headless platform. Exercise the
        // same public path called by the TopLevel wheel hook without forcing a popup host.
        var popover = new MdPopover { Anchor = "Open", PopoverContent = "Transient", IsOpen = true };
        Assert.True(popover.IsOpen);
        popover.NotifyOwnerScrolled();
        Assert.False(popover.IsOpen);
    }

    [AvaloniaFact]
    public void Usage_Language_Group_Is_Compact()
    {
        var example = new CodeExample { XamlCode = "<md:MdButton />", CSharpCode = "new MdButton();" };
        using var host = Show(example, 700, 360);
        Dispatcher.UIThread.RunJobs();
        var group = example.GetVisualDescendants().OfType<MdSegmentedButtonGroup>().Single();
        Assert.InRange(group.Bounds.Width, 140, 148);
        Assert.Equal(32, group.Bounds.Height);
        Assert.All(group.GetVisualDescendants().OfType<MdSegmentedButton>(), button => Assert.Equal(new Thickness(0), button.BorderThickness));
    }

    [AvaloniaFact]
    public void Borderless_Adapter_Receives_Native_Frame_Preservation_Option()
    {
        var adapter = new CapturingAdapter();
        var window = new MdBorderlessWindow { PlatformAdapter = adapter, PreserveNativeBorder = true };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.NotNull(adapter.Options);
            Assert.True(adapter.Options!.PreserveNativeBorder);
            Assert.Equal(40, adapter.Options.TitleBarHeight);
        }
        finally { window.Close(); }
    }

    private static Scope Show(Control content, double width, double height)
    {
        var window = new Window { Width = width, Height = height, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new Scope(window);
    }

    private sealed class Scope(Window window) : IDisposable
    {
        public Window Window { get; } = window;
        public void Dispose() => Window.Close();
    }

    private sealed class CapturingAdapter : IMdWindowPlatformAdapter
    {
        public MdWindowPlatform Platform => MdWindowPlatform.Windows;
        public MdWindowCapabilities Capabilities => MdWindowCapabilities.All;
        public MdBorderlessWindowOptions? Options { get; private set; }
        public void Apply(MdBorderlessWindow window, MdBorderlessWindowOptions options) => Options = options;
        public bool TryBeginMove(MdBorderlessWindow window, PointerPressedEventArgs args) => true;
        public bool TryBeginResize(MdBorderlessWindow window, WindowEdge edge, PointerPressedEventArgs args) => true;
        public bool TryShowSystemMenu(MdBorderlessWindow window, Point clientPoint) => false;
    }
}
