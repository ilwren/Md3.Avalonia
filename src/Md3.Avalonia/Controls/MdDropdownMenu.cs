using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>A reusable Material dropdown surface whose native popup and custom surface share one lifetime.</summary>
[PseudoClasses(":opening", ":open", ":closing", ":closed", ":reduced-motion", ":no-motion")]
public sealed class MdDropdownMenu : ContentControl, IMdPopupOwner
{
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<MdDropdownMenu, bool>(nameof(IsOpen),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsPopupOpenProperty =
        AvaloniaProperty.Register<MdDropdownMenu, bool>(nameof(IsPopupOpen));

    public static readonly StyledProperty<Control?> PlacementTargetProperty =
        AvaloniaProperty.Register<MdDropdownMenu, Control?>(nameof(PlacementTarget));

    public static readonly StyledProperty<double> MinimumMenuWidthProperty =
        AvaloniaProperty.Register<MdDropdownMenu, double>(nameof(MinimumMenuWidth));

    private Popup? _popup;
    private Border? _surface;
    private bool _closingInternally;
    private int _openVersion;

    static MdDropdownMenu()
    {
        IsOpenProperty.Changed.AddClassHandler<MdDropdownMenu>((menu, _) => menu.UpdateOpenState());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdDropdownMenu>((menu, _) => menu.UpdateMotion());
    }

    public MdDropdownMenu() => SetPseudoState(closed: true);

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    bool IMdPopupOwner.IsMaterialPopupOpen
    {
        get => IsOpen;
        set => SetCurrentValue(IsOpenProperty, value);
    }

    public bool IsPopupOpen
    {
        get => GetValue(IsPopupOpenProperty);
        private set => SetCurrentValue(IsPopupOpenProperty, value);
    }

    public Control? PlacementTarget
    {
        get => GetValue(PlacementTargetProperty);
        set => SetValue(PlacementTargetProperty, value);
    }

    public double MinimumMenuWidth
    {
        get => GetValue(MinimumMenuWidthProperty);
        set => SetValue(MinimumMenuWidthProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_popup is not null)
            _popup.Closed -= OnPopupClosed;

        base.OnApplyTemplate(e);
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        _surface = e.NameScope.Find<Border>("PART_Surface");
        if (_popup is not null)
            _popup.Closed += OnPopupClosed;
        UpdateMotion();
    }

    private void UpdateOpenState()
    {
        var version = ++_openVersion;
        MdPopupCoordinator.NotifyStateChanged(this);
        if (IsOpen)
        {
            _closingInternally = false;
            ConfigureSurfaceTransitions(MdMotionSpeed.Slow);
            // Mount a transparent, fully styled surface first, then commit the open target on the
            // render queue. PopupRoot is transparent, so this cannot expose a white native shell.
            SetPseudoState(opening: true);
            IsPopupOpen = true;
            if (MdMotion.GetScheme(this) == MdMotionScheme.None)
            {
                SetPseudoState(open: true);
            }
            else
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (version == _openVersion && IsOpen && IsPopupOpen)
                        SetPseudoState(open: true);
                }, DispatcherPriority.Render);
            }
            return;
        }

        // A native popup is a separate top-level window. Keeping that window alive for a custom
        // fade/scale exit can expose a platform fallback rectangle behind the shrinking surface.
        // Close the host in the same UI turn; other in-tree overlays use MdPresenceController.
        ConfigureSurfaceTransitions(MdMotionSpeed.Fast);
        SetPseudoState(closed: true);
        if (!IsPopupOpen) return;
        _closingInternally = true;
        IsPopupOpen = false;
        _closingInternally = false;
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        ++_openVersion;
        if (!_closingInternally && IsOpen)
            SetCurrentValue(IsOpenProperty, false);
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        ConfigureSurfaceTransitions(IsOpen ? MdMotionSpeed.Slow : MdMotionSpeed.Fast);
    }

    private void ConfigureSurfaceTransitions(MdMotionSpeed speed)
    {
        if (_surface is null) return;
        _surface.Transitions = MdMotionTransitions.Collect(
            MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, speed),
            MdMotionTransitions.CreateTransform(this, RenderTransformProperty, speed));
    }

    private void SetPseudoState(bool opening = false, bool open = false, bool closing = false, bool closed = false)
    {
        PseudoClasses.Set(":opening", opening);
        PseudoClasses.Set(":open", open);
        PseudoClasses.Set(":closing", closing);
        PseudoClasses.Set(":closed", closed);
    }
}
