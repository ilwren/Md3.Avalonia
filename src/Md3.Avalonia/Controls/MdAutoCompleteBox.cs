using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>
/// A scoped Material exposed autocomplete field. Native AutoCompleteBox filtering, async
/// population, text completion, selection, keyboard and MVVM properties remain available.
/// </summary>
[PseudoClasses(":filled", ":outlined", ":md-error", ":has-label", ":has-supporting-text", ":popup-present", ":reduced-motion", ":no-motion")]
public sealed class MdAutoCompleteBox : AutoCompleteBox, IMdPopupOwner, IMdPopupPresenceOwner
{
    public static readonly StyledProperty<MdTextBoxVariant> VariantProperty =
        AvaloniaProperty.Register<MdAutoCompleteBox, MdTextBoxVariant>(nameof(Variant), MdTextBoxVariant.Outlined);
    public static readonly StyledProperty<object?> LabelProperty =
        AvaloniaProperty.Register<MdAutoCompleteBox, object?>(nameof(Label));
    public static readonly StyledProperty<object?> SupportingTextProperty =
        AvaloniaProperty.Register<MdAutoCompleteBox, object?>(nameof(SupportingText));
    public static readonly StyledProperty<bool> IsErrorProperty =
        AvaloniaProperty.Register<MdAutoCompleteBox, bool>(nameof(IsError));
    public static readonly DirectProperty<MdAutoCompleteBox, bool> IsPopupOpenProperty =
        AvaloniaProperty.RegisterDirect<MdAutoCompleteBox, bool>(nameof(IsPopupOpen), control => control.IsPopupOpen);

    private MdPresenceController? _popupPresence;
    private bool _isPopupOpen;
    private Popup? _popup;
    private Border? _suggestionsContainer;

    static MdAutoCompleteBox()
    {
        VariantProperty.Changed.AddClassHandler<MdAutoCompleteBox>((control, _) => control.UpdatePseudoClasses());
        LabelProperty.Changed.AddClassHandler<MdAutoCompleteBox>((control, _) => control.UpdatePseudoClasses());
        SupportingTextProperty.Changed.AddClassHandler<MdAutoCompleteBox>((control, _) => control.UpdatePseudoClasses());
        IsErrorProperty.Changed.AddClassHandler<MdAutoCompleteBox>((control, _) => control.UpdatePseudoClasses());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdAutoCompleteBox>((control, _) => control.UpdateMotion());
    }

    public MdAutoCompleteBox()
    {
        _popupPresence = new MdPresenceController(SetPopupPresence);
        _popupPresence.Initialize(IsDropDownOpen);
        UpdatePseudoClasses();
        UpdatePopupState();
    }

    public MdTextBoxVariant Variant { get => GetValue(VariantProperty); set => SetValue(VariantProperty, value); }
    public object? Label { get => GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public object? SupportingText { get => GetValue(SupportingTextProperty); set => SetValue(SupportingTextProperty, value); }
    public bool IsError { get => GetValue(IsErrorProperty); set => SetValue(IsErrorProperty, value); }
    public bool IsPopupOpen
    {
        get => _isPopupOpen;
        private set => SetAndRaise(IsPopupOpenProperty, ref _isPopupOpen, value);
    }

    bool IMdPopupOwner.IsMaterialPopupOpen
    {
        get => IsDropDownOpen;
        set => SetCurrentValue(IsDropDownOpenProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsDropDownOpenProperty && _popupPresence is not null)
        {
            UpdatePopupState();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_popup is not null) _popup.Closed -= OnPopupClosed;
        base.OnApplyTemplate(e);
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        _suggestionsContainer = e.NameScope.Find<Border>("PART_SuggestionsContainer");
        if (_popup is not null) _popup.Closed += OnPopupClosed;
        if (e.NameScope.Find<TextBox>("PART_TextBox") is { } input)
        {
            MdTextEditingContextMenu.Attach(input);
        }
        UpdateMotion();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _popupPresence?.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    void IMdPopupPresenceOwner.ClosePopupImmediately() => _popupPresence?.Initialize(false);

    private void UpdatePopupState()
    {
        if (_popupPresence is null) return;
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
        _popupPresence?.Initialize(false);
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        ConfigurePopupTransitions(IsDropDownOpen ? MdMotionSpeed.Slow : MdMotionSpeed.Fast);
        if (!IsDropDownOpen)
            _popupPresence?.Update(false,
                MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
    }

    private void ConfigurePopupTransitions(MdMotionSpeed speed)
    {
        if (_suggestionsContainer is null) return;
        _suggestionsContainer.Transitions = MdMotionTransitions.Collect(
            MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, speed),
            MdMotionTransitions.CreateTransform(this, RenderTransformProperty, speed));
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":filled", Variant == MdTextBoxVariant.Filled);
        PseudoClasses.Set(":outlined", Variant == MdTextBoxVariant.Outlined);
        PseudoClasses.Set(":md-error", IsError);
        PseudoClasses.Set(":has-label", Label is not null);
        PseudoClasses.Set(":has-supporting-text", SupportingText is not null);
    }
}
