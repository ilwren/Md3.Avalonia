using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
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

        Assert.Equal(WindowDecorations.Full, window.WindowDecorations);
        Assert.True(window.ExtendClientAreaToDecorationsHint);
        Assert.Equal(44, window.ExtendClientAreaTitleBarHeightHint);
        Assert.True(window.CanResize);

        adapter.Apply(window, new MdBorderlessWindowOptions(true, true, 44, false));
        Assert.Equal(WindowDecorations.None, window.WindowDecorations);
    }

    [AvaloniaFact]
    public void Borderless_Window_Default_Template_Owns_Chrome_Content_And_Eight_Resize_Roles()
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
            var frame = window.GetVisualDescendants().OfType<Border>()
                .Single(control => control.Name == "PART_WindowFrame");
            var titleBar = window.GetVisualDescendants().OfType<MdWindowTitleBar>()
                .Single(control => control.Name == "PART_DefaultTitleBar");
            var presenter = window.GetVisualDescendants().OfType<ContentPresenter>()
                .Single(control => control.Name == "PART_ContentPresenter");
            var grips = window.GetVisualDescendants().OfType<MdWindowResizeGrip>().ToArray();

            // Avalonia 12 draws extended-client decorations outside the Window template. The
            // dedicated empty theme prevents Fluent/Simple caption visuals from becoming a
            // second title bar while WindowDecorations.Full keeps native DWM style bits.
            Assert.NotNull(window.WindowDecorationsTheme);
            Assert.Equal(new Thickness(1), frame.Margin);
            Assert.Equal(9, frame.CornerRadius.TopLeft);
            Assert.Equal(new Thickness(1), frame.BorderThickness);
            Assert.True(frame.ClipToBounds);
            Assert.Equal("Material application", titleBar.Content);
            Assert.True(titleBar.ShowIcon);
            Assert.Equal(44, titleBar.Height);
            Assert.Equal(44, presenter.Margin.Top);
            Assert.Same(content, presenter.Content);
            Assert.Equal(WindowDecorationsElementRole.TitleBar,
                WindowDecorationProperties.GetElementRole(titleBar));
            Assert.Equal(8, grips.Length);
            Assert.Equal(8, grips.Select(WindowDecorationProperties.GetElementRole).Distinct().Count());
            Assert.Contains(WindowDecorationsElementRole.ResizeN,
                grips.Select(WindowDecorationProperties.GetElementRole));
            Assert.Contains(WindowDecorationsElementRole.ResizeSE,
                grips.Select(WindowDecorationProperties.GetElementRole));

            window.TitleBarHeight = 52;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(52, titleBar.Height);
            Assert.Equal(52, presenter.Margin.Top);

            window.IsCustomChromeEnabled = false;
            Dispatcher.UIThread.RunJobs();
            Assert.Null(window.WindowDecorationsTheme);
            Assert.False(titleBar.IsVisible);

            window.PreserveNativeBorder = false;
            window.IsCustomChromeEnabled = true;
            Dispatcher.UIThread.RunJobs();
            Assert.NotNull(window.WindowDecorationsTheme);
            Assert.True(titleBar.IsVisible);
            Assert.Equal(new Thickness(0), frame.Margin);
            Assert.Equal(new CornerRadius(0), frame.CornerRadius);
            Assert.Equal(new Thickness(0), frame.BorderThickness);
            Assert.False(frame.ClipToBounds);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Caption_Buttons_Expose_Native_Roles_And_Restore_State()
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
            Assert.Equal(WindowDecorationsElementRole.MinimizeButton,
                WindowDecorationProperties.GetElementRole(buttons.Single(button => button.Kind == MdCaptionButtonKind.Minimize)));
            Assert.Equal(WindowDecorationsElementRole.MaximizeButton,
                WindowDecorationProperties.GetElementRole(buttons.Single(button => button.Kind == MdCaptionButtonKind.MaximizeRestore)));
            Assert.Equal(WindowDecorationsElementRole.CloseButton,
                WindowDecorationProperties.GetElementRole(buttons.Single(button => button.Kind == MdCaptionButtonKind.Close)));

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
