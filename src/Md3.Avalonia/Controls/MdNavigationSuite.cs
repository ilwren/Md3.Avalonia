using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace Md3.Avalonia.Controls;

/// <summary>Chooses a navigation bar, rail, or drawer from the available width.</summary>
[PseudoClasses(":bar", ":rail", ":drawer")]
public sealed class MdNavigationSuite : ContentControl
{
    public static readonly StyledProperty<MdNavigationSuiteMode> ModeProperty = AvaloniaProperty.Register<MdNavigationSuite, MdNavigationSuiteMode>(nameof(Mode));
    public static readonly StyledProperty<object?> NavigationBarProperty = AvaloniaProperty.Register<MdNavigationSuite, object?>(nameof(NavigationBar));
    public static readonly StyledProperty<object?> NavigationRailProperty = AvaloniaProperty.Register<MdNavigationSuite, object?>(nameof(NavigationRail));
    public static readonly StyledProperty<object?> NavigationDrawerProperty = AvaloniaProperty.Register<MdNavigationSuite, object?>(nameof(NavigationDrawer));
    public static readonly StyledProperty<double> RailBreakpointProperty = AvaloniaProperty.Register<MdNavigationSuite, double>(nameof(RailBreakpoint), 600);
    public static readonly StyledProperty<double> DrawerBreakpointProperty = AvaloniaProperty.Register<MdNavigationSuite, double>(nameof(DrawerBreakpoint), 1000);
    public static readonly DirectProperty<MdNavigationSuite, MdNavigationSuiteMode> EffectiveModeProperty = AvaloniaProperty.RegisterDirect<MdNavigationSuite, MdNavigationSuiteMode>(nameof(EffectiveMode), suite => suite.EffectiveMode);

    private MdNavigationSuiteMode _effectiveMode = MdNavigationSuiteMode.NavigationBar;

    static MdNavigationSuite()
    {
        ModeProperty.Changed.AddClassHandler<MdNavigationSuite>((suite, _) => suite.UpdateMode());
        RailBreakpointProperty.Changed.AddClassHandler<MdNavigationSuite>((suite, _) => suite.UpdateMode());
        DrawerBreakpointProperty.Changed.AddClassHandler<MdNavigationSuite>((suite, _) => suite.UpdateMode());
    }

    public MdNavigationSuite()
    {
        SizeChanged += (_, _) => UpdateMode();
        UpdateMode();
    }

    public MdNavigationSuiteMode Mode { get => GetValue(ModeProperty); set => SetValue(ModeProperty, value); }
    public object? NavigationBar { get => GetValue(NavigationBarProperty); set => SetValue(NavigationBarProperty, value); }
    public object? NavigationRail { get => GetValue(NavigationRailProperty); set => SetValue(NavigationRailProperty, value); }
    public object? NavigationDrawer { get => GetValue(NavigationDrawerProperty); set => SetValue(NavigationDrawerProperty, value); }
    public double RailBreakpoint { get => GetValue(RailBreakpointProperty); set => SetValue(RailBreakpointProperty, value); }
    public double DrawerBreakpoint { get => GetValue(DrawerBreakpointProperty); set => SetValue(DrawerBreakpointProperty, value); }
    public MdNavigationSuiteMode EffectiveMode => _effectiveMode;

    private void UpdateMode()
    {
        var mode = Mode == MdNavigationSuiteMode.Auto
            ? Bounds.Width >= DrawerBreakpoint ? MdNavigationSuiteMode.NavigationDrawer
            : Bounds.Width >= RailBreakpoint ? MdNavigationSuiteMode.NavigationRail
            : MdNavigationSuiteMode.NavigationBar
            : Mode;
        SetAndRaise(EffectiveModeProperty, ref _effectiveMode, mode);
        PseudoClasses.Set(":bar", mode == MdNavigationSuiteMode.NavigationBar);
        PseudoClasses.Set(":rail", mode == MdNavigationSuiteMode.NavigationRail);
        PseudoClasses.Set(":drawer", mode == MdNavigationSuiteMode.NavigationDrawer);
    }
}
