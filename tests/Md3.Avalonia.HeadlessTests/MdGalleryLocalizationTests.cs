using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Md3.Avalonia.Gallery;
using Md3.Avalonia.Gallery.Pages;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// The Gallery translates by key match: <c>Translate</c> returns the English source whenever a key
/// is absent, so a page missing an entry is silently half-translated and nothing fails. These tests
/// are the runtime half of that bargain, which the static checker in
/// <c>scripts/check-gallery-localization.py</c> cannot cover: the dictionary is built in a static
/// initialiser, so a key written twice throws when the first page is localized, and <c>Apply</c> has
/// to survive every control type a page happens to hold.
/// </summary>
public sealed class MdGalleryLocalizationTests
{
    private static readonly CultureInfo Chinese = CultureInfo.GetCultureInfo("zh-CN");
    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

    private static string Snapshot(UserControl page) =>
        string.Join("|", page.GetLogicalDescendants().OfType<TextBlock>().Select(block => block.Text));

    [AvaloniaFact]
    public void Apply_Translates_A_Body_Into_Chinese_And_Restores_The_English_Source()
    {
        const string source = "Switches immediately toggle a single setting on or off.";
        const string translated = "开关用于即时开启或关闭单个设置项。";

        var page = new SwitchGalleryPage();
        var body = page.GetLogicalDescendants().OfType<TextBlock>().Single(block => block.Text == source);

        // Touching GalleryLocalization at all builds the dictionary, so this is also the check that
        // no key is defined twice: a duplicate makes the static initialiser throw here.
        GalleryLocalization.Apply(page, Chinese);
        Assert.Equal(translated, body.Text);

        // The originals are cached per control, so switching back has to restore the source rather
        // than translate the translation.
        GalleryLocalization.Apply(page, English);
        Assert.Equal(source, body.Text);
    }

    [AvaloniaFact]
    public void Apply_Translates_Something_On_Every_Page_It_Is_Given_And_Restores_All_Of_Them()
    {
        // A sample rather than all 107 pages: MdGalleryAdaptiveTests already instantiates every
        // page, and a second full sweep would push the suite towards its own timeout. The cascader
        // is here because it is the one page whose placeholder text is rewritten too.
        UserControl[] pages =
        {
            new SwitchGalleryPage(),
            new CascaderGalleryPage(),
            new TabGalleryPage(),
            new ComponentsOverviewGalleryPage(),
            new BeforeAfterGalleryPage(),
        };

        foreach (var page in pages)
        {
            var english = Snapshot(page);

            GalleryLocalization.Apply(page, Chinese);
            Assert.True(english != Snapshot(page),
                $"{page.GetType().Name} has no translated prose, so Apply did nothing to it");

            GalleryLocalization.Apply(page, English);
            Assert.Equal(english, Snapshot(page));
        }
    }
}
