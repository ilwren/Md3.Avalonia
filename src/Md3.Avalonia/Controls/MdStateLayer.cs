using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Media;

namespace Md3.Avalonia.Controls;

/// <summary>A reusable, non-hit-testable Material state overlay for custom component authors.</summary>
public sealed class MdStateLayer : Control
{
    public static readonly StyledProperty<IBrush?> StateBrushProperty =
        AvaloniaProperty.Register<MdStateLayer, IBrush?>(nameof(StateBrush));
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<MdStateLayer, CornerRadius>(nameof(CornerRadius));

    public MdStateLayer()
    {
        IsHitTestVisible = false;
        AutomationProperties.SetAccessibilityView(this, AccessibilityView.Raw);
    }

    public IBrush? StateBrush { get => GetValue(StateBrushProperty); set => SetValue(StateBrushProperty, value); }
    public CornerRadius CornerRadius { get => GetValue(CornerRadiusProperty); set => SetValue(CornerRadiusProperty, value); }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (StateBrush is null) return;
        var radius = Math.Max(CornerRadius.TopLeft, Math.Max(CornerRadius.TopRight,
            Math.Max(CornerRadius.BottomLeft, CornerRadius.BottomRight)));
        context.DrawRectangle(StateBrush, null, new RoundedRect(new Rect(Bounds.Size), radius));
    }
}
