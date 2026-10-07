using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Headless hosts have no notification area, so these tests inject a recording adapter through the
/// platform seam. That verifies what the Material API hands to a platform - icon, tooltip,
/// visibility, exported native menu and click routing - without depending on a desktop session.
/// </summary>
public sealed class MdTrayIconTests
{
    private sealed class RecordingTrayAdapter : IMdTrayIconPlatformAdapter
    {
        public MdTrayPlatform Platform { get; init; } = MdTrayPlatform.Windows;
        public bool IsSupported { get; init; } = true;
        public WindowIcon? Icon { get; private set; }
        public string? ToolTipText { get; private set; }
        public bool? IsVisible { get; private set; }
        public NativeMenu? Menu { get; private set; }
        public int Disposals { get; private set; }

        public event EventHandler? Clicked;

        public void SetIcon(WindowIcon? icon) => Icon = icon;
        public void SetToolTipText(string? text) => ToolTipText = text;
        public void SetVisible(bool visible) => IsVisible = visible;
        public void SetMenu(NativeMenu? menu) => Menu = menu;
        public void Dispose() => Disposals++;
        public void RaiseClicked() => Clicked?.Invoke(this, EventArgs.Empty);
    }

    private sealed class RecordingCommand : ICommand
    {
        public int Executions { get; private set; }
        public object? LastParameter { get; private set; }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            Executions++;
            LastParameter = parameter;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
    }

    // The headless icon loader accepts any stream and returns a stub window icon, so the payload
    // does not matter here; the identity of the object the adapter receives does.
    private static WindowIcon CreateIcon() => new(new MemoryStream([1, 2, 3, 4]));

    [AvaloniaFact]
    public void State_Is_Pushed_To_The_Platform_Adapter()
    {
        var adapter = new RecordingTrayAdapter();
        using var tray = new MdTrayIcon
        {
            PlatformAdapter = adapter,
            Icon = CreateIcon(),
            ToolTipText = "Material Gallery",
            IsVisible = false
        };

        Assert.Equal(MdTrayPlatform.Windows, tray.Platform);
        Assert.True(tray.IsSupported);
        Assert.Same(tray.Icon, adapter.Icon);
        Assert.Equal("Material Gallery", adapter.ToolTipText);
        Assert.False(adapter.IsVisible);

        tray.Show();
        Assert.True(tray.IsVisible);
        Assert.True(adapter.IsVisible);

        tray.Hide();
        Assert.False(adapter.IsVisible);
    }

    [AvaloniaFact]
    public void Menu_Items_Are_Exported_As_A_Native_Menu()
    {
        var adapter = new RecordingTrayAdapter();
        var command = new RecordingCommand();
        using var tray = new MdTrayIcon { PlatformAdapter = adapter };

        var open = new MdTrayMenuItem("Open")
        {
            Command = command,
            Gesture = new KeyGesture(Key.O, KeyModifiers.Control),
            ToolTip = "Open the gallery window"
        };
        var light = new MdTrayMenuItem("Light") { IsChecked = true, ToggleType = MenuItemToggleType.Radio };
        var dark = new MdTrayMenuItem("Dark") { ToggleType = MenuItemToggleType.Radio };
        var theme = new MdTrayMenuItem("Theme");
        theme.Items.Add(light);
        theme.Items.Add(dark);

        tray.Items.Add(open);
        tray.Items.Add(theme);
        tray.Items.Add(new MdTrayMenuItemSeparator());
        tray.Items.Add(new MdTrayMenuItem("Quit") { IsEnabled = false });

        var menu = Assert.IsType<NativeMenu>(tray.Menu);
        Assert.Same(menu, adapter.Menu);
        Assert.Equal(4, menu.Items.Count);
        Assert.Same(open, menu.Items[0]);
        Assert.Null(menu.Items[0].Parent);
        Assert.Equal(MenuItemToggleType.Radio, light.ToggleType);
        Assert.True(light.IsChecked);
        Assert.Equal("-", ((NativeMenuItem)menu.Items[2]).Header);
        Assert.False(((NativeMenuItem)menu.Items[3]).IsEnabled);

        var submenu = Assert.IsType<NativeMenu>(theme.Menu);
        Assert.Equal(2, submenu.Items.Count);
        Assert.Same(light, submenu.Items[0]);
        Assert.Equal("Light", ((NativeMenuItem)submenu.Items[0]).Header);
        Assert.Same(theme, submenu.Parent);
    }

    [AvaloniaFact]
    public void Rebuilding_Releases_Items_From_The_Previous_Menu()
    {
        var adapter = new RecordingTrayAdapter();
        using var tray = new MdTrayIcon { PlatformAdapter = adapter };

        var first = new MdTrayMenuItem("First");
        tray.Items.Add(first);
        var originalMenu = Assert.IsType<NativeMenu>(tray.Menu);

        // Adding a second entry rebuilds the exported menu. The items are reused, so the rebuild
        // only works when the previous menu has already released its parent references.
        tray.Items.Add(new MdTrayMenuItem("Second"));

        Assert.NotSame(originalMenu, tray.Menu);
        Assert.Empty(originalMenu.Items);
        Assert.Equal(2, tray.Menu!.Items.Count);
        Assert.Same(first, tray.Menu.Items[0]);
        Assert.Same(tray.Menu, first.Parent);

        // Removing and re-adding the same object must not trip the single-parent rule either.
        tray.Items.Remove(first);
        Assert.Single(tray.Menu!.Items);
        tray.Items.Add(first);
        Assert.Equal(2, tray.Menu!.Items.Count);
    }

    [AvaloniaFact]
    public void A_Menu_Entry_Belongs_To_One_Menu_At_A_Time()
    {
        var adapter = new RecordingTrayAdapter();
        using var tray = new MdTrayIcon { PlatformAdapter = adapter };
        using var other = new MdTrayIcon { PlatformAdapter = new RecordingTrayAdapter() };

        var entry = new MdTrayMenuItem("Open");
        tray.Items.Add(entry);

        // NativeMenu enforces one parent per entry. Pinning the rule here documents why a rebuild
        // has to clear the previous menu instead of handing the same entries to a second one.
        Assert.Throws<InvalidOperationException>(() => other.Items.Add(entry));
    }

    [AvaloniaFact]
    public void Emptying_A_Submenu_Makes_The_Entry_A_Leaf_Again()
    {
        var parent = new MdTrayMenuItem("Theme");
        var child = new MdTrayMenuItem("Light");

        parent.Items.Add(child);
        Assert.NotNull(parent.Menu);

        parent.Items.Remove(child);
        Assert.Null(parent.Menu);
        Assert.Equal("Theme", parent.Header);
    }

    [AvaloniaFact]
    public void Platform_Click_Raises_The_Event_And_Executes_The_Command()
    {
        var adapter = new RecordingTrayAdapter();
        var command = new RecordingCommand();
        using var tray = new MdTrayIcon
        {
            PlatformAdapter = adapter,
            Command = command,
            CommandParameter = "tray"
        };

        var clicks = 0;
        tray.Clicked += (_, _) => clicks++;

        adapter.RaiseClicked();

        Assert.Equal(1, clicks);
        Assert.Equal(1, command.Executions);
        Assert.Equal("tray", command.LastParameter);

        // Activate is the same path a host drives from a keyboard shortcut or a test.
        tray.Activate();
        Assert.Equal(2, clicks);
        Assert.Equal(2, command.Executions);
    }

    [AvaloniaFact]
    public void Platforms_Without_A_Notification_Area_Keep_The_Model_Usable()
    {
        var adapter = new MdNoOpTrayIconPlatformAdapter { Platform = MdTrayPlatform.Android };
        using var tray = new MdTrayIcon { PlatformAdapter = adapter };

        tray.Items.Add(new MdTrayMenuItem("Open"));
        tray.Icon = CreateIcon();
        tray.ToolTipText = "Gallery";
        tray.Show();
        tray.Activate();

        Assert.Equal(MdTrayPlatform.Android, tray.Platform);
        Assert.False(tray.IsSupported);
        // The menu model still exists, so a host can inspect or serialize it for a fallback UI.
        Assert.NotNull(tray.Menu);
    }

    [AvaloniaFact]
    public void Disposing_Detaches_From_The_Platform_Once()
    {
        var adapter = new RecordingTrayAdapter();
        var tray = new MdTrayIcon { PlatformAdapter = adapter };
        tray.Items.Add(new MdTrayMenuItem("Open"));

        tray.Dispose();
        Assert.Equal(1, adapter.Disposals);

        // Everything stays callable after disposal; nothing re-attaches a platform handle.
        tray.IsVisible = false;
        tray.ToolTipText = "after dispose";
        tray.Items.Add(new MdTrayMenuItem("Second"));
        tray.Dispose();
        Assert.Equal(1, adapter.Disposals);
    }

    [AvaloniaFact]
    public void Registered_Icons_Round_Trip_Through_The_Application()
    {
        var application = Application.Current!;
        var previous = MdTrayIcon.GetIcons(application);
        var icons = new MdTrayIcons();
        using var tray = new MdTrayIcon { PlatformAdapter = new RecordingTrayAdapter() };
        icons.Add(tray);

        try
        {
            MdTrayIcon.SetIcons(application, icons);
            Assert.Same(icons, MdTrayIcon.GetIcons(application));
            Assert.Same(tray, Assert.Single(icons));
        }
        finally
        {
            icons.Clear();
            MdTrayIcon.SetIcons(application, previous);
        }
    }

    [AvaloniaFact]
    public void The_Resolved_Adapter_Reports_The_Running_Platform()
    {
        var adapter = MdTrayIconPlatformAdapterResolver.Resolve();
        try
        {
            Assert.Equal(MdTrayIconPlatformAdapterResolver.CurrentPlatform, adapter.Platform);
        }
        finally
        {
            adapter.Dispose();
        }

        // The headless host has no windowing platform, so no tray implementation is created and
        // the adapter reports itself unsupported instead of pretending an icon exists.
        if (MdTrayIconPlatformAdapterResolver.CurrentPlatform is MdTrayPlatform.Linux or MdTrayPlatform.MacOS or MdTrayPlatform.Windows)
        {
            using var probe = new MdAvaloniaTrayIconPlatformAdapter(MdTrayIconPlatformAdapterResolver.CurrentPlatform);
            Assert.False(probe.IsSupported);
            Assert.Null(probe.TrayIcon?.NativeMenuExporter);
        }
    }
}
