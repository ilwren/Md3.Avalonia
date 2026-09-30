using Avalonia.Controls;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Ecosystem.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class BreadcrumbGalleryPage : UserControl
{
    public BreadcrumbGalleryPage()
    {
        InitializeComponent();
        DemoBreadcrumb.ItemsSource = new[] { "Home", "Settings", "Account", "Security" };
        IconBreadcrumb.ItemsSource = new object[]
        {
            new MdBreadcrumbItem { Label = "Home", Icon = MdSymbols.Home },
            new MdBreadcrumbItem { Label = "Documents", Icon = MdSymbols.Folder },
            new MdBreadcrumbItem { Label = "Reports", Icon = MdSymbols.Description, IsCurrent = true }
        };
        DeepBreadcrumb.ItemsSource = new[] { "Root", "usr", "local", "share", "fonts", "MaterialSymbols.ttf" };
    }

    private void OnBreadcrumbItemInvoked(object? sender, object? item)
    {
        if (BreadcrumbStatus is not null)
            BreadcrumbStatus.Text = $"Navigated to: {item}";
    }

    private void OnIconBreadcrumbInvoked(object? sender, object? item)
    {
        if (BreadcrumbStatus is not null)
            BreadcrumbStatus.Text = $"Selected path item: {item}";
    }

    private void OnDeepBreadcrumbInvoked(object? sender, object? item)
    {
        if (BreadcrumbStatus is not null)
            BreadcrumbStatus.Text = $"Jumped to directory: {item}";
    }
}
