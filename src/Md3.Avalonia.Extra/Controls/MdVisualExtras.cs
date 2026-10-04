using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Extra.Controls;

public enum MdComparisonOrientation { Horizontal, Vertical }

/// <summary>Reveals two pieces of content through a draggable before/after divider.</summary>
[PseudoClasses(":dragging", ":horizontal", ":vertical")]
public sealed class MdBeforeAfter : TemplatedControl
{
    public static readonly StyledProperty<object?> BeforeProperty = AvaloniaProperty.Register<MdBeforeAfter, object?>(nameof(Before));
    public static readonly StyledProperty<object?> AfterProperty = AvaloniaProperty.Register<MdBeforeAfter, object?>(nameof(After));
    public static readonly StyledProperty<double> PositionProperty = AvaloniaProperty.Register<MdBeforeAfter, double>(nameof(Position), .5, defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<MdComparisonOrientation> OrientationProperty = AvaloniaProperty.Register<MdBeforeAfter, MdComparisonOrientation>(nameof(Orientation));
    public static readonly StyledProperty<double> DividerThicknessProperty = AvaloniaProperty.Register<MdBeforeAfter, double>(nameof(DividerThickness), 2);
    public static readonly StyledProperty<IBrush?> DividerBrushProperty = AvaloniaProperty.Register<MdBeforeAfter, IBrush?>(nameof(DividerBrush));
    public static readonly StyledProperty<bool> IsInteractiveProperty = AvaloniaProperty.Register<MdBeforeAfter, bool>(nameof(IsInteractive), true);
    private Border? _afterClip;
    private Border? _divider;
    private bool _dragging;
    static MdBeforeAfter()
    {
        AffectsMeasure<MdBeforeAfter>(OrientationProperty, DividerThicknessProperty);
        // Raised here rather than from the pointer handler: the event is called PositionChanged,
        // so the arrow keys and a two-way binding have to raise it too, not just a drag.
        PositionProperty.Changed.AddClassHandler<MdBeforeAfter>((control, _) =>
        {
            control.UpdateGeometry();
            control.PositionChanged?.Invoke(control, EventArgs.Empty);
        });
        OrientationProperty.Changed.AddClassHandler<MdBeforeAfter>((control, _) => control.UpdateOrientation());
        IsInteractiveProperty.Changed.AddClassHandler<MdBeforeAfter>((control, _) => control.UpdateInteractivity());
    }
    public object? Before { get => GetValue(BeforeProperty); set => SetValue(BeforeProperty, value); }
    public object? After { get => GetValue(AfterProperty); set => SetValue(AfterProperty, value); }
    public double Position { get => GetValue(PositionProperty); set => SetValue(PositionProperty, Math.Clamp(value, 0, 1)); }
    public MdComparisonOrientation Orientation { get => GetValue(OrientationProperty); set => SetValue(OrientationProperty, value); }
    public double DividerThickness { get => GetValue(DividerThicknessProperty); set => SetValue(DividerThicknessProperty, value); }
    public IBrush? DividerBrush { get => GetValue(DividerBrushProperty); set => SetValue(DividerBrushProperty, value); }
    public bool IsInteractive { get => GetValue(IsInteractiveProperty); set => SetValue(IsInteractiveProperty, value); }
    public event EventHandler? PositionChanged;
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _afterClip = e.NameScope.Find<Border>("PART_AfterClip");
        _divider = e.NameScope.Find<Border>("PART_Divider");
        UpdateOrientation();
        UpdateGeometry();
    }
    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = base.ArrangeOverride(finalSize);
        UpdateGeometry();
        return result;
    }
    private void UpdateOrientation()
    {
        PseudoClasses.Set(":horizontal", Orientation == MdComparisonOrientation.Horizontal);
        PseudoClasses.Set(":vertical", Orientation == MdComparisonOrientation.Vertical);
        UpdateGeometry();
    }
    private void UpdateGeometry()
    {
        if (_afterClip is null) return;
        var position = Math.Clamp(Position, 0, 1);
        if (Orientation == MdComparisonOrientation.Horizontal)
        {
            _afterClip.Width = Bounds.Width * position;
            _afterClip.Height = double.NaN;
            _afterClip.HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Left;
            _afterClip.VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch;
            if (_divider is not null) { _divider.Width = DividerThickness; _divider.Height = double.NaN; _divider.Margin = new Thickness(Bounds.Width * position - DividerThickness / 2, 0, 0, 0); _divider.HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Left; }
        }
        else
        {
            _afterClip.Height = Bounds.Height * position;
            _afterClip.Width = double.NaN;
            _afterClip.HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch;
            _afterClip.VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Top;
            if (_divider is not null) { _divider.Height = DividerThickness; _divider.Width = double.NaN; _divider.Margin = new Thickness(0, Bounds.Height * position - DividerThickness / 2, 0, 0); _divider.VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Top; }
        }
    }
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (!IsInteractive || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) { base.OnPointerPressed(e); return; }
        _dragging = true; PseudoClasses.Set(":dragging", true); e.Pointer.Capture(this); UpdateFromPointer(e.GetPosition(this)); e.Handled = true;
    }
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (_dragging) { UpdateFromPointer(e.GetPosition(this)); e.Handled = true; }
        base.OnPointerMoved(e);
    }
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (_dragging) { _dragging = false; PseudoClasses.Set(":dragging", false); e.Pointer.Capture(null); e.Handled = true; }
        base.OnPointerReleased(e);
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        // IsInteractive used to gate the pointer only, so a comparison meant to be display-only
        // was still a tab stop whose divider moved under the arrow keys.
        if (!IsInteractive) { base.OnKeyDown(e); return; }
        var step = e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? .1 : .02;
        if (e.Key is Key.Left or Key.Down) { Position -= step; e.Handled = true; }
        else if (e.Key is Key.Right or Key.Up) { Position += step; e.Handled = true; }
        else base.OnKeyDown(e);
    }

    private void UpdateInteractivity() => SetCurrentValue(FocusableProperty, IsInteractive);
    private void UpdateFromPointer(Point point)
    {
        var value = Orientation == MdComparisonOrientation.Horizontal ? point.X / Math.Max(1, Bounds.Width) : point.Y / Math.Max(1, Bounds.Height);
        Position = value;
    }
}

public enum MdAnimatedTextEffect { Typewriter, Fade, Pop, None }

/// <summary>Small, provider-free animated text control inspired by common Flutter animated-text widgets.</summary>
[PseudoClasses(":playing", ":complete")]
public sealed class MdAnimatedText : TemplatedControl
{
    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<MdAnimatedText, string?>(nameof(Text));
    public static readonly new StyledProperty<MdAnimatedTextEffect> EffectProperty = AvaloniaProperty.Register<MdAnimatedText, MdAnimatedTextEffect>(nameof(Effect), MdAnimatedTextEffect.Typewriter);
    public static readonly StyledProperty<TimeSpan> DurationProperty = AvaloniaProperty.Register<MdAnimatedText, TimeSpan>(nameof(Duration), TimeSpan.FromMilliseconds(900));
    public static readonly StyledProperty<TimeSpan> PauseProperty = AvaloniaProperty.Register<MdAnimatedText, TimeSpan>(nameof(Pause), TimeSpan.Zero);
    public static readonly StyledProperty<bool> AutoPlayProperty = AvaloniaProperty.Register<MdAnimatedText, bool>(nameof(AutoPlay), true);
    public static readonly StyledProperty<bool> LoopProperty = AvaloniaProperty.Register<MdAnimatedText, bool>(nameof(Loop));
    public static readonly DirectProperty<MdAnimatedText, bool> IsPlayingProperty = AvaloniaProperty.RegisterDirect<MdAnimatedText, bool>(nameof(IsPlaying), c => c._playing);
    public static readonly DirectProperty<MdAnimatedText, string> DisplayTextProperty = AvaloniaProperty.RegisterDirect<MdAnimatedText, string>(nameof(DisplayText), c => c._displayText);
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(30) };
    private TextBlock? _textBlock;
    private DateTime _started;
    private bool _playing;
    private string _displayText = string.Empty;
    static MdAnimatedText()
    {
        TextProperty.Changed.AddClassHandler<MdAnimatedText>((c, _) => { if (c.AutoPlay) c.Start(); });
        EffectProperty.Changed.AddClassHandler<MdAnimatedText>((c, _) => { if (c.AutoPlay) c.Start(); });
        DurationProperty.Changed.AddClassHandler<MdAnimatedText>((c, _) => { if (c.AutoPlay) c.Start(); });
    }
    public MdAnimatedText() { Focusable = false; _timer.Tick += (_, _) => Tick(); }
    public string? Text { get => GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public new MdAnimatedTextEffect Effect { get => GetValue(EffectProperty); set => SetValue(EffectProperty, value); }
    public TimeSpan Duration { get => GetValue(DurationProperty); set => SetValue(DurationProperty, value); }
    public TimeSpan Pause { get => GetValue(PauseProperty); set => SetValue(PauseProperty, value); }
    public bool AutoPlay { get => GetValue(AutoPlayProperty); set => SetValue(AutoPlayProperty, value); }
    public bool Loop { get => GetValue(LoopProperty); set => SetValue(LoopProperty, value); }
    public bool IsPlaying => _playing;
    public string DisplayText => _displayText;
    public event EventHandler? Completed;
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) { base.OnApplyTemplate(e); _textBlock = e.NameScope.Find<TextBlock>("PART_Text"); if (!AutoPlay) { _displayText = Text ?? string.Empty; } ApplyVisualState(); if (AutoPlay) Start(); }
    protected override void OnAttachedToVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e) { base.OnAttachedToVisualTree(e); if (AutoPlay) Start(); }
    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e) { Stop(); base.OnDetachedFromVisualTree(e); }
    public void Start()
    {
        Stop(); _started = DateTime.UtcNow; SetAndRaise(IsPlayingProperty, ref _playing, true); PseudoClasses.Set(":playing", true); PseudoClasses.Set(":complete", false); _timer.Start(); Tick();
    }
    public void Stop() { _timer.Stop(); if (_playing) { SetAndRaise(IsPlayingProperty, ref _playing, false); PseudoClasses.Set(":playing", false); } }
    private void Tick()
    {
        var value = Text ?? string.Empty; var duration = Math.Max(1, Duration.TotalMilliseconds); var progress = Math.Clamp((DateTime.UtcNow - _started).TotalMilliseconds / duration, 0, 1);
        if (Effect == MdAnimatedTextEffect.Typewriter) _displayText = value[..Math.Clamp((int)Math.Ceiling(value.Length * progress), 0, value.Length)]; else _displayText = value;
        SetAndRaise(DisplayTextProperty, ref _displayText, _displayText); ApplyVisualState();
        if (progress < 1) return;
        _timer.Stop(); SetAndRaise(IsPlayingProperty, ref _playing, false); PseudoClasses.Set(":playing", false); PseudoClasses.Set(":complete", true); Completed?.Invoke(this, EventArgs.Empty);
        if (Loop) { _started = DateTime.UtcNow + Pause; _timer.Start(); }
    }
    private void ApplyVisualState() { if (_textBlock is null) return; _textBlock.Text = _displayText; _textBlock.Opacity = Effect == MdAnimatedTextEffect.Fade && IsPlaying ? Math.Clamp((DateTime.UtcNow - _started).TotalMilliseconds / Math.Max(1, Duration.TotalMilliseconds), 0, 1) : 1; }
}

public enum MdSpinKitKind { RotatingPlain, ThreeBounce, Wave, FadingCircle, ChasingDots }

/// <summary>Extra loading indicators for applications that need non-Material SpinKit-like motion.</summary>
public sealed class MdSpinKit : Control
{
    public static readonly StyledProperty<MdSpinKitKind> KindProperty = AvaloniaProperty.Register<MdSpinKit, MdSpinKitKind>(nameof(Kind), MdSpinKitKind.RotatingPlain);
    public static readonly StyledProperty<double> SizeProperty = AvaloniaProperty.Register<MdSpinKit, double>(nameof(Size), 40);
    public static readonly StyledProperty<IBrush?> IndicatorBrushProperty = AvaloniaProperty.Register<MdSpinKit, IBrush?>(nameof(IndicatorBrush));
    public static readonly StyledProperty<bool> IsActiveProperty = AvaloniaProperty.Register<MdSpinKit, bool>(nameof(IsActive), true);
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(33) }; private double _phase;
    static MdSpinKit()
    {
        AffectsRender<MdSpinKit>(KindProperty, SizeProperty, IndicatorBrushProperty, IsActiveProperty);
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdSpinKit>((c, _) => c.UpdateTimer());
        IsActiveProperty.Changed.AddClassHandler<MdSpinKit>((c, _) => c.UpdateTimer());
    }
    public MdSpinKit() { Width = 40; Height = 40; _timer.Tick += (_, _) => { _phase = (_phase + .035) % 1; InvalidateVisual(); }; }
    public MdSpinKitKind Kind { get => GetValue(KindProperty); set => SetValue(KindProperty, value); }
    public double Size { get => GetValue(SizeProperty); set => SetValue(SizeProperty, value); }
    public IBrush? IndicatorBrush { get => GetValue(IndicatorBrushProperty); set => SetValue(IndicatorBrushProperty, value); }
    public bool IsActive { get => GetValue(IsActiveProperty); set => SetValue(IsActiveProperty, value); }
    protected override void OnAttachedToVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e) { base.OnAttachedToVisualTree(e); UpdateTimer(); }
    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e) { _timer.Stop(); base.OnDetachedFromVisualTree(e); }
    private void UpdateTimer() { var scheme = MdMotion.GetScheme(this); if (IsActive && scheme is MdMotionScheme.Expressive or MdMotionScheme.Standard && this.IsAttachedToVisualTree()) _timer.Start(); else _timer.Stop(); }
    public override void Render(DrawingContext context)
    {
        base.Render(context); if (!IsActive) return; var brush = IndicatorBrush ?? new SolidColorBrush(Color.FromRgb(103, 80, 164)); var center = new Point(Bounds.Width / 2, Bounds.Height / 2); var radius = Math.Min(Bounds.Width, Bounds.Height) * .35;
        switch (Kind)
        {
            case MdSpinKitKind.ThreeBounce: for (var i = 0; i < 3; i++) { var scale = .55 + .45 * (Math.Sin((_phase + i / 3d) * Math.PI * 2) + 1) / 2; context.DrawEllipse(brush, null, new Point(center.X + (i - 1) * radius * .8, center.Y), radius * .18 * scale, radius * .18 * scale); } break;
            case MdSpinKitKind.Wave: for (var i = 0; i < 5; i++) { var h = radius * (.35 + .65 * (Math.Sin((_phase + i / 5d) * Math.PI * 2) + 1) / 2); context.FillRectangle(brush, new Rect(center.X + (i - 2) * radius * .35 - radius * .08, center.Y - h / 2, radius * .16, h)); } break;
            case MdSpinKitKind.FadingCircle: for (var i = 0; i < 8; i++) { var a = (i / 8d + _phase) * Math.PI * 2; var opacity = .15 + .85 * i / 8d; var color = brush is SolidColorBrush solid ? solid.Color : Colors.White; context.DrawEllipse(new SolidColorBrush(color, opacity), null, center + new Vector(Math.Cos(a) * radius, Math.Sin(a) * radius), radius * .14, radius * .14); } break;
            default: context.DrawEllipse(null, new Pen(brush, Math.Max(2, radius * .18)), center, radius, radius); context.DrawLine(new Pen(brush, Math.Max(2, radius * .18)), center, center + new Vector(Math.Cos(_phase * Math.PI * 2) * radius, Math.Sin(_phase * Math.PI * 2) * radius)); break;
        }
    }
}

/// <summary>StackPanel convenience control for staggered item entrance animations.</summary>
public sealed class MdStaggeredPanel : StackPanel
{
    public static readonly StyledProperty<TimeSpan> StaggerProperty = AvaloniaProperty.Register<MdStaggeredPanel, TimeSpan>(nameof(Stagger), TimeSpan.FromMilliseconds(60));
    public static readonly StyledProperty<TimeSpan> DurationProperty = AvaloniaProperty.Register<MdStaggeredPanel, TimeSpan>(nameof(Duration), TimeSpan.FromMilliseconds(240));
    public static readonly StyledProperty<double> OffsetProperty = AvaloniaProperty.Register<MdStaggeredPanel, double>(nameof(Offset), 16);
    public static readonly StyledProperty<bool> AutoPlayProperty = AvaloniaProperty.Register<MdStaggeredPanel, bool>(nameof(AutoPlay), true);
    private CancellationTokenSource? _cts;
    public TimeSpan Stagger { get => GetValue(StaggerProperty); set => SetValue(StaggerProperty, value); }
    public TimeSpan Duration { get => GetValue(DurationProperty); set => SetValue(DurationProperty, value); }
    public double Offset { get => GetValue(OffsetProperty); set => SetValue(OffsetProperty, value); }
    public bool AutoPlay { get => GetValue(AutoPlayProperty); set => SetValue(AutoPlayProperty, value); }
    public async ValueTask PlayAsync(CancellationToken token = default)
    {
        _cts?.Cancel(); _cts = CancellationTokenSource.CreateLinkedTokenSource(token); var linked = _cts.Token; var motion = MdMotion.Resolve(this, MdMotionKind.Spatial, MdMotionSpeed.Fast);
        var children = Children.ToArray(); foreach (var child in children) { child.Opacity = 0; child.RenderTransform = new TranslateTransform(0, Offset); }
        if (!motion.IsEnabled || MdMotion.GetScheme(this) == MdMotionScheme.Reduced) { foreach (var child in children) { child.Opacity = 1; child.RenderTransform = null; } return; }
        foreach (var child in children)
        {
            linked.ThrowIfCancellationRequested(); child.Transitions = MdMotionTransitions.Collect(MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast), MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast)); child.Opacity = 1; child.RenderTransform = null;
            if (Stagger > TimeSpan.Zero) await Task.Delay(Stagger, linked);
        }
    }
    protected override void OnAttachedToVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e) { base.OnAttachedToVisualTree(e); if (AutoPlay) _ = PlayAsync(); }
    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e) { _cts?.Cancel(); base.OnDetachedFromVisualTree(e); }
}
