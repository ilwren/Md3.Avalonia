using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>
/// Material 3 assist, filter, input and suggestion chips. Filter chips use the inherited
/// two-way IsChecked property; input chips can raise RemoveRequested and execute RemoveCommand.
/// </summary>
[PseudoClasses(":assist", ":filter", ":input", ":suggestion", ":has-leading-icon", ":has-trailing-icon", ":removable", ":reduced-motion", ":no-motion")]
public class MdChip : ToggleButton
{
    public static readonly StyledProperty<MdChipVariant> VariantProperty =
        AvaloniaProperty.Register<MdChip, MdChipVariant>(nameof(Variant));

    public static readonly StyledProperty<object?> LeadingIconProperty =
        AvaloniaProperty.Register<MdChip, object?>(nameof(LeadingIcon));

    public static readonly StyledProperty<object?> TrailingIconProperty =
        AvaloniaProperty.Register<MdChip, object?>(nameof(TrailingIcon));

    public static readonly StyledProperty<bool> IsRemovableProperty =
        AvaloniaProperty.Register<MdChip, bool>(nameof(IsRemovable));

    public static readonly StyledProperty<ICommand?> RemoveCommandProperty =
        AvaloniaProperty.Register<MdChip, ICommand?>(nameof(RemoveCommand));

    public static readonly StyledProperty<object?> RemoveCommandParameterProperty =
        AvaloniaProperty.Register<MdChip, object?>(nameof(RemoveCommandParameter));

    private Button? _removeButton;
    private Border? _container;
    private Border? _stateLayer;
    private Control? _leadingIcon;
    private Control? _selectedIcon;

    static MdChip()
    {
        VariantProperty.Changed.AddClassHandler<MdChip>((chip, _) => chip.UpdatePseudoClasses());
        LeadingIconProperty.Changed.AddClassHandler<MdChip>((chip, _) => chip.UpdatePseudoClasses());
        TrailingIconProperty.Changed.AddClassHandler<MdChip>((chip, _) => chip.UpdatePseudoClasses());
        IsRemovableProperty.Changed.AddClassHandler<MdChip>((chip, _) => chip.UpdatePseudoClasses());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdChip>((chip, _) => chip.UpdateMotion());
    }

    public MdChip() => UpdatePseudoClasses();

    public event EventHandler? RemoveRequested;

    public MdChipVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public object? LeadingIcon
    {
        get => GetValue(LeadingIconProperty);
        set => SetValue(LeadingIconProperty, value);
    }

    public object? TrailingIcon
    {
        get => GetValue(TrailingIconProperty);
        set => SetValue(TrailingIconProperty, value);
    }

    public bool IsRemovable
    {
        get => GetValue(IsRemovableProperty);
        set => SetValue(IsRemovableProperty, value);
    }

    public ICommand? RemoveCommand
    {
        get => GetValue(RemoveCommandProperty);
        set => SetValue(RemoveCommandProperty, value);
    }

    public object? RemoveCommandParameter
    {
        get => GetValue(RemoveCommandParameterProperty);
        set => SetValue(RemoveCommandParameterProperty, value);
    }

    protected override void OnClick()
    {
        var wasChecked = IsChecked;
        base.OnClick();
        if (Variant != MdChipVariant.Filter)
        {
            SetCurrentValue(IsCheckedProperty, wasChecked);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_removeButton is not null)
        {
            _removeButton.Click -= OnRemoveClick;
        }

        base.OnApplyTemplate(e);
        _removeButton = e.NameScope.Find<Button>("PART_RemoveButton");
        _container = e.NameScope.Find<Border>("PART_Container");
        _stateLayer = e.NameScope.Find<Border>("PART_StateLayer");
        _leadingIcon = e.NameScope.Find<Control>("PART_LeadingIcon");
        _selectedIcon = e.NameScope.Find<Control>("PART_SelectedIcon");
        if (_removeButton is not null)
        {
            _removeButton.Click += OnRemoveClick;
        }
        UpdateMotion();
    }

    private void OnRemoveClick(object? sender, RoutedEventArgs e)
    {
        var parameter = RemoveCommandParameter ?? DataContext;
        if (RemoveCommand?.CanExecute(parameter) == true)
        {
            RemoveCommand.Execute(parameter);
        }
        RemoveRequested?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        Transitions = MdMotionTransitions.Collect(
            MdMotionTransitions.CreateBrush(this, ForegroundProperty, MdMotionSpeed.Fast),
            MdMotionTransitions.CreateBrush(this, BackgroundProperty, MdMotionSpeed.Fast),
            MdMotionTransitions.CreateBrush(this, BorderBrushProperty, MdMotionSpeed.Fast));
        if (_container is not null)
        {
            _container.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateBrush(this, BackgroundProperty, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateBrush(this, BorderBrushProperty, MdMotionSpeed.Fast));
        }
        foreach (var control in new[] { _stateLayer, _leadingIcon, _selectedIcon })
        {
            if (control is not null)
                control.Transitions = MdMotionTransitions.Collect(
                    MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":assist", Variant == MdChipVariant.Assist);
        PseudoClasses.Set(":filter", Variant == MdChipVariant.Filter);
        PseudoClasses.Set(":input", Variant == MdChipVariant.Input);
        PseudoClasses.Set(":suggestion", Variant == MdChipVariant.Suggestion);
        PseudoClasses.Set(":has-leading-icon", LeadingIcon is not null);
        PseudoClasses.Set(":has-trailing-icon", TrailingIcon is not null);
        PseudoClasses.Set(":removable", IsRemovable || Variant == MdChipVariant.Input);
    }
}
