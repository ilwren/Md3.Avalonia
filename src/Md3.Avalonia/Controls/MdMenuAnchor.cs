using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>Anchors a Material menu to arbitrary trigger content.</summary>
[TemplatePart("PART_Popup", typeof(Popup))]
[TemplatePart("PART_MenuSurface", typeof(Border))]
[PseudoClasses(":open", ":closed", ":present", ":reduced-motion", ":no-motion")]
public sealed class MdMenuAnchor : ContentControl, IMdPopupOwner, IMdPopupPresenceOwner
{
    public static readonly StyledProperty<object?> MenuProperty =
        AvaloniaProperty.Register<MdMenuAnchor, object?>(nameof(Menu));
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<MdMenuAnchor, bool>(nameof(IsOpen),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<PlacementMode> PlacementProperty =
        AvaloniaProperty.Register<MdMenuAnchor, PlacementMode>(nameof(Placement), PlacementMode.BottomEdgeAlignedLeft);
    public static readonly DirectProperty<MdMenuAnchor, bool> IsPopupOpenProperty =
        AvaloniaProperty.RegisterDirect<MdMenuAnchor, bool>(nameof(IsPopupOpen), anchor => anchor.IsPopupOpen);

    private readonly MdPresenceController _presence;
    private bool _isPopupOpen;
    private Popup? _popup;
    private Border? _surface;

    static MdMenuAnchor()
    {
        IsOpenProperty.Changed.AddClassHandler<MdMenuAnchor>((anchor, _) => anchor.UpdateState());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdMenuAnchor>((anchor, _) => anchor.UpdateMotion());
    }

    public MdMenuAnchor()
    {
        _presence = new MdPresenceController(SetPopupPresence);
        _presence.Initialize(IsOpen);
        UpdateState();
    }

    public object? Menu { get => GetValue(MenuProperty); set => SetValue(MenuProperty, value); }
    public bool IsOpen { get => GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }
    public PlacementMode Placement { get => GetValue(PlacementProperty); set => SetValue(PlacementProperty, value); }
    public bool IsPopupOpen
    {
        get => _isPopupOpen;
        private set => SetAndRaise(IsPopupOpenProperty, ref _isPopupOpen, value);
    }

    bool IMdPopupOwner.IsMaterialPopupOpen { get => IsOpen; set => SetCurrentValue(IsOpenProperty, value); }

    public void Show() => SetCurrentValue(IsOpenProperty, true);
    public void Dismiss() => SetCurrentValue(IsOpenProperty, false);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_popup is not null) _popup.Closed -= OnPopupClosed;
        base.OnApplyTemplate(e);
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        _surface = e.NameScope.Find<Border>("PART_MenuSurface");
        if (_popup is not null) _popup.Closed += OnPopupClosed;
        UpdateMotion();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateState();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _presence.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            SetCurrentValue(IsOpenProperty, !IsOpen);
            e.Handled = true;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Space)
        {
            SetCurrentValue(IsOpenProperty, !IsOpen);
            e.Handled = true;
            return;
        }
        if (e.Key == Key.Escape && IsOpen)
        {
            Dismiss();
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        // Native light-dismiss closes the popup before managed exit motion can be retained.
        // Commit that state immediately so one-way presence binding cannot reopen it.
        if (IsOpen) SetCurrentValue(IsOpenProperty, false);
        _presence.Initialize(false);
    }

    void IMdPopupPresenceOwner.ClosePopupImmediately() => _presence.Initialize(false);

    private void SetPopupPresence(bool value)
    {
        IsPopupOpen = value;
        PseudoClasses.Set(":present", value);
    }

    private void UpdateState()
    {
        ConfigureSurfaceTransitions(IsOpen ? MdMotionSpeed.Slow : MdMotionSpeed.Fast);
        if (IsOpen)
        {
            _presence.Update(true, TimeSpan.Zero);
            PseudoClasses.Set(":open", true);
            PseudoClasses.Set(":closed", false);
            MdPopupCoordinator.NotifyStateChanged(this);
        }
        else
        {
            PseudoClasses.Set(":open", false);
            PseudoClasses.Set(":closed", true);
            _presence.Update(false, MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
        }
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        ConfigureSurfaceTransitions(IsOpen ? MdMotionSpeed.Slow : MdMotionSpeed.Fast);
        if (!IsOpen)
            _presence.Update(false, MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
    }

    private void ConfigureSurfaceTransitions(MdMotionSpeed speed)
    {
        if (_surface is null) return;
        _surface.Transitions = MdMotionTransitions.Collect(
            MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, speed),
            MdMotionTransitions.CreateTransform(this, RenderTransformProperty, speed));
    }
}
