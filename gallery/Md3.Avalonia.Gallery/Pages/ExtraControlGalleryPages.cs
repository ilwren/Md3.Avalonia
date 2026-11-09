using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Gallery.Components;

namespace Md3.Avalonia.Gallery.Pages;

/// <summary>Shared layout for the individually indexed Extra control demos.</summary>
public abstract class ExtraControlGalleryPage : UserControl
{
    private readonly StackPanel _body;

    protected ExtraControlGalleryPage(string title, string description, string typeName, string sample)
    {
        _body = new StackPanel { Spacing = 20, Margin = new Thickness(0, 0, 12, 24), MaxWidth = 820, HorizontalAlignment = HorizontalAlignment.Stretch };
        _body.Children.Add(Label(title, "h1"));
        _body.Children.Add(Label(description, "body"));
        // The class name is intentionally visible and searchable, not translated.
        var type = Label(typeName, "body");
        type.Classes.Add("verbatim");
        _body.Children.Add(type);
        Content = new MdScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            AllowMouseDrag = false,
            Content = _body
        };
        Sample = sample;
    }

    protected string Sample { get; }
    protected static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);
    protected static TextBlock Label(string text, string style = "body")
    {
        var label = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap };
        label.Classes.Add(style);
        return label;
    }
    protected static MdButton Button(string text) => new() { Content = text, HorizontalAlignment = HorizontalAlignment.Left };
    protected void Add(Control control) => _body.Children.Add(control);
    protected void AddDemo(Control content)
    {
        Add(Label(L("Interactive example", "交互示例"), "h2"));
        Add(new MdCard { IsInteractive = false, Padding = new Thickness(20), HorizontalAlignment = HorizontalAlignment.Stretch, Content = content });
        Add(Label(L("Usage", "用法"), "h2"));
        Add(new CodeExample { XamlCode = Sample });
    }
}

public sealed class AutoSizeTextDemoPage : ExtraControlGalleryPage
{
    public AutoSizeTextDemoPage() : base(L("Auto-size text", "自动缩放文本"),
        L("Change the available width to see the text fit without changing the requested font size.", "改变可用宽度，查看文本如何缩放而不改变请求的字号。"),
        "MdAutoSizeText", "<eco:MdAutoSizeText Text=\"{Binding Title}\" FontSize=\"30\" MinFontSize=\"10\" MaxLines=\"1\" />")
    {
        var text = new MdAutoSizeText { Text = L("A title that fits its container", "可自动适应容器宽度的标题"), FontSize = 30, MinFontSize = 10, MaxLines = 1 };
        var box = new Border { Width = 240, Height = 64, HorizontalAlignment = HorizontalAlignment.Left, Child = text, ClipToBounds = true };
        var narrow = Button(L("Narrow", "收窄"));
        var wide = Button(L("Wide", "加宽"));
        narrow.Click += (_, _) => box.Width = 150;
        wide.Click += (_, _) => box.Width = 240;
        var actions = new WrapPanel { Children = { narrow, wide } };
        AddDemo(new StackPanel { Spacing = 12, Children = { box, actions } });
    }
}

public sealed class ConfettiDemoPage : ExtraControlGalleryPage
{
    public ConfettiDemoPage() : base(L("Confetti", "庆祝粒子"),
        L("Start a finite burst. Reduced or disabled motion suppresses decorative particles.", "触发一次有限粒子动画；减少或关闭动效时会跳过装饰粒子。"),
        "MdConfetti", "<eco:MdConfetti x:Name=\"Celebration\" />\n<!-- Call Celebration.Burst() after attachment. -->")
    {
        var burst = new MdConfetti();
        var button = Button(L("Celebrate", "庆祝"));
        button.HorizontalAlignment = HorizontalAlignment.Center;
        button.VerticalAlignment = VerticalAlignment.Center;
        button.Click += (_, _) => burst.Burst();
        AddDemo(new Grid { Height = 180, ClipToBounds = true, Children = { button, burst } });
    }
}

public sealed class EmojiPickerDemoPage : ExtraControlGalleryPage
{
    public EmojiPickerDemoPage() : base(L("Emoji picker", "表情选择器"),
        L("Search and filter a small sample; the host receives the selected emoji.", "搜索并筛选少量示例表情；宿主接收选中的表情。"),
        "MdEmojiPicker", "<eco:MdEmojiPicker EmojiSelected=\"OnEmojiSelected\" />")
    {
        var status = Label(L("No emoji selected", "尚未选择表情"));
        status.Name = "EmojiDemoStatus";
        var picker = new MdEmojiPicker { MaxWidth = 480, HorizontalAlignment = HorizontalAlignment.Stretch };
        picker.EmojiSelected += (_, item) => status.Text = item.Emoji;
        AddDemo(new StackPanel { Spacing = 12, Children = { picker, status } });
    }
}

public sealed class ExpandableTextDemoPage : ExtraControlGalleryPage
{
    public ExpandableTextDemoPage() : base(L("Expandable text", "可展开文本"),
        L("Expand or collapse a long paragraph using the accessible toggle.", "使用无障碍切换按钮展开或收起长段落。"),
        "MdExpandableText", "<eco:MdExpandableText Text=\"{Binding Description}\" CollapsedLines=\"2\" />")
    {
        var text = new MdExpandableText
        {
            MaxWidth = 360, HorizontalAlignment = HorizontalAlignment.Stretch, CollapsedLines = 2,
            Text = L("Material components organize information and actions. Expand this paragraph to read the rest of the example, then collapse it when you are done.",
                "Material 组件用于组织信息与操作。展开此段落以阅读示例的其余内容，阅读完成后可以再次收起。")
        };
        AddDemo(text);
    }
}

public sealed class MarqueeDemoPage : ExtraControlGalleryPage
{
    public MarqueeDemoPage() : base(L("Marquee", "滚动文本"),
        L("Long text scrolls within the available width; reduced motion shows a static label.", "长文本在可用宽度内滚动；减少动效时显示静态文本。"),
        "MdMarquee", "<eco:MdMarquee Text=\"{Binding Announcement}\" Speed=\"40\" />")
    {
        var marquee = new MdMarquee
        {
            Padding = new Thickness(8, 0), Speed = 40, Gap = 30, PauseAfterRound = TimeSpan.FromSeconds(1),
            Text = L("A long announcement scrolls across the available space while the full text remains accessible.", "这是一条较长的公告，它会在可用空间内滚动，同时保留完整的无障碍文本。")
        };
        AddDemo(new Border { MaxWidth = 320, Height = 48, HorizontalAlignment = HorizontalAlignment.Stretch, Child = marquee });
    }
}

public sealed class MediaProgressDemoPage : ExtraControlGalleryPage
{
    public MediaProgressDemoPage() : base(L("Media progress bar", "媒体进度条"),
        L("Seek a host-controlled timeline using the pointer or keyboard. This example does not play audio.", "使用指针或键盘调整宿主控制的时间轴；此示例不播放音频。"),
        "MdMediaProgressBar", "<eco:MdMediaProgressBar Maximum=\"180\" Value=\"34\" BufferedSeconds=\"110\" SeekRequested=\"OnSeek\" />")
    {
        var status = Label(L("Seek position: 34 seconds", "定位位置：34 秒"));
        status.Name = "MediaDemoStatus";
        var bar = new MdMediaProgressBar { Minimum = 0, Maximum = 180, Value = 34, BufferedSeconds = 110, HorizontalAlignment = HorizontalAlignment.Stretch };
        bar.SeekRequested += (_, seconds) => status.Text = L($"Seek position: {seconds:0} seconds", $"定位位置：{seconds:0} 秒");
        AddDemo(new StackPanel { Spacing = 12, Children = { bar, status } });
    }
}

public sealed class PageIndicatorDemoPage : ExtraControlGalleryPage
{
    public PageIndicatorDemoPage() : base(L("Page indicator", "页面指示器"),
        L("Select a dot or use the keyboard to request a different page.", "选择圆点或使用键盘请求切换页面。"),
        "MdPageIndicator", "<eco:MdPageIndicator PageCount=\"4\" IndicatorEffect=\"Worm\" PageRequested=\"OnPageRequested\" />")
    {
        var status = Label(L("Page 1 of 4", "第 1 页，共 4 页"));
        status.Name = "PageIndicatorDemoStatus";
        var indicator = new MdPageIndicator { PageCount = 4, IndicatorEffect = MdPageIndicatorEffect.Worm, HorizontalAlignment = HorizontalAlignment.Left };
        indicator.PageRequested += (_, page) => status.Text = L($"Page {page + 1} of 4", $"第 {page + 1} 页，共 4 页");
        AddDemo(new StackPanel { Spacing = 12, Children = { indicator, status } });
    }
}

public sealed class SlideToConfirmDemoPage : ExtraControlGalleryPage
{
    public SlideToConfirmDemoPage() : base(L("Slide to confirm", "滑动确认"),
        L("Drag the handle to the end, or focus it and press Enter or Space. Reset allows another confirmation.", "将手柄拖到末端，或聚焦后按回车键或空格键。重置后可再次确认。"),
        "MdSlideToConfirm", "<eco:MdSlideToConfirm Confirmed=\"OnConfirmed\" />")
    {
        var status = Label(L("Waiting for confirmation.", "等待确认。"));
        status.Name = "SlideDemoStatus";
        var slide = new MdSlideToConfirm { MaxWidth = 360, HorizontalAlignment = HorizontalAlignment.Stretch };
        slide.Confirmed += (_, _) => status.Text = L("Confirmation received.", "已收到确认。");
        var reset = Button(L("Reset", "重置"));
        reset.Click += (_, _) => { slide.Reset(); status.Text = L("Waiting for confirmation.", "等待确认。"); };
        var hint = Label(L("Try a quick flick or a slow drag. Only releasing beyond the threshold confirms.",
            "试试快速甩动或缓慢拖动。只有松手时超过阈值才会确认。"));
        AddDemo(new StackPanel { Spacing = 12, Children = { slide, hint, reset, status } });
    }
}

public sealed class WheelPickerDemoPage : ExtraControlGalleryPage
{
    public WheelPickerDemoPage() : base(L("Wheel picker", "数字滚轮"),
        L("Choose a bounded integer by dragging, tapping a row, using the wheel, or using the keyboard.", "拖动、点击行、滚动鼠标滚轮或使用键盘选择有界整数。"),
        "MdWheelPicker", "<eco:MdWheelPicker Minimum=\"0\" Maximum=\"30\" Step=\"5\" Value=\"{Binding Quantity}\" />")
    {
        var status = Label(L("Selected: 10", "已选择：10"));
        status.Name = "WheelDemoStatus";
        var wheel = new MdWheelPicker { Width = 120, HorizontalAlignment = HorizontalAlignment.Left, Minimum = 0, Maximum = 30, Step = 5, Value = 10 };
        wheel.ValueChanged += (_, value) => status.Text = L($"Selected: {value}", $"已选择：{value}");
        var hint = Label(L("Flick to continue scrolling; pause before release to stop without momentum.",
            "快速甩动可继续滚动；松手前停顿可消除惯性。"));
        AddDemo(new StackPanel { Spacing = 12, Children = { wheel, status, hint } });
    }
}
