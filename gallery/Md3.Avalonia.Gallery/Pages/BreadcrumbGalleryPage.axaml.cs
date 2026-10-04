using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class BreadcrumbGalleryPage : UserControl
{
    // A breadcrumb is a navigation control, so the demo has to actually navigate. Showing a
    // status line alone made the control look inert: the path never changed, which is the one
    // response a user expects from clicking a parent segment.
    private static readonly string[] FullPath = ["Home", "Settings", "Account", "Security", "Two-factor"];

    private readonly ObservableCollection<string> _path = new(FullPath);
    private readonly ObservableCollection<MdBreadcrumbItem> _iconPath = new();

    public BreadcrumbGalleryPage()
    {
        InitializeComponent();

        DemoBreadcrumb.ItemsSource = _path;
        ResetIconPath();
        IconBreadcrumb.ItemsSource = _iconPath;
        DeepBreadcrumb.ItemsSource = new[] { "Root", "usr", "local", "share", "fonts", "MaterialSymbols.ttf" };
    }

    private void OnBreadcrumbItemInvoked(object? sender, object? item)
    {
        if (item is not string segment) return;
        NavigateTo(_path, segment);
        BreadcrumbStatus.Text = $"Navigated to: {segment} — the trailing segments were dropped.";
    }

    private void OnResetPath(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        _path.Clear();
        foreach (var segment in FullPath) _path.Add(segment);
        BreadcrumbStatus.Text = "Click any breadcrumb segment to navigate to it.";
    }

    private void OnIconBreadcrumbInvoked(object? sender, object? item)
    {
        if (item is not MdBreadcrumbItem model) return;
        var index = _iconPath.IndexOf(model);
        if (index < 0) return;
        while (_iconPath.Count > index + 1) _iconPath.RemoveAt(_iconPath.Count - 1);
        foreach (var entry in _iconPath) entry.IsCurrent = false;
        _iconPath[^1].IsCurrent = true;
        BreadcrumbStatus.Text = $"Selected path item: {model.Label}";
    }

    private void OnDeepBreadcrumbInvoked(object? sender, object? item)
        => BreadcrumbStatus.Text = $"Jumped to directory: {item}";

    private static void NavigateTo(IList<string> path, string segment)
    {
        var index = path.IndexOf(segment);
        if (index < 0) return;
        while (path.Count > index + 1) path.RemoveAt(path.Count - 1);
    }

    private void ResetIconPath()
    {
        _iconPath.Clear();
        foreach (var entry in new[]
                 {
                     new MdBreadcrumbItem { Label = "Home", Icon = MdSymbols.Home },
                     new MdBreadcrumbItem { Label = "Documents", Icon = MdSymbols.Folder },
                     new MdBreadcrumbItem { Label = "Reports", Icon = MdSymbols.Description }
                 })
        {
            _iconPath.Add(entry);
        }

        _iconPath.Last().IsCurrent = true;
    }
}
