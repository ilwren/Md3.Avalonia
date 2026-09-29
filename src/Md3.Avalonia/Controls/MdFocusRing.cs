using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Media;

namespace Md3.Avalonia.Controls;

/// <summary>A reusable Material focus indicator drawn without changing layout measurement.</summary>
public sealed class MdFocusRing : Control
{
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<MdFocusRing, bool>(nameof(IsActive));
    public static readonly StyledProperty<IBrush?> RingBrushProperty =
        AvaloniaProperty.Register<MdFocusRing, IBrush?>(nameof(RingBrush));
    public static readonly StyledProperty<double> RingThicknessProperty =
        AvaloniaProperty.Register<MdFocusRing, double>(nameof(RingThickness), 3);
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<MdFocusRing, CornerRadius>(nameof(CornerRadius));

    static MdFocusRing()
    {
        AffectsRender<MdFocusRing>(IsActiveProperty, RingBrushProperty, RingThicknessProperty, CornerRadiusProperty);
    }

    public MdFocusRing()
    {
        IsHitTestVisible = false;
        AutomationProperties.SetAccessibilityView(this, AccessibilityView.Raw);
    }

    public bool IsActive { get => GetValue(IsActiveProperty); set => SetValue(IsActiveProperty, value); }
    public IBrush? RingBrush { get => GetValue(RingBrushProperty); set => SetValue(RingBrushProperty, value); }
    public double RingThickness { get => GetValue(RingThicknessProperty); set => SetValue(RingThicknessProperty, value); }
    public CornerRadius CornerRadius { get => GetValue(CornerRadiusProperty); set => SetValue(CornerRadiusProperty, value); }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (!IsActive || RingBrush is null || RingThickness <= 0) return;
        var inset = RingThickness / 2;
        var rect = new Rect(inset, inset, Math.Max(0, Bounds.Width - RingThickness), Math.Max(0, Bounds.Height - RingThickness));
        var radius = Math.Max(CornerRadius.TopLeft, Math.Max(CornerRadius.TopRight,
            Math.Max(CornerRadius.BottomLeft, CornerRadius.BottomRight)));
        context.DrawRectangle(null, new Pen(RingBrush, RingThickness), new RoundedRect(rect, radius));
    }
}
