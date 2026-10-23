using Avalonia.Controls;
using Md3.Avalonia.Controls;
using System;

namespace Md3.Avalonia.Gallery.Pages;

public partial class CommitGraphGalleryPage : UserControl
{
    public CommitGraphGalleryPage()
    {
        InitializeComponent();

        var now = DateTimeOffset.Now;
        var ago = (int days) => now.AddDays(-days).ToUniversalTime();

        // mainline, a side branch and its merge - the shape the control exists to draw.
        DemoGraph.ItemsSource = new MdCommitNode[]
        {
            new MdCommitNode
            {
                Id = "a1",
                Message = "Merge branch 'feature/export' into main",
                Author = "ilwren",
                Date = ago(0),
                ParentIds = ["a2", "b1"],
            },
            new MdCommitNode
            {
                Id = "b1",
                Message = "feature: export theme JSON",
                Author = "wren",
                Date = ago(1),
                ParentIds = ["a2"],
            },
            new MdCommitNode
            {
                Id = "b0",
                Message = "feature: export scaffolding",
                Author = "wren",
                Date = ago(2),
                ParentIds = ["a2"],
            },
            new MdCommitNode
            {
                Id = "a2",
                Message = "refactor: split theme tokens",
                Author = "ilwren",
                Date = ago(4),
                Tag = "v2.0.0",
                ParentIds = ["a3"],
            },
            new MdCommitNode
            {
                Id = "a3",
                Message = "fix: carousel settle on last page",
                Author = "ilwren",
                Date = ago(6),
                ParentIds = ["a4"],
            },
            new MdCommitNode
            {
                Id = "a4",
                Message = "chore: bump Avalonia to 12.1.2",
                Author = "wren",
                Date = ago(9),
                Tag = "v1.4.0",
                ParentIds = ["a5"],
            },
            new MdCommitNode
            {
                Id = "a5",
                Message = "feat: material theming pipeline",
                Author = "ilwren",
                Date = ago(14),
                ParentIds = ["a6"],
            },
            new MdCommitNode
            {
                Id = "a6",
                Message = "Initial commit",
                Author = "ilwren",
                Date = ago(21),
            },
        };
    }
}
