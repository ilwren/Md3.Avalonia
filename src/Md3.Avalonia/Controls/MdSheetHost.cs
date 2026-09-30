using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>Hosts modal or standard Material bottom/side sheet content without a platform-specific window.</summary>
[TemplatePart("PART_Scrim", typeof(Control))]
[TemplatePart("PART_DragHandle", typeof(Control))]
[TemplatePart("PART_Surface", typeof(Border))]
[PseudoClasses(":open", ":closed", ":present", ":modal", ":standard", ":bottom", ":left", ":right", ":dragging", ":reduced-motion", ":no-motion")]
public sealed class MdSheetHost : ContentControl
{
    public static readonly StyledProperty<object?> SheetContentProperty =
        AvaloniaProperty.Register<MdSheetHost, object?>(nameof(SheetContent));
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<MdSheetHost, bool>(nameof(IsOpen), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<bool> IsModalProperty =
        AvaloniaProperty.Register<MdSheetHost, bool>(nameof(IsModal), true);
    public static readonly StyledProperty<bool> DismissOnScrimClickProperty =
        AvaloniaProperty.Register<MdSheetHost, bool>(nameof(DismissOnScrimClick), true);
    public static readonly StyledProperty<MdSheetPlacement> PlacementProperty =
        AvaloniaProperty.Register<MdSheetHost, MdSheetPlacement>(nameof(Placement));
    public static readonly StyledProperty<double> SheetExtentProperty =
        AvaloniaProperty.Register<MdSheetHost, double>(nameof(SheetExtent), 320);
    public static readonly StyledProperty<bool> IsDragDismissEnabledProperty =
        AvaloniaProperty.Register<MdSheetHost, bool>(nameof(IsDragDismissEnabled), true);

    private readonly MdPresenceController _presence;
    private readonly TranslateTransform _motionTransform = new();
    private Control? _scrim;
    private Control? _dragHandle;
    private Border? _surface;
    private Transitions? _surfaceTransitions;
    private Transitions? _transformTransitions;
    private double _dragStartY;
    private double _dragOffset;
    private bool _dragging;

    static MdSheetHost()
    {
        IsOpenProperty.Changed.AddClassHandler<MdSheetHost>((host, _) => host.UpdateVisualState());
        IsModalProperty.Changed.AddClassHandler<MdSheetHost>((host, _) => host.UpdateVisualState());
        PlacementProperty.Changed.AddClassHandler<MdSheetHost>((host, _) => host.UpdateVisualState());
        SheetExtentProperty.Changed.AddClassHandler<MdSheetHost>((host, _) => host.UpdateSurfaceTarget());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdSheetHost>((host, _) => host.UpdateMotion());
    }

    public MdSheetHost()
    {
        _presence = new MdPresenceController(value => PseudoClasses.Set(":present", value));
        _presence.Initialize(IsOpen);
        UpdateVisualState();
    }

    public object? SheetContent { get => GetValue(SheetContentProperty); set => SetValue(SheetContentProperty, value); }
    public bool IsOpen { get => GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }
    public bool IsModal { get => GetValue(IsModalProperty); set => SetValue(IsModalProperty, value); }
    public bool DismissOnScrimClick { get => GetValue(DismissOnScrimClickProperty); set => SetValue(DismissOnScrimClickProperty, value); }
    public MdSheetPlacement Placement { get => GetValue(PlacementProperty); set => SetValue(PlacementProperty, value); }
    public double SheetExtent { get => GetValue(SheetExtentProperty); set => SetValue(SheetExtentProperty, value); }
    public bool IsDragDismissEnabled { get => GetValue(IsDragDismissEnabledProperty); set => SetValue(IsDragDismissEnabledProperty, value); }

    public void Show() => SetCurrentValue(IsOpenProperty, true);
    public void Dismiss() => SetCurrentValue(IsOpenProperty, false);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachTemplateHandlers();
        base.OnApplyTemplate(e);
        _scrim = e.NameScope.Find<Control>("PART_Scrim");
        _dragHandle = e.NameScope.Find<Control>("PART_DragHandle");
        _surface = e.NameScope.Find<Border>("PART_Surface");
        if (_scrim is not null) _scrim.PointerPressed += OnScrimPressed;
        if (_dragHandle is not null)
        {
            _dragHandle.PointerPressed += OnDragPressed;
            _dragHandle.PointerMoved += OnDragMoved;
            _dragHandle.PointerReleased += OnDragReleased;
            _dragHandle.PointerCaptureLost += OnDragCaptureLost;
        }
        if (_surface is not null) _surface.RenderTransform = _motionTransform;
        UpdateMotion();
        UpdateSurfaceTarget();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateVisualState();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _presence.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && IsModal && IsOpen)
        {
            Dismiss();
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    private void DetachTemplateHandlers()
    {
        if (_scrim is not null) _scrim.PointerPressed -= OnScrimPressed;
        if (_dragHandle is null) return;
        _dragHandle.PointerPressed -= OnDragPressed;
        _dragHandle.PointerMoved -= OnDragMoved;
        _dragHandle.PointerReleased -= OnDragReleased;
        _dragHandle.PointerCaptureLost -= OnDragCaptureLost;
    }

    private void OnScrimPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DismissOnScrimClick && IsModal && ReferenceEquals(e.Source, _scrim))
        {
            Dismiss();
            e.Handled = true;
        }
    }

    private void OnDragPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsDragDismissEnabled || !IsOpen || Placement != MdSheetPlacement.Bottom || _dragHandle is null) return;
        _dragging = true;
        _dragOffset = 0;
        _dragStartY = e.GetPosition(this).Y;
        if (_surface is not null)
        {
            // Pointer-driven motion must remain 1:1. Suspend both effects and spatial transitions;
            // the same transform instance is mutated without per-frame allocations.
            _surfaceTransitions = _surface.Transitions;
            _transformTransitions = _motionTransform.Transitions;
            _surface.Transitions = null;
            _motionTransform.Transitions = null;
            _motionTransform.X = 0;
            _motionTransform.Y = 0;
        }
        e.Pointer.Capture(_dragHandle);
        UpdatePseudoClasses();
        e.Handled = true;
    }

    private void OnDragMoved(object? sender, PointerEventArgs e)
    {
        if (!_dragging || _surface is null) return;
        _dragOffset = Math.Clamp(e.GetPosition(this).Y - _dragStartY, 0, Math.Max(0, SheetExtent));
        _motionTransform.Y = _dragOffset;
        e.Handled = true;
    }

    private void OnDragReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_dragging) return;
        CompleteDrag();
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private void OnDragCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (_dragging) CompleteDrag();
    }

    private void CompleteDrag()
    {
        _dragging = false;
        var shouldDismiss = _dragOffset >= Math.Min(112, SheetExtent * 0.3);
        _dragOffset = 0;
        if (_surface is not null)
        {
            _surface.Transitions = _surfaceTransitions;
            _motionTransform.Transitions = _transformTransitions;
            _surfaceTransitions = null;
            _transformTransitions = null;
        }
        if (shouldDismiss) Dismiss(); else UpdateSurfaceTarget();
        UpdatePseudoClasses();
    }

    private void UpdateVisualState()
    {
        var speed = IsOpen ? MdMotionSpeed.Default : MdMotionSpeed.Fast;
        ConfigureTransitions(speed);
        if (IsOpen)
        {
            _presence.Update(true, TimeSpan.Zero);
            PseudoClasses.Set(":open", true);
            PseudoClasses.Set(":closed", false);
        }
        else
        {
            PseudoClasses.Set(":open", false);
            PseudoClasses.Set(":closed", true);
            _presence.Update(false, MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
        }
        UpdatePseudoClasses();
        UpdateSurfaceTarget();
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":modal", IsModal);
        PseudoClasses.Set(":standard", !IsModal);
        PseudoClasses.Set(":bottom", Placement == MdSheetPlacement.Bottom);
        PseudoClasses.Set(":left", Placement == MdSheetPlacement.Left);
        PseudoClasses.Set(":right", Placement == MdSheetPlacement.Right);
        PseudoClasses.Set(":dragging", _dragging);
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        var speed = IsOpen ? MdMotionSpeed.Default : MdMotionSpeed.Fast;
        ConfigureTransitions(speed);

        if (!IsOpen) _presence.Update(false, MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
        UpdateSurfaceTarget();
    }

    private void ConfigureTransitions(MdMotionSpeed speed)
    {
        var surfaceTransitions = MdMotionTransitions.Collect(
            MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, speed));
        var transformTransitions = MdMotionTransitions.Collect(
            MdMotionTransitions.CreateDouble(this, TranslateTransform.XProperty, MdMotionKind.Spatial, speed),
            MdMotionTransitions.CreateDouble(this, TranslateTransform.YProperty, MdMotionKind.Spatial, speed));

        if (_dragging)
        {
            _surfaceTransitions = surfaceTransitions;
            _transformTransitions = transformTransitions;
        }
        else
        {
            if (_surface is not null) _surface.Transitions = surfaceTransitions;
            _motionTransform.Transitions = transformTransitions;
        }
        if (_scrim is not null)
        {
            _scrim.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, speed));
        }
    }

    private void UpdateSurfaceTarget()
    {
        if (_dragging) return;
        var reduced = MdMotion.GetScheme(this) == MdMotionScheme.Reduced;
        if (IsOpen || reduced)
        {
            _motionTransform.X = 0;
            _motionTransform.Y = 0;
            return;
        }

        var extent = Math.Max(0, SheetExtent);
        _motionTransform.X = Placement switch
        {
            MdSheetPlacement.Left => -extent,
            MdSheetPlacement.Right => extent,
            _ => 0
        };
        _motionTransform.Y = Placement == MdSheetPlacement.Bottom ? extent : 0;
    }
}
