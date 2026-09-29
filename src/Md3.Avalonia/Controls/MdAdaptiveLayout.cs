using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Layout;

namespace Md3.Avalonia.Controls;

public enum MdAdaptiveBreakpoint { Compact, Medium, Expanded, Large, ExtraLarge }
public enum MdAdaptiveInputMode { Automatic, Touch, Pointer, Keyboard }

/// <summary>
/// Selects content from five Material width classes. Legacy three-mode APIs remain available;
/// Large/ExtraLarge fall back to Expanded content. Input and orientation decisions are observable.
/// </summary>
[PseudoClasses(":compact", ":medium", ":expanded", ":large", ":extra-large", ":portrait", ":landscape", ":touch", ":pointer", ":keyboard")]
public sealed class MdAdaptiveLayout : ContentControl
{
    public static readonly StyledProperty<object?> CompactContentProperty = AvaloniaProperty.Register<MdAdaptiveLayout, object?>(nameof(CompactContent));
    public static readonly StyledProperty<object?> MediumContentProperty = AvaloniaProperty.Register<MdAdaptiveLayout, object?>(nameof(MediumContent));
    public static readonly StyledProperty<object?> ExpandedContentProperty = AvaloniaProperty.Register<MdAdaptiveLayout, object?>(nameof(ExpandedContent));
    public static readonly StyledProperty<object?> LargeContentProperty = AvaloniaProperty.Register<MdAdaptiveLayout, object?>(nameof(LargeContent));
    public static readonly StyledProperty<object?> ExtraLargeContentProperty = AvaloniaProperty.Register<MdAdaptiveLayout, object?>(nameof(ExtraLargeContent));
    public static readonly StyledProperty<double> MediumBreakpointProperty = AvaloniaProperty.Register<MdAdaptiveLayout, double>(nameof(MediumBreakpoint), 600);
    public static readonly StyledProperty<double> ExpandedBreakpointProperty = AvaloniaProperty.Register<MdAdaptiveLayout, double>(nameof(ExpandedBreakpoint), 840);
    public static readonly StyledProperty<double> LargeBreakpointProperty = AvaloniaProperty.Register<MdAdaptiveLayout, double>(nameof(LargeBreakpoint), 1200);
    public static readonly StyledProperty<double> ExtraLargeBreakpointProperty = AvaloniaProperty.Register<MdAdaptiveLayout, double>(nameof(ExtraLargeBreakpoint), 1600);
    public static readonly StyledProperty<MdAdaptiveInputMode> InputModeProperty = AvaloniaProperty.Register<MdAdaptiveLayout, MdAdaptiveInputMode>(nameof(InputMode));
    public static readonly DirectProperty<MdAdaptiveLayout, MdAdaptiveLayoutMode> ModeProperty = AvaloniaProperty.RegisterDirect<MdAdaptiveLayout, MdAdaptiveLayoutMode>(nameof(Mode), control => control.Mode);
    public static readonly DirectProperty<MdAdaptiveLayout, MdAdaptiveBreakpoint> BreakpointProperty = AvaloniaProperty.RegisterDirect<MdAdaptiveLayout, MdAdaptiveBreakpoint>(nameof(Breakpoint), control => control.Breakpoint);
    public static readonly DirectProperty<MdAdaptiveLayout, string> BreakpointNameProperty = AvaloniaProperty.RegisterDirect<MdAdaptiveLayout, string>(nameof(BreakpointName), control => control.BreakpointName);
    public static readonly DirectProperty<MdAdaptiveLayout, Orientation> OrientationProperty = AvaloniaProperty.RegisterDirect<MdAdaptiveLayout, Orientation>(nameof(Orientation), control => control.Orientation);
    public static readonly DirectProperty<MdAdaptiveLayout, object?> ResolvedLargeContentProperty = AvaloniaProperty.RegisterDirect<MdAdaptiveLayout, object?>(nameof(ResolvedLargeContent), control => control.ResolvedLargeContent);
    public static readonly DirectProperty<MdAdaptiveLayout, object?> ResolvedExtraLargeContentProperty = AvaloniaProperty.RegisterDirect<MdAdaptiveLayout, object?>(nameof(ResolvedExtraLargeContent), control => control.ResolvedExtraLargeContent);

    private MdAdaptiveLayoutMode _mode;
    private MdAdaptiveBreakpoint _breakpoint;
    private string _breakpointName = "Compact";
    private Orientation _orientation = Orientation.Vertical;
    private object? _resolvedLargeContent;
    private object? _resolvedExtraLargeContent;
    private MdAdaptiveInputMode _lastInputMode = MdAdaptiveInputMode.Pointer;

    static MdAdaptiveLayout()
    {
        MediumBreakpointProperty.Changed.AddClassHandler<MdAdaptiveLayout>((control, _) => control.UpdateMode());
        ExpandedBreakpointProperty.Changed.AddClassHandler<MdAdaptiveLayout>((control, _) => control.UpdateMode());
        LargeBreakpointProperty.Changed.AddClassHandler<MdAdaptiveLayout>((control, _) => control.UpdateMode());
        ExtraLargeBreakpointProperty.Changed.AddClassHandler<MdAdaptiveLayout>((control, _) => control.UpdateMode());
        ExpandedContentProperty.Changed.AddClassHandler<MdAdaptiveLayout>((control, _) => control.UpdateResolvedContent());
        LargeContentProperty.Changed.AddClassHandler<MdAdaptiveLayout>((control, _) => control.UpdateResolvedContent());
        ExtraLargeContentProperty.Changed.AddClassHandler<MdAdaptiveLayout>((control, _) => control.UpdateResolvedContent());
        InputModeProperty.Changed.AddClassHandler<MdAdaptiveLayout>((control, _) => control.UpdatePseudoClasses());
    }

    public MdAdaptiveLayout()
    {
        SizeChanged += (_, _) => UpdateMode();
        AddHandler(PointerPressedEvent, OnAnyPointerPressed, global::Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
        AddHandler(KeyDownEvent, OnAnyKeyDown, global::Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
        UpdateResolvedContent();
        UpdateMode();
    }

    public object? CompactContent { get => GetValue(CompactContentProperty); set => SetValue(CompactContentProperty, value); }
    public object? MediumContent { get => GetValue(MediumContentProperty); set => SetValue(MediumContentProperty, value); }
    public object? ExpandedContent { get => GetValue(ExpandedContentProperty); set => SetValue(ExpandedContentProperty, value); }
    public object? LargeContent { get => GetValue(LargeContentProperty); set => SetValue(LargeContentProperty, value); }
    public object? ExtraLargeContent { get => GetValue(ExtraLargeContentProperty); set => SetValue(ExtraLargeContentProperty, value); }
    public double MediumBreakpoint { get => GetValue(MediumBreakpointProperty); set => SetValue(MediumBreakpointProperty, value); }
    public double ExpandedBreakpoint { get => GetValue(ExpandedBreakpointProperty); set => SetValue(ExpandedBreakpointProperty, value); }
    public double LargeBreakpoint { get => GetValue(LargeBreakpointProperty); set => SetValue(LargeBreakpointProperty, value); }
    public double ExtraLargeBreakpoint { get => GetValue(ExtraLargeBreakpointProperty); set => SetValue(ExtraLargeBreakpointProperty, value); }
    public MdAdaptiveInputMode InputMode { get => GetValue(InputModeProperty); set => SetValue(InputModeProperty, value); }
    public MdAdaptiveLayoutMode Mode { get => _mode; private set => SetAndRaise(ModeProperty, ref _mode, value); }
    public MdAdaptiveBreakpoint Breakpoint { get => _breakpoint; private set => SetAndRaise(BreakpointProperty, ref _breakpoint, value); }
    public string BreakpointName { get => _breakpointName; private set => SetAndRaise(BreakpointNameProperty, ref _breakpointName, value); }
    public Orientation Orientation { get => _orientation; private set => SetAndRaise(OrientationProperty, ref _orientation, value); }
    public object? ResolvedLargeContent => _resolvedLargeContent;
    public object? ResolvedExtraLargeContent => _resolvedExtraLargeContent;
    public MdAdaptiveInputMode EffectiveInputMode => InputMode == MdAdaptiveInputMode.Automatic ? _lastInputMode : InputMode;

    public event EventHandler<MdAdaptiveBreakpoint>? BreakpointChanged;

    private void UpdateResolvedContent()
    {
        var large = LargeContent ?? ExpandedContent;
        var extraLarge = ExtraLargeContent ?? large;
        SetAndRaise(ResolvedLargeContentProperty, ref _resolvedLargeContent, large);
        SetAndRaise(ResolvedExtraLargeContentProperty, ref _resolvedExtraLargeContent, extraLarge);
    }

    private void UpdateMode()
    {
        var width = Bounds.Width;
        var next = width >= ExtraLargeBreakpoint ? MdAdaptiveBreakpoint.ExtraLarge
            : width >= LargeBreakpoint ? MdAdaptiveBreakpoint.Large
            : width >= ExpandedBreakpoint ? MdAdaptiveBreakpoint.Expanded
            : width >= MediumBreakpoint ? MdAdaptiveBreakpoint.Medium
            : MdAdaptiveBreakpoint.Compact;
        if (next != Breakpoint)
        {
            Breakpoint = next;
            BreakpointName = next.ToString();
            BreakpointChanged?.Invoke(this, next);
        }
        Mode = next is MdAdaptiveBreakpoint.Compact ? MdAdaptiveLayoutMode.Compact
            : next is MdAdaptiveBreakpoint.Medium ? MdAdaptiveLayoutMode.Medium
            : MdAdaptiveLayoutMode.Expanded;
        Orientation = Bounds.Width >= Bounds.Height ? Orientation.Horizontal : Orientation.Vertical;
        UpdatePseudoClasses();
    }

    private void OnAnyPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _lastInputMode = e.Pointer.Type == PointerType.Touch ? MdAdaptiveInputMode.Touch : MdAdaptiveInputMode.Pointer;
        UpdatePseudoClasses();
    }

    private void OnAnyKeyDown(object? sender, KeyEventArgs e)
    {
        _lastInputMode = MdAdaptiveInputMode.Keyboard;
        UpdatePseudoClasses();
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":compact", Breakpoint == MdAdaptiveBreakpoint.Compact);
        PseudoClasses.Set(":medium", Breakpoint == MdAdaptiveBreakpoint.Medium);
        PseudoClasses.Set(":expanded", Breakpoint == MdAdaptiveBreakpoint.Expanded);
        PseudoClasses.Set(":large", Breakpoint == MdAdaptiveBreakpoint.Large);
        PseudoClasses.Set(":extra-large", Breakpoint == MdAdaptiveBreakpoint.ExtraLarge);
        PseudoClasses.Set(":portrait", Orientation == Orientation.Vertical);
        PseudoClasses.Set(":landscape", Orientation == Orientation.Horizontal);
        PseudoClasses.Set(":touch", EffectiveInputMode == MdAdaptiveInputMode.Touch);
        PseudoClasses.Set(":pointer", EffectiveInputMode == MdAdaptiveInputMode.Pointer);
        PseudoClasses.Set(":keyboard", EffectiveInputMode == MdAdaptiveInputMode.Keyboard);
    }
}
