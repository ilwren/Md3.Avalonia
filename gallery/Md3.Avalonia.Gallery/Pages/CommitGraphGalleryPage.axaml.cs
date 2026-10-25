using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class CommitGraphGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    private readonly MdCommitNode[] _nodes;
    private readonly MdCommitNode[] _mergeNodes;
    private MdCommitNode? _selected;

    public CommitGraphGalleryPage()
    {
        InitializeComponent();

        var now = DateTimeOffset.Now;
        var ago = (int days) => now.AddDays(-days).ToUniversalTime();

        // A dense history the control exists to make readable: a mainline with two release
        // tags, a feature branch that merged in (its absorbed tail renders dashed), an open
        // docs branch at HEAD, two short-lived spikes, and a shallow root whose first parent
        // is not in the data.
        _nodes = new MdCommitNode[]
        {
            new() { Id = "h5", Message = "docs: contribution guide", Author = "wren", Date = ago(0), ParentIds = ["h4"] },
            new() { Id = "m3", Message = "chore: release v2.1.0", Author = "ilwren", Date = ago(2), Tag = "v2.1.0", ParentIds = ["m2", "f2"] },
            new() { Id = "h4", Message = "docs: api reference draft", Author = "wren", Date = ago(3), ParentIds = ["m2"] },
            new() { Id = "f2", Message = "payments: export invoice CSV", Author = "ada", Date = ago(4), ParentIds = ["f1"] },
            new() { Id = "m2", Message = "fix: race in theme cache", Author = "ilwren", Date = ago(6), ParentIds = ["m1"] },
            new() { Id = "f1", Message = "payments: card capture", Author = "ada", Date = ago(8), ParentIds = ["f0"] },
            new() { Id = "m1", Message = "feat: onboarding checklist", Author = "ilwren", Date = ago(11), Tag = "v2.0.0", ParentIds = ["m0"] },
            new() { Id = "f0", Message = "payments: provider scaffolding", Author = "ada", Date = ago(13), ParentIds = ["m1"] },
            new() { Id = "x1", Message = "wip: dark theme contrast spike", Author = "wren", Date = ago(15), ParentIds = ["m0"] },
            new() { Id = "m0", Message = "refactor: token pipeline", Author = "ilwren", Date = ago(18), ParentIds = ["t1"] },
            new() { Id = "x0", Message = "wip: carousel physics spike", Author = "wren", Date = ago(20), ParentIds = ["t1"] },
            new() { Id = "t1", Message = "chore: initial import (shallow)", Author = "ilwren", Date = ago(30), ParentIds = ["ghost"] },
        };
        DemoGraph.ItemsSource = _nodes;

        // A history that is mostly merges: two feature lines cut from the mainline and folded
        // back in, so the graph must keep each branch in its own lane and mark every
        // absorbed segment dashed.
        _mergeNodes = new MdCommitNode[]
        {
            new() { Id = "mh4", Message = "docs: launch notes", Author = "wren", Date = ago(1), ParentIds = ["mm3"] },
            new() { Id = "mm3", Message = "merge: web dashboard", Author = "ilwren", Date = ago(3), Tag = "v3.0.0", ParentIds = ["mm2", "mg1"] },
            new() { Id = "mg1", Message = "web: metric widgets", Author = "grace", Date = ago(4), ParentIds = ["mg0"] },
            new() { Id = "mm2", Message = "merge: payments provider", Author = "ilwren", Date = ago(5), ParentIds = ["mm1", "mf1"] },
            new() { Id = "mg0", Message = "web: dashboard scaffold", Author = "grace", Date = ago(7), ParentIds = ["mm1"] },
            new() { Id = "mf1", Message = "payments: ledger entries", Author = "ada", Date = ago(8), ParentIds = ["mf0"] },
            new() { Id = "mm1", Message = "fix: checkout total rounding", Author = "ilwren", Date = ago(9), ParentIds = ["mm0"] },
            new() { Id = "mf0", Message = "payments: provider adapter", Author = "ada", Date = ago(11), ParentIds = ["mm0"] },
            new() { Id = "mm0", Message = "feat: base checkout flow", Author = "ilwren", Date = ago(14), ParentIds = ["mt0"] },
            new() { Id = "mt0", Message = "chore: initial import (shallow)", Author = "ilwren", Date = ago(21), ParentIds = ["ghost"] },
        };
        MergeGraph.ItemsSource = _mergeNodes;

        BuildLegend();
        ShowDetail(null, DemoGraph);

        // The chips and the legend carry brushes resolved from theme tokens; a theme switch
        // re-renders both so they stay in step.
        ActualThemeVariantChanged += (_, _) =>
        {
            ShowDetail(_selected, _selectedSource ?? DemoGraph);
            BuildLegend();
        };
    }

    private MdCommitGraph? _selectedSource;

    private void GraphCommitClicked(object? sender, MdCommitNode? e)
    {
        ShowDetail(e, DemoGraph);
    }

    private void MergeGraphCommitClicked(object? sender, MdCommitNode? e)
    {
        ShowDetail(e, MergeGraph);
    }

    private void ShowDetail(MdCommitNode? node, MdCommitGraph source)
    {
        _selected = node;
        _selectedSource = node is null ? null : source;
        DetailContent.Children.Clear();
        if (node is null)
        {
            DetailContent.Children.Add(Info(L("Click a commit in either graph to inspect it here.", "点击任一提交图中的任意提交，在这里查看详情。")));
            return;
        }

        DetailContent.Children.Add(new TextBlock
        {
            Text = node.Message,
            FontSize = 15,
            FontWeight = FontWeight.SemiBold,
            TextWrapping = TextWrapping.Wrap,
        });

        var meta = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        meta.Children.Add(Info(node.Author + " · " + (node.Date?.ToString("MMM d, yyyy") ?? string.Empty)));
        if (node.Tag is { Length: > 0 } tag)
            meta.Children.Add(Chip(tag, "Md.Sys.Color.PrimaryContainer.Brush"));
        if (source.HeadId == node.Id)
            meta.Children.Add(Chip(L("HEAD", "HEAD"), "Md.Sys.Color.SurfaceContainerHighest.Brush"));
        DetailContent.Children.Add(meta);

        var all = source == MergeGraph ? _mergeNodes : _nodes;
        var parents = node.ParentIds.ToArray();
        var mergeHint = parents.Length > 1
            ? L(" Merge commit: the second parent's line is dashed.", " 合并提交：第二父提交所在的线以虚线表示。")
            : "";
        var parentLine = L($"Parents: {string.Join(", ", parents)}", $"父提交：{string.Join("，", parents)}") + mergeHint;
        var hint = parents.Any(p => all.All(n => n.Id != p))
            ? L(" (not in this shallow history)", "（不在当前的浅历史中）")
            : "";
        DetailContent.Children.Add(Info(parentLine + hint));
        DetailContent.Children.Add(Info(L($"Lanes in use: {source.LaneCount}", $"当前使用的泳道数：{source.LaneCount}")));
    }

    private void BuildLegend()
    {
        LegendHost.Children.Clear();
        var brush = MdThemeResources.FindBrush(this, "Md.Sys.Color.Primary.Brush", ActualThemeVariant);
        if (brush is null)
            return;

        LegendHost.Children.Add(LegendLine(brush, dashed: false,
            L("Solid: first-parent / mainline", "实线：主线 / 第一父提交")));
        LegendHost.Children.Add(LegendLine(brush, dashed: true,
            L("Dashed: absorbed by a merge", "虚线：被合并吸收的线")));
    }

    private StackPanel LegendLine(IBrush brush, bool dashed, string label)
    {
        var pen = new Pen(brush, 2);
        if (dashed)
            pen.DashStyle = new DashStyle(new double[] { 6, 4 }, 0);

        var line = new Path
        {
            Data = StreamGeometry.Parse("M0,1 L28,1"),
            Stroke = pen,
            Width = 28,
            Height = 2,
            VerticalAlignment = VerticalAlignment.Center,
        };

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                line,
                new TextBlock
                {
                    Text = label,
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            },
        };
    }

    private static TextBlock Info(string text) => new()
    {
        Text = text,
        FontSize = 13,
        TextWrapping = TextWrapping.Wrap,
    };

    private Border Chip(string text, string backgroundToken) => new()
    {
        CornerRadius = new CornerRadius(10),
        Padding = new Thickness(8, 2, 8, 2),
        Background = MdThemeResources.FindBrush(this, backgroundToken, ActualThemeVariant),
        VerticalAlignment = VerticalAlignment.Center,
        Child = new TextBlock
        {
            Text = text,
            FontSize = 11,
            FontWeight = FontWeight.SemiBold,
        },
    };
}
