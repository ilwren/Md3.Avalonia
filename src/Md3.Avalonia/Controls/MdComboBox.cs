using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>
/// A Material Design 3 exposed dropdown menu built on Avalonia's native <see cref="ComboBox"/>
/// selection, keyboard navigation, popup and automation behavior.
/// </summary>
[PseudoClasses(
    ":filled", ":outlined", ":md-error", ":has-label", ":no-label", ":has-value", ":no-value",
    ":has-leading-icon", ":has-supporting-text", ":has-error-text", ":popup-present", ":reduced-motion", ":no-motion")]
public class MdComboBox : ComboBox, IMdPopupOwner, IMdPopupPresenceOwner
{

    public static readonly StyledProperty<MdTextBoxVariant> VariantProperty =
        AvaloniaProperty.Register<MdComboBox, MdTextBoxVariant>(
            nameof(Variant),
            MdTextBoxVariant.Filled);

    public static readonly StyledProperty<object?> LabelProperty =
        AvaloniaProperty.Register<MdComboBox, object?>(nameof(Label));

    public static readonly StyledProperty<object?> SupportingTextProperty =
        AvaloniaProperty.Register<MdComboBox, object?>(nameof(SupportingText));

    public static readonly StyledProperty<object?> ErrorTextProperty =
        AvaloniaProperty.Register<MdComboBox, object?>(nameof(ErrorText));

    public static readonly StyledProperty<object?> LeadingIconProperty =
        AvaloniaProperty.Register<MdComboBox, object?>(nameof(LeadingIcon));

    public static readonly StyledProperty<bool> IsErrorProperty =
        AvaloniaProperty.Register<MdComboBox, bool>(nameof(IsError));

    public static readonly StyledProperty<IBrush?> LabelBrushProperty =
        AvaloniaProperty.Register<MdComboBox, IBrush?>(nameof(LabelBrush));

    public static readonly StyledProperty<IBrush?> SupportingTextBrushProperty =
        AvaloniaProperty.Register<MdComboBox, IBrush?>(nameof(SupportingTextBrush));

    public static readonly StyledProperty<IBrush?> LeadingIconBrushProperty =
        AvaloniaProperty.Register<MdComboBox, IBrush?>(nameof(LeadingIconBrush));

    public static readonly StyledProperty<IBrush?> ActiveIndicatorBrushProperty =
        AvaloniaProperty.Register<MdComboBox, IBrush?>(nameof(ActiveIndicatorBrush));

    public static readonly DirectProperty<MdComboBox, bool> IsPopupOpenProperty =
        AvaloniaProperty.RegisterDirect<MdComboBox, bool>(nameof(IsPopupOpen), comboBox => comboBox.IsPopupOpen);

    private readonly MdPresenceController _popupPresence;
    private bool _isPopupOpen;
    private Popup? _popup;
    private Border? _menuSurface;
    private ContentPresenter? _labelPresenter;
    private Grid? _inputRow;
    private Grid? _dropDownIcon;

    static MdComboBox()
    {
        VariantProperty.Changed.AddClassHandler<MdComboBox>((comboBox, _) => comboBox.UpdatePseudoClasses());
        LabelProperty.Changed.AddClassHandler<MdComboBox>((comboBox, _) => comboBox.UpdatePseudoClasses());
        SupportingTextProperty.Changed.AddClassHandler<MdComboBox>((comboBox, _) => comboBox.UpdatePseudoClasses());
        ErrorTextProperty.Changed.AddClassHandler<MdComboBox>((comboBox, _) => comboBox.UpdatePseudoClasses());
        LeadingIconProperty.Changed.AddClassHandler<MdComboBox>((comboBox, _) => comboBox.UpdatePseudoClasses());
        IsErrorProperty.Changed.AddClassHandler<MdComboBox>((comboBox, _) => comboBox.UpdatePseudoClasses());
        SelectedIndexProperty.Changed.AddClassHandler<MdComboBox>((comboBox, _) => comboBox.UpdateValuePseudoClass());
        TextProperty.Changed.AddClassHandler<MdComboBox>((comboBox, _) => comboBox.UpdateValuePseudoClass());
        IsDropDownOpenProperty.Changed.AddClassHandler<MdComboBox>((comboBox, _) =>
            comboBox.UpdatePopupState());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdComboBox>((comboBox, _) => comboBox.UpdateMotion());
    }

    public MdComboBox()
    {
        _popupPresence = new MdPresenceController(SetPopupPresence);
        _popupPresence.Initialize(IsDropDownOpen);
        UpdatePseudoClasses();
        UpdatePopupState();
    }

    /// <summary>Gets or sets the filled or outlined exposed-dropdown appearance.</summary>
    public MdTextBoxVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>Gets or sets the field label.</summary>
    public object? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Gets or sets supporting content shown below the field.</summary>
    public object? SupportingText
    {
        get => GetValue(SupportingTextProperty);
        set => SetValue(SupportingTextProperty, value);
    }

    /// <summary>Gets or sets error content shown below the field while an error state is active.</summary>
    public object? ErrorText
    {
        get => GetValue(ErrorTextProperty);
        set => SetValue(ErrorTextProperty, value);
    }

    /// <summary>Gets or sets optional leading icon content.</summary>
    public object? LeadingIcon
    {
        get => GetValue(LeadingIconProperty);
        set => SetValue(LeadingIconProperty, value);
    }

    /// <summary>Gets or sets an explicit Material error state independent of binding validation.</summary>
    public bool IsError
    {
        get => GetValue(IsErrorProperty);
        set => SetValue(IsErrorProperty, value);
    }

    public IBrush? LabelBrush
    {
        get => GetValue(LabelBrushProperty);
        set => SetValue(LabelBrushProperty, value);
    }

    public IBrush? SupportingTextBrush
    {
        get => GetValue(SupportingTextBrushProperty);
        set => SetValue(SupportingTextBrushProperty, value);
    }

    public IBrush? LeadingIconBrush
    {
        get => GetValue(LeadingIconBrushProperty);
        set => SetValue(LeadingIconBrushProperty, value);
    }

    public IBrush? ActiveIndicatorBrush
    {
        get => GetValue(ActiveIndicatorBrushProperty);
        set => SetValue(ActiveIndicatorBrushProperty, value);
    }

    /// <summary>The actual popup-host lifetime, which may outlive IsDropDownOpen for exit motion.</summary>
    public bool IsPopupOpen
    {
        get => _isPopupOpen;
        private set => SetAndRaise(IsPopupOpenProperty, ref _isPopupOpen, value);
    }

    private void UpdatePseudoClasses()
    {
        var outlined = Variant == MdTextBoxVariant.Outlined;
        PseudoClasses.Set(":filled", !outlined);
        PseudoClasses.Set(":outlined", outlined);
        PseudoClasses.Set(":md-error", IsError);
        PseudoClasses.Set(":has-label", Label is not null);
        PseudoClasses.Set(":no-label", Label is null);
        PseudoClasses.Set(":has-leading-icon", LeadingIcon is not null);
        PseudoClasses.Set(":has-supporting-text", SupportingText is not null);
        PseudoClasses.Set(":has-error-text", ErrorText is not null);
        UpdateValuePseudoClass();
    }

    private void UpdateValuePseudoClass()
    {
        var hasValue = SelectedIndex >= 0 || !string.IsNullOrEmpty(Text);
        PseudoClasses.Set(":has-value", hasValue);
        PseudoClasses.Set(":no-value", !hasValue);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_popup is not null) _popup.Closed -= OnPopupClosed;
        base.OnApplyTemplate(e);
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        _menuSurface = e.NameScope.Find<Border>("PART_MenuSurface");
        _labelPresenter = e.NameScope.Find<ContentPresenter>("PART_Label");
        _inputRow = e.NameScope.Find<Grid>("PART_InputRow");
        _dropDownIcon = e.NameScope.Find<Grid>("PART_DropDownIcon");
        if (_popup is not null) _popup.Closed += OnPopupClosed;
        UpdateMotion();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _popupPresence.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        // Apply the option theme at the owner/container-generation boundary. Popup-local
        // descendant styles are too late and too fragile to own the container's first measure.
        if (container is ComboBoxItem option && option.Theme is null &&
            ResourceNodeExtensions.FindResource(this, "MdComboBoxItemTheme") is ControlTheme theme)
        {
            option.SetCurrentValue(ThemeProperty, theme);
        }
    }

    bool IMdPopupOwner.IsMaterialPopupOpen
    {
        get => IsDropDownOpen;
        set => SetCurrentValue(IsDropDownOpenProperty, value);
    }

    void IMdPopupPresenceOwner.ClosePopupImmediately() => _popupPresence.Initialize(false);

    private void UpdatePopupState()
    {
        ConfigurePopupTransitions(IsDropDownOpen ? MdMotionSpeed.Slow : MdMotionSpeed.Fast);
        if (IsDropDownOpen)
        {
            _popupPresence.Update(true, TimeSpan.Zero);
            MdPopupCoordinator.NotifyStateChanged(this);
        }
        else
        {
            _popupPresence.Update(false,
                MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
            MdPopupCoordinator.NotifyStateChanged(this);
        }
    }

    private void SetPopupPresence(bool value)
    {
        IsPopupOpen = value;
        PseudoClasses.Set(":popup-present", value);
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        if (IsDropDownOpen) SetCurrentValue(IsDropDownOpenProperty, false);
        _popupPresence.Initialize(false);
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_labelPresenter is not null)
            _labelPresenter.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateBrush(this, ForegroundProperty, MdMotionSpeed.Fast));
        if (_inputRow is not null)
            _inputRow.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast));
        if (_dropDownIcon is not null)
            _dropDownIcon.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast));
        ConfigurePopupTransitions(IsDropDownOpen ? MdMotionSpeed.Slow : MdMotionSpeed.Fast);
        if (!IsDropDownOpen)
            _popupPresence.Update(false,
                MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
    }

    private void ConfigurePopupTransitions(MdMotionSpeed speed)
    {
        if (_menuSurface is null) return;
        _menuSurface.Transitions = MdMotionTransitions.Collect(
            MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, speed),
            MdMotionTransitions.CreateTransform(this, RenderTransformProperty, speed));
    }
}
