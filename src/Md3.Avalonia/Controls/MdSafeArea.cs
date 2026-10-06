using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;

namespace Md3.Avalonia.Controls;

/// <summary>Edges an <see cref="MdSafeArea"/> insets its content on.</summary>
[Flags]
public enum MdSafeAreaEdges
{
    /// <summary>Inset nothing; the content runs under every system surface.</summary>
    None = 0,
    /// <summary>Inset the leading edge, typically a landscape cutout.</summary>
    Left = 1,
    /// <summary>Inset the top edge, typically the status bar or a notch.</summary>
    Top = 2,
    /// <summary>Inset the trailing edge, typically a landscape cutout.</summary>
    Right = 4,
    /// <summary>Inset the bottom edge, typically the navigation bar or gesture handle.</summary>
    Bottom = 8,
    /// <summary>Inset both side edges.</summary>
    Horizontal = Left | Right,
    /// <summary>Inset the top and bottom edges.</summary>
    Vertical = Top | Bottom,
    /// <summary>Inset every edge.</summary>
    All = Left | Top | Right | Bottom
}

/// <summary>
/// Insets its content by the window's safe area, so content clears the status bar, the navigation
/// bar, a display cutout and the gesture handle. The Material counterpart of Flutter's SafeArea.
/// </summary>
/// <remarks>
/// <para>
/// Avalonia pads the whole root view to the safe area by default, which is correct but blunt: with
/// it on, a top app bar can never paint its own surface behind the status bar, and a bottom
/// navigation bar can never paint behind the gesture handle — both of which Material asks for.
/// Set <c>TopLevel.AutoSafeAreaPadding="False"</c> on the root view and wrap only the parts that
/// must stay clear, which is how an edge-to-edge Material layout is built.
/// </para>
/// <para>
/// <see cref="SafeAreaPadding"/> is written from the platform's insets manager whenever it
/// changes. It is a settable property on purpose: desktop and headless have no insets manager, so
/// setting it is how a layout is previewed, and tested, without a phone.
/// </para>
/// </remarks>
public class MdSafeArea : Decorator
{
    /// <summary>Which edges to inset. Defaults to every edge.</summary>
    public static readonly StyledProperty<MdSafeAreaEdges> EdgesProperty =
        AvaloniaProperty.Register<MdSafeArea, MdSafeAreaEdges>(nameof(Edges), MdSafeAreaEdges.All);

    /// <summary>A floor applied per edge, so content still has breathing room where the inset is zero.</summary>
    public static readonly StyledProperty<Thickness> MinimumPaddingProperty =
        AvaloniaProperty.Register<MdSafeArea, Thickness>(nameof(MinimumPadding));

    /// <summary>The platform's current safe area. Written by the insets manager when one exists.</summary>
    public static readonly StyledProperty<Thickness> SafeAreaPaddingProperty =
        AvaloniaProperty.Register<MdSafeArea, Thickness>(nameof(SafeAreaPadding));

    private IInsetsManager? _insets;

    static MdSafeArea() =>
        AffectsMeasure<MdSafeArea>(EdgesProperty, MinimumPaddingProperty, SafeAreaPaddingProperty);

    /// <inheritdoc cref="EdgesProperty"/>
    public MdSafeAreaEdges Edges
    {
        get => GetValue(EdgesProperty);
        set => SetValue(EdgesProperty, value);
    }

    /// <inheritdoc cref="MinimumPaddingProperty"/>
    public Thickness MinimumPadding
    {
        get => GetValue(MinimumPaddingProperty);
        set => SetValue(MinimumPaddingProperty, value);
    }

    /// <inheritdoc cref="SafeAreaPaddingProperty"/>
    public Thickness SafeAreaPadding
    {
        get => GetValue(SafeAreaPaddingProperty);
        set => SetValue(SafeAreaPaddingProperty, value);
    }

    /// <summary>The inset actually applied, after edge filtering and <see cref="MinimumPadding"/>.</summary>
    public Thickness EffectivePadding => Resolve();

    /// <summary>True when the platform reports a non-zero inset on at least one selected edge.</summary>
    public bool IsSafeAreaActive => Resolve() != MinimumPadding;

    /// <summary>
    /// Reads the platform's safe area for the window hosting <paramref name="visual"/>, or a zero
    /// thickness where the platform has no insets manager (desktop, browser, headless).
    /// </summary>
    public static Thickness GetPlatformInsets(Visual visual)
    {
        ArgumentNullException.ThrowIfNull(visual);
        return TopLevel.GetTopLevel(visual)?.InsetsManager?.SafeAreaPadding ?? default;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Subscribe(TopLevel.GetTopLevel(this)?.InsetsManager);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Subscribe(null);
        base.OnDetachedFromVisualTree(e);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var padding = Resolve();
        if (Child is not { } child)
            return new Size(padding.Left + padding.Right, padding.Top + padding.Bottom);

        child.Measure(availableSize.Deflate(padding));
        return child.DesiredSize.Inflate(padding);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        Child?.Arrange(new Rect(finalSize).Deflate(Resolve()));
        return finalSize;
    }

    private void Subscribe(IInsetsManager? insets)
    {
        if (ReferenceEquals(_insets, insets)) return;
        if (_insets is not null) _insets.SafeAreaChanged -= OnSafeAreaChanged;
        _insets = insets;
        if (_insets is null) return;

        _insets.SafeAreaChanged += OnSafeAreaChanged;
        // SetCurrentValue, not SetValue: an author or a test may have supplied a simulated inset
        // as a local value, and the platform taking over must not erase their binding.
        SetCurrentValue(SafeAreaPaddingProperty, _insets.SafeAreaPadding);
    }

    private void OnSafeAreaChanged(object? sender, SafeAreaChangedArgs e) =>
        SetCurrentValue(SafeAreaPaddingProperty, e.SafeAreaPadding);

    private Thickness Resolve()
    {
        var edges = Edges;
        var padding = SafeAreaPadding;
        var minimum = MinimumPadding;
        return new Thickness(
            Math.Max(minimum.Left, edges.HasFlag(MdSafeAreaEdges.Left) ? padding.Left : 0d),
            Math.Max(minimum.Top, edges.HasFlag(MdSafeAreaEdges.Top) ? padding.Top : 0d),
            Math.Max(minimum.Right, edges.HasFlag(MdSafeAreaEdges.Right) ? padding.Right : 0d),
            Math.Max(minimum.Bottom, edges.HasFlag(MdSafeAreaEdges.Bottom) ? padding.Bottom : 0d));
    }
}
