using System.Globalization;
using Md3.Avalonia.Extra.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdBreadcrumbPathConverterTests
{
    [Fact]
    public void Converts_Path_To_Prefix_Items_And_Accents_Ancestors()
    {
        var converter = new MdBreadcrumbPathConverter();
        var items = Assert.IsAssignableFrom<IReadOnlyList<MdBreadcrumbItem>>(
            converter.Convert("Home\\Settings/Security", typeof(IReadOnlyList<MdBreadcrumbItem>), null, CultureInfo.InvariantCulture));

        Assert.Equal(3, items.Count);
        Assert.Equal("Home", Assert.IsType<string>(items[0].Label));
        Assert.Equal("Home/Settings", Assert.IsType<string>(items[1].CommandParameter));
        Assert.Equal("Home/Settings/Security", Assert.IsType<string>(items[2].CommandParameter));
        Assert.True(items[0].UseAccentColor);
        Assert.True(items[1].UseAccentColor);
        Assert.True(items[2].IsCurrent);
        Assert.False(items[2].UseAccentColor);
    }

    [Fact]
    public void Converts_Rooted_Path_With_A_Clickable_Root_Item()
    {
        var converter = new MdBreadcrumbPathConverter();
        var items = Assert.IsAssignableFrom<IReadOnlyList<MdBreadcrumbItem>>(
            converter.Convert("/Home/Settings", typeof(IReadOnlyList<MdBreadcrumbItem>), null, CultureInfo.InvariantCulture));

        Assert.Equal(3, items.Count);
        Assert.Equal("/", Assert.IsType<string>(items[0].Label));
        Assert.Equal("/", Assert.IsType<string>(items[0].CommandParameter));
        Assert.True(items[0].UseAccentColor);
        Assert.Equal("/Home/Settings", Assert.IsType<string>(items[^1].CommandParameter));
        Assert.True(items[^1].IsCurrent);
        Assert.False(items[^1].UseAccentColor);
    }

    [Fact]
    public void Empty_Path_Produces_No_Items()
    {
        var converter = new MdBreadcrumbPathConverter();
        var items = Assert.IsAssignableFrom<IReadOnlyList<MdBreadcrumbItem>>(
            converter.Convert("  ", typeof(IReadOnlyList<MdBreadcrumbItem>), null, CultureInfo.InvariantCulture));

        Assert.Empty(items);
    }
}
