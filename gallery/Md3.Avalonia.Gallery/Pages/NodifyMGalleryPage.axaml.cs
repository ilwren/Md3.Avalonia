using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using NodifyM.Avalonia.ViewModelBase;

namespace Md3.Avalonia.Gallery.Pages;

public partial class NodifyMGalleryPage : UserControl
{
    public NodifyMGalleryPage()
    {
        InitializeComponent();

        // The editor is bound the way the upstream samples bind it: a view model that owns the node
        // and connection collections, with the anchor of every pin written back into its own pin.
        DemoEditor.DataContext = new NodifyDemoGraph();
    }
}

/// <summary>
/// The graph the demo editor shows: three nodes, one knot and three wires, which between them cover
/// every control the theme restyles - a node, its two kinds of pin, a plain connector on a knot, a
/// connection with a label and the editor's own canvas.
/// </summary>
public sealed class NodifyDemoGraph : NodifyEditorViewModelBase
{
    public NodifyDemoGraph()
    {
        var loadOut = Pin("Rows", ConnectorViewModelBase.ConnectorFlow.Output);
        var filterIn = Pin("Rows", ConnectorViewModelBase.ConnectorFlow.Input);
        var filterOut = Pin("Kept", ConnectorViewModelBase.ConnectorFlow.Output);
        var renderIn = Pin("Kept", ConnectorViewModelBase.ConnectorFlow.Input);
        var renderOut = Pin("Frame", ConnectorViewModelBase.ConnectorFlow.Output);
        var knot = new KnotNodeViewModel { Location = new Point(470, 250) };

        Nodes = new ObservableCollection<object?>
        {
            Node("Load", "csv · 12 kB", new Point(24, 40), [], [loadOut]),
            Node("Filter", "rows > 0", new Point(272, 168), [filterIn], [filterOut]),
            Node("Render", "surface 320×180", new Point(592, 40), [renderIn], [renderOut]),
            knot
        };

        Connections.Add(new ConnectionViewModelBase(this, loadOut, filterIn));
        Connections.Add(new ConnectionViewModelBase(this, filterOut, knot.Connector));
        Connections.Add(new ConnectionViewModelBase(this, knot.Connector, renderIn));

        foreach (var pin in new[] { loadOut, filterIn, filterOut, renderIn, renderOut, knot.Connector })
        {
            pin.IsConnected = true;
        }
    }

    private static ConnectorViewModelBase Pin(string title, ConnectorViewModelBase.ConnectorFlow flow) => new()
    {
        Title = title,
        Flow = flow
    };

    private static NodeViewModelBase Node(
        string title,
        string footer,
        Point location,
        ConnectorViewModelBase[] inputs,
        ConnectorViewModelBase[] outputs) => new()
    {
        Title = title,
        Footer = footer,
        Location = location,
        Input = new ObservableCollection<object>(inputs),
        Output = new ObservableCollection<object>(outputs)
    };
}
