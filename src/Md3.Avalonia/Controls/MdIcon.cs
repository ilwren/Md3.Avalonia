using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;

namespace Md3.Avalonia.Controls;

/// <summary>Renders a glyph using the host-selected icon font. The core package does not require a specific icon-font package.</summary>
public sealed class MdIcon : TextBlock
{
    private IDisposable? _symbolFontValue;

    public static readonly StyledProperty<string?> GlyphProperty =
        AvaloniaProperty.Register<MdIcon, string?>(nameof(Glyph));

    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<MdIcon, double>(nameof(Size), 24);

    static MdIcon()
    {
        GlyphProperty.Changed.AddClassHandler<MdIcon>((icon, _) =>
            icon.SetCurrentValue(TextProperty, icon.Glyph));
        SizeProperty.Changed.AddClassHandler<MdIcon>((icon, _) =>
            icon.SetValue(FontSizeProperty, icon.Size));
    }

    public MdIcon()
    {
        SetValue(FontSizeProperty, Size);
        SetCurrentValue(TextAlignmentProperty, TextAlignment.Center);
        SetCurrentValue(HorizontalAlignmentProperty, global::Avalonia.Layout.HorizontalAlignment.Center);
        SetCurrentValue(VerticalAlignmentProperty, global::Avalonia.Layout.VerticalAlignment.Center);
    }

    protected override void OnAttachedToVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (MdSymbolFontResolver.TryResolve(this, out var family))
        {
            _symbolFontValue?.Dispose();
            // Template priority overrides the unresolved theme token while preserving any
            // explicit local FontFamily supplied by the host.
            _symbolFontValue = SetValue(FontFamilyProperty, family, BindingPriority.Template);
        }
    }

    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        _symbolFontValue?.Dispose();
        _symbolFontValue = null;
        base.OnDetachedFromVisualTree(e);
    }

    public string? Glyph
    {
        get => GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }
}
