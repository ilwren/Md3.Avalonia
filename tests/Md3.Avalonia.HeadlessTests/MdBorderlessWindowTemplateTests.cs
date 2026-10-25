using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless.XUnit;
using Path = Avalonia.Controls.Shapes.Path;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdBorderlessWindowTemplateTests
{
    [AvaloniaFact]
    public void Windows_Adapter_Preserves_Full_Caption_Styles_For_Native_Dwm_State_Animations()
    {
        var window = new MdBorderlessWindow { PlatformAdapter = new TemplateAdapter() };
        var adapter = new MdWindowsWindowPlatformAdapter();

        adapter.Apply(window, new MdBorderlessWindowOptions(true, true, 44, true));

        Assert.Equal(SystemDecorations.Full, window.SystemDecorations);
        Assert.True(window.ExtendClientAreaToDecorationsHint);
        Assert.Equal(44, window.ExtendClientAreaTitleBarHeightHint);
        Assert.True(window.CanResize);

        adapter.Apply(window, new MdBorderlessWindowOptions(true, true, 44, false));
        Assert.Equal(SystemDecorations.None, window.SystemDecorations);
    }

    [AvaloniaFact]
    public void Borderless_Window_Default_Template_Owns_Chrome_Content_And_Eight_Resize_Grips()
    {
        var content = new TextBlock { Text = "Application content" };
        var window = new MdBorderlessWindow
        {
            PlatformAdapter = new TemplateAdapter(),
            Title = "Material application",
            TitleBarHeight = 44,
            Width = 720,
            Height = 480,
            Content = content
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            // Re-apply the native-frame state after the window is attached so the pseudo-class
            // style is deterministic across headless and desktop-window initialization paths.
            window.PreserveNativeBorder = false;
            window.PreserveNativeBorder = true;
            Dispatcher.UIThread.RunJobs();
            var frame = window.GetVisualDescendants().OfType<Border>()
                .Single(control => control.Name == "PART_WindowFrame");
            var titleBar = window.GetVisualDescendants().OfType<MdWindowTitleBar>()
                .Single(control => control.Name == "PART_DefaultTitleBar");
            var presenter = window.GetVisualDescendants().OfType<ContentPresenter>()
                .Single(control => control.Name == "PART_ContentPresenter");
            var grips = window.GetVisualDescendants().OfType<MdWindowResizeGrip>().ToArray();

            // Avalonia 11 keeps native SystemDecorations while this template owns the
            // extended client title bar, so Fluent/Simple does not add a second caption row.
            Assert.Equal("Material application", titleBar.Content);
            Assert.True(titleBar.ShowIcon);
            Assert.Equal(44, titleBar.Height);
            Assert.Equal(44, presenter.Margin.Top);
            Assert.Same(content, presenter.Content);
            Assert.Equal(8, grips.Length);
            Assert.Equal(8, grips.Select(grip => grip.Edge).Distinct().Count());
            Assert.Contains(grips, grip => grip.Edge == WindowEdge.North);
            Assert.Contains(grips, grip => grip.Edge == WindowEdge.SouthEast);

            window.TitleBarHeight = 52;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(52, titleBar.Height);
            Assert.Equal(52, presenter.Margin.Top);

            window.IsCustomChromeEnabled = false;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(SystemDecorations.Full, window.SystemDecorations);
            Assert.False(window.ExtendClientAreaToDecorationsHint);
            Assert.False(titleBar.IsVisible);

            window.PreserveNativeBorder = false;
            window.IsCustomChromeEnabled = true;
            Dispatcher.UIThread.RunJobs();
            Assert.True(titleBar.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Caption_Buttons_Expose_Accessible_Names_And_Restore_State()
    {
        var window = new MdBorderlessWindow
        {
            PlatformAdapter = new TemplateAdapter(),
            Width = 640,
            Height = 420,
            Content = new Border()
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var buttons = window.GetVisualDescendants().OfType<MdCaptionButton>().ToArray();
            Assert.Equal(3, buttons.Length);
            Assert.Equal("Minimize", AutomationProperties.GetName(
                buttons.Single(button => button.Kind == MdCaptionButtonKind.Minimize)));
            Assert.Equal("Close", AutomationProperties.GetName(
                buttons.Single(button => button.Kind == MdCaptionButtonKind.Close)));

            var maximize = buttons.Single(button => button.Kind == MdCaptionButtonKind.MaximizeRestore);
            Assert.Equal("Maximize", AutomationProperties.GetName(maximize));
            window.WindowState = WindowState.Maximized;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("Restore", AutomationProperties.GetName(maximize));
            var restoreGlyph = maximize.GetVisualDescendants().OfType<Path>()
                .Single(path => path.Name == "PART_RestoreGlyph");
            Assert.True(restoreGlyph.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Caption_Button_Visibility_And_Enabled_State_Are_Independently_Controllable()
    {
        var window = new MdBorderlessWindow
        {
            PlatformAdapter = new TemplateAdapter(),
            ShowMinimizeButton = true,
            ShowMaximizeButton = true,
            ShowCloseButton = true,
            IsMinimizeButtonEnabled = false,
            IsMaximizeButtonEnabled = false,
            IsCloseButtonEnabled = true,
            Content = new Border()
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var buttons = window.GetVisualDescendants().OfType<MdCaptionButton>().ToArray();
            Assert.False(buttons.Single(button => button.Kind == MdCaptionButtonKind.Minimize).IsEnabled);
            Assert.False(buttons.Single(button => button.Kind == MdCaptionButtonKind.MaximizeRestore).IsEnabled);
            Assert.True(buttons.Single(button => button.Kind == MdCaptionButtonKind.Close).IsEnabled);

            window.ShowMinimizeButton = false;
            Dispatcher.UIThread.RunJobs();
            Assert.False(buttons.Single(button => button.Kind == MdCaptionButtonKind.Minimize).IsVisible);
        }
        finally { window.Close(); }
    }

    private sealed class TemplateAdapter : IMdWindowPlatformAdapter
    {
        public MdWindowPlatform Platform => MdWindowPlatform.Unknown;
        public MdWindowCapabilities Capabilities => MdWindowCapabilities.All;
        public void Apply(MdBorderlessWindow window, MdBorderlessWindowOptions options) { }
        public bool TryBeginMove(MdBorderlessWindow window, PointerPressedEventArgs args) => true;
        public bool TryBeginResize(MdBorderlessWindow window, WindowEdge edge, PointerPressedEventArgs args) => true;
        public bool TryShowSystemMenu(MdBorderlessWindow window, Point clientPoint) => false;
    }
}
