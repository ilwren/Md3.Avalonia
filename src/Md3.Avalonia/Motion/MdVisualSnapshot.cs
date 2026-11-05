using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace Md3.Avalonia.Motion;

/// <summary>
/// Renders a live control into a bitmap so an exit transition can keep a frozen image of the old
/// content on screen while the live element has already moved on to its next parent or state.
/// </summary>
/// <remarks>
/// <para>
/// A visual may have exactly one parent, so a swap cannot show the same <see cref="Control"/> in
/// both the outgoing and the incoming presenter. A snapshot is a plain image and can be retained,
/// faded and transformed independently; this is what lets the shared-axis, fade-through and
/// container-transform exits cross-fade their old content instead of skipping the phase.
/// </para>
/// <para>
/// The captured control must have been measured and arranged; snapshots are two-dimensional and
/// carry no hit testing, live bindings or accessibility tree.
/// </para>
/// </remarks>
public static class MdVisualSnapshot
{
    /// <summary>Captures <paramref name="control"/> at its current size, throwing when it cannot be rendered.</summary>
    public static RenderTargetBitmap Capture(Control control)
    {
        ArgumentNullException.ThrowIfNull(control);
        var scale = TopLevel.GetTopLevel(control)?.RenderScaling ?? 1;
        var size = new PixelSize(
            Math.Max(1, (int)Math.Ceiling(control.Bounds.Width * scale)),
            Math.Max(1, (int)Math.Ceiling(control.Bounds.Height * scale)));
        var bitmap = new RenderTargetBitmap(size, new Vector(96 * scale, 96 * scale));
        bitmap.Render(control);
        return bitmap;
    }

    /// <summary>
    /// Attempts to capture <paramref name="control"/>; returns <c>null</c> when it has no size yet
    /// or the platform cannot render it offscreen.
    /// </summary>
    public static RenderTargetBitmap? TryCapture(Control control)
    {
        if (control.Bounds.Width <= 0 || control.Bounds.Height <= 0) return null;
        try
        {
            return Capture(control);
        }
        catch
        {
            return null;
        }
    }
}
