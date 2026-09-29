using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Layout;
using Avalonia.Media;

namespace Md3.Avalonia.Controls;

/// <summary>A one-DIP Material divider for grouping list and container content.</summary>
[PseudoClasses(":horizontal", ":vertical", ":inset")]
public sealed class MdDivider : Control
{
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<MdDivider, Orientation>(nameof(Orientation), Orientation.Horizontal);
    public static readonly StyledProperty<double> InsetProperty =
        AvaloniaProperty.Register<MdDivider, double>(nameof(Inset));
    public static readonly StyledProperty<double> LineThicknessProperty =
        AvaloniaProperty.Register<MdDivider, double>(nameof(LineThickness), 1);
    public static readonly StyledProperty<IBrush?> LineBrushProperty =
        AvaloniaProperty.Register<MdDivider, IBrush?>(nameof(LineBrush));

    static MdDivider()
    {
        AffectsRender<MdDivider>(OrientationProperty, InsetProperty, LineThicknessProperty, LineBrushProperty);
        OrientationProperty.Changed.AddClassHandler<MdDivider>((divider, _) => divider.UpdatePseudoClasses());
        InsetProperty.Changed.AddClassHandler<MdDivider>((divider, _) => divider.UpdatePseudoClasses());
    }

    public MdDivider() => UpdatePseudoClasses();

    public Orientation Orientation { get => GetValue(OrientationProperty); set => SetValue(OrientationProperty, value); }
    public double Inset { get => GetValue(InsetProperty); set => SetValue(InsetProperty, value); }
    public double LineThickness { get => GetValue(LineThicknessProperty); set => SetValue(LineThicknessProperty, value); }
    public IBrush? LineBrush { get => GetValue(LineBrushProperty); set => SetValue(LineBrushProperty, value); }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (LineBrush is null)
        {
            return;
        }

        var pen = new Pen(LineBrush, Math.Max(1, LineThickness));
        if (Orientation == Orientation.Horizontal)
        {
            context.DrawLine(pen, new Point(Math.Max(0, Inset), Bounds.Height / 2),
                new Point(Bounds.Width, Bounds.Height / 2));
        }
        else
        {
            context.DrawLine(pen, new Point(Bounds.Width / 2, Math.Max(0, Inset)),
                new Point(Bounds.Width / 2, Bounds.Height));
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":horizontal", Orientation == Orientation.Horizontal);
        PseudoClasses.Set(":vertical", Orientation == Orientation.Vertical);
        PseudoClasses.Set(":inset", Inset > 0);
    }
}
