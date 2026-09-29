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

            Assert.Equal(9, frame.CornerRadius.TopLeft);
            Assert.Equal("Material application", titleBar.Content);
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
