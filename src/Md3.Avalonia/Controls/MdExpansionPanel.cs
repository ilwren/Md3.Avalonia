using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>A Flutter-style Material expansion panel with two-way expanded state.</summary>
[PseudoClasses(":expanded", ":collapsed", ":content-present", ":header-tappable", ":reduced-motion", ":no-motion")]
public sealed class MdExpansionPanel : HeaderedContentControl
{
    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<MdExpansionPanel, bool>(nameof(IsExpanded),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<bool> CanTapOnHeaderProperty =
        AvaloniaProperty.Register<MdExpansionPanel, bool>(nameof(CanTapOnHeader), true);
    public static readonly StyledProperty<object?> LeadingContentProperty =
        AvaloniaProperty.Register<MdExpansionPanel, object?>(nameof(LeadingContent));

    private Button? _headerButton;
    private ContentPresenter? _contentPresenter;
    private Control? _expandIcon;
    private readonly MdPresenceController _presence;

    static MdExpansionPanel()
    {
        IsExpandedProperty.Changed.AddClassHandler<MdExpansionPanel>((panel, _) => panel.OnExpandedChanged());
        CanTapOnHeaderProperty.Changed.AddClassHandler<MdExpansionPanel>((panel, _) => panel.UpdatePseudoClasses());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdExpansionPanel>((panel, _) => panel.UpdateMotion());
    }

    public MdExpansionPanel()
    {
        _presence = new MdPresenceController(present =>
        {
            PseudoClasses.Set(":content-present", present);
            UpdateContentHitTesting();
        });
        _presence.Initialize(IsExpanded);
        UpdatePseudoClasses();
    }

    public bool IsExpanded { get => GetValue(IsExpandedProperty); set => SetValue(IsExpandedProperty, value); }
    public bool CanTapOnHeader { get => GetValue(CanTapOnHeaderProperty); set => SetValue(CanTapOnHeaderProperty, value); }
    public object? LeadingContent { get => GetValue(LeadingContentProperty); set => SetValue(LeadingContentProperty, value); }

    public event EventHandler<bool>? ExpandedChanged;

    public void Expand() => SetCurrentValue(IsExpandedProperty, true);
    public void Collapse() => SetCurrentValue(IsExpandedProperty, false);
    public void Toggle() => SetCurrentValue(IsExpandedProperty, !IsExpanded);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_headerButton is not null) _headerButton.Click -= OnHeaderClick;
        base.OnApplyTemplate(e);
        _headerButton = e.NameScope.Find<Button>("PART_HeaderButton");
        _contentPresenter = e.NameScope.Find<ContentPresenter>("PART_ExpandedContent");
        _expandIcon = e.NameScope.Find<Control>("PART_ExpandIcon");
        if (_headerButton is not null) _headerButton.Click += OnHeaderClick;
        _presence.Initialize(IsExpanded);
        UpdateMotion();
        UpdateContentHitTesting();
    }

    private void OnHeaderClick(object? sender, RoutedEventArgs e)
    {
        if (CanTapOnHeader) Toggle();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _presence.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    private void OnExpandedChanged()
    {
        UpdatePseudoClasses();
        _presence.Update(IsExpanded, MdMotion.GetExitDuration(this));
        UpdateContentHitTesting();
        if (IsExpanded)
            this.GetLogicalAncestors().OfType<MdExpansionPanelList>().FirstOrDefault()?.NotifyExpanded(this);
        ExpandedChanged?.Invoke(this, IsExpanded);
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_contentPresenter is not null)
        {
            _contentPresenter.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects),
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty));
        }
        if (_expandIcon is not null)
        {
            _expandIcon.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast));
        }
        if (!IsExpanded) _presence.Update(false, MdMotion.GetExitDuration(this));
    }

    private void UpdateContentHitTesting()
    {
        if (_contentPresenter is not null) _contentPresenter.IsHitTestVisible = IsExpanded;
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":expanded", IsExpanded);
        PseudoClasses.Set(":collapsed", !IsExpanded);
        PseudoClasses.Set(":header-tappable", CanTapOnHeader);
    }
}
