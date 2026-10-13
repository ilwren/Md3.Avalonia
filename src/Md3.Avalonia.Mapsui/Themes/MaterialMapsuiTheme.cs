using System;
using System.Collections.Generic;
using System.Linq;
// Avalonia.Styling is here for ThemeVariant and carries an IStyle of its own, so the one a layer
// carries is written out in full wherever it is named.
using Avalonia.Styling;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.UI.Avalonia;
using MapsuiBrush = Mapsui.Styles.Brush;
using MapsuiColor = Mapsui.Styles.Color;
using MapsuiPen = Mapsui.Styles.Pen;

namespace Md3.Avalonia.Themes;

/// <summary>
/// Material 3 appearance for a Mapsui map: the canvas behind the layers, the default vector and label
/// styles a layer is drawn with, the type scale a label is typeset in, and the Material palette a
/// thematic map takes its colours from.
/// </summary>
/// <remarks>
/// <para>
/// Mapsui has no style collection to register in: <see cref="MapControl"/> draws its own surface from
/// the colours a <see cref="Map"/> and its layers carry, and it asks no Avalonia resource host for
/// anything. So this theme is applied to a map - or to the control that draws it - instead of being
/// added to <c>Application.Styles</c>, the way <c>Md3.Avalonia.ScottPlot</c> and
/// <c>Md3.Avalonia.LiveCharts2</c> apply theirs. Nothing here reaches an Avalonia style tree, so only
/// the maps an application asks for are themed.
/// </para>
/// <para>
/// The rules are the same ones those two providers write: a surface carries the drawing, the
/// medium-emphasis ink carries lines and labels, a boundary is one <c>Outline</c> hairline, a region
/// is its accent at twelve percent, and the palette is the same eight <c>Md.Sys.Color</c> roles in the
/// same order. Everything Mapsui decides stays as it ships - projection, tile fetching, navigation,
/// hit testing, animation - the theme only writes colours, sizes and the palette. A layer whose style
/// was chosen by somebody keeps it; a map's own content is nobody's to recolour from outside.
/// </para>
/// </remarks>
public sealed class MaterialMapsuiTheme
{
    /// <summary>The Material colour roles the palette walks, in order.</summary>
    private static readonly string[] PaletteRoleNames =
    [
        "Primary",
        "Tertiary",
        "Secondary",
        "Error",
        "PrimaryContainer",
        "TertiaryContainer",
        "InversePrimary",
        "Outline",
    ];

    /// <summary>Initializes a theme that reads one variant's tokens.</summary>
    /// <param name="dark">Whether the dark variant's tokens should be read.</param>
    public MaterialMapsuiTheme(bool dark)
    {
        IsDark = dark;
    }

    /// <summary>Gets whether this theme reads the dark variant's tokens.</summary>
    public bool IsDark { get; }

    /// <summary>
    /// Gets the <c>Md.Sys.Color</c> roles the palette walks, in the order it walks them.
    /// </summary>
    /// <remarks>
    /// The first four give a palette of four hues; the remaining four are further tones of the same
    /// accent hues, so a thematic map with more than four classes still reads as Material instead of
    /// inventing colours. The roles and their order are the ones the ScottPlot and LiveCharts2
    /// providers use, so a map and a chart beside it agree.
    /// </remarks>
    public static IReadOnlyList<string> SeriesPaletteRoles => PaletteRoleNames;

    /// <summary>Builds the Material palette for one variant.</summary>
    /// <param name="dark">Whether the dark variant's tokens should be read.</param>
    /// <returns>The palette over <see cref="SeriesPaletteRoles"/>, as Mapsui colours.</returns>
    public static IReadOnlyList<MapsuiColor> SeriesPalette(bool dark) =>
        PaletteRoleNames.Select(role => MdMapsuiTokens.Color(role, dark)).ToArray();

    /// <summary>Applies the Material rules for one variant to a map.</summary>
    /// <param name="map">The map to style.</param>
    /// <param name="dark">Whether the dark variant's tokens should be read.</param>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> is <c>null</c>.</exception>
    public static void Apply(Map map, bool dark)
    {
        ArgumentNullException.ThrowIfNull(map);
        new MaterialMapsuiTheme(dark).Apply(map);
    }

    /// <summary>Applies the Material rules to a map.</summary>
    /// <param name="map">The map to style.</param>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> is <c>null</c>.</exception>
    /// <remarks>
    /// The canvas becomes <c>Surface</c>, so a map inside a card blends with the card, and every layer
    /// that nobody has styled is given the Material vector default instead. A layer whose style was
    /// chosen is left alone.
    /// </remarks>
    public void Apply(Map map)
    {
        ArgumentNullException.ThrowIfNull(map);

        map.BackColor = MdMapsuiTokens.Color("Surface", IsDark);

        foreach (var layer in map.Layers)
        {
            Apply(layer);
        }
    }

    /// <summary>Applies the Material rules to one layer, if nobody has styled it yet.</summary>
    /// <param name="layer">The layer to style.</param>
    /// <exception cref="ArgumentNullException"><paramref name="layer"/> is <c>null</c>.</exception>
    /// <remarks>
    /// Every layer Mapsui creates already carries a style - <c>BaseLayer</c>'s constructor gives it a
    /// grey outline, a black line and a white fill - so "nobody has styled it" cannot mean "its style
    /// is null": it means the style is still the one Mapsui handed out. A layer whose style differs
    /// from that default was chosen by somebody - a thematic map picks per feature, a labelled layer
    /// carries a label style - and is left exactly as it is.
    /// </remarks>
    public void Apply(ILayer layer)
    {
        ArgumentNullException.ThrowIfNull(layer);

        if (!IsMapsuiDefault(layer.Style))
        {
            return;
        }

        var styled = VectorStyle();

        // Only the colours are replaced. What a layer shows and at which resolutions is its content's
        // business, so those travel across even when the style they were set on did not.
        if (layer.Style is { } previous)
        {
            styled.Enabled = previous.Enabled;
            styled.Opacity = previous.Opacity;
            styled.MinVisible = previous.MinVisible;
            styled.MaxVisible = previous.MaxVisible;
        }

        layer.Style = styled;
    }

    /// <summary>Whether a layer's style is still the one Mapsui gave it.</summary>
    /// <param name="style">The style to look at.</param>
    /// <returns><c>true</c> when nobody has chosen a style for the layer yet.</returns>
    /// <remarks>
    /// <see cref="VectorStyle"/> overrides no equality member, but the pen and brush it is made of
    /// compare by value, so the comparison is made field by field against a freshly built default.
    /// Anything that is not a vector style - a label style, a symbol style, a collection of them - is
    /// somebody's choice by definition.
    /// </remarks>
    private static bool IsMapsuiDefault(Mapsui.Styles.IStyle? style)
    {
        if (style is null)
        {
            return true;
        }

        if (style is not VectorStyle vector)
        {
            return false;
        }

        var defaults = new VectorStyle();
        return Equals(vector.Outline, defaults.Outline) &&
               Equals(vector.Line, defaults.Line) &&
               Equals(vector.Fill, defaults.Fill);
    }

    /// <summary>Builds the Material style for line and polygon features.</summary>
    /// <returns>A vector style in the variant's tokens.</returns>
    /// <remarks>
    /// A polygon's boundary is a hairline in <c>Outline</c>, a line feature is a heavier stroke in the
    /// medium-emphasis ink, and a region is filled with <c>Primary</c> at twelve percent - the tint a
    /// Material container lays over a surface. Mapsui's brush takes a colour rather than an opacity,
    /// so the tint is mixed here rather than set on the brush.
    /// </remarks>
    public VectorStyle VectorStyle() => new()
    {
        Outline = new MapsuiPen(MdMapsuiTokens.Color("Outline", IsDark), 1),
        Line = new MapsuiPen(MdMapsuiTokens.Color("OnSurfaceVariant", IsDark), 1.5),
        Fill = new MapsuiBrush(MdMapsuiTokens.Color("Primary", IsDark, 0.12)),
    };

    /// <summary>Builds the Material style for a text label.</summary>
    /// <returns>A label style in the variant's tokens.</returns>
    /// <remarks>
    /// A label floats over whatever is under it, so it is a <c>SurfaceContainer</c> chip with the soft
    /// <c>OutlineVariant</c> border and <c>ExtraSmall</c> corner a Material badge carries, inked in
    /// <c>OnSurface</c> and typeset at <c>LabelMedium</c> in the application's plain typeface.
    /// </remarks>
    public LabelStyle LabelStyle() => new()
    {
        ForeColor = MdMapsuiTokens.Color("OnSurface", IsDark),
        BackColor = new MapsuiBrush(MdMapsuiTokens.Color("SurfaceContainer", IsDark)),
        BorderColor = MdMapsuiTokens.Color("OutlineVariant", IsDark),
        BorderThickness = 1,
        CornerRounding = (int)MdMapsuiTokens.Corner("ExtraSmall", 4),
        Font = new Font
        {
            FontFamily = MdMapsuiTokens.FontFamily,
            Size = MdMapsuiTokens.Size("LabelMedium", 12),
        },
    };

    /// <summary>
    /// Applies the Material rules to a <see cref="MapControl"/> using the variant the control actually
    /// has, now and after every theme-variant change.
    /// </summary>
    /// <param name="control">The control whose map should follow the application's variant.</param>
    /// <exception cref="ArgumentNullException"><paramref name="control"/> is <c>null</c>.</exception>
    /// <remarks>
    /// Mapsui bakes colours into a map when they are set, so - unlike an Avalonia style - a themed map
    /// does not re-resolve them on its own. This is the one-liner that keeps a map in step with the
    /// light/dark switch: it applies the rules immediately and re-applies them from the control's own
    /// <c>ActualThemeVariantChanged</c> event, so the subscription lives exactly as long as the control
    /// does. Applying the theme by hand instead is fine - call it again with the new variant.
    /// </remarks>
    public static void Follow(MapControl control)
    {
        ArgumentNullException.ThrowIfNull(control);
        ApplyForVariant(control);
        control.ActualThemeVariantChanged += (_, _) => ApplyForVariant(control);
    }

    /// <summary>Applies the rules for the variant the given control has, and redraws it.</summary>
    private static void ApplyForVariant(MapControl control)
    {
        Apply(control.Map, control.ActualThemeVariant == ThemeVariant.Dark);
        control.RefreshGraphics();
    }
}
