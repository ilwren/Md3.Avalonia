using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>
/// A Material tab strip and selected-content host with native <see cref="TabControl"/> semantics.
/// Content is presented only by the required PART_SelectedContentHost; this avoids re-parenting the
/// selected visual through a second transition host when tabs are changed rapidly.
/// </summary>
[PseudoClasses(":scrollable", ":fixed", ":reduced-motion", ":no-motion")]
public sealed class MdTabView : TabControl
{
    public static readonly StyledProperty<bool> IsScrollableProperty =
        AvaloniaProperty.Register<MdTabView, bool>(nameof(IsScrollable));

    // Retained as a read-only compatibility property. The template intentionally uses
    // PART_SelectedContentHost directly rather than presenting this object a second time.
    public static readonly DirectProperty<MdTabView, object?> AnimatedSelectedContentProperty =
        AvaloniaProperty.RegisterDirect<MdTabView, object?>(nameof(AnimatedSelectedContent), view => view.AnimatedSelectedContent);

    private readonly TranslateTransform _contentTransform = new();
    private object? _animatedSelectedContent;
    private ContentPresenter? _selectedContentHost;
    private int _contentAnimationVersion;
    private int _lastSelectedIndex = -1;

    static MdTabView()
    {
        IsScrollableProperty.Changed.AddClassHandler<MdTabView>((view, _) => view.UpdatePseudoClasses());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdTabView>((view, _) => view.UpdateMotion());
    }

    public MdTabView()
    {
        UpdatePseudoClasses();
        SelectionChanged += OnSelectionChanged;
    }

    public bool IsScrollable
    {
        get => GetValue(IsScrollableProperty);
        set => SetValue(IsScrollableProperty, value);
    }

    public object? AnimatedSelectedContent => _animatedSelectedContent;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _selectedContentHost = e.NameScope.Find<ContentPresenter>("PART_SelectedContentHost");
        if (_selectedContentHost is not null) _selectedContentHost.RenderTransform = _contentTransform;
        _lastSelectedIndex = SelectedIndex;
        UpdateMotion();
        SetAndRaise(AnimatedSelectedContentProperty, ref _animatedSelectedContent, SelectedContent);
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        SetAndRaise(AnimatedSelectedContentProperty, ref _animatedSelectedContent, SelectedContent);
        if (_selectedContentHost is not { } host)
            return;

        // A single presenter receives a fade-through plus a short directional shared-axis move.
        // Reduced keeps only the fade; None commits synchronously. Versioning coalesces bursts.
        var direction = SelectedIndex >= _lastSelectedIndex ? 1 : -1;
        _lastSelectedIndex = SelectedIndex;
        var scheme = MdMotion.GetScheme(this);
        if (scheme == MdMotionScheme.None)
        {
            _contentTransform.X = 0;
            host.Opacity = 1;
            return;
        }

        var version = ++_contentAnimationVersion;
        host.Opacity = 0;
        _contentTransform.X = scheme == MdMotionScheme.Reduced ? 0 : direction * 16;
        Dispatcher.UIThread.Post(() =>
        {
            if (version == _contentAnimationVersion && ReferenceEquals(host, _selectedContentHost))
            {
                _contentTransform.X = 0;
                host.Opacity = 1;
            }
        }, DispatcherPriority.Render);
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":scrollable", IsScrollable);
        PseudoClasses.Set(":fixed", !IsScrollable);
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_selectedContentHost is not null)
        {
            _selectedContentHost.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects));
        }
        _contentTransform.Transitions = MdMotionTransitions.Collect(
            MdMotionTransitions.CreateDouble(this, TranslateTransform.XProperty, MdMotionKind.Spatial));
        if (scheme is MdMotionScheme.Reduced or MdMotionScheme.None) _contentTransform.X = 0;
    }
}
