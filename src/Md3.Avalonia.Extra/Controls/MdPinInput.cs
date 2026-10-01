using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>Immutable visual state for one <see cref="MdPinInput"/> cell.</summary>
public sealed record MdPinCell(int Index, string DisplayText, bool IsFilled, bool IsActive, bool IsError, double Size, Thickness Margin);

/// <summary>
/// A fixed-length Material PIN/OTP input. One native text editor preserves paste, selection,
/// IME and Android soft-keyboard behavior while the value is projected into accessible cells.
/// </summary>
[PseudoClasses(":complete", ":md-error", ":obscured")]
public sealed class MdPinInput : TemplatedControl
{
    public static readonly StyledProperty<string?> CodeProperty =
        AvaloniaProperty.Register<MdPinInput, string?>(nameof(Code), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<int> LengthProperty =
        AvaloniaProperty.Register<MdPinInput, int>(nameof(Length), 6, validate: value => value is >= 1 and <= 16);

    public static readonly StyledProperty<bool> AcceptsOnlyDigitsProperty =
        AvaloniaProperty.Register<MdPinInput, bool>(nameof(AcceptsOnlyDigits), true);

    public static readonly StyledProperty<bool> IsObscuredProperty =
        AvaloniaProperty.Register<MdPinInput, bool>(nameof(IsObscured));

    public static readonly StyledProperty<char> MaskCharacterProperty =
        AvaloniaProperty.Register<MdPinInput, char>(nameof(MaskCharacter), '•');

    public static readonly StyledProperty<bool> IsErrorProperty =
        AvaloniaProperty.Register<MdPinInput, bool>(nameof(IsError));

    public static readonly StyledProperty<double> CellSizeProperty =
        AvaloniaProperty.Register<MdPinInput, double>(nameof(CellSize), 52, validate: value => value >= 40);

    public static readonly StyledProperty<double> CellSpacingProperty =
        AvaloniaProperty.Register<MdPinInput, double>(nameof(CellSpacing), 8, validate: value => value >= 0);

    public static readonly StyledProperty<ICommand?> CompletedCommandProperty =
        AvaloniaProperty.Register<MdPinInput, ICommand?>(nameof(CompletedCommand));

    public static readonly DirectProperty<MdPinInput, IReadOnlyList<MdPinCell>> CellsProperty =
        AvaloniaProperty.RegisterDirect<MdPinInput, IReadOnlyList<MdPinCell>>(nameof(Cells), control => control.Cells);

    private IReadOnlyList<MdPinCell> _cells = [];
    private TextBox? _input;
    private string? _lastCompletedCode;
    private bool _normalizing;
    private bool _synchronizingInput;
    private int _caretIndex;

    static MdPinInput()
    {
        CodeProperty.Changed.AddClassHandler<MdPinInput>((control, _) => control.OnCodeChanged());
        LengthProperty.Changed.AddClassHandler<MdPinInput>((control, _) => control.OnLengthChanged());
        AcceptsOnlyDigitsProperty.Changed.AddClassHandler<MdPinInput>((control, _) => control.OnCodeChanged());
        IsObscuredProperty.Changed.AddClassHandler<MdPinInput>((control, _) => control.RefreshCells());
        MaskCharacterProperty.Changed.AddClassHandler<MdPinInput>((control, _) => control.RefreshCells());
        IsErrorProperty.Changed.AddClassHandler<MdPinInput>((control, _) => control.UpdatePseudoClasses());
        CellSizeProperty.Changed.AddClassHandler<MdPinInput>((control, _) => control.RefreshCells());
        CellSpacingProperty.Changed.AddClassHandler<MdPinInput>((control, _) => control.RefreshCells());
    }

    public MdPinInput()
    {
        Focusable = true;
        AutomationProperties.SetName(this, "Verification code");
        AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        GotFocus += (_, _) =>
        {
            RefreshCells();
            if (_input is not null && !_input.IsKeyboardFocusWithin)
            {
                _input.Focus();
                _caretIndex = _input.Text?.Length ?? 0;
                _input.CaretIndex = _caretIndex;
            }
        };
        LostFocus += (_, _) => RefreshCells();
        RefreshCells();
    }

    /// <summary>Gets or sets the current sanitized code. Binding is two-way by default.</summary>
    public string? Code { get => GetValue(CodeProperty); set => SetValue(CodeProperty, value); }

    /// <summary>Gets or sets the required code length, from one to sixteen cells.</summary>
    public int Length { get => GetValue(LengthProperty); set => SetValue(LengthProperty, value); }

    public bool AcceptsOnlyDigits { get => GetValue(AcceptsOnlyDigitsProperty); set => SetValue(AcceptsOnlyDigitsProperty, value); }
    public bool IsObscured { get => GetValue(IsObscuredProperty); set => SetValue(IsObscuredProperty, value); }
    public char MaskCharacter { get => GetValue(MaskCharacterProperty); set => SetValue(MaskCharacterProperty, value); }
    public bool IsError { get => GetValue(IsErrorProperty); set => SetValue(IsErrorProperty, value); }
    public double CellSize { get => GetValue(CellSizeProperty); set => SetValue(CellSizeProperty, value); }
    public double CellSpacing { get => GetValue(CellSpacingProperty); set => SetValue(CellSpacingProperty, value); }
    public ICommand? CompletedCommand { get => GetValue(CompletedCommandProperty); set => SetValue(CompletedCommandProperty, value); }
    public IReadOnlyList<MdPinCell> Cells { get => _cells; private set => SetAndRaise(CellsProperty, ref _cells, value); }

    /// <summary>Raised once whenever a newly completed code reaches <see cref="Length"/>.</summary>
    public event EventHandler<string>? Completed;

    /// <summary>Clears the code and returns keyboard focus to the native editor.</summary>
    public void Clear()
    {
        SetCurrentValue(CodeProperty, string.Empty);
        _input?.Focus();
    }

    /// <summary>Sets a value through the same sanitization path used by typing and paste.</summary>
    public void SetCode(string? code) => SetCurrentValue(CodeProperty, code);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_input is not null)
        {
            _input.TextChanged -= OnInputTextChanged;
            _input.PropertyChanged -= OnInputPropertyChanged;
        }
        base.OnApplyTemplate(e);
        _input = e.NameScope.Find<TextBox>("PART_Input");
        if (_input is not null)
        {
            AutomationProperties.SetName(_input, AutomationProperties.GetName(this) ?? "Verification code");
            _input.MaxLength = Length;
            _synchronizingInput = true;
            _input.Text = Normalize(Code);
            _synchronizingInput = false;
            _caretIndex = _input.Text?.Length ?? 0;
            _input.CaretIndex = _caretIndex;
            _input.TextChanged += OnInputTextChanged;
            _input.PropertyChanged += OnInputPropertyChanged;
        }
        RefreshCells();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsEnabled) return;
        var visual = e.Source as Visual;
        var cell = visual as MdPinCellPresenter ?? visual?.FindAncestorOfType<MdPinCellPresenter>();
        (_input as InputElement ?? this).Focus();
        if (_input is not null)
        {
            var hitIndex = cell?.Cell?.Index ?? Math.Clamp((int)(e.GetPosition(this).X / Math.Max(1, CellSize + CellSpacing)), 0, Length - 1);
            _caretIndex = Math.Clamp(hitIndex, 0, _input.Text?.Length ?? 0);
            _input.CaretIndex = _caretIndex;
            _input.SelectionStart = _caretIndex;
            _input.SelectionEnd = _caretIndex;
        }
        RefreshCells();
    }

    private void OnLengthChanged()
    {
        OnCodeChanged();
        if (_input is not null) _input.MaxLength = Length;
    }

    private void OnCodeChanged()
    {
        if (_normalizing) return;
        var raw = Code ?? string.Empty;
        var normalized = Normalize(raw);
        if (!string.Equals(raw, normalized, StringComparison.Ordinal))
        {
            _normalizing = true;
            SetCurrentValue(CodeProperty, normalized);
            _normalizing = false;
        }
        if (_input is not null && !_synchronizingInput && !string.Equals(_input.Text, normalized, StringComparison.Ordinal))
        {
            _synchronizingInput = true;
            _input.Text = normalized;
            _caretIndex = Math.Min(_caretIndex, normalized.Length);
            _input.CaretIndex = _caretIndex;
            _synchronizingInput = false;
        }

        RefreshCells(normalized);
        if (normalized.Length == Length)
        {
            if (!string.Equals(_lastCompletedCode, normalized, StringComparison.Ordinal))
            {
                _lastCompletedCode = normalized;
                if (CompletedCommand?.CanExecute(normalized) == true) CompletedCommand.Execute(normalized);
                Completed?.Invoke(this, normalized);
            }
        }
        else
        {
            _lastCompletedCode = null;
        }
    }

    private void OnInputTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_input is null || _synchronizingInput) return;
        var caret = _input.CaretIndex;
        var normalized = Normalize(_input.Text);
        if (!string.Equals(_input.Text, normalized, StringComparison.Ordinal))
        {
            _synchronizingInput = true;
            _input.Text = normalized;
            _caretIndex = Math.Clamp(caret, 0, normalized.Length);
            _input.CaretIndex = _caretIndex;
            _synchronizingInput = false;
        }
        else
        {
            // Do not write CaretIndex during ordinary typing. Native TextBox updates it as part of
            // the same input transaction; writing it back here can interrupt rapid OTP entry.
            _caretIndex = Math.Clamp(_input.CaretIndex, 0, normalized.Length);
        }
        SetCurrentValue(CodeProperty, normalized);
        RefreshCells(normalized);
    }

    private void OnInputPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (_input is null || e.Property != TextBox.CaretIndexProperty) return;
        _caretIndex = _input.CaretIndex;
        RefreshCells();
    }

    private string Normalize(string? value)
    {
        value ??= string.Empty;
        return AcceptsOnlyDigits
            ? new string(value.Where(char.IsDigit).Take(Length).ToArray())
            : new string(value.Where(c => !char.IsControl(c)).Take(Length).ToArray());
    }

    private void RefreshCells(string? value = null)
    {
        value ??= Code ?? string.Empty;
        var hasFocus = IsKeyboardFocusWithin || _input?.IsKeyboardFocusWithin == true;
        var half = CellSpacing / 2;
        var cells = new MdPinCell[Length];
        for (var index = 0; index < cells.Length; index++)
        {
            var filled = index < value.Length;
            var display = filled ? (IsObscured ? MaskCharacter.ToString() : value[index].ToString()) : string.Empty;
            cells[index] = new MdPinCell(index, display, filled, hasFocus && index == Math.Clamp(_caretIndex, 0, Length - 1), IsError, CellSize,
                new Thickness(index == 0 ? 0 : half, 0, index == Length - 1 ? 0 : half, 0));
        }
        Cells = cells;
        UpdatePseudoClasses();
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":complete", (Code?.Length ?? 0) == Length);
        PseudoClasses.Set(":md-error", IsError);
        PseudoClasses.Set(":obscured", IsObscured);
    }
}

/// <summary>Theme-owned presenter for one PIN cell.</summary>
[PseudoClasses(":filled", ":active", ":md-error")]
public sealed class MdPinCellPresenter : TemplatedControl
{
    public static readonly StyledProperty<MdPinCell?> CellProperty =
        AvaloniaProperty.Register<MdPinCellPresenter, MdPinCell?>(nameof(Cell));

    private Border? _cellBorder;

    static MdPinCellPresenter()
    {
        CellProperty.Changed.AddClassHandler<MdPinCellPresenter>((control, _) => control.UpdateState());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdPinCellPresenter>((control, _) => control.UpdateMotion());
    }

    public MdPinCellPresenter() => AutomationProperties.SetAccessibilityView(this, AccessibilityView.Raw);
    public MdPinCell? Cell { get => GetValue(CellProperty); set => SetValue(CellProperty, value); }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _cellBorder = e.NameScope.Find<Border>("PART_Cell");
        UpdateMotion();
    }

    private void UpdateMotion()
    {
        if (_cellBorder is not null)
            _cellBorder.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateBrush(this, Border.BackgroundProperty, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateBrush(this, Border.BorderBrushProperty, MdMotionSpeed.Fast));
    }

    private void UpdateState()
    {
        PseudoClasses.Set(":filled", Cell?.IsFilled == true);
        PseudoClasses.Set(":active", Cell?.IsActive == true);
        PseudoClasses.Set(":md-error", Cell?.IsError == true);
    }
}
