using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Animation;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Md3.Avalonia.Motion;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Controls;

/// <summary>
/// An expandable Android Material 3 settings card that reveals nested settings items when expanded.
/// </summary>
[PseudoClasses(":expanded")]
public class MdSettingsExpander : ItemsControl
{
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<MdSettingsExpander, object?>(nameof(Header));

    public static readonly StyledProperty<object?> DescriptionProperty =
        AvaloniaProperty.Register<MdSettingsExpander, object?>(nameof(Description));

    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<MdSettingsExpander, object?>(nameof(Icon));

    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<MdSettingsExpander, bool>(nameof(IsExpanded), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    static MdSettingsExpander()
    {
        IsExpandedProperty.Changed.AddClassHandler<MdSettingsExpander>((expander, _) =>
        {
            expander.UpdatePseudoClasses();
            expander.UpdateExpansion();
        });
    }

    public MdSettingsExpander()
    {
        UpdatePseudoClasses();
    }

    public object? Header { get => GetValue(HeaderProperty); set => SetValue(HeaderProperty, value); }
    public object? Description { get => GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }
    public object? Icon { get => GetValue(IconProperty); set => SetValue(IconProperty, value); }
    public bool IsExpanded { get => GetValue(IsExpandedProperty); set => SetValue(IsExpandedProperty, value); }

    private Border? _contentArea;
    private int _expansionVersion;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _contentArea = e.NameScope.Find<Border>("PART_ContentArea");
        if (_contentArea is not null && !IsExpanded) _contentArea.IsVisible = false;
    }

    /// <summary>
    /// M3 expand/collapse: the content area grows/shrinks with the motion spring instead of
    /// popping, and the chevron rotation transitions in the template.
    /// </summary>
    private void UpdateExpansion()
    {
        if (_contentArea is null) return;
        var version = ++_expansionVersion;
        var spec = MdMotion.Resolve(MdMotion.GetScheme(this), MdMotionKind.Spatial, MdMotionSpeed.Default);
        if (!spec.IsEnabled)
        {
            _contentArea.Height = double.NaN;
            _contentArea.IsVisible = IsExpanded;
            return;
        }

        _contentArea.Transitions = new Transitions
        {
            new DoubleTransition { Property = HeightProperty, Duration = spec.Duration, Easing = spec.CreateEasing() }
        };

        if (IsExpanded)
        {
            _contentArea.IsVisible = true;
            _contentArea.Height = 0;
            _contentArea.Measure(new Size(Bounds.Width > 0 ? Bounds.Width : double.PositiveInfinity, double.PositiveInfinity));
            var target = _contentArea.DesiredSize.Height;
            Dispatcher.UIThread.Post(() =>
            {
                if (version != _expansionVersion) return;
                _contentArea.Height = target;
                // Release the explicit height once settled so later content changes reflow.
                var timer = new DispatcherTimer { Interval = spec.Duration + TimeSpan.FromMilliseconds(50) };
                timer.Tick += (_, _) =>
                {
                    timer.Stop();
                    if (version == _expansionVersion && IsExpanded) _contentArea.Height = double.NaN;
                };
                timer.Start();
            }, DispatcherPriority.Render);
        }
        else
        {
            var current = _contentArea.Bounds.Height > 0 ? _contentArea.Bounds.Height : _contentArea.DesiredSize.Height;
            _contentArea.Height = current;
            Dispatcher.UIThread.Post(() =>
            {
                if (version != _expansionVersion) return;
                _contentArea.Height = 0;
                var timer = new DispatcherTimer { Interval = spec.Duration + TimeSpan.FromMilliseconds(50) };
                timer.Tick += (_, _) =>
                {
                    timer.Stop();
                    if (version == _expansionVersion && !IsExpanded) _contentArea.IsVisible = false;
                };
                timer.Start();
            }, DispatcherPriority.Render);
        }
    }

    public void Toggle() => IsExpanded = !IsExpanded;

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":expanded", IsExpanded);
    }
}
