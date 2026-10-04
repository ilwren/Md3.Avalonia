using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Gallery;
using Md3.Avalonia.Gallery.Pages;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdGalleryTests
{
    [AvaloniaFact]
    public void Gallery_Shell_And_All_Documentation_Pages_Render()
    {
        Control[] pages =
        [
            new GettingStartedGalleryPage(),
            new DeveloperGalleryPage(),
            new StylesGalleryPage(),
            new ComponentsOverviewGalleryPage(),
            new ButtonGalleryPage(),
            new IconButtonGalleryPage(),
            new FabGalleryPage(),
            new AppBarGalleryPage(),
            new BadgeGalleryPage(),
            new TextBoxGalleryPage(),
            new CheckBoxGalleryPage(),
            new RadioButtonGalleryPage(),
            new ComboBoxGalleryPage(),
            new CarouselGalleryPage(),
            new CardGalleryPage(),
            new ChipGalleryPage(),
            new PickerGalleryPage(),
            new DialogGalleryPage(),
            new DividerGalleryPage(),
            new ListGalleryPage(),
            new LoadingGalleryPage(),
            new ProgressGalleryPage(),
            new MenuGalleryPage(),
            new NavigationBarGalleryPage(),
            new NavigationDrawerGalleryPage(),
            new SearchGalleryPage(),
            new SheetGalleryPage(),
            new SliderGalleryPage(),
            new SnackbarGalleryPage(),
            new SwitchGalleryPage(),
            new TabGalleryPage(),
            new ToolbarGalleryPage(),
            new TooltipGalleryPage(),
            new FlutterParityGalleryPage(),
            new AdvancedSelectionGalleryPage(),
            new AdaptiveGalleryPage(),
            new DesktopAdaptersGalleryPage(),
            new ThemeResourcesGalleryPage(),
            new SymbolGalleryPage(),
            new MotionGalleryPage(),
            new ColorPickerGalleryPage()
        ];

        foreach (var page in pages)
        {
            var userPage = Assert.IsAssignableFrom<UserControl>(page);
            var pageScroller = userPage.Content as MdScrollViewer ??
                               (userPage.Content as MdDialogHost)?.Content as MdScrollViewer ??
                               (userPage.Content as Grid)?.Children.OfType<MdScrollViewer>().FirstOrDefault();
            Assert.NotNull(pageScroller);
            var host = new Window { Width = 1100, Height = 760, Content = page };
            host.Show();
            try
            {
                Assert.True(page.Bounds.Width > 0);
                var pageFrame = host.CaptureRenderedFrame();
                Assert.NotNull(pageFrame);
                MdPreviewAssets.Save(pageFrame, $"{page.GetType().Name}.png");
            }
            finally
            {
                host.Close();
            }
        }

        var gallery = new MainWindow { Width = 1280, Height = 800 };
        gallery.Show();
        try
        {
            var frame = gallery.CaptureRenderedFrame();
            Assert.NotNull(frame);
            MdPreviewAssets.Save(frame, "MdGalleryPreview.png");

            var themeSelector = gallery.GetVisualDescendants().OfType<MdComboBox>()
                .Single(control => control.Name == "ThemeSelector");
            themeSelector.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();
            var darkFrame = gallery.CaptureRenderedFrame();
            Assert.NotNull(darkFrame);
            MdPreviewAssets.Save(darkFrame, "MdGalleryDarkPreview.png");
        }
        finally
        {
            gallery.Close();
        }
    }
}
