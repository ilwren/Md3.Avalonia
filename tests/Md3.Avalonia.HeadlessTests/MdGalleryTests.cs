using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using AvaloniaEdit;
using Md3.Avalonia.Gallery;
using Md3.Avalonia.Gallery.Components;
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
            new FormValidationGalleryPage(),
            new SimpleDialogGalleryPage(),
            new AboutDialogGalleryPage(),
            new PickerRestorationGalleryPage(),
            new DraggableSheetGalleryPage(),
            new KeyboardAvoidanceGalleryPage(),
            new AdaptiveControlsGalleryPage(),
            new FocusShortcutHeroGalleryPage(),
            new BannerGalleryPage(),
            new ExpansionPanelGalleryPage(),
            new DataTableGalleryPage(),
            new StepperGalleryPage(),
            new RefreshIndicatorGalleryPage(),
            new PaginatedDataTableGalleryPage(),
            new ReorderableListGalleryPage(),
            new GridTileGalleryPage(),
            new DismissibleGalleryPage(),
            new SegmentedButtonGalleryPage(),
            new RangeSliderGalleryPage(),
            new DateRangePickerGalleryPage(),
            new AutoCompleteGalleryPage(),
            new SurfaceGalleryPage(),
            new ResponsiveContentGalleryPage(),
            new ScrollViewerGalleryPage(),
            new AdaptiveGalleryPage(),
            new ThemeResourcesGalleryPage(),
            new SymbolGalleryPage(),
            new MotionGalleryPage(),
            new ColorPickerGalleryPage(),
            new PopoverGalleryPage(),
            new HoverCardGalleryPage(),
            new CommandPaletteGalleryPage(),
            new DensityGalleryPage(),
            new SlidableItemGalleryPage(),
            new DataGridGalleryPage(),
            new MasonryPanelGalleryPage(),
            new PagedItemsGalleryPage(),
            new AsyncSelectGalleryPage(),
            new CalendarGalleryPage(),
            new TimelineGalleryPage(),
            new CascaderGalleryPage(),
            new TransferGalleryPage(),
            new ResultViewGalleryPage(),
            new ChartGalleryPage(),
            new RichEditorGalleryPage(),
            new ChatViewGalleryPage(),
            new BeforeAfterGalleryPage(),
            new AnimatedTextGalleryPage(),
            new SpinKitGalleryPage(),
            new StaggeredPanelGalleryPage(),
            new SkeletonGalleryPage(),
            new AnimationSequenceGalleryPage(),
            new PinInputGalleryPage(),
            new TreeViewGalleryPage(),
            new TagInputGalleryPage(),
            new AvatarGalleryPage(),
            new RatingGalleryPage()
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

    [AvaloniaFact]
    public void Code_Examples_Never_Show_A_Placeholder_Where_A_Sample_Belongs()
    {
        // Thirty-two pages supply only AXAML. The component used to stand in a TODO comment for
        // the missing C#, so every one of them had a tab you could click into and find nothing
        // but an instruction to the authors - which is what the review reported as empty Usage.
        var xamlOnly = new CodeExample { XamlCode = "<md:MdButton Content=\"Send\" />" };
        var host = new Window { Width = 800, Height = 600, Content = xamlOnly };
        host.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            var csharp = xamlOnly.GetVisualDescendants().OfType<TextEditor>()
                .Single(editor => editor.Name == "CSharpEditor");
            Assert.Equal(string.Empty, csharp.Text);
            Assert.DoesNotContain("Add the equivalent", csharp.Text, StringComparison.Ordinal);

            var tab = xamlOnly.GetVisualDescendants().OfType<MdSegmentedButton>()
                .Single(button => button.Name == "CSharpTab");
            Assert.False(tab.IsVisible, "a language with nothing to show must not offer a tab");
        }
        finally { host.Close(); }
    }
}
