using Avalonia.Controls.Presenters;
using Avalonia.Data;

namespace Md3.Avalonia.Controls;

/// <summary>
/// Presents an icon slot while still accepting arbitrary control content. The icon font is supplied
/// through the Md.Sys.Typeface.Symbols.Rounded resource and can come from any host package.
/// </summary>
public sealed class MdSymbolPresenter : ContentPresenter
{
    private IDisposable? _symbolFontValue;

    protected override void OnAttachedToVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (MdSymbolFontResolver.TryResolve(this, out var family))
        {
            _symbolFontValue?.Dispose();
            // Template priority supersedes the unresolved theme token but not a locally assigned
            // family, so icon slots remain directly customizable.
            _symbolFontValue = SetValue(FontFamilyProperty, family, BindingPriority.Template);
        }
    }

    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        _symbolFontValue?.Dispose();
        _symbolFontValue = null;
        base.OnDetachedFromVisualTree(e);
    }
}
