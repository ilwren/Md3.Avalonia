using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Md3.Avalonia.HeadlessTests.Spec;

/// <summary>Visual-tree helpers shared by the geometry and golden-image layers.</summary>
internal static class MdVisualTreeQuery
{
    /// <summary>
    /// Shows <paramref name="content"/> in a single window and pumps the dispatcher so that
    /// templates are applied and layout has run.
    /// </summary>
    /// <remarks>
    /// The whole conformance matrix is deliberately hosted in ONE window. Avalonia's headless
    /// platform owns a single process-wide dispatcher and compositor, so each additional
    /// show/close cycle is pure overhead; batching a few dozen controls into one layout pass is
    /// roughly an order of magnitude cheaper than one window per case.
    /// </remarks>
    public static IDisposable Show(Control content, double width = 1000, double height = 800)
    {
        var application = Application.Current;
        if (application is not null)
        {
            application.RequestedThemeVariant = ThemeVariant.Light;
        }

        var window = new Window
        {
            Width = width,
            Height = height,
            Content = content
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new WindowScope(window);
    }

    /// <summary>
    /// Finds the first template part matching any of <paramref name="names"/>, in preference order.
    /// </summary>
    /// <remarks>
    /// Returns null rather than throwing: a control that does not expose the expected part is a
    /// finding to record, not a reason to abandon the rest of the matrix.
    /// </remarks>
    public static T? FindPart<T>(Visual root, params string[] names)
        where T : StyledElement
    {
        var descendants = root.GetVisualDescendants().OfType<T>().ToList();
        foreach (var name in names)
        {
            foreach (var candidate in descendants)
            {
                if (string.Equals(candidate.Name, name, StringComparison.Ordinal))
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    /// <summary>Corner radius actually applied to a template part, when the part is a border.</summary>
    public static CornerRadius? CornerRadiusOf(StyledElement? part) =>
        part is Border border ? (CornerRadius?)border.CornerRadius : null;

    /// <summary>Rendered size of a template part after layout.</summary>
    public static Size? SizeOf(StyledElement? part) =>
        part is Visual visual ? (Size?)visual.Bounds.Size : null;

    private sealed class WindowScope(Window window) : IDisposable
    {
        public void Dispose() => window.Close();
    }
}
