using System.Windows.Input;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.VisualTree;

namespace Md3.Avalonia.Controls;

[Flags]
public enum MdWindowCapabilities { None = 0, Move = 1, Resize = 2, Minimize = 4, Maximize = 8, Close = 16, SystemMenu = 32, All = Move | Resize | Minimize | Maximize | Close | SystemMenu }
public enum MdWindowPlatform { Unknown, Windows, MacOS, Linux, Android }
public enum MdCaptionButtonKind { Minimize, MaximizeRestore, Close }

public sealed record MdBorderlessWindowOptions(bool CanResize, bool ExtendIntoTitleBar, double TitleBarHeight, bool PreserveNativeBorder = true);

/// <summary>Computed chrome geometry consumed by window templates without platform interop.</summary>
public sealed record MdWindowTemplateSettings(
    double TitleBarHeight,
    Thickness ContentMargin,
    double CaptionButtonWidth,
    bool IsClientAreaExtended,
    bool IsMaximized);

/// <summary>Platform boundary for native-window operations; host applications may replace it.</summary>
public interface IMdWindowPlatformAdapter
{
    MdWindowPlatform Platform { get; }
    MdWindowCapabilities Capabilities { get; }
    void Apply(MdBorderlessWindow window, MdBorderlessWindowOptions options);
    bool TryBeginMove(MdBorderlessWindow window, PointerPressedEventArgs args);
    bool TryBeginResize(MdBorderlessWindow window, WindowEdge edge, PointerPressedEventArgs args);
    bool TryShowSystemMenu(MdBorderlessWindow window, Point clientPoint);
}

/// <summary>Cross-platform Avalonia fallback. OS-specific adapters can extend this without leaking native APIs into controls.</summary>
public class MdAvaloniaWindowPlatformAdapter(MdWindowPlatform platform) : IMdWindowPlatformAdapter
{
    public MdWindowPlatform Platform { get; } = platform;
    public virtual MdWindowCapabilities Capabilities => Platform == MdWindowPlatform.Android ? MdWindowCapabilities.None : MdWindowCapabilities.All & ~MdWindowCapabilities.SystemMenu;
    public virtual void Apply(MdBorderlessWindow window, MdBorderlessWindowOptions options)
    {
        if (Platform == MdWindowPlatform.Android) return;
        // Keep the platform-owned resize frame, shadow and corner treatment. This mirrors the
        // FluentAvalonia AppWindow split: app content owns the caption area while the native
        // non-client frame remains responsible for window geometry and OS effects.
        window.WindowDecorations = options.PreserveNativeBorder ? WindowDecorations.BorderOnly : WindowDecorations.None;
        window.ExtendClientAreaToDecorationsHint = options.ExtendIntoTitleBar;
        window.ExtendClientAreaTitleBarHeightHint = Math.Max(0, options.TitleBarHeight);
        window.CanResize = options.CanResize;
    }
    public virtual bool TryBeginMove(MdBorderlessWindow window, PointerPressedEventArgs args)
    {
        if (!Capabilities.HasFlag(MdWindowCapabilities.Move)) return false;
        window.BeginMoveDrag(args); return true;
    }
    public virtual bool TryBeginResize(MdBorderlessWindow window, WindowEdge edge, PointerPressedEventArgs args)
    {
        if (!Capabilities.HasFlag(MdWindowCapabilities.Resize) || !window.CanResize) return false;
        window.BeginResizeDrag(edge, args); return true;
    }
    public virtual bool TryShowSystemMenu(MdBorderlessWindow window, Point clientPoint) => false;
}
public sealed class MdWindowsWindowPlatformAdapter() : MdAvaloniaWindowPlatformAdapter(MdWindowPlatform.Windows);
public sealed class MdMacOsWindowPlatformAdapter() : MdAvaloniaWindowPlatformAdapter(MdWindowPlatform.MacOS);
public sealed class MdLinuxWindowPlatformAdapter() : MdAvaloniaWindowPlatformAdapter(MdWindowPlatform.Linux);
public sealed class MdAndroidWindowPlatformAdapter() : MdAvaloniaWindowPlatformAdapter(MdWindowPlatform.Android);

public static class MdWindowPlatformAdapterResolver
{
    public static IMdWindowPlatformAdapter Resolve() => OperatingSystem.IsWindows() ? new MdWindowsWindowPlatformAdapter()
        : OperatingSystem.IsMacOS() ? new MdMacOsWindowPlatformAdapter()
        : OperatingSystem.IsLinux() ? new MdLinuxWindowPlatformAdapter()
        : OperatingSystem.IsAndroid() ? new MdAndroidWindowPlatformAdapter()
        : new MdAvaloniaWindowPlatformAdapter(MdWindowPlatform.Unknown);
}

/// <summary>Material borderless window host with replaceable platform adapter and Android-safe no-op behavior.</summary>
[PseudoClasses(":active", ":inactive", ":maximized", ":custom-chrome")]
public class MdBorderlessWindow : MdWindow
{
    public static readonly StyledProperty<bool> IsCustomChromeEnabledProperty = AvaloniaProperty.Register<MdBorderlessWindow, bool>(nameof(IsCustomChromeEnabled), true);
    public static readonly StyledProperty<bool> ExtendIntoTitleBarProperty = AvaloniaProperty.Register<MdBorderlessWindow, bool>(nameof(ExtendIntoTitleBar), true);
    public static readonly StyledProperty<double> TitleBarHeightProperty = AvaloniaProperty.Register<MdBorderlessWindow, double>(nameof(TitleBarHeight), 40);
    public static readonly StyledProperty<bool> PreserveNativeBorderProperty = AvaloniaProperty.Register<MdBorderlessWindow, bool>(nameof(PreserveNativeBorder), true);
    public static readonly StyledProperty<bool> ShowMinimizeButtonProperty = AvaloniaProperty.Register<MdBorderlessWindow, bool>(nameof(ShowMinimizeButton), true);
    public static readonly StyledProperty<bool> ShowMaximizeButtonProperty = AvaloniaProperty.Register<MdBorderlessWindow, bool>(nameof(ShowMaximizeButton), true);
    public static readonly StyledProperty<bool> ShowCloseButtonProperty = AvaloniaProperty.Register<MdBorderlessWindow, bool>(nameof(ShowCloseButton), true);
    public static readonly DirectProperty<MdBorderlessWindow, MdWindowTemplateSettings> TemplateSettingsProperty =
        AvaloniaProperty.RegisterDirect<MdBorderlessWindow, MdWindowTemplateSettings>(nameof(TemplateSettings), window => window.TemplateSettings);
    private IMdWindowPlatformAdapter _platformAdapter;
    private MdWindowTemplateSettings _templateSettings = new(40, new Thickness(0, 40, 0, 0), 46, true, false);
    private bool _isActive;

    static MdBorderlessWindow()
    {
        IsCustomChromeEnabledProperty.Changed.AddClassHandler<MdBorderlessWindow>((window, _) => window.ApplyChrome());
        ExtendIntoTitleBarProperty.Changed.AddClassHandler<MdBorderlessWindow>((window, _) => window.ApplyChrome());
        TitleBarHeightProperty.Changed.AddClassHandler<MdBorderlessWindow>((window, _) => window.ApplyChrome());
        PreserveNativeBorderProperty.Changed.AddClassHandler<MdBorderlessWindow>((window, _) => window.ApplyChrome());
        CanResizeProperty.Changed.AddClassHandler<MdBorderlessWindow>((window, _) => window.ApplyChrome());
        WindowStateProperty.Changed.AddClassHandler<MdBorderlessWindow>((window, _) => window.UpdateWindowState());
    }
    protected override Type StyleKeyOverride => typeof(MdBorderlessWindow);

    public MdBorderlessWindow()
    {
        _platformAdapter = MdWindowPlatformAdapterResolver.Resolve();
        MinimizeCommand = new WindowCommand(_ => Minimize());
        ToggleMaximizeCommand = new WindowCommand(_ => ToggleMaximizeRestore());
        CloseCommand = new WindowCommand(_ => RequestClose());
        Opened += (_, _) => { ApplyChrome(); SetActiveState(true); };
        Activated += (_, _) => SetActiveState(true);
        Deactivated += (_, _) => SetActiveState(false);
        Closed += (_, _) => SetActiveState(false);
        SetActiveState(false);
        UpdateWindowState();
    }
    public bool IsCustomChromeEnabled { get => GetValue(IsCustomChromeEnabledProperty); set => SetValue(IsCustomChromeEnabledProperty, value); }
    public bool ExtendIntoTitleBar { get => GetValue(ExtendIntoTitleBarProperty); set => SetValue(ExtendIntoTitleBarProperty, value); }
    public double TitleBarHeight { get => GetValue(TitleBarHeightProperty); set => SetValue(TitleBarHeightProperty, value); }
    public bool PreserveNativeBorder { get => GetValue(PreserveNativeBorderProperty); set => SetValue(PreserveNativeBorderProperty, value); }
    public bool ShowMinimizeButton { get => GetValue(ShowMinimizeButtonProperty); set => SetValue(ShowMinimizeButtonProperty, value); }
    public bool ShowMaximizeButton { get => GetValue(ShowMaximizeButtonProperty); set => SetValue(ShowMaximizeButtonProperty, value); }
    public bool ShowCloseButton { get => GetValue(ShowCloseButtonProperty); set => SetValue(ShowCloseButtonProperty, value); }
    public MdWindowTemplateSettings TemplateSettings => _templateSettings;
    public bool IsWindowActive => _isActive;
    public IMdWindowPlatformAdapter PlatformAdapter { get => _platformAdapter; set { _platformAdapter = value ?? throw new ArgumentNullException(nameof(value)); ApplyChrome(); } }
    public MdWindowCapabilities Capabilities => PlatformAdapter.Capabilities;
    public ICommand MinimizeCommand { get; }
    public ICommand ToggleMaximizeCommand { get; }
    public ICommand CloseCommand { get; }
    public event EventHandler<Point>? SystemMenuRequested;
    public event EventHandler? MinimizeRequested;
    public event EventHandler? MaximizeRestoreRequested;
    public event EventHandler? CloseRequested;
    public void Minimize() { if (!Capabilities.HasFlag(MdWindowCapabilities.Minimize)) return; MinimizeRequested?.Invoke(this, EventArgs.Empty); WindowState = WindowState.Minimized; }
    public void ToggleMaximizeRestore() { if (!Capabilities.HasFlag(MdWindowCapabilities.Maximize)) return; MaximizeRestoreRequested?.Invoke(this, EventArgs.Empty); WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized; }
    public void RequestClose() { if (!Capabilities.HasFlag(MdWindowCapabilities.Close)) return; CloseRequested?.Invoke(this, EventArgs.Empty); Close(); }
    public bool ShowSystemMenu(Point point) { var shown = PlatformAdapter.TryShowSystemMenu(this, point); if (!shown) SystemMenuRequested?.Invoke(this, point); return shown; }
    internal bool TryBeginMove(PointerPressedEventArgs args) => PlatformAdapter.TryBeginMove(this, args);
    internal bool TryBeginResize(WindowEdge edge, PointerPressedEventArgs args) => PlatformAdapter.TryBeginResize(this, edge, args);
    private void ApplyChrome()
    {
        var height = Math.Max(0, TitleBarHeight);
        if (IsCustomChromeEnabled)
            PlatformAdapter.Apply(this, new MdBorderlessWindowOptions(CanResize, ExtendIntoTitleBar, height, PreserveNativeBorder));
        else
        {
            WindowDecorations = WindowDecorations.Full;
            ExtendClientAreaToDecorationsHint = false;
        }
        var settings = new MdWindowTemplateSettings(
            height,
            IsCustomChromeEnabled && ExtendIntoTitleBar ? new Thickness(0, height, 0, 0) : default,
            46,
            IsCustomChromeEnabled && ExtendIntoTitleBar,
            WindowState == WindowState.Maximized);
        SetAndRaise(TemplateSettingsProperty, ref _templateSettings, settings);
        PseudoClasses.Set(":custom-chrome", IsCustomChromeEnabled);
        UpdateWindowState();
    }

    private void SetActiveState(bool active)
    {
        _isActive = active;
        PseudoClasses.Set(":active", active);
        PseudoClasses.Set(":inactive", !active);
    }

    private void UpdateWindowState()
    {
        PseudoClasses.Set(":maximized", WindowState == WindowState.Maximized);
        if (_templateSettings.IsMaximized == (WindowState == WindowState.Maximized)) return;
        var settings = _templateSettings with { IsMaximized = WindowState == WindowState.Maximized };
        SetAndRaise(TemplateSettingsProperty, ref _templateSettings, settings);
    }
    private sealed class WindowCommand(Action<object?> execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => execute(parameter);
    }
}

/// <summary>Hit-test-aware drag region and caption-button host for MdBorderlessWindow.</summary>
[PseudoClasses(":active", ":inactive")]
public sealed class MdWindowTitleBar : ContentControl
{
    public static readonly StyledProperty<object?> LeadingContentProperty = AvaloniaProperty.Register<MdWindowTitleBar, object?>(nameof(LeadingContent));
    public static readonly StyledProperty<object?> TrailingContentProperty = AvaloniaProperty.Register<MdWindowTitleBar, object?>(nameof(TrailingContent));
    public static readonly StyledProperty<object?> SubtitleProperty = AvaloniaProperty.Register<MdWindowTitleBar, object?>(nameof(Subtitle));
    public static readonly StyledProperty<bool> ShowCaptionButtonsProperty = AvaloniaProperty.Register<MdWindowTitleBar, bool>(nameof(ShowCaptionButtons), true);
    public static readonly StyledProperty<bool> ShowMinimizeButtonProperty = AvaloniaProperty.Register<MdWindowTitleBar, bool>(nameof(ShowMinimizeButton), true);
    public static readonly StyledProperty<bool> ShowMaximizeButtonProperty = AvaloniaProperty.Register<MdWindowTitleBar, bool>(nameof(ShowMaximizeButton), true);
    public static readonly StyledProperty<bool> ShowCloseButtonProperty = AvaloniaProperty.Register<MdWindowTitleBar, bool>(nameof(ShowCloseButton), true);
    private MdBorderlessWindow? _window;
    public object? LeadingContent { get => GetValue(LeadingContentProperty); set => SetValue(LeadingContentProperty, value); }
    public object? TrailingContent { get => GetValue(TrailingContentProperty); set => SetValue(TrailingContentProperty, value); }
    public object? Subtitle { get => GetValue(SubtitleProperty); set => SetValue(SubtitleProperty, value); }
    public bool ShowCaptionButtons { get => GetValue(ShowCaptionButtonsProperty); set => SetValue(ShowCaptionButtonsProperty, value); }
    public bool ShowMinimizeButton { get => GetValue(ShowMinimizeButtonProperty); set => SetValue(ShowMinimizeButtonProperty, value); }
    public bool ShowMaximizeButton { get => GetValue(ShowMaximizeButtonProperty); set => SetValue(ShowMaximizeButtonProperty, value); }
    public bool ShowCloseButton { get => GetValue(ShowCloseButtonProperty); set => SetValue(ShowCloseButtonProperty, value); }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _window = this.FindAncestorOfType<MdBorderlessWindow>();
        if (_window is null) return;
        _window.Activated += OnWindowActivated;
        _window.Deactivated += OnWindowDeactivated;
        _window.PropertyChanged += OnWindowPropertyChanged;
        SetCurrentValue(HeightProperty, _window.TemplateSettings.TitleBarHeight);
        UpdateActiveState(_window.IsWindowActive);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_window is not null)
        {
            _window.Activated -= OnWindowActivated;
            _window.Deactivated -= OnWindowDeactivated;
            _window.PropertyChanged -= OnWindowPropertyChanged;
            _window = null;
        }
        base.OnDetachedFromVisualTree(e);
    }

    private void OnWindowActivated(object? sender, EventArgs e) => UpdateActiveState(true);
    private void OnWindowDeactivated(object? sender, EventArgs e) => UpdateActiveState(false);
    private void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == MdBorderlessWindow.TitleBarHeightProperty ||
            e.Property == MdBorderlessWindow.TemplateSettingsProperty)
        {
            SetCurrentValue(HeightProperty, _window?.TemplateSettings.TitleBarHeight ?? 40);
        }
    }

    private void UpdateActiveState(bool active)
    {
        PseudoClasses.Set(":active", active);
        PseudoClasses.Set(":inactive", !active);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.Source is Visual source && IsInteractiveDescendant(source, this)) return;
        if (e.GetCurrentPoint(this).Properties.PointerUpdateKind is not PointerUpdateKind.LeftButtonPressed) return;
        if (this.FindAncestorOfType<MdBorderlessWindow>() is not { } window) return;
        if (e.ClickCount == 2) window.ToggleMaximizeRestore(); else window.TryBeginMove(e);
        e.Handled = true;
    }
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (e.Source is Visual source && IsInteractiveDescendant(source, this)) { base.OnPointerReleased(e); return; }
        if (e.InitialPressMouseButton == MouseButton.Right && this.FindAncestorOfType<MdBorderlessWindow>() is { } window) { window.ShowSystemMenu(e.GetPosition(window)); e.Handled = true; }
        base.OnPointerReleased(e);
    }

    internal static bool IsInteractiveDescendant(Visual source, Visual boundary)
    {
        foreach (var current in source.GetVisualAncestors().Prepend(source))
        {
            if (ReferenceEquals(current, boundary)) break;
            if (current is InputElement { Focusable: true } || current is TextBox || current is SelectingItemsControl) return true;
        }
        return false;
    }
}

/// <summary>Accessible caption action button with native non-client role metadata.</summary>
[PseudoClasses(":minimize", ":maximize", ":restore", ":close")]
public class MdCaptionButton : Button
{
    public static readonly StyledProperty<MdCaptionButtonKind> KindProperty =
        AvaloniaProperty.Register<MdCaptionButton, MdCaptionButtonKind>(nameof(Kind));

    private MdBorderlessWindow? _window;

    static MdCaptionButton() =>
        KindProperty.Changed.AddClassHandler<MdCaptionButton>((button, _) => button.UpdateCaptionState());

    public MdCaptionButton() => UpdateCaptionState();

    public MdCaptionButtonKind Kind { get => GetValue(KindProperty); set => SetValue(KindProperty, value); }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _window = this.FindAncestorOfType<MdBorderlessWindow>();
        if (_window is not null) _window.PropertyChanged += OnWindowPropertyChanged;
        UpdateCaptionState();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_window is not null) _window.PropertyChanged -= OnWindowPropertyChanged;
        _window = null;
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnClick()
    {
        base.OnClick();
        if (this.FindAncestorOfType<MdBorderlessWindow>() is not { } window) return;
        switch (Kind)
        {
            case MdCaptionButtonKind.Minimize: window.Minimize(); break;
            case MdCaptionButtonKind.MaximizeRestore: window.ToggleMaximizeRestore(); break;
            case MdCaptionButtonKind.Close: window.RequestClose(); break;
        }
    }

    private void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Window.WindowStateProperty) UpdateCaptionState();
    }

    private void UpdateCaptionState()
    {
        var restoring = Kind == MdCaptionButtonKind.MaximizeRestore && _window?.WindowState == WindowState.Maximized;
        PseudoClasses.Set(":minimize", Kind == MdCaptionButtonKind.Minimize);
        PseudoClasses.Set(":maximize", Kind == MdCaptionButtonKind.MaximizeRestore && !restoring);
        PseudoClasses.Set(":restore", restoring);
        PseudoClasses.Set(":close", Kind == MdCaptionButtonKind.Close);

        WindowDecorationProperties.SetElementRole(this, Kind switch
        {
            MdCaptionButtonKind.Minimize => WindowDecorationsElementRole.MinimizeButton,
            MdCaptionButtonKind.MaximizeRestore => WindowDecorationsElementRole.MaximizeButton,
            _ => WindowDecorationsElementRole.CloseButton
        });
        AutomationProperties.SetName(this, Kind switch
        {
            MdCaptionButtonKind.Minimize => "Minimize",
            MdCaptionButtonKind.MaximizeRestore when restoring => "Restore",
            MdCaptionButtonKind.MaximizeRestore => "Maximize",
            _ => "Close"
        });
    }
}

/// <summary>Compatibility name for the Material window caption action button.</summary>
public sealed class MdWindowCaptionButton : MdCaptionButton
{
    protected override Type StyleKeyOverride => typeof(MdCaptionButton);
}

/// <summary>Explicit draggable title-bar region that excludes interactive descendants.</summary>
public sealed class MdWindowDragRegion : ContentControl
{
    public MdWindowDragRegion() =>
        WindowDecorationProperties.SetElementRole(this, WindowDecorationsElementRole.TitleBar);

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.Source is Visual source && MdWindowTitleBar.IsInteractiveDescendant(source, this)) return;
        if (e.GetCurrentPoint(this).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed) return;
        if (this.FindAncestorOfType<MdBorderlessWindow>()?.TryBeginMove(e) == true) e.Handled = true;
    }
}

/// <summary>Invisible or visible edge grip that starts platform resize without native interop in app code.</summary>
public sealed class MdWindowResizeGrip : Control
{
    public static readonly StyledProperty<WindowEdge> EdgeProperty =
        AvaloniaProperty.Register<MdWindowResizeGrip, WindowEdge>(nameof(Edge), WindowEdge.SouthEast);

    static MdWindowResizeGrip() =>
        EdgeProperty.Changed.AddClassHandler<MdWindowResizeGrip>((grip, _) => grip.UpdateElementRole());

    public MdWindowResizeGrip() => UpdateElementRole();

    public WindowEdge Edge { get => GetValue(EdgeProperty); set => SetValue(EdgeProperty, value); }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed) return;
        if (this.FindAncestorOfType<MdBorderlessWindow>()?.TryBeginResize(Edge, e) == true) e.Handled = true;
    }

    private void UpdateElementRole() => WindowDecorationProperties.SetElementRole(this, Edge switch
    {
        WindowEdge.North => WindowDecorationsElementRole.ResizeN,
        WindowEdge.South => WindowDecorationsElementRole.ResizeS,
        WindowEdge.East => WindowDecorationsElementRole.ResizeE,
        WindowEdge.West => WindowDecorationsElementRole.ResizeW,
        WindowEdge.NorthEast => WindowDecorationsElementRole.ResizeNE,
        WindowEdge.NorthWest => WindowDecorationsElementRole.ResizeNW,
        WindowEdge.SouthWest => WindowDecorationsElementRole.ResizeSW,
        _ => WindowDecorationsElementRole.ResizeSE
    });
}
