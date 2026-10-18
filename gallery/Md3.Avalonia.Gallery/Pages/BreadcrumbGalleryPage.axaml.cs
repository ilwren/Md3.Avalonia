using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class BreadcrumbGalleryPage : UserControl
{
    private static readonly string[] FullPath = ["Home", "Settings", "Account", "Security", "Two-factor"];

    public static readonly StyledProperty<string> CurrentPathProperty =
        AvaloniaProperty.Register<BreadcrumbGalleryPage, string>(nameof(CurrentPath), string.Join("/", FullPath));

    private readonly ObservableCollection<MdBreadcrumbItem> _iconPath = new();

    public BreadcrumbGalleryPage()
    {
        InitializeComponent();

        ResetIconPath();
        IconBreadcrumb.ItemsSource = _iconPath;
        DeepBreadcrumb.ItemsSource = new[] { "Root", "usr", "local", "share", "fonts", "MaterialSymbols.ttf" };
    }

    public string CurrentPath
    {
        get => GetValue(CurrentPathProperty);
        private set => SetValue(CurrentPathProperty, value);
    }

    private void OnBreadcrumbItemInvoked(object? sender, object? item)
    {
        if (item is not MdBreadcrumbItem model || model.CommandParameter is not string path) return;
        CurrentPath = path;
        BreadcrumbStatus.Text = $"Navigated to: {model.Label} — the trailing segments were dropped.";
    }

    private void OnResetPath(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        CurrentPath = string.Join("/", FullPath);
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

    private void OnPowerlineBreadcrumbInvoked(object? sender, object? item)
    {
        if (item is not MdBreadcrumbItem model || model.CommandParameter is not string path) return;
        CurrentPath = path;
        PowerlineStatus.Text = $"Navigated to: {model.Label} — trailing segments were dropped.";
    }

    private void OnDeepBreadcrumbInvoked(object? sender, object? item)
        => BreadcrumbStatus.Text = $"Jumped to directory: {item}";

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
