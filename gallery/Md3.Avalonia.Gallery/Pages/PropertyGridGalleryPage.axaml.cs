using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Media;

namespace Md3.Avalonia.Gallery.Pages;

public partial class PropertyGridGalleryPage : UserControl
{
    public PropertyGridGalleryPage()
    {
        InitializeComponent();

        // The grid reads the object it is given from its DataContext, the way the upstream samples
        // bind it, and builds one cell editor per property from the built-in factories - which is
        // exactly the set of native controls the theme has to cover.
        DemoGrid.DataContext = new GridDemoOptions();
    }
}

/// <summary>One enum cell: the grid draws it as a combo box.</summary>
public enum GridDemoAccent
{
    Primary,
    Secondary,
    Tertiary,
    Error
}

/// <summary>
/// The object the demo grid edits. Every property type here is one the grid's built-in cell
/// factories handle, so the page exercises the whole set of native controls the theme covers.
/// </summary>
public sealed class GridDemoOptions
{
    [Category("Text")]
    [DisplayName("Title")]
    [Description("A string cell: the Material filled field.")]
    public string Title { get; set; } = "Material 3";

    [Category("Text")]
    [DisplayName("Watermark")]
    [Description("A read-only cell keeps the field's disabled state.")]
    [ReadOnly(true)]
    public string Author { get; set; } = "Md3.Avalonia";

    [Category("Numbers")]
    [DisplayName("Volume")]
    [Description("A numeric cell: a filled field with Material spinners.")]
    public int Volume { get; set; } = 42;

    [Category("Numbers")]
    [DisplayName("Ratio")]
    public double Ratio { get; set; } = 0.75;

    [Category("Choices")]
    [DisplayName("Accent")]
    [Description("An enum cell: a Material combo box.")]
    public GridDemoAccent Accent { get; set; } = GridDemoAccent.Primary;

    [Category("Choices")]
    [DisplayName("Enabled")]
    [Description("A boolean cell: a Material checkbox.")]
    public bool Enabled { get; set; } = true;

    [Category("Choices")]
    [DisplayName("Compact")]
    public bool Compact { get; set; }

    [Category("Date and time")]
    [DisplayName("Published")]
    [Description("A DateTime cell: the picker's own wheel flyout in a Material card.")]
    public DateTime Published { get; set; } = new(2026, 10, 12);

    [Category("Date and time")]
    [DisplayName("Duration")]
    public TimeSpan Duration { get; set; } = new(1, 30, 0);

    [Category("Appearance")]
    [DisplayName("Tint")]
    [Description("A colour cell: Avalonia's own colour previewer, which the grid themes itself.")]
    public Color Tint { get; set; } = Color.Parse("#6750A4");

    [Category("Collections")]
    [DisplayName("Tags")]
    [Description("A collection cell: the grid's own list editor with Material buttons.")]
    public List<string> Tags { get; set; } = ["material", "theme", "avalonia"];

    [Category("Layout")]
    [DisplayName("Metrics")]
    [Description("An expandable object: the grid nests another grid for it.")]
    public GridDemoMetrics Metrics { get; set; } = new();
}

/// <summary>The nested object, which the grid expands into a grid of its own.</summary>
public sealed class GridDemoMetrics
{
    [DisplayName("Spacing")]
    public int Spacing { get; set; } = 12;

    [DisplayName("Rounded")]
    public bool Rounded { get; set; } = true;

    [DisplayName("Label")]
    public string Label { get; set; } = "Density";
}
