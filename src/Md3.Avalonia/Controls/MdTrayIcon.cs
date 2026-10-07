using System.Collections.Specialized;
using System.Windows.Input;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Metadata;
using Avalonia.Threading;

namespace Md3.Avalonia.Controls;

/// <summary>Platform family that owns the notification area (system tray) for a tray icon.</summary>
public enum MdTrayPlatform { Unknown, Windows, MacOS, Linux, Android, Browser }

/// <summary>
/// Platform boundary for the notification area. A host can replace the adapter to route the icon
/// through its own implementation (a companion process, a different tray library, or a test fake)
/// without changing the Material API or the menu model.
/// </summary>
public interface IMdTrayIconPlatformAdapter : IDisposable
{
    /// <summary>Platform family that resolved this adapter.</summary>
    MdTrayPlatform Platform { get; }

    /// <summary>
    /// Whether a tray icon can actually appear. Android, the browser and hosts that expose no
    /// windowing platform report <see langword="false"/> and every call becomes a no-op.
    /// </summary>
    bool IsSupported { get; }

    void SetIcon(WindowIcon? icon);
    void SetToolTipText(string? text);
    void SetVisible(bool visible);
    void SetMenu(NativeMenu? menu);

    /// <summary>Raised when the platform reports a primary activation of the icon.</summary>
    event EventHandler? Clicked;
}

/// <summary>
/// Default adapter. It forwards to Avalonia's <see cref="TrayIcon"/>, which already owns the
/// Win32, freedesktop/DBus and macOS status-item implementations. Where the platform supplies no
/// implementation Avalonia returns a null handle and every call stays a no-op, so Android and the
/// headless test host are safe without a conditional in the control.
/// </summary>
public class MdAvaloniaTrayIconPlatformAdapter : IMdTrayIconPlatformAdapter
{
    private readonly TrayIcon? _trayIcon;
    private bool _disposed;

    public MdAvaloniaTrayIconPlatformAdapter()
        : this(MdTrayIconPlatformAdapterResolver.CurrentPlatform)
    {
    }

    public MdAvaloniaTrayIconPlatformAdapter(MdTrayPlatform platform)
    {
        Platform = platform;
        _trayIcon = CreateTrayIcon();
        if (_trayIcon is not null) _trayIcon.Clicked += OnTrayIconClicked;
    }

    /// <summary>The underlying Avalonia tray icon, for hosts that need the raw handle.</summary>
    public TrayIcon? TrayIcon => _trayIcon;

    /// <summary>The exported native menu, so a host or test can inspect what the platform receives.</summary>
    public NativeMenu? Menu => _trayIcon?.Menu;

    public MdTrayPlatform Platform { get; }

    /// <summary>
    /// Best-effort capability probe: the platform family must own a notification area and the
    /// windowing platform must have supplied a tray implementation, which Avalonia exposes through
    /// the native-menu exporter. Headless hosts and the X11 XEmbed fallback therefore report
    /// <see langword="false"/> instead of promising an icon that never appears.
    /// </summary>
    public bool IsSupported =>
        !_disposed && _trayIcon is { NativeMenuExporter: not null } &&
        Platform is MdTrayPlatform.Windows or MdTrayPlatform.MacOS or MdTrayPlatform.Linux;

    public event EventHandler? Clicked;

    public void SetIcon(WindowIcon? icon)
    {
        if (_trayIcon is not null) _trayIcon.Icon = icon;
    }

    public void SetToolTipText(string? text)
    {
        if (_trayIcon is not null) _trayIcon.ToolTipText = text;
    }

    public void SetVisible(bool visible)
    {
        if (_trayIcon is not null) _trayIcon.IsVisible = visible;
    }

    public void SetMenu(NativeMenu? menu)
    {
        if (_trayIcon is not null) _trayIcon.Menu = menu;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_trayIcon is null) return;
        _trayIcon.Clicked -= OnTrayIconClicked;
        // Disposing removes the icon from the notification area. The native menu is left assigned
        // so a host that re-inspects the adapter after disposal still sees the last menu.
        _trayIcon.Dispose();
    }

    private void OnTrayIconClicked(object? sender, EventArgs e) => Clicked?.Invoke(this, EventArgs.Empty);

    private static TrayIcon? CreateTrayIcon()
    {
        try
        {
            return new TrayIcon();
        }
        catch (InvalidOperationException)
        {
            // Avalonia throws when there is no windowing platform at all (a plain unit-test host,
            // or a design-time surface). That is the same supported outcome as a null handle.
            return null;
        }
    }
}

/// <summary>
/// Adapter for platforms with no notification area. It accepts the whole Material API and does
/// nothing, which is what keeps <see cref="MdTrayIcon"/> referenceable from shared code that also
/// runs on Android, exactly like the Android window-chrome adapter.
/// </summary>
public sealed class MdNoOpTrayIconPlatformAdapter : IMdTrayIconPlatformAdapter
{
    public MdTrayPlatform Platform { get; init; } = MdTrayPlatform.Unknown;

    public bool IsSupported => false;

    public event EventHandler? Clicked
    {
        add { }
        remove { }
    }

    public void SetIcon(WindowIcon? icon) { }
    public void SetToolTipText(string? text) { }
    public void SetVisible(bool visible) { }
    public void SetMenu(NativeMenu? menu) { }
    public void Dispose() { }
}

/// <summary>Resolves the platform adapter used by a <see cref="MdTrayIcon"/> that has none injected.</summary>
public static class MdTrayIconPlatformAdapterResolver
{
    /// <summary>Operating-system family of the running process, independent of adapter choice.</summary>
    public static MdTrayPlatform CurrentPlatform =>
        OperatingSystem.IsWindows() ? MdTrayPlatform.Windows
        : OperatingSystem.IsMacOS() ? MdTrayPlatform.MacOS
        : OperatingSystem.IsLinux() ? MdTrayPlatform.Linux
        : OperatingSystem.IsAndroid() ? MdTrayPlatform.Android
        : OperatingSystem.IsBrowser() ? MdTrayPlatform.Browser
        : MdTrayPlatform.Unknown;

    /// <summary>
    /// A desktop host receives the Avalonia adapter; every other platform receives the no-op
    /// adapter, so shared code never has to test the platform before creating a tray icon.
    /// </summary>
    public static IMdTrayIconPlatformAdapter Resolve() =>
        CurrentPlatform is MdTrayPlatform.Windows or MdTrayPlatform.MacOS or MdTrayPlatform.Linux
            ? new MdAvaloniaTrayIconPlatformAdapter(CurrentPlatform)
            : new MdNoOpTrayIconPlatformAdapter { Platform = CurrentPlatform };

    /// <summary>
    /// True when this platform family has a notification area implementation. The check is a
    /// platform test rather than an adapter probe so that asking the question never creates a
    /// platform handle; a host that injects its own adapter decides support itself.
    /// </summary>
    public static bool IsSupported =>
        CurrentPlatform is MdTrayPlatform.Windows or MdTrayPlatform.MacOS or MdTrayPlatform.Linux;
}

/// <summary>
/// A Material API surface for the notification area: an icon, tooltip text, click command and a
/// menu. The icon lives in the operating system's tray host rather than in a visual tree, so there
/// is no surface of its own to style; what this type owns is naming, defaults, lifetime and the
/// replaceable platform seam, and the menu is handed over as an Avalonia
/// <see cref="NativeMenu"/>.
/// </summary>
/// <remarks>
/// Attachment is deferred until the icon is actually used - a property is assigned, an entry is
/// added, <see cref="Show"/> is called, or an adapter is injected - so holding one in a view model
/// or a data template does not create a platform handle. Register the instance in
/// <see cref="Icons"/> on the <see cref="Application"/> to have it disposed at shutdown. On Windows
/// the tray menu is drawn by Avalonia's own managed popup rather than by the system, so a host can
/// restyle it there; that is a per-platform detail this API does not promise.
/// </remarks>
public class MdTrayIcon : AvaloniaObject, IDisposable
{
    private NativeMenu? _menu;
    private IMdTrayIconPlatformAdapter? _adapter;
    private bool _isAttached;
    private bool _disposed;

    public static readonly StyledProperty<WindowIcon?> IconProperty =
        AvaloniaProperty.Register<MdTrayIcon, WindowIcon?>(nameof(Icon));

    public static readonly StyledProperty<string?> ToolTipTextProperty =
        AvaloniaProperty.Register<MdTrayIcon, string?>(nameof(ToolTipText));

    public static readonly StyledProperty<bool> IsVisibleProperty =
        AvaloniaProperty.Register<MdTrayIcon, bool>(nameof(IsVisible), true);

    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<MdTrayIcon, ICommand?>(nameof(Command));

    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<MdTrayIcon, object?>(nameof(CommandParameter));

    public static readonly StyledProperty<IMdTrayIconPlatformAdapter?> PlatformAdapterProperty =
        AvaloniaProperty.Register<MdTrayIcon, IMdTrayIconPlatformAdapter?>(nameof(PlatformAdapter));

    /// <summary>Defines the <see cref="MdTrayIcons"/> attached property.</summary>
    public static readonly AttachedProperty<MdTrayIcons?> IconsProperty =
        AvaloniaProperty.RegisterAttached<MdTrayIcon, Application, MdTrayIcons?>("Icons");

    /// <summary>Defines the read-only <see cref="Menu"/> property.</summary>
    public static readonly DirectProperty<MdTrayIcon, NativeMenu?> MenuProperty =
        AvaloniaProperty.RegisterDirect<MdTrayIcon, NativeMenu?>(nameof(Menu), icon => icon._menu);

    static MdTrayIcon()
    {
        IconProperty.Changed.AddClassHandler<MdTrayIcon>((icon, _) => icon.PushIcon());
        ToolTipTextProperty.Changed.AddClassHandler<MdTrayIcon>((icon, _) => icon.PushToolTipText());
        IsVisibleProperty.Changed.AddClassHandler<MdTrayIcon>((icon, _) => icon.PushVisibility());
        PlatformAdapterProperty.Changed.AddClassHandler<MdTrayIcon>((icon, _) => icon.Reattach());
        Dispatcher.UIThread.ShutdownStarted += DisposeRegisteredIcons;
    }

    public MdTrayIcon() => Items.CollectionChanged += OnItemsChanged;

    /// <summary>Raised when the platform reports a primary activation of the icon.</summary>
    public event EventHandler? Clicked;

    /// <summary>
    /// Raised when the platform asks for the menu to be refreshed before it opens. Bind or assign
    /// entry state here rather than on a timer.
    /// </summary>
    public event EventHandler? MenuRefreshRequested;

    /// <summary>
    /// Menu entries. This is the content property, so entries can be declared directly inside
    /// <c>md:MdTrayIcon</c>. Use <see cref="MdTrayMenuItem.Items"/> on an entry for a submenu.
    /// </summary>
    [Content]
    public AvaloniaList<NativeMenuItemBase> Items { get; } = [];

    /// <summary>The native menu currently handed to the platform, or <see langword="null"/>.</summary>
    public NativeMenu? Menu => _menu;

    public WindowIcon? Icon { get => GetValue(IconProperty); set => SetValue(IconProperty, value); }

    /// <summary>Text the platform shows when the pointer rests over the icon.</summary>
    public string? ToolTipText { get => GetValue(ToolTipTextProperty); set => SetValue(ToolTipTextProperty, value); }

    public bool IsVisible { get => GetValue(IsVisibleProperty); set => SetValue(IsVisibleProperty, value); }

    /// <summary>Executed on primary activation, after <see cref="Clicked"/> is raised.</summary>
    public ICommand? Command { get => GetValue(CommandProperty); set => SetValue(CommandProperty, value); }

    public object? CommandParameter { get => GetValue(CommandParameterProperty); set => SetValue(CommandParameterProperty, value); }

    /// <summary>
    /// Optional replacement for the resolved platform adapter. Setting one detaches the previous
    /// adapter, so a host can swap implementations at runtime.
    /// </summary>
    public IMdTrayIconPlatformAdapter? PlatformAdapter
    {
        get => GetValue(PlatformAdapterProperty);
        set => SetValue(PlatformAdapterProperty, value);
    }

    /// <summary>Platform family of the active adapter, or of the running platform before first use.</summary>
    public MdTrayPlatform Platform => _adapter?.Platform ?? MdTrayIconPlatformAdapterResolver.CurrentPlatform;

    /// <summary>Whether the active adapter can actually show an icon on this platform.</summary>
    public bool IsSupported => _adapter?.IsSupported ?? MdTrayIconPlatformAdapterResolver.IsSupported;

    public static void SetIcons(Application application, MdTrayIcons? icons) =>
        application.SetValue(IconsProperty, icons);

    public static MdTrayIcons? GetIcons(Application application) => application.GetValue(IconsProperty);

    /// <summary>Makes the icon visible, attaching to the platform if it is not attached yet.</summary>
    public void Show()
    {
        IsVisible = true;
        AttachIfNeeded();
        _adapter?.SetVisible(true);
    }

    /// <summary>Hides the icon without releasing the platform handle.</summary>
    public void Hide() => IsVisible = false;

    /// <summary>Re-raises the platform activation as if the user had clicked the icon.</summary>
    public void Activate()
    {
        Clicked?.Invoke(this, EventArgs.Empty);
        if (Command?.CanExecute(CommandParameter) == true) Command.Execute(CommandParameter);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Items.CollectionChanged -= OnItemsChanged;
        Detach();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Attaching is deferred until the icon is actually used - a property is set, an entry is added,
    /// <see cref="Show"/> is called, or an adapter is injected. Constructing one to hold state in a
    /// view model therefore does not put an icon in the notification area on its own, and
    /// <c>new MdTrayIcon { PlatformAdapter = fake }</c> never creates the real handle first.
    /// </summary>
    private void AttachIfNeeded()
    {
        if (_disposed || _isAttached) return;
        _adapter = PlatformAdapter ?? MdTrayIconPlatformAdapterResolver.Resolve();
        _adapter.Clicked += OnPlatformClicked;
        _isAttached = true;
        _adapter.SetIcon(Icon);
        _adapter.SetToolTipText(ToolTipText);
        RebuildMenu();
        _adapter.SetVisible(IsVisible);
    }

    private void PushIcon()
    {
        AttachIfNeeded();
        _adapter?.SetIcon(Icon);
    }

    private void PushToolTipText()
    {
        AttachIfNeeded();
        _adapter?.SetToolTipText(ToolTipText);
    }

    private void PushVisibility()
    {
        AttachIfNeeded();
        _adapter?.SetVisible(IsVisible);
    }

    private void Reattach()
    {
        Detach();
        AttachIfNeeded();
    }

    private void Detach()
    {
        if (!_isAttached) return;
        _isAttached = false;
        if (_adapter is { } adapter)
        {
            adapter.Clicked -= OnPlatformClicked;
            adapter.Dispose();
        }
        _adapter = null;
        // Release the entries from the menu that is being dropped. Without this, re-attaching
        // would hand the same entries to a new menu while they still name the old one as their
        // parent, which the single-parent check rejects.
        _menu?.Items.Clear();
        SetMenu(null);
    }

    private void OnPlatformClicked(object? sender, EventArgs e) => Activate();

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        AttachIfNeeded();
        RebuildMenu();
    }

    /// <summary>
    /// Rebuilds the exported menu from <see cref="Items"/>. A fresh <see cref="NativeMenu"/> is
    /// used every time on purpose: an entry can only have one parent, so removing the previous
    /// menu's entries (<see cref="NativeMenu.Items"/> clears the parent on removal) is what makes
    /// the same entry objects reusable across rebuilds instead of throwing on the second one.
    /// </summary>
    private void RebuildMenu()
    {
        if (_adapter is null) return;
        // Release the entries from the previous menu first: adding an entry that still has a parent
        // is an error, so the clear has to happen before the new menu claims them.
        _menu?.Items.Clear();
        var menu = new NativeMenu();
        foreach (var item in Items) menu.Items.Add(item);
        SetMenu(menu);
        _adapter.SetMenu(menu);
        // Mirror the platform's own opening refresh: entries whose enabled or checked state depends
        // on application state are usually updated here by the host.
        menu.NeedsUpdate += (_, _) => MenuRefreshRequested?.Invoke(this, EventArgs.Empty);
    }

    private void SetMenu(NativeMenu? menu) => SetAndRaise(MenuProperty, ref _menu, menu);

    private static void DisposeRegisteredIcons(object? sender, EventArgs e)
    {
        if (Application.Current is not { } application) return;
        if (GetIcons(application) is not { } icons) return;
        foreach (var icon in icons.ToArray()) icon.Dispose();
        icons.Clear();
    }
}

/// <summary>
/// The set of tray icons owned by an <see cref="Application"/>. Register one with
/// <see cref="MdTrayIcon.SetIcons"/>; its entries are disposed when the UI thread shuts down.
/// </summary>
public sealed class MdTrayIcons : AvaloniaList<MdTrayIcon>
{
}

/// <summary>A Material-named tray menu entry. It is an Avalonia <see cref="NativeMenuItem"/>, so
/// commands, gestures, toggle types, check state, icons and enabled state behave exactly as the
/// platform's native menu bridge defines them.</summary>
public class MdTrayMenuItem : NativeMenuItem
{
    private NativeMenu? _submenu;

    public MdTrayMenuItem() => Items.CollectionChanged += OnItemsChanged;

    public MdTrayMenuItem(string header) : base(header) => Items.CollectionChanged += OnItemsChanged;

    /// <summary>
    /// Submenu entries. Declaring them here creates the entry's <see cref="NativeMenuItem.Menu"/>
    /// on demand, which keeps the submenu out of the XAML as a second nested element.
    /// </summary>
    [Content]
    public AvaloniaList<NativeMenuItemBase> Items { get; } = [];

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (NativeMenuItemBase item in e.OldItems) _submenu?.Items.Remove(item);
        }

        if (Items.Count == 0)
        {
            // An empty submenu would still draw an expander arrow on some platforms, so an entry
            // whose last child was removed goes back to being a leaf.
            SetCurrentValue(MenuProperty, null);
            _submenu = null;
            return;
        }

        if (e.NewItems is null) return;
        var submenu = EnsureSubmenu();
        foreach (NativeMenuItemBase item in e.NewItems)
        {
            if (!submenu.Items.Contains(item)) submenu.Items.Add(item);
        }
    }

    /// <summary>
    /// Returns the submenu, reusing an explicitly assigned <see cref="NativeMenuItem.Menu"/> when
    /// the entry already has one, so <see cref="Items"/> and a hand-built menu are the same thing
    /// rather than two competing children.
    /// </summary>
    private NativeMenu EnsureSubmenu()
    {
        if (_submenu is not null) return _submenu;
        if (Menu is { } existing) return _submenu = existing;
        _submenu = new NativeMenu();
        SetCurrentValue(MenuProperty, _submenu);
        return _submenu;
    }
}

/// <summary>A separator for <see cref="MdTrayIcon.Items"/>.</summary>
public class MdTrayMenuItemSeparator : NativeMenuItemSeparator
{
}
