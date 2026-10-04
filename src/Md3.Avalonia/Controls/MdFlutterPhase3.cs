using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Md3.Avalonia.Localization;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

public enum MdAutovalidateMode { Disabled, OnUserInteraction, Always }

/// <summary>Coordinates validation and submission for descendant <see cref="MdFormField"/> controls.</summary>
[PseudoClasses(":valid", ":invalid")]
public sealed class MdForm : ContentControl
{
    public static readonly StyledProperty<MdAutovalidateMode> AutovalidateModeProperty = AvaloniaProperty.Register<MdForm, MdAutovalidateMode>(nameof(AutovalidateMode));
    public static readonly StyledProperty<ICommand?> SubmitCommandProperty = AvaloniaProperty.Register<MdForm, ICommand?>(nameof(SubmitCommand));
    public static readonly DirectProperty<MdForm, bool> IsValidProperty = AvaloniaProperty.RegisterDirect<MdForm, bool>(nameof(IsValid), form => form.IsValid);

    private bool _isValid = true;

    public MdForm() => UpdatePseudoClasses();
    public MdAutovalidateMode AutovalidateMode { get => GetValue(AutovalidateModeProperty); set => SetValue(AutovalidateModeProperty, value); }
    public ICommand? SubmitCommand { get => GetValue(SubmitCommandProperty); set => SetValue(SubmitCommandProperty, value); }
    public bool IsValid { get => _isValid; private set { SetAndRaise(IsValidProperty, ref _isValid, value); UpdatePseudoClasses(); } }

    public event EventHandler<MdFormSubmittedEventArgs>? Submitted;

    public bool Validate()
    {
        var valid = true;
        foreach (var field in GetFields()) valid &= field.Validate();
        IsValid = valid;
        return valid;
    }

    public void Reset()
    {
        foreach (var field in GetFields()) field.Reset();
        IsValid = true;
    }

    public bool Submit()
    {
        var valid = Validate();
        var args = new MdFormSubmittedEventArgs(valid);
        if (valid && SubmitCommand?.CanExecute(args) == true) SubmitCommand.Execute(args);
        Submitted?.Invoke(this, args);
        return valid;
    }

    internal void NotifyFieldChanged()
    {
        if (AutovalidateMode == MdAutovalidateMode.Disabled) return;
        var fields = AutovalidateMode == MdAutovalidateMode.Always
            ? GetFields()
            : GetFields().Where(field => field.IsTouched);
        var isValid = true;
        foreach (var field in fields) isValid &= field.Validate();
        IsValid = isValid;
    }

    private IEnumerable<MdFormField> GetFields() => this.GetLogicalDescendants().OfType<MdFormField>();
    private void UpdatePseudoClasses() { PseudoClasses.Set(":valid", IsValid); PseudoClasses.Set(":invalid", !IsValid); }
}

public sealed class MdFormSubmittedEventArgs(bool isValid) : EventArgs { public bool IsValid { get; } = isValid; }

/// <summary>A validation/decorator field. Set <see cref="Validator"/> for direct use or bind ErrorText from a view model.</summary>
[PseudoClasses(":valid", ":invalid", ":touched")]
public class MdFormField : ContentControl
{
    public static readonly StyledProperty<object?> ValueProperty = AvaloniaProperty.Register<MdFormField, object?>(nameof(Value), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<string?> ErrorTextProperty = AvaloniaProperty.Register<MdFormField, string?>(nameof(ErrorText));
    public static readonly StyledProperty<object?> SupportingTextProperty = AvaloniaProperty.Register<MdFormField, object?>(nameof(SupportingText));
    public static readonly StyledProperty<bool> IsRequiredProperty = AvaloniaProperty.Register<MdFormField, bool>(nameof(IsRequired));
    public static readonly StyledProperty<bool> IsTouchedProperty = AvaloniaProperty.Register<MdFormField, bool>(nameof(IsTouched), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly DirectProperty<MdFormField, bool> IsValidProperty = AvaloniaProperty.RegisterDirect<MdFormField, bool>(nameof(IsValid), field => field.IsValid);

    private object? _initialValue;
    private bool _isValid = true;
    private bool _capturedInitialValue;

    static MdFormField()
    {
        ValueProperty.Changed.AddClassHandler<MdFormField>((field, _) => field.OnValueChanged());
        ErrorTextProperty.Changed.AddClassHandler<MdFormField>((field, _) => field.UpdateValidity());
        SupportingTextProperty.Changed.AddClassHandler<MdFormField>((field, _) => field.UpdateAccessibility());
        IsRequiredProperty.Changed.AddClassHandler<MdFormField>((field, _) => field.UpdateAccessibility());
        IsTouchedProperty.Changed.AddClassHandler<MdFormField>((field, _) => field.UpdatePseudoClasses());
        MdLocalization.CultureProperty.Changed.AddClassHandler<MdFormField>((field, _) => field.UpdateAccessibility());
    }

    public MdFormField()
    {
        AutomationProperties.SetLiveSetting(this, AutomationLiveSetting.Assertive);
        UpdatePseudoClasses();
        UpdateAccessibility();
    }
    public object? Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public string? ErrorText { get => GetValue(ErrorTextProperty); set => SetValue(ErrorTextProperty, value); }
    public object? SupportingText { get => GetValue(SupportingTextProperty); set => SetValue(SupportingTextProperty, value); }
    public bool IsRequired { get => GetValue(IsRequiredProperty); set => SetValue(IsRequiredProperty, value); }
    public bool IsTouched { get => GetValue(IsTouchedProperty); set => SetValue(IsTouchedProperty, value); }
    public bool IsValid { get => _isValid; private set { SetAndRaise(IsValidProperty, ref _isValid, value); UpdatePseudoClasses(); } }
    public Func<object?, string?>? Validator { get; set; }

    public event EventHandler? ValueChanged;
    public event EventHandler? ValidationChanged;

    public bool Validate()
    {
        var error = Validator?.Invoke(Value);
        if (string.IsNullOrEmpty(error) && IsRequired && (Value is null || Value is string text && string.IsNullOrWhiteSpace(text)))
            error = MdLocalization.GetString("RequiredField", this);
        SetCurrentValue(ErrorTextProperty, error);
        UpdateValidity();
        return IsValid;
    }

    public void Reset()
    {
        if (_capturedInitialValue) SetCurrentValue(ValueProperty, _initialValue);
        SetCurrentValue(ErrorTextProperty, null);
        SetCurrentValue(IsTouchedProperty, false);
        IsValid = true;
    }

    public void MarkTouched()
    {
        SetCurrentValue(IsTouchedProperty, true);
        this.FindLogicalAncestorOfType<MdForm>()?.NotifyFieldChanged();
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        if (!_capturedInitialValue) { _initialValue = Value; _capturedInitialValue = true; }
        if (this.FindLogicalAncestorOfType<MdForm>()?.AutovalidateMode == MdAutovalidateMode.Always) Validate();
    }

    private void OnValueChanged()
    {
        if (!_capturedInitialValue) { _initialValue = Value; _capturedInitialValue = true; }
        ValueChanged?.Invoke(this, EventArgs.Empty);
        var form = this.FindLogicalAncestorOfType<MdForm>();
        if (form?.AutovalidateMode is MdAutovalidateMode.Always || form?.AutovalidateMode is MdAutovalidateMode.OnUserInteraction && IsTouched) Validate();
        form?.NotifyFieldChanged();
    }

    private void UpdateValidity()
    {
        var value = string.IsNullOrWhiteSpace(ErrorText);
        if (value != IsValid) { IsValid = value; ValidationChanged?.Invoke(this, EventArgs.Empty); }
        else UpdatePseudoClasses();
        UpdateAccessibility();
    }

    private void UpdateAccessibility()
    {
        var supporting = SupportingText switch
        {
            TextBlock textBlock => textBlock.Text,
            null => null,
            _ => SupportingText.ToString()
        };
        AutomationProperties.SetHelpText(this, !string.IsNullOrWhiteSpace(ErrorText)
            ? MdLocalization.Format("ErrorPrefix", this, ErrorText)
            : IsRequired
                ? string.Join(" ", new[] { supporting, MdLocalization.GetString("RequiredField", this) }.Where(value => !string.IsNullOrWhiteSpace(value)))
                : supporting);
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":valid", IsValid);
        PseudoClasses.Set(":invalid", !IsValid);
        PseudoClasses.Set(":touched", IsTouched);
    }
}

/// <summary>A Material dropdown combined with <see cref="MdFormField"/> validation semantics.</summary>
public sealed class MdDropdownFormField : MdFormField
{
    public static readonly StyledProperty<object?> LabelProperty = AvaloniaProperty.Register<MdDropdownFormField, object?>(nameof(Label));
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty = AvaloniaProperty.Register<MdDropdownFormField, IEnumerable?>(nameof(ItemsSource));
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty = AvaloniaProperty.Register<MdDropdownFormField, IDataTemplate?>(nameof(ItemTemplate));
    private MdComboBox? _editor;
    public object? Label { get => GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public IEnumerable? ItemsSource { get => GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
    public IDataTemplate? ItemTemplate { get => GetValue(ItemTemplateProperty); set => SetValue(ItemTemplateProperty, value); }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_editor is not null) _editor.SelectionChanged -= OnEditorSelectionChanged;
        base.OnApplyTemplate(e);
        _editor = e.NameScope.Find<MdComboBox>("PART_Editor");
        if (_editor is not null) _editor.SelectionChanged += OnEditorSelectionChanged;
    }

    private void OnEditorSelectionChanged(object? sender, SelectionChangedEventArgs e) => MarkTouched();
}

/// <summary>A compact option dialog surface with a selectable item list.</summary>
[PseudoClasses(":open", ":present", ":reduced-motion", ":no-motion")]
public sealed class MdSimpleDialog : TemplatedControl
{
    public static readonly StyledProperty<object?> TitleProperty = AvaloniaProperty.Register<MdSimpleDialog, object?>(nameof(Title));
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty = AvaloniaProperty.Register<MdSimpleDialog, IEnumerable?>(nameof(ItemsSource));
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty = AvaloniaProperty.Register<MdSimpleDialog, IDataTemplate?>(nameof(ItemTemplate));
    public static readonly StyledProperty<object?> SelectedItemProperty = AvaloniaProperty.Register<MdSimpleDialog, object?>(nameof(SelectedItem), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<MdSimpleDialog, bool>(nameof(IsOpen), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<ICommand?> SelectionCommandProperty = AvaloniaProperty.Register<MdSimpleDialog, ICommand?>(nameof(SelectionCommand));
    public static readonly StyledProperty<object?> CancelTextProperty = AvaloniaProperty.Register<MdSimpleDialog, object?>(nameof(CancelText), "Cancel");
    private SelectingItemsControl? _itemsHost;
    private Button? _cancelButton;
    private Grid? _overlay;
    private Border? _scrim;
    private Border? _surface;
    private readonly MdPresenceController _presence;
    private readonly MdModalFocusController _modalFocus;
    private object? _defaultCancelText = "Cancel";

    static MdSimpleDialog()
    {
        IsOpenProperty.Changed.AddClassHandler<MdSimpleDialog>((dialog, _) => dialog.UpdateOpenState());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdSimpleDialog>((dialog, _) => dialog.UpdateMotion());
        MdLocalization.CultureProperty.Changed.AddClassHandler<MdSimpleDialog>((dialog, _) => dialog.UpdateLocalizedText());
    }

    public MdSimpleDialog()
    {
        _modalFocus = new MdModalFocusController(this);
        _presence = new MdPresenceController(SetPresence);
        _presence.Initialize(IsOpen);
        PseudoClasses.Set(":open", IsOpen);
        UpdateLocalizedText();
    }

    public object? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public IEnumerable? ItemsSource { get => GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
    public IDataTemplate? ItemTemplate { get => GetValue(ItemTemplateProperty); set => SetValue(ItemTemplateProperty, value); }
    public object? SelectedItem { get => GetValue(SelectedItemProperty); set => SetValue(SelectedItemProperty, value); }
    public bool IsOpen { get => GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }
    public ICommand? SelectionCommand { get => GetValue(SelectionCommandProperty); set => SetValue(SelectionCommandProperty, value); }
    public object? CancelText { get => GetValue(CancelTextProperty); set => SetValue(CancelTextProperty, value); }
    public event EventHandler<object?>? ItemSelected;
    public event EventHandler? Dismissed;

    private void UpdateLocalizedText()
    {
        if (!Equals(CancelText, _defaultCancelText)) return;
        _defaultCancelText = MdLocalization.GetString("Cancel", this);
        SetCurrentValue(CancelTextProperty, _defaultCancelText);
    }

    public void Show() => SetCurrentValue(IsOpenProperty, true);

    /// <summary>Closes the dialog without a choice and raises <see cref="Dismissed"/>.</summary>
    public void Dismiss() => Close(notifyDismissed: true);

    // Choosing an item closes the dialog too, but a choice is not a dismissal. Raising both
    // events for one click forced every listener to handle two contradictory outcomes, and the
    // cancel branch -- running second -- overwrote the selection that had just been reported.
    private void Close(bool notifyDismissed)
    {
        var wasOpen = IsOpen;
        SetCurrentValue(IsOpenProperty, false);
        if (wasOpen && notifyDismissed) Dismissed?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_itemsHost is not null) _itemsHost.SelectionChanged -= OnSelectionChanged;
        if (_cancelButton is not null) _cancelButton.Click -= OnCancel;
        if (_scrim is not null) _scrim.PointerPressed -= OnScrimPressed;
        base.OnApplyTemplate(e);
        _overlay = e.NameScope.Find<Grid>("PART_Overlay");
        _itemsHost = e.NameScope.Find<SelectingItemsControl>("PART_ItemsHost");
        _cancelButton = e.NameScope.Find<Button>("PART_CancelButton");
        _scrim = e.NameScope.Find<Border>("PART_Scrim");
        _surface = e.NameScope.Find<Border>("PART_Surface");
        if (_itemsHost is not null) _itemsHost.SelectionChanged += OnSelectionChanged;
        if (_cancelButton is not null) _cancelButton.Click += OnCancel;
        if (_scrim is not null) _scrim.PointerPressed += OnScrimPressed;
        _presence.Initialize(IsOpen);
        SetPresence(_presence.IsPresent);
        UpdateMotion();
        UpdateHitTesting();
        UpdateModalFocus();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateModalFocus();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _presence.Stop();
        _modalFocus.Deactivate();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (IsOpen && e.Key == Key.Escape)
        {
            Dismiss();
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    private void UpdateOpenState()
    {
        if (IsOpen)
        {
            _presence.Update(true, TimeSpan.Zero);
            PseudoClasses.Set(":open", true);
        }
        else
        {
            PseudoClasses.Set(":open", false);
            _presence.Update(false, MdMotion.GetExitDuration(this));
        }
        UpdateHitTesting();
        UpdateModalFocus();
    }

    private void UpdateModalFocus()
    {
        _modalFocus.Update(IsOpen, _surface, initialFocus: _itemsHost as Control ?? _cancelButton);
    }

    private void OnScrimPressed(object? sender, PointerPressedEventArgs e)
    {
        if (IsOpen && ReferenceEquals(e.Source, _scrim))
        {
            Dismiss();
            e.Handled = true;
        }
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_surface is not null)
        {
            _surface.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects),
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty));
        }
        if (!IsOpen) _presence.Update(false, MdMotion.GetExitDuration(this));
    }

    private void SetPresence(bool present)
    {
        PseudoClasses.Set(":present", present);
        // Presence controls lifecycle, not just styling. Set the template part directly so a
        // zero-duration scheme cannot retain a stale :present style value for another layout pass.
        if (_overlay is not null) _overlay.IsVisible = present;
        if (_surface is not null) _surface.IsVisible = present;
    }

    private void UpdateHitTesting()
    {
        if (_surface is not null) _surface.IsHitTestVisible = IsOpen;
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => Dismiss();

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_itemsHost?.SelectedItem is not { } selected) return;
        SetCurrentValue(SelectedItemProperty, selected);
        if (SelectionCommand?.CanExecute(selected) == true) SelectionCommand.Execute(selected);
        ItemSelected?.Invoke(this, selected);
        Close(notifyDismissed: false);
    }
}

public sealed record MdLicenseEntry(string Package, string License, string Text, Uri? ProjectUrl = null);

/// <summary>A Material about surface with application metadata and a license-page hook.</summary>
public sealed class MdAboutDialog : ContentControl
{
    public static readonly StyledProperty<object?> ApplicationIconProperty = AvaloniaProperty.Register<MdAboutDialog, object?>(nameof(ApplicationIcon));
    public static readonly StyledProperty<string?> ApplicationNameProperty = AvaloniaProperty.Register<MdAboutDialog, string?>(nameof(ApplicationName));
    public static readonly StyledProperty<string?> ApplicationVersionProperty = AvaloniaProperty.Register<MdAboutDialog, string?>(nameof(ApplicationVersion));
    public static readonly StyledProperty<string?> LegaleseProperty = AvaloniaProperty.Register<MdAboutDialog, string?>(nameof(Legalese));
    public static readonly StyledProperty<IEnumerable<MdLicenseEntry>?> LicensesProperty = AvaloniaProperty.Register<MdAboutDialog, IEnumerable<MdLicenseEntry>?>(nameof(Licenses));
    public object? ApplicationIcon { get => GetValue(ApplicationIconProperty); set => SetValue(ApplicationIconProperty, value); }
    public string? ApplicationName { get => GetValue(ApplicationNameProperty); set => SetValue(ApplicationNameProperty, value); }
    public string? ApplicationVersion { get => GetValue(ApplicationVersionProperty); set => SetValue(ApplicationVersionProperty, value); }
    public string? Legalese { get => GetValue(LegaleseProperty); set => SetValue(LegaleseProperty, value); }
    public IEnumerable<MdLicenseEntry>? Licenses { get => GetValue(LicensesProperty); set => SetValue(LicensesProperty, value); }
    public event EventHandler? LicensesRequested;
    public void ShowLicenses() => LicensesRequested?.Invoke(this, EventArgs.Empty);
}

/// <summary>A searchable, selectable license list suitable for dialog or routed-page presentation.</summary>
[PseudoClasses(":compact")]
public sealed class MdLicensePage : TemplatedControl
{
    public static readonly StyledProperty<IEnumerable<MdLicenseEntry>?> LicensesProperty = AvaloniaProperty.Register<MdLicensePage, IEnumerable<MdLicenseEntry>?>(nameof(Licenses));
    public static readonly StyledProperty<string?> FilterProperty = AvaloniaProperty.Register<MdLicensePage, string?>(nameof(Filter));
    public static readonly StyledProperty<string?> FilterLabelProperty = AvaloniaProperty.Register<MdLicensePage, string?>(nameof(FilterLabel), "Filter packages");
    public static readonly StyledProperty<MdLicenseEntry?> SelectedLicenseProperty = AvaloniaProperty.Register<MdLicensePage, MdLicenseEntry?>(nameof(SelectedLicense), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly DirectProperty<MdLicensePage, IReadOnlyList<MdLicenseEntry>> FilteredLicensesProperty = AvaloniaProperty.RegisterDirect<MdLicensePage, IReadOnlyList<MdLicenseEntry>>(nameof(FilteredLicenses), page => page.FilteredLicenses);
    private IReadOnlyList<MdLicenseEntry> _filteredLicenses = Array.Empty<MdLicenseEntry>();

    static MdLicensePage()
    {
        LicensesProperty.Changed.AddClassHandler<MdLicensePage>((page, _) => page.Refresh());
        FilterProperty.Changed.AddClassHandler<MdLicensePage>((page, _) => page.Refresh());
        BoundsProperty.Changed.AddClassHandler<MdLicensePage>((page, _) => page.UpdateResponsiveState());
    }
    public MdLicensePage()
    {
        Refresh();
        UpdateResponsiveState();
    }
    public IEnumerable<MdLicenseEntry>? Licenses { get => GetValue(LicensesProperty); set => SetValue(LicensesProperty, value); }
    public string? Filter { get => GetValue(FilterProperty); set => SetValue(FilterProperty, value); }
    public string? FilterLabel { get => GetValue(FilterLabelProperty); set => SetValue(FilterLabelProperty, value); }
    public MdLicenseEntry? SelectedLicense { get => GetValue(SelectedLicenseProperty); set => SetValue(SelectedLicenseProperty, value); }
    public IReadOnlyList<MdLicenseEntry> FilteredLicenses => _filteredLicenses;
    private void UpdateResponsiveState() => PseudoClasses.Set(":compact", Bounds.Width > 0 && Bounds.Width < 640);
    private void Refresh()
    {
        var query = Filter?.Trim();
        var entries = Licenses ?? Array.Empty<MdLicenseEntry>();
        var value = entries.Where(entry => string.IsNullOrEmpty(query) || entry.Package.Contains(query, StringComparison.OrdinalIgnoreCase) || entry.License.Contains(query, StringComparison.OrdinalIgnoreCase)).ToArray();
        SetAndRaise(FilteredLicensesProperty, ref _filteredLicenses, value);
        if (SelectedLicense is null || !value.Contains(SelectedLicense)) SetCurrentValue(SelectedLicenseProperty, value.FirstOrDefault());
    }
}

/// <summary>Simple provider-agnostic state bag for restoring picker values by stable restoration id.</summary>
public sealed class MdPickerRestorationStore
{
    private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> Values => _values;
    public void SaveDate(string id, DateTimeOffset? value) { if (!string.IsNullOrWhiteSpace(id)) _values[id] = value?.ToString("O") ?? string.Empty; }
    public DateTimeOffset? RestoreDate(string id) => _values.TryGetValue(id, out var value) && DateTimeOffset.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out var result) ? result : null;
    public void SaveTime(string id, TimeSpan? value) { if (!string.IsNullOrWhiteSpace(id)) _values[id] = value?.ToString("c") ?? string.Empty; }
    public TimeSpan? RestoreTime(string id) => _values.TryGetValue(id, out var value) && TimeSpan.TryParse(value, out var result) ? result : null;
    public void Clear(string id) => _values.Remove(id);
}
