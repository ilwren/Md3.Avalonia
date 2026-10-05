using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
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

                var demanded = page.DesiredSize.Width;
                if (demanded > NarrowWidth + 1)
                {
                    tooWide.Add($"{type.Name} wants {demanded:0}");
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
