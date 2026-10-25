using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Gallery.Pages;

public partial class PaginationGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    // (symbol, accent role, English title, Chinese title, English body, Chinese body) -
    // every one of the 12 pages shows something different.
    private static readonly (string Symbol, string Accent, string TitleEn, string TitleZh, string BodyEn, string BodyZh)[] NumberedPages =
    [
        ("◧", "Md.Sys.Color.Primary", "Dashboard", "仪表盘",
         "KPIs, charts and the last activity at a glance.", "关键指标、图表与最新动态，一屏尽览。"),
        ("✉", "Md.Sys.Color.Secondary", "Inbox", "收件箱",
         "Unread threads and quick replies, no search required.", "未读会话与快速回复，无需搜索。"),
        ("✦", "Md.Sys.Color.Tertiary", "Ideas", "灵感",
         "Every half-formed thought the team saved this week.", "团队本周收藏的所有半成品想法。"),
        ("▣", "Md.Sys.Color.Error", "Issues", "问题",
         "Blocked, failing and still open - in one honest list.", "被阻塞、失败与仍开放的问题，一张诚实的清单。"),
        ("◐", "Md.Sys.Color.PrimaryContainer", "Builds", "构建",
         "The latest pipeline runs and what broke them.", "最新的流水线运行结果与失败原因。"),
        ("◆", "Md.Sys.Color.SecondaryContainer", "Roadmap", "路线图",
         "Quarterly goals ordered by the effort they really take.", "按真实投入排序的季度目标。"),
        ("★", "Md.Sys.Color.TertiaryContainer", "Favorites", "收藏",
         "Documents, boards and dashboards pinned by you.", "你固定的文档、看板与仪表盘。"),
        ("➜", "Md.Sys.Color.ErrorContainer", "Deploys", "部署",
         "Staging and production, with one-tape rollbacks.", "预发与生产环境，一键回滚。"),
        ("✚", "Md.Sys.Color.SurfaceVariant", "Team", "团队",
         "Who is where, in what timezone and on which plan.", "谁在哪里、处于哪个时区、使用哪个套餐。"),
        ("◈", "Md.Sys.Color.OnSurfaceVariant", "Audit", "审计",
         "Every change, who made it and why, in order.", "每一次变更、操作者与原因，按序排列。"),
        ("▤", "Md.Sys.Color.Outline", "Docs", "文档",
         "How-to guides written by the people who built it.", "由构建者撰写的操作指南。"),
        ("✳", "Md.Sys.Color.InversePrimary", "Settings", "设置",
         "The switches that change how the whole workspace behaves.", "改变整个工作区行为的开关。"),
    ];

    private static readonly (string TitleEn, string TitleZh, string BodyEn, string BodyZh)[] VerticalDotsPages =
    [
        ("Welcome", "欢迎",
         "Four quick steps set up a workspace your team will actually use.",
         "四个简单步骤就能建好一个团队真正会用起来的工作区。"),
        ("Pick a home", "选择起点",
         "Start from a template or keep an empty board - both stay fast.",
         "从模板开始或保留空白看板，两者同样快。"),
        ("Invite people", "邀请成员",
         "A link and a role per person; nobody gets more access than needed.",
         "每人一个链接和一个角色，没有人会获得多余的权限。"),
        ("Go", "完成",
         "That is everything. The workspace is live the moment you continue.",
         "就是这些。点击继续，工作区即刻上线。"),
    ];

    private const double VerticalDotsPageHeight = 200;
    private const double VerticalDotsPageSpacing = 16;

    private bool _syncing;
    private bool _initialized;
    private Border[]? _numberedPages;
    private readonly DispatcherTimer _verticalDotsSettleTimer = new();

    public PaginationGalleryPage()
    {
        InitializeComponent();
        SlideOneText.Text = L("Slide 1", "第 1 张");
        SlideTwoText.Text = L("Slide 2", "第 2 张");
        SlideThreeText.Text = L("Slide 3", "第 3 张");
        PageStatus.Text = L("Page 1 of 12", "第 1 页，共 12 页");
        DotsStatus.Text = L("Slide 1 of 3", "第 1 张，共 3 张");
        VerticalStatus.Text = L("Page 2 of 6", "第 2 页，共 6 页");
        VerticalDotsStatus.Text = L("Dots page 1 of 4", "圆点第 1 页，共 4 页");

        _verticalDotsSettleTimer.Tick += (_, _) =>
        {
            _verticalDotsSettleTimer.Stop();
            if (VerticalDotsScroll is not null)
                VerticalDotsScroll.Transitions = null;
        };

        RebuildPages();
        ShowNumberedPage(NumberedPager.SelectedPageIndex);
        BuildVerticalDotsPages();

        // The per-page accents and the card surface are resolved from theme tokens; a theme
        // switch rebuilds the pages so they stay in step.
        ActualThemeVariantChanged += (_, _) =>
        {
            RebuildPages();
            ShowNumberedPage(NumberedPager.SelectedPageIndex);
            BuildVerticalDotsPages();
        };

        // XAML property setters (SelectedPageIndex, SelectedIndex) raise their change events
        // while the tree is still being parsed, before later named fields exist. The handler
        // would then dereference a field that the loader has not assigned yet, so swallow the
        // early events: the XAML default texts already describe the initial state.
        _initialized = true;
    }

    private void NumberedPagerChanged(object? sender, PipsPagerSelectedIndexChangedEventArgs e)
    {
        if (!_initialized) return;
        PageProgress.Value = NumberedPager.SelectedPageIndex;
        PageStatus.Text = L($"Page {NumberedPager.SelectedPageIndex + 1} of 12", $"第 {NumberedPager.SelectedPageIndex + 1} 页，共 12 页");
        ShowNumberedPage(NumberedPager.SelectedPageIndex);
    }

    private void ShowNumberedPage(int index)
    {
        if (_numberedPages is null) return;
        for (var i = 0; i < _numberedPages.Length; i++)
            _numberedPages[i].IsVisible = i == index;
    }

    private void RebuildPages()
    {
        _numberedPages = NumberedPages.Select(spec => BuildNumberedPage(spec)).ToArray();
        NumberedPagesHost.Children.Clear();
        foreach (var page in _numberedPages)
            NumberedPagesHost.Children.Add(page);
    }

    private Border BuildNumberedPage(
        (string Symbol, string Accent, string TitleEn, string TitleZh, string BodyEn, string BodyZh) spec)
    {
        var (symbol, accent, titleEn, titleZh, bodyEn, bodyZh) = spec;
        return new Border
        {
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(24),
            Background = MdThemeResources.FindBrush(this, "Md.Sys.Color.SurfaceContainerLow.Brush", ActualThemeVariant),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Child = new StackPanel
            {
                Spacing = 6,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new TextBlock
                    {
                        Text = symbol,
                        FontSize = 40,
                        Foreground = MdThemeResources.FindBrush(this, accent + ".Brush", ActualThemeVariant),
                    },
                    new TextBlock
                    {
                        Text = L(titleEn, titleZh),
                        FontSize = 18,
                        FontWeight = FontWeight.SemiBold,
                    },
                    new TextBlock
                    {
                        Text = L(bodyEn, bodyZh),
                        FontSize = 13,
                        TextWrapping = TextWrapping.Wrap,
                    },
                },
            },
        };
    }

    private void DotsPagerChanged(object? sender, PipsPagerSelectedIndexChangedEventArgs e)
    {
        if (!_initialized || _syncing) return;
        _syncing = true;
        DotsCarousel.SelectedIndex = DotsPager.SelectedPageIndex;
        _syncing = false;
        DotsStatus.Text = L($"Slide {DotsPager.SelectedPageIndex + 1} of 3", $"第 {DotsPager.SelectedPageIndex + 1} 张，共 3 张");
    }

    private void DotsCarouselChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!_initialized || _syncing || DotsCarousel.SelectedIndex < 0) return;
        _syncing = true;
        DotsPager.SelectedPageIndex = DotsCarousel.SelectedIndex;
        _syncing = false;
        DotsStatus.Text = L($"Slide {DotsCarousel.SelectedIndex + 1} of 3", $"第 {DotsCarousel.SelectedIndex + 1} 张，共 3 张");
    }

    private void VerticalDotsPagerChanged(object? sender, PipsPagerSelectedIndexChangedEventArgs e)
    {
        if (!_initialized) return;
        VerticalDotsStatus.Text = L($"Dots page {VerticalDotsPager.SelectedPageIndex + 1} of 4", $"圆点第 {VerticalDotsPager.SelectedPageIndex + 1} 页，共 4 页");
        ScrollVerticalDotsTo(VerticalDotsPager.SelectedPageIndex);
    }

    private void VerticalDotsScrollPressed(object? sender, PointerPressedEventArgs e)
    {
        // A press on a peeked strip of the neighbour page jumps to it, the way the carousel
        // does; a press on the centred page stays put.
        if (!_initialized || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        var position = e.GetPosition(VerticalDotsHost);
        var step = VerticalDotsPageHeight + VerticalDotsPageSpacing;
        var index = (int)Math.Floor((position.Y - VerticalDotsHost.Margin.Top) / step);
        index = Math.Clamp(index, 0, VerticalDotsPages.Length - 1);
        if (index == VerticalDotsPager.SelectedPageIndex)
            return;

        VerticalDotsPager.SelectedPageIndex = index;
    }

    private void BuildVerticalDotsPages()
    {
        VerticalDotsHost.Children.Clear();
        for (var i = 0; i < VerticalDotsPages.Length; i++)
        {
            var spec = VerticalDotsPages[i];
            VerticalDotsHost.Children.Add(new Border
            {
                CornerRadius = new CornerRadius(16),
                Height = VerticalDotsPageHeight,
                Margin = new Thickness(14, 0, 14, 0),
                Padding = new Thickness(24),
                Background = MdThemeResources.FindBrush(this, "Md.Sys.Color.SurfaceContainer.Brush", ActualThemeVariant),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Child = new StackPanel
                {
                    Spacing = 8,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = (i + 1).ToString(),
                            FontSize = 34,
                            FontWeight = FontWeight.Bold,
                            Foreground = MdThemeResources.FindBrush(this, "Md.Sys.Color.Primary.Brush", ActualThemeVariant),
                        },
                        new TextBlock
                        {
                            Text = L(spec.TitleEn, spec.TitleZh),
                            FontSize = 18,
                            FontWeight = FontWeight.SemiBold,
                        },
                        new TextBlock
                        {
                            Text = L(spec.BodyEn, spec.BodyZh),
                            FontSize = 13,
                            TextWrapping = TextWrapping.Wrap,
                        },
                    },
                },
            });
        }
    }

    private void ScrollVerticalDotsTo(int index)
    {
        var scroll = VerticalDotsScroll;
        var viewport = scroll.ActualHeight;
        if (viewport <= 0)
            return; // Not laid out yet; offset zero already centres nothing and the first
                   // navigation will position the stack.

        // Center the selected page in the viewport, clamped so the first and last page rest
        // at the stack's ends - that is what leaves a strip of the neighbour peeking in.
        var step = VerticalDotsPageHeight + VerticalDotsPageSpacing;
        var target = VerticalDotsHost.Margin.Top + index * step + VerticalDotsPageHeight / 2 - viewport / 2;
        target = Math.Clamp(target, 0, Math.Max(0, scroll.Extent.Height - viewport));

        // Same settle pattern as the carousel: attach the transition, move the offset, then
        // clear the transition once the run finishes so later scrolls do not inherit it.
        scroll.Transitions = MdMotionTransitions.Collect(
            MdMotionTransitions.CreateVector(this, ScrollViewer.OffsetProperty));
        scroll.Offset = new Vector(0, target);

        var spec = MdMotion.Resolve(this, MdMotionKind.Spatial, MdMotionSpeed.Default);
        if (spec.IsEnabled)
        {
            _verticalDotsSettleTimer.Stop();
            _verticalDotsSettleTimer.Interval = spec.Duration + TimeSpan.FromMilliseconds(20);
            _verticalDotsSettleTimer.Start();
        }
        else
        {
            scroll.Transitions = null;
        }
    }

    private void VerticalPagerChanged(object? sender, PipsPagerSelectedIndexChangedEventArgs e)
    {
        if (!_initialized) return;
        VerticalStatus.Text = L($"Page {VerticalPager.SelectedPageIndex + 1} of 6", $"第 {VerticalPager.SelectedPageIndex + 1} 页，共 6 页");
    }
}
