using System.ComponentModel;
using System.Globalization;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Gallery;
using Md3.Avalonia.Gallery.Pages;
using Md3.Avalonia.Localization;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdExpandableTextTests
{
    private const string Paragraph = "One two three four five six seven eight nine ten eleven twelve thirteen fourteen.";

    [AvaloniaFact]
    public void Wrapped_Line_Limit_Reflows_And_Hides_The_Toggle_When_Text_Fits()
    {
        var text = Create(Paragraph, 120);
        text.CollapsedLines = 2;
        var window = Show(text);
        try
        {
            var part = Part<TextBlock>(text, "PART_Text");
            var button = Part<MdButton>(text, "PART_Toggle");
            Assert.True(text.CanExpand);
            Assert.True(button.IsVisible);
            Assert.Equal(2, part.MaxLines);
            Assert.True(part.TextLayout.TextLines.Count <= 2);
            text.Width = 700;
            Dispatcher.UIThread.RunJobs();
            Assert.False(text.CanExpand);
            Assert.False(button.IsVisible);
            text.Width = 120;
            Dispatcher.UIThread.RunJobs();
            Assert.True(text.CanExpand);
            text.Text = "Short";
            Dispatcher.UIThread.RunJobs();
            Assert.False(text.CanExpand);
            Assert.False(button.IsVisible);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Pointer_And_Keyboard_Toggle_A_TwoWay_Bound_State()
    {
        var model = new ExpansionModel();
        var text = Create(Paragraph, 130);
        MdLocalization.SetCulture(text, CultureInfo.GetCultureInfo("en-US"));
        text.Bind(MdExpandableText.IsExpandedProperty, new Binding(nameof(ExpansionModel.Expanded)) { Source = model, Mode = BindingMode.TwoWay });
        var window = Show(text);
        try
        {
            var button = Part<MdButton>(text, "PART_Toggle");
            var part = Part<TextBlock>(text, "PART_Text");
            Assert.Equal("Expand", button.Content);
            Assert.Equal("Expand", ControlAutomationPeer.CreatePeerForElement(button).GetName());
            var point = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value;
            window.MouseMove(point, RawInputModifiers.None);
            window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
            window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            Assert.True(text.IsExpanded);
            Assert.True(model.Expanded);
            Assert.Equal(0, part.MaxLines);
            Assert.True(part.TextLayout.TextLines.Count > 2);
            Assert.Equal("Collapse", button.Content);
            Assert.True(button.Focus());
            window.KeyPress(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
            window.KeyRelease(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
            Dispatcher.UIThread.RunJobs();
            Assert.False(text.IsExpanded);
            Assert.False(model.Expanded);
            model.Expanded = true;
            Dispatcher.UIThread.RunJobs();
            Assert.True(text.IsExpanded);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Character_Mode_Uses_Unicode_Text_Elements_And_Updates_When_Changed()
    {
        var text = Create("A👩‍👩‍👧‍👦e\u0301Z", 300);
        text.TrimMode = MdExpandableTextTrimMode.Characters;
        text.CollapsedCharacters = 3;
        var window = Show(text);
        try
        {
            Assert.True(text.CanExpand);
            Assert.Equal("A👩‍👩‍👧‍👦e\u0301…", text.DisplayText);
            Assert.Equal(0, text.DisplayMaxLines);
            text.IsExpanded = true;
            Assert.Equal(text.Text, text.DisplayText);
            text.IsExpanded = false;
            text.Text = "👩‍👩‍👧‍👦e\u0301";
            Assert.False(text.CanExpand);
            Assert.Equal(text.Text, text.DisplayText);
            text.CollapsedCharacters = 0;
            Assert.Equal(1, text.CollapsedCharacters);
            Assert.Equal("👩‍👩‍👧‍👦…", text.DisplayText);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Explicit_Lines_Rtl_And_Dynamic_Labels_Are_Supported()
    {
        var text = Create("الأول\nالثاني\nالثالث", 400);
        text.CollapsedLines = 1;
        text.FlowDirection = FlowDirection.RightToLeft;
        var window = Show(text);
        try
        {
            var button = Part<MdButton>(text, "PART_Toggle");
            Assert.True(text.CanExpand);
            MdLocalization.SetCulture(text, CultureInfo.GetCultureInfo("zh-CN"));
            Assert.Equal("展开", button.Content);
            text.ExpandLabel = "More details";
            Assert.Equal("More details", button.Content);
            text.CollapseLabel = "Less";
            text.IsExpanded = true;
            Assert.Equal("Less", button.Content);
            text.CollapseLabel = null;
            Assert.Equal("收起", button.Content);
            text.CollapsedLines = 0;
            Assert.Equal(1, text.CollapsedLines);
            Assert.NotNull(window.CaptureRenderedFrame());
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Gallery_Translates_And_Restores_Expandable_Text()
    {
        var page = new AnimatedTextGalleryPage();
        var text = page.GetLogicalDescendants().OfType<MdExpandableText>().Single();
        var original = text.Text;
        GalleryLocalization.Apply(page, CultureInfo.GetCultureInfo("zh-CN"));
        Assert.StartsWith("Material 组件", text.Text);
        GalleryLocalization.Apply(page, CultureInfo.GetCultureInfo("en-US"));
        Assert.Equal(original, text.Text);
    }

    private static MdExpandableText Create(string value, double width) => new()
    {
        Text = value, Width = width, FontSize = 16,
        HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top
    };

    private static Window Show(Control control)
    {
        var window = new Window { Width = 800, Height = 400, Content = control };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static T Part<T>(Control control, string name) where T : Control =>
        control.GetVisualDescendants().OfType<T>().Single(element => element.Name == name);

    private sealed class ExpansionModel : INotifyPropertyChanged
    {
        private bool _expanded;
        public bool Expanded { get => _expanded; set { _expanded = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Expanded))); } }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
