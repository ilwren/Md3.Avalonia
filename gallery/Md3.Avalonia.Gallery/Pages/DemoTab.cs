using System.Collections.Generic;
using System.Linq;

namespace Md3.Avalonia.Gallery.Pages;

/// <summary>
/// One draggable tab on the Tabalonia demo page. Public because the page's data templates are
/// compiled against it (<c>x:DataType</c>), and mutable because dragging and closing mutate the
/// collection the strip is bound to.
/// </summary>
public sealed class DemoTab(string header, string body)
{
    /// <summary>The one body every demo tab starts with; also what the zh/CJK localization maps.</summary>
    public const string DefaultBody =
        "Material tokens drive this pane: the strip sits on Surface, the selected tab is a Primary container with OnPrimary ink, and hover is the 0.08 onSurface state layer.";

    public string Header { get; set; } = header;

    public string Body { get; set; } = body;

    public static List<DemoTab> Create(params string[] headers) =>
        headers.Select(header => new DemoTab(header, DefaultBody)).ToList();
}
