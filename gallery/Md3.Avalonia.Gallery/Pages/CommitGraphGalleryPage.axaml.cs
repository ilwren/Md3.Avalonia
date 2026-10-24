using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Md3.Avalonia.Controls;
using System;
using System.Linq;

namespace Md3.Avalonia.Gallery.Pages;

public partial class CommitGraphGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    private readonly MdCommitNode[] _nodes;
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

        ShowDetail(null);

        // The chips carry brushes resolved from theme tokens; a theme switch re-renders the
        // detail so they stay in step.
        ActualThemeVariantChanged += (_, _) => ShowDetail(_selected);
    }

    private void GraphCommitClicked(object? sender, MdCommitNode? e)
    {
        ShowDetail(e);
    }

    private void ShowDetail(MdCommitNode? node)
    {
        _selected = node;
        DetailContent.Children.Clear();
        if (node is null)
        {
            DetailContent.Children.Add(Info(L("Click a commit in the graph to inspect it here.", "点击提交图中的任意提交，在这里查看详情。")));
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
        meta.Children.Add(Info(node.Author + " · " + node.Date.ToString("MMM d, yyyy")));
        if (node.Tag is { Length: > 0 } tag)
            meta.Children.Add(Chip(tag, "Md.Sys.Color.PrimaryContainer.Brush"));
        if (DemoGraph.HeadId == node.Id)
            meta.Children.Add(Chip(L("HEAD", "HEAD"), "Md.Sys.Color.SurfaceContainerHighest.Brush"));
        DetailContent.Children.Add(meta);

        var parents = node.ParentIds.ToArray();
        var parentLine = L($"Parents: {string.Join(", ", parents)}", $"父提交：{string.Join("，", parents)}");
        var hint = node.ParentIds.Any(p => _nodes.All(n => n.Id != p))
            ? L(" (not in this shallow history)", "（不在当前的浅历史中）")
            : "";
        DetailContent.Children.Add(Info(parentLine + hint));
        DetailContent.Children.Add(Info(L($"Lanes in use: {DemoGraph.LaneCount}", $"当前使用的泳道数：{DemoGraph.LaneCount}")));
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
