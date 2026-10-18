using System.Linq;
using Avalonia.Controls;
using Avalonia.Styling;
using Mapsui;
using Mapsui.Features;
using Mapsui.Layers;
using Mapsui.Styles;
using Md3.Avalonia.Themes;

namespace Md3.Avalonia.Gallery.Pages;

public partial class MapsuiGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    private readonly MemoryLayer _sensors;

    public MapsuiGalleryPage()
    {
        InitializeComponent();

        // Four points held in memory, in the metres the map's own CRS counts in and close enough
        // together to fit one screen. The gallery never fetches a tile, so this page renders the same
        // offline as it does online - and a page that needs the network to show its theme is a page
        // that cannot be tested.
        var places = new (string English, string Chinese, double X, double Y)[]
        {
            ("North gate", "北门", -180, 120),
            ("Pump house", "泵站", 140, 96),
            ("Reservoir", "蓄水池", 60, -140),
            ("Substation", "变电站", -210, -96),
        };

        var features = places.Select(place =>
        {
            var feature = new PointFeature(place.X, place.Y);
            feature["name"] = L(place.English, place.Chinese);
            return feature;
        }).ToArray();

        _sensors = new MemoryLayer { Name = L("Sensors", "传感器"), Features = features };

        var map = DemoMap.Map;
        map.Layers.Add(_sensors);
        map.Navigator.CenterOnAndZoomTo(new MPoint(0, 0), 1);

        // The canvas is themed the way an application would theme it: for the variant the control
        // actually has, and again whenever the application switches, because Mapsui bakes colours into
        // a map when they are set.
        MaterialMapsuiTheme.Follow(DemoMap);

        // The layer's own styles are built for the variant too, and rebuilt when it changes - the same
        // reason Follow exists. A style collection is a style somebody chose, so Follow leaves it be.
        var dark = DemoMap.ActualThemeVariant == ThemeVariant.Dark;
        StyleLayer(dark);
        ActualThemeVariantChanged += (_, _) => StyleLayer(DemoMap.ActualThemeVariant == ThemeVariant.Dark);
    }

    /// <summary>Gives the demo layer the marker and label styles for one variant.</summary>
    private void StyleLayer(bool dark)
    {
        var theme = new MaterialMapsuiTheme(dark);

        // The label reads its text off a field of the feature, which is how Mapsui asks for it; the
        // theme only decides what the chip looks like.
        var labels = theme.LabelStyle();
        labels.LabelColumn = "name";

        // Both styles in one collection, so a feature is drawn as a marker and labelled: the vector
        // style gives the marker its fill and hairline, the label style the chip.
        _sensors.Style = new StyleCollection { Styles = { theme.VectorStyle(), labels } };
    }
}
