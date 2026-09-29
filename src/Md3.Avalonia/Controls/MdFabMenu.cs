using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>An M3 FAB menu for two to six related actions with reversible expand/collapse motion.</summary>
[PseudoClasses(":opening", ":open", ":closing", ":closed", ":primary", ":secondary", ":tertiary", ":reduced-motion", ":no-motion")]
public class MdFabMenu : ItemsControl
{
    private ItemsPresenter? _menuItems;
    public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<MdFabMenu, bool>(
        nameof(IsOpen), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<bool> AreItemsVisibleProperty =
        AvaloniaProperty.Register<MdFabMenu, bool>(nameof(AreItemsVisible));
    public static readonly StyledProperty<MdFabColor> ColorStyleProperty =
        AvaloniaProperty.Register<MdFabMenu, MdFabColor>(nameof(ColorStyle), MdFabColor.PrimaryContainer);
    public static readonly StyledProperty<object?> OpenIconProperty =
        AvaloniaProperty.Register<MdFabMenu, object?>(nameof(OpenIcon));
    public static readonly StyledProperty<object?> CloseIconProperty =
        AvaloniaProperty.Register<MdFabMenu, object?>(nameof(CloseIcon));

    private readonly DispatcherTimer _closeTimer;
    private CancellationTokenSource? _openFrameCancellation;
    private bool _isAttached;

    static MdFabMenu()
    {
        IsOpenProperty.Changed.AddClassHandler<MdFabMenu>((menu, _) => menu.UpdateOpenState());
        ColorStyleProperty.Changed.AddClassHandler<MdFabMenu>((menu, _) => menu.UpdateColorPseudoClasses());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdFabMenu>((menu, _) => menu.UpdateMotion());
    }

    public MdFabMenu()
    {
        _closeTimer = new DispatcherTimer();
        _closeTimer.Tick += (_, _) => FinishClosing();
        UpdateColorPseudoClasses();
        SetMotionPseudoClass(closed: true);
        LayoutUpdated += (_, _) => UpdateMenuItemGeometry();
    }

    public bool IsOpen { get => GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }
    public bool AreItemsVisible { get => GetValue(AreItemsVisibleProperty); private set => SetCurrentValue(AreItemsVisibleProperty, value); }
    public MdFabColor ColorStyle { get => GetValue(ColorStyleProperty); set => SetValue(ColorStyleProperty, value); }
    public object? OpenIcon { get => GetValue(OpenIconProperty); set => SetValue(OpenIconProperty, value); }
    public object? CloseIcon { get => GetValue(CloseIconProperty); set => SetValue(CloseIconProperty, value); }

    public void Show() => SetCurrentValue(IsOpenProperty, true);
    public void Dismiss() => SetCurrentValue(IsOpenProperty, false);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _menuItems = e.NameScope.Find<ItemsPresenter>("PART_MenuItems");
        UpdateMotion();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        UpdateOpenState();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttached = false;
        _openFrameCancellation?.Cancel();
        _openFrameCancellation?.Dispose();
        _openFrameCancellation = null;
        _closeTimer.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && IsOpen)
        {
            Dismiss();
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    private void UpdateOpenState()
    {
        _closeTimer.Stop();
        _openFrameCancellation?.Cancel();
        _openFrameCancellation?.Dispose();
        _openFrameCancellation = null;
        if (IsOpen)
        {
            AreItemsVisible = true;
            SetMotionPseudoClass(opening: true);
            if (!_isAttached) return;

            var cancellation = _openFrameCancellation = new CancellationTokenSource();
            Dispatcher.UIThread.Post(() =>
            {
                if (!cancellation.IsCancellationRequested && _isAttached && IsOpen)
                    SetMotionPseudoClass(open: true);
                if (ReferenceEquals(_openFrameCancellation, cancellation))
                {
                    _openFrameCancellation.Dispose();
                    _openFrameCancellation = null;
                }
            }, DispatcherPriority.Loaded);
        }
        else if (AreItemsVisible)
        {
            SetMotionPseudoClass(closing: true);
            var exitDuration = MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast);
            if (exitDuration <= TimeSpan.Zero)
            {
                FinishClosing();
            }
            else
            {
                _closeTimer.Interval = exitDuration;
                _closeTimer.Start();
            }
        }
        else
        {
            SetMotionPseudoClass(closed: true);
        }
    }

    private void FinishClosing()
    {
        _closeTimer.Stop();
        AreItemsVisible = false;
        SetMotionPseudoClass(closed: true);
    }

    private void SetMotionPseudoClass(bool opening = false, bool open = false, bool closing = false, bool closed = false)
    {
        PseudoClasses.Set(":opening", opening);
        PseudoClasses.Set(":open", open);
        PseudoClasses.Set(":closing", closing);
        PseudoClasses.Set(":closed", closed);
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_menuItems is not null)
            _menuItems.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, MaxHeightProperty, MdMotionKind.Spatial, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast));
        if (!IsOpen && AreItemsVisible)
        {
            _closeTimer.Stop();
            var exitDuration = MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast);
            if (exitDuration <= TimeSpan.Zero) FinishClosing();
            else { _closeTimer.Interval = exitDuration; _closeTimer.Start(); }
        }
    }

    private void UpdateMenuItemGeometry()
    {
        foreach (var item in this.GetVisualDescendants().OfType<MdExtendedFloatingActionButton>())
        {
            item.SetCurrentValue(MdExtendedFloatingActionButton.ContainerHeightProperty, 56);
            item.SetCurrentValue(MdExtendedFloatingActionButton.ContainerCornerRadiusProperty, new CornerRadius(28));
            item.SetCurrentValue(MdExtendedFloatingActionButton.IconSizeProperty, 24);
            item.SetCurrentValue(MdExtendedFloatingActionButton.IconSpacingProperty, 8);
        }
    }

    private void UpdateColorPseudoClasses()
    {
        PseudoClasses.Set(":primary", ColorStyle is MdFabColor.PrimaryContainer or MdFabColor.Primary);
        PseudoClasses.Set(":secondary", ColorStyle is MdFabColor.SecondaryContainer or MdFabColor.Secondary);
        PseudoClasses.Set(":tertiary", ColorStyle is MdFabColor.TertiaryContainer or MdFabColor.Tertiary);
    }
}
