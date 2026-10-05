using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.VisualTree;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Md3.Avalonia.Gallery.Pages;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>Every gallery page has to survive a narrow window.</summary>
public sealed class MdGalleryAdaptiveTests
{
    // A desktop window dragged narrow. Pages that demand more than this have a fixed-width
    // element in them and will clip or push their content off screen rather than reflow.
    private const double NarrowWidth = 480;

    // Popup content is placed in an overlay, so its edge translated into the page is not a
    // page-layout measurement at all.
    private static bool IsInsidePopup(Control control) =>
        control.GetSelfAndVisualAncestors().Any(visual => visual is PopupRoot or OverlayPopupHost);

    private static bool IsHorizontallyScrollable(Control control) =>
        control.GetSelfAndVisualAncestors().OfType<ScrollViewer>().Any(viewer =>
            viewer.HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled);

    [AvaloniaFact]
    public void Every_Gallery_Page_Fits_A_Narrow_Window()
    {
        var pageTypes = typeof(ButtonGalleryPage).Assembly.GetTypes()
            .Where(type => type is { IsAbstract: false, IsPublic: true }
                           && typeof(UserControl).IsAssignableFrom(type)
                           && type.Namespace == typeof(ButtonGalleryPage).Namespace
                           && type.GetConstructor(Type.EmptyTypes) is not null)
            .OrderBy(type => type.Name)
            .ToArray();

        Assert.True(pageTypes.Length > 40, $"only found {pageTypes.Length} gallery pages to check");

        var tooWide = new List<string>();
        var broken = new List<string>();

        foreach (var type in pageTypes)
        {
            UserControl page;
            try
            {
                page = (UserControl)Activator.CreateInstance(type)!;
            }
            catch (Exception ex)
            {
                broken.Add($"{type.Name}: {(ex as TargetInvocationException)?.InnerException?.Message ?? ex.Message}");
                continue;
            }

            var window = new Window { Width = NarrowWidth, Height = 900, Content = page };
            window.Show();
            try
            {
                Dispatcher.UIThread.RunJobs();
                window.Measure(new Size(NarrowWidth, 900));
                window.Arrange(new Rect(0, 0, NarrowWidth, 900));
                Dispatcher.UIThread.RunJobs();

                // DesiredSize is useless here: measuring inside a 480 dip window clamps it to
                // 480, so asserting on it passes for every page whether it reflows or not.
                // What actually shows a page failing to adapt is content arranged past the
                // right edge, with nothing horizontally scrollable to reach it.
                var worst = page.GetVisualDescendants()
                    .OfType<Control>()
                    .Where(control => control.Bounds.Width > 0
                                      && !IsHorizontallyScrollable(control)
                                      && !IsInsidePopup(control))
                    .Select(control => new
                    {
                        Control = control,
                        Right = control.TranslatePoint(new Point(control.Bounds.Width, 0), page)?.X ?? 0,
                    })
                    .Where(item => item.Right > NarrowWidth + 1)
                    .OrderByDescending(item => item.Right)
                    .FirstOrDefault();

                if (worst is not null)
                {
                    tooWide.Add($"{type.Name} ({worst.Control.GetType().Name} reaches {worst.Right:0})");
                }
            }
            finally { window.Close(); }
        }

        Assert.True(broken.Count == 0, "pages that would not even construct: " + string.Join("; ", broken));
        Assert.True(tooWide.Count == 0,
            $"{tooWide.Count} of {pageTypes.Length} pages cannot reflow below {NarrowWidth} dip: " +
            string.Join("; ", tooWide));
    }
}
