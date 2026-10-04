using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>Hosts a standard or modal Material navigation drawer beside application content.</summary>
[TemplatePart("PART_Scrim", typeof(Border))]
[TemplatePart("PART_Drawer", typeof(Border))]
[PseudoClasses(":open", ":closed", ":present", ":modal", ":standard", ":left", ":right", ":reduced-motion", ":no-motion")]
public sealed class MdNavigationDrawer : ContentControl
{
    public static readonly StyledProperty<object?> DrawerContentProperty =
        AvaloniaProperty.Register<MdNavigationDrawer, object?>(nameof(DrawerContent));
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<MdNavigationDrawer, bool>(nameof(IsOpen), true,
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<bool> IsModalProperty =
        AvaloniaProperty.Register<MdNavigationDrawer, bool>(nameof(IsModal));
    public static readonly StyledProperty<bool> DismissOnScrimClickProperty =
        AvaloniaProperty.Register<MdNavigationDrawer, bool>(nameof(DismissOnScrimClick), true);
    public static readonly StyledProperty<MdNavigationDrawerPlacement> PlacementProperty =
        AvaloniaProperty.Register<MdNavigationDrawer, MdNavigationDrawerPlacement>(nameof(Placement));
    public static readonly StyledProperty<double> DrawerWidthProperty =
        AvaloniaProperty.Register<MdNavigationDrawer, double>(nameof(DrawerWidth), 360);

    private readonly MdPresenceController _presence;
    private readonly MdModalFocusController _modalFocus;
    private readonly TranslateTransform _motionTransform = new();
    private Border? _scrim;
    private Border? _drawer;
    private ContentPresenter? _mainContent;

    static MdNavigationDrawer()
    {
        IsOpenProperty.Changed.AddClassHandler<MdNavigationDrawer>((drawer, _) => drawer.UpdateVisualState());
        IsModalProperty.Changed.AddClassHandler<MdNavigationDrawer>((drawer, _) => drawer.UpdateVisualState());
        PlacementProperty.Changed.AddClassHandler<MdNavigationDrawer>((drawer, _) => drawer.UpdateVisualState());
        DrawerWidthProperty.Changed.AddClassHandler<MdNavigationDrawer>((drawer, _) => drawer.UpdateTransformTarget());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdNavigationDrawer>((drawer, _) => drawer.UpdateMotion());
    }

    // Android has no Escape key. The system back gesture arrives as TopLevel.BackRequested and
    // has to dismiss this surface, or it is unreachable by the one gesture phone users rely on.
    private readonly MdBackScope _backScope;

    public MdNavigationDrawer()
    {
        _backScope = new MdBackScope(this, OnBackRequested);
        _modalFocus = new MdModalFocusController(this);
        _presence = new MdPresenceController(value => PseudoClasses.Set(":present", value));
        _presence.Initialize(IsOpen);
        UpdateVisualState();
    }

    public object? DrawerContent { get => GetValue(DrawerContentProperty); set => SetValue(DrawerContentProperty, value); }
    public bool IsOpen { get => GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }
    public bool IsModal { get => GetValue(IsModalProperty); set => SetValue(IsModalProperty, value); }
    public bool DismissOnScrimClick { get => GetValue(DismissOnScrimClickProperty); set => SetValue(DismissOnScrimClickProperty, value); }
    public MdNavigationDrawerPlacement Placement { get => GetValue(PlacementProperty); set => SetValue(PlacementProperty, value); }
    public double DrawerWidth { get => GetValue(DrawerWidthProperty); set => SetValue(DrawerWidthProperty, value); }

    public void Show() => SetCurrentValue(IsOpenProperty, true);
    public void Dismiss() => SetCurrentValue(IsOpenProperty, false);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_scrim is not null) _scrim.PointerPressed -= OnScrimPressed;
        base.OnApplyTemplate(e);
        _scrim = e.NameScope.Find<Border>("PART_Scrim");
        _drawer = e.NameScope.Find<Border>("PART_Drawer");
        _mainContent = e.NameScope.Find<ContentPresenter>("PART_MainContent");
        if (_scrim is not null) _scrim.PointerPressed += OnScrimPressed;
        if (_drawer is not null) _drawer.RenderTransform = _motionTransform;
        UpdateMotion();
        UpdateTransformTarget();
        UpdateModalFocus();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateVisualState();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _presence.Stop();
        _modalFocus.Deactivate();
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

    private void OnScrimPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DismissOnScrimClick && IsModal && ReferenceEquals(e.Source, _scrim))
        {
            Dismiss();
            e.Handled = true;
        }
    }

    private void UpdateVisualState()
    {
        _backScope.Update(IsModal && IsOpen);
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
            _presence.Update(false, MdMotion.GetExitDuration(this));
        }
        PseudoClasses.Set(":modal", IsModal);
        PseudoClasses.Set(":standard", !IsModal);
        PseudoClasses.Set(":left", Placement == MdNavigationDrawerPlacement.Left);
        PseudoClasses.Set(":right", Placement == MdNavigationDrawerPlacement.Right);
        UpdateTransformTarget();
        UpdateModalFocus();
    }

    private void UpdateModalFocus()
    {
        _modalFocus.Update(IsOpen && IsModal, _drawer, _mainContent);
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_drawer is not null)
        {
            _drawer.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects));
        }
        _motionTransform.Transitions = MdMotionTransitions.Collect(
            MdMotionTransitions.CreateDouble(this, TranslateTransform.XProperty, MdMotionKind.Spatial));
        if (_scrim is not null)
        {
            _scrim.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects));
        }
        if (!IsOpen) _presence.Update(false, MdMotion.GetExitDuration(this));
        UpdateTransformTarget();
    }

    private void UpdateTransformTarget()
    {
        if (IsOpen || MdMotion.GetScheme(this) == MdMotionScheme.Reduced)
        {
            _motionTransform.X = 0;
            return;
        }
        _motionTransform.X = Placement == MdNavigationDrawerPlacement.Right
            ? Math.Max(0, DrawerWidth)
            : -Math.Max(0, DrawerWidth);
    }

    private bool OnBackRequested()
    {
        if (!IsModal || !IsOpen) return false;
        Dismiss();
        return true;
    }
}
