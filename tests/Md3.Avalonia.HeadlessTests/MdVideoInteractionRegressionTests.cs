using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Gallery.Pages;
using Md3.Avalonia.Motion;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Regressions visible in the 2026-10-07 Gallery recording. The recording predates this branch,
/// but these tests exercise the current controls so the same non-crashing interaction failures do
/// not return unnoticed.
/// </summary>
public sealed class MdVideoInteractionRegressionTests
{
    [AvaloniaFact]
    public void Compact_Banner_Stacks_Actions_Instead_Of_Crushing_Its_Message()
    {
        var message = new TextBlock
        {
            Text = "Your changes could not be synchronized. The banner remains visible.",
            TextWrapping = global::Avalonia.Media.TextWrapping.Wrap
        };
        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new MdButton { Content = "Learn more", Variant = MdButtonVariant.Text },
                new MdButton { Content = "Retry", Variant = MdButtonVariant.Text }
            }
        };
        var banner = new MdBanner
        {
            Width = 360,
            LeadingContent = new MdIcon { Glyph = "!" },
            Content = message,
            Actions = actions,
            IsDismissible = true
        };
        using var scope = Show(banner, 420, 420);

        var actionPresenter = Part<ContentPresenter>(banner, "PART_Actions");
        Assert.Equal(1, Grid.GetRow(actionPresenter));
        Assert.True(message.Bounds.Width >= 200,
            $"The compact message column was only {message.Bounds.Width:0.#} dip wide.");

        banner.Width = 720;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(0, Grid.GetRow(actionPresenter));

        banner.ForceActionsBelow = true;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(1, Grid.GetRow(actionPresenter));
    }

    [AvaloniaFact]
    public void Input_Time_Field_Opens_The_Editor_And_Selects_The_Hour()
    {
        var picker = new MdTimePicker
        {
            Mode = MdTimePickerMode.Input,
            SelectedTime = new TimeSpan(9, 15, 0)
        };
        MdMotion.SetScheme(picker, MdMotionScheme.None);
        using var scope = Show(picker, 520, 560);

        PointerInput.Click(Part<Button>(picker, "PART_AnchorButton"));
        Dispatcher.UIThread.RunJobs();

        Assert.True(picker.IsOpen);
        var popup = Part<Popup>(picker, "PART_Popup");
        var hour = popup.Child!.GetVisualDescendants().OfType<MdTextBox>()
            .Single(control => control.Name == "PART_HourInput");
        Assert.True(hour.IsFocused, "Input mode must put a visible caret in the first editable field.");
        Assert.Equal(0, hour.SelectionStart);
        Assert.Equal(hour.Text?.Length ?? 0, hour.SelectionEnd);
    }

    [AvaloniaFact]
    public void Overlay_Only_Dialog_Isolates_The_Real_Page_And_Preserves_Its_Scroll_Position()
    {
        var opener = new Button { Content = "Open" };
        var page = new StackPanel
        {
            Children =
            {
                new Border { Height = 360 },
                opener,
                new Border { Height = 720 }
            }
        };
        var scroll = new ScrollViewer { Content = page };
        var dialogAction = new Button { Content = "Confirm" };
        var host = new MdDialogHost
        {
            Dialog = new MdDialog { Headline = "Dialog", Content = dialogAction }
        };
        MdMotion.SetScheme(host, MdMotionScheme.None);
        var root = new Grid { Children = { scroll, host } };
        using var scope = Show(root, 420, 520);

        Assert.True(opener.Focus());
        scroll.Offset = new Vector(0, 240);
        Dispatcher.UIThread.RunJobs();
        var originalOffset = scroll.Offset;

        host.IsOpen = true;
        Dispatcher.UIThread.RunJobs();
        Assert.False(scroll.IsHitTestVisible);
        Assert.True(dialogAction.IsFocused);
        Assert.Equal(originalOffset, scroll.Offset);

        host.IsOpen = false;
        Dispatcher.UIThread.RunJobs();
        Assert.True(scroll.IsHitTestVisible);
        Assert.True(opener.IsFocused);
        Assert.Equal(originalOffset, scroll.Offset);
    }

    [AvaloniaFact]
    public void License_Dialog_Reflows_On_A_Phone_Focuses_Filter_And_Reports_Real_State()
    {
        var page = new AboutDialogGalleryPage();
        using var scope = Show(page, 390, 720);
        var status = page.GetVisualDescendants().OfType<TextBlock>()
            .Single(control => control.Name == "LicenseStatus");
        var aboutButton = page.GetVisualDescendants().OfType<MdButton>()
            .Single(button => Equals(button.Content, "About this app"));

        PointerInput.Click(aboutButton);
        var host = page.GetVisualDescendants().OfType<MdDialogHost>().Single();
        Assert.True(host.IsOpen);
        Assert.Contains("opened", status.Text);

        var viewLicenses = host.GetVisualDescendants().OfType<MdButton>()
            .Single(button => Equals(button.Content, "View licenses"));
        PointerInput.Click(viewLicenses);
        Dispatcher.UIThread.RunJobs();

        var dialog = Assert.IsType<MdDialog>(host.Dialog);
        var licenses = Assert.IsType<MdLicensePage>(dialog.Content);
        Assert.True(dialog.Bounds.Width <= scope.Window.ClientSize.Width - 48 + 0.5,
            $"The {dialog.Bounds.Width:0.#} dip dialog overflows a {scope.Window.ClientSize.Width:0.#} dip window.");
        Assert.True(licenses.Bounds.Width < 640, "The phone must select the compact license layout.");
        var compact = Part<Grid>(licenses, "PART_CompactLayout");
        var wide = Part<Grid>(licenses, "PART_WideLayout");
        Assert.True(compact.IsVisible);
        Assert.False(wide.IsVisible);
        var filter = licenses.GetVisualDescendants().OfType<MdTextBox>().First();
        Assert.True(filter.IsFocused, "The searchable task must open with its visible filter focused.");
        Assert.Contains("License dialog opened", status.Text);

        host.Close();
        Dispatcher.UIThread.RunJobs();
        Assert.False(host.IsOpen);
        Assert.Equal("About dialog is closed.", status.Text);
    }

    private static T Part<T>(Visual root, string name) where T : Visual =>
        root.GetVisualDescendants().OfType<T>()
            .Single(control => (control as StyledElement)?.Name == name);

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
}
