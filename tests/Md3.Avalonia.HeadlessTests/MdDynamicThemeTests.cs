using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Gallery;
using Md3.Avalonia.Gallery.Pages;
using Md3.Avalonia.Themes.Dynamic;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdDynamicThemeTests
{
    [Fact]
    public void Baseline_Seed_Matches_Golden_Hct_Vectors()
    {
        var light = MdThemeGenerator.Generate(new MdThemeOptions(), dark: false);
        var dark = MdThemeGenerator.Generate(new MdThemeOptions(), dark: true);

        Assert.Equal("#65558F", Hex(light["Primary"]));
        Assert.Equal("#E9DDFF", Hex(light["PrimaryContainer"]));
        Assert.Equal("#FDF8FD", Hex(light["Surface"]));
        Assert.Equal("#CFBDFE", Hex(dark["Primary"]));
        Assert.Equal("#4D3D75", Hex(dark["PrimaryContainer"]));
        Assert.Equal("#141316", Hex(dark["Surface"]));
        Assert.Equal(49, light.Count);
        Assert.Equal(49, dark.Count);
    }

    [Fact]
    public void Arbitrary_Seed_Variants_And_Contrast_Are_Deterministic()
    {
        var options = new MdThemeOptions
        {
            SeedColor = "#006A6A",
            SchemeVariant = MdThemeSchemeVariant.Expressive,
            ContrastLevel = MdThemeContrastLevel.High
        };
        var first = MdThemeGenerator.Generate(options, false);
        var second = MdThemeGenerator.Generate(options, false);
        Assert.Equal(first, second);
        Assert.All(MdThemeManager.Diagnose(first), diagnostic => Assert.True(diagnostic.Passes,
            $"{diagnostic.ForegroundRole}/{diagnostic.BackgroundRole} = {diagnostic.Ratio:0.00}"));

        var monochrome = MdThemeGenerator.Generate(options with { SchemeVariant = MdThemeSchemeVariant.Monochrome }, false);
        var monochromePrimary = monochrome["Primary"];
        Assert.InRange(Math.Abs(monochromePrimary.R - monochromePrimary.G), 0, 2);
        Assert.InRange(Math.Abs(monochromePrimary.G - monochromePrimary.B), 0, 2);
        Assert.NotEqual(first["Primary"], monochrome["Primary"]);
    }

    [Fact]
    public void Theme_Json_Round_Trips_All_User_Choices()
    {
        var expected = new MdThemeOptions
        {
            SeedColor = "#B3261E",
            SchemeVariant = MdThemeSchemeVariant.Fidelity,
            ContrastLevel = MdThemeContrastLevel.Medium,
            ThemeMode = MdThemeMode.Dark,
            MotionScheme = Md3.Avalonia.Motion.MdMotionScheme.Reduced,
            FontProfile = MdThemeFontProfile.Plain,
            ShapeScale = MdThemeShapeScale.Expressive
        };

        var json = MdThemeJson.Serialize(expected);
        Assert.Equal(expected, MdThemeJson.Deserialize(json));
        Assert.Contains("\"seedColor\": \"#B3261E\"", json, StringComparison.Ordinal);
        Assert.Contains("\"schemeVariant\": \"fidelity\"", json, StringComparison.Ordinal);
    }

    [AvaloniaFact]
    public void Manager_Applies_Colors_Brushes_And_Shape_Resources()
    {
        var application = Assert.IsAssignableFrom<Application>(Application.Current);
        var options = new MdThemeOptions { SeedColor = "#386A20", ShapeScale = MdThemeShapeScale.Compact };
        var roles = MdThemeManager.Apply(application, options, dark: false);

        Assert.Equal(roles["Primary"], Assert.IsType<Color>(application.Resources["Md.Sys.Color.Primary"]));
        Assert.IsType<SolidColorBrush>(application.Resources["Md.Sys.Color.Primary.Brush"]);
        var radius = Assert.IsType<CornerRadius>(application.Resources["Md.Comp.Button.Small.Shape.Round"]);
        Assert.Equal(14.4, radius.TopLeft, precision: 10);
    }

    [AvaloniaFact]
    public void Gallery_Uses_Five_Breakpoint_Bands_And_Search_Index()
    {
        var gallery = new MainWindow { Width = 580, Height = 760 };
        gallery.Show();
        try
        {
            var nav = gallery.FindControl<MdNavigationDrawer>("NavigationPane")!;
            var toc = gallery.FindControl<Border>("TableOfContentsPane")!;
            var menu = gallery.FindControl<MdIconButton>("NavigationMenuButton")!;
            var primaryRail = gallery.FindControl<Border>("PrimaryNavigationRail")!;
            Assert.IsType<ComponentsOverviewGalleryPage>(gallery.FindControl<ContentControl>("PageHost")!.Content);
            Assert.False(nav.IsOpen);
            Assert.False(primaryRail.IsVisible);
            Assert.True(menu.IsVisible);
            Assert.False(toc.IsVisible);

            gallery.Width = 700;
            Dispatcher.UIThread.RunJobs();
            Assert.True(menu.IsVisible);
            Assert.True(primaryRail.IsVisible);
            Assert.Equal(104, gallery.FindControl<Grid>("RootLayout")!.ColumnDefinitions[0].Width.Value);
            Assert.False(nav.IsOpen);

            gallery.Width = 1000;
            Dispatcher.UIThread.RunJobs();
            Assert.True(menu.IsVisible);
            Assert.True(primaryRail.IsVisible);
            Assert.False(nav.IsOpen);
            Assert.False(toc.IsVisible);

            gallery.Width = 1300;
            Dispatcher.UIThread.RunJobs();
            Assert.True(nav.IsOpen);
            Assert.True(toc.IsVisible);

            gallery.Width = 1700;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(240, gallery.FindControl<Grid>("ShellGrid")!.ColumnDefinitions[2].Width.Value);

            var search = gallery.FindControl<MdSearchBar>("GallerySearch")!;
            search.Text = "theme json";
            Dispatcher.UIThread.RunJobs();
            Assert.True(gallery.FindControl<MdButton>("ThemeResourcesNav")!.IsVisible);
            Assert.False(gallery.FindControl<MdButton>("ButtonNav")!.IsVisible);
            search.Text = string.Empty;
            Dispatcher.UIThread.RunJobs();
            Assert.All(gallery.GetVisualDescendants().OfType<MdButton>()
                .Where(button => button.Name?.EndsWith("Nav", StringComparison.Ordinal) == true),
                button => Assert.True(button.IsVisible));
        }
        finally
        {
            gallery.Close();
        }
    }

    private static string Hex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";
}
